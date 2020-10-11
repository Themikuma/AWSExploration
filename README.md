# AWS Exploration
Using an AWS Lambda with a custom event in order to manually read from an S3 bucket and write to a DynamoDB.

The project relies on having the following default case sensitive environment variables in the Lambda:
 - BucketName
 - DbName
 - Separator

All of these can be changed inside the code and updated inside the function. It is important to just keep them in sync.

In order to run the project download the latest version of the AWS Toolkit for Visual studio: https://aws.amazon.com/visualstudio/. Follow the instructions in order to deploy the Lambda function and set up the S3 bucket. The DynamoDB table should automatically create.

The current setup of the project requires a specific setup of the data in the S3 bucket:
 - Id - Guid
 - Name - string
 - Password - string
 - Created - string (datetime)
Default separator needs to be provided in the Separator environment variable.
