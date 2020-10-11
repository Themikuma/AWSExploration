using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ABSAProject.User;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using Amazon.Lambda.Core;
using Amazon.Lambda.S3Events;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Util;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace ABSAProject
{
    public class Function
    {
        IAmazonS3 S3Client { get; }
        public string BucketName { get; }
        public string TableName { get; set; }
        public AmazonDynamoDBClient DbClient { get; }

        /// <summary>
        /// Default constructor. This constructor is used by Lambda to construct the instance. When invoked in a Lambda environment
        /// the AWS credentials will come from the IAM role associated with the function and the AWS region will be set to the
        /// region the Lambda function is executed in.
        /// </summary>
        public Function()
        {
            S3Client = new AmazonS3Client();
            BucketName = Environment.GetEnvironmentVariable("BucketName");
            TableName = Environment.GetEnvironmentVariable("DbName");
            DbClient = new AmazonDynamoDBClient();
        }

        /// <summary>
        /// Constructs an instance with a preconfigured S3 client. This can be used for testing the outside of the Lambda environment.
        /// </summary>
        /// <param name="s3Client"></param>
        public Function(IAmazonS3 s3Client)
        {
            this.S3Client = s3Client;
        }

        /// <summary>
        /// This method is called for every Lambda invocation. It is overloaded with a custom event
        /// in order to use the Lambda from console.
        /// </summary>
        /// <param name="evnt"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task<string> FunctionHandler(ABSATestEvent evnt, ILambdaContext context)
        {
            context.Logger.LogLine($"Got an event. The bucket from config is {BucketName}");
            var s3Files = await S3Client.ListObjectsV2Async(new ListObjectsV2Request() { BucketName = BucketName, Prefix = evnt.Path });
            var keysList = s3Files.S3Objects.Where(t => t.Key.EndsWith(".csv")).Select(t => t.Key);
            var tableExists = await EnsureTableExist(evnt.Path, context);
            if (tableExists)
            {
                context.Logger.LogLine($"Table {TableName} already exists");
            }
            List<Task<PutItemResponse>> tasks = new List<Task<PutItemResponse>>();
            foreach (var file in keysList)
            {
                context.Logger.LogLine($"Reading from key: {file}");
                var dataResponse = await S3Client.GetObjectAsync(BucketName, file);
                using (StreamReader sr = new StreamReader(dataResponse.ResponseStream))
                {
                    while (!sr.EndOfStream)
                    {
                        var user = UserFactory.CreateUser(sr.ReadLine());
                        tasks.Add(DbClient.PutItemAsync(TableName, user.GetUserDocument(evnt.Path)));
                    }
                }
            }
            await Task.WhenAll(tasks);
            return "Function ran successfully";
        }

        private async Task<bool> EnsureTableExist(string path, ILambdaContext context)
        {
            var tables = await DbClient.ListTablesAsync();
            if (tables.TableNames.Contains(TableName))
            {
                return true;
            }

            const string idColumn = "Id";
            const string partition = "Path";
            var attributes = new List<AttributeDefinition>();
            attributes.Add(new AttributeDefinition(partition, ScalarAttributeType.S));
            attributes.Add(new AttributeDefinition(idColumn, ScalarAttributeType.S));

            var keySchema = new List<KeySchemaElement>();
            keySchema.Add(new KeySchemaElement(partition, KeyType.HASH));
            keySchema.Add(new KeySchemaElement(idColumn, KeyType.RANGE));

            ProvisionedThroughput throughput = new ProvisionedThroughput(10, 5);

            var request = new CreateTableRequest(TableName, keySchema, attributes, throughput);
            context.Logger.LogLine($"Creating table {TableName}");

            var response = await DbClient.CreateTableAsync(request);
            var status = response.TableDescription.TableStatus;
            //Need to improve this to wait for the correct status. Do it blocking
            while (status != TableStatus.ACTIVE)
            {
                status = DbClient.DescribeTableAsync(new DescribeTableRequest(TableName)).Result.Table.TableStatus;
                Thread.Sleep(500);
            }
            context.Logger.LogLine($"Table {TableName} has status: {status}");
            return false;

        }
    }
}
