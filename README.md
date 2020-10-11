# AWS Exploration
Using an AWS Lambda with a custom event in order to manually read from an S3 bucket and write to a DynamoDB.

The project relies on having the following default case sensitive environment variables in the Lambda:
 - BucketName
 - DbName
 - Separator

All of these can be changed inside the code and updated inside the function.
