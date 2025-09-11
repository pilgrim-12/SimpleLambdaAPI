# Simple Lambda API

Serverless Todo API running on AWS Lambda with DynamoDB storage.

## Features

- ✅ Full CRUD operations for Todo items
- ✅ Persistent storage with DynamoDB  
- ✅ REST API via API Gateway
- ✅ Automatic deployment with GitHub Actions
- ✅ Serverless architecture (pay per request)

## API Endpoints

Base URL: `https://gvzgfebzu5.execute-api.us-east-2.amazonaws.com/default/TodoLambdaFunction`

### Get all todos
```
GET /
```

Response:
```json
[
    {
        "Id": 1,
        "Title": "Learn AWS Lambda",
        "IsCompleted": false
    },
    {
        "Id": 2,
        "Title": "Deploy to AWS",
        "IsCompleted": true
    }
]
```

### Create a new todo
```
POST /
Content-Type: application/json

{
    "Title": "Your task title",
    "IsCompleted": false
}
```

Response:
```json
{
    "Id": 3,
    "Title": "Your task title",
    "IsCompleted": false
}
```

### Update existing todo
```
PUT /
Content-Type: application/json

{
    "Id": 1,
    "Title": "Updated title",
    "IsCompleted": true
}
```

Response:
```json
{
    "Id": 1,
    "Title": "Updated title",
    "IsCompleted": true
}
```

### Delete all todos
```
DELETE /
```

Response:
```json
{
    "message": "All todos deleted"
}
```

### Delete specific todo
```
DELETE /
Content-Type: application/json

{
    "Id": 1
}
```

Response:
```json
{
    "message": "Todo deleted"
}
```

## Technologies

- **Backend**: C# / .NET 8
- **Compute**: AWS Lambda
- **API**: AWS API Gateway (REST)
- **Database**: Amazon DynamoDB
- **CI/CD**: GitHub Actions
- **IaC**: AWS CloudFormation (via Lambda/API Gateway)

## Architecture

```
┌─────────────┐     ┌──────────────┐     ┌─────────────┐     ┌──────────────┐
│   Client    │────▶│ API Gateway  │────▶│   Lambda    │────▶│  DynamoDB    │
│ (Postman)   │◀────│  REST API    │◀────│  Function   │◀────│    Table     │
└─────────────┘     └──────────────┘     └─────────────┘     └──────────────┘
                           │                      │
                           ▼                      ▼
                    ┌─────────────┐       ┌──────────────┐
                    │   GitHub    │       │ CloudWatch   │
                    │   Actions   │       │    Logs      │
                    └─────────────┘       └──────────────┘
```

## Testing with cURL

### Get all todos
```bash
curl https://gvzgfebzu5.execute-api.us-east-2.amazonaws.com/default/TodoLambdaFunction
```

### Create a new todo
```bash
curl -X POST https://gvzgfebzu5.execute-api.us-east-2.amazonaws.com/default/TodoLambdaFunction \
  -H "Content-Type: application/json" \
  -d '{"Title":"New Task","IsCompleted":false}'
```

### Update a todo
```bash
curl -X PUT https://gvzgfebzu5.execute-api.us-east-2.amazonaws.com/default/TodoLambdaFunction \
  -H "Content-Type: application/json" \
  -d '{"Id":1,"Title":"Updated Task","IsCompleted":true}'
```

### Delete all todos
```bash
curl -X DELETE https://gvzgfebzu5.execute-api.us-east-2.amazonaws.com/default/TodoLambdaFunction
```

### Delete specific todo
```bash
curl -X DELETE https://gvzgfebzu5.execute-api.us-east-2.amazonaws.com/default/TodoLambdaFunction \
  -H "Content-Type: application/json" \
  -d '{"Id":1}'
```

## Testing with PowerShell

### Get all todos
```powershell
Invoke-RestMethod -Uri "https://gvzgfebzu5.execute-api.us-east-2.amazonaws.com/default/TodoLambdaFunction" -Method Get
```

### Create a new todo
```powershell
$body = @{
    Title = "New Task from PowerShell"
    IsCompleted = $false
} | ConvertTo-Json

Invoke-RestMethod -Uri "https://gvzgfebzu5.execute-api.us-east-2.amazonaws.com/default/TodoLambdaFunction" `
    -Method Post `
    -Body $body `
    -ContentType "application/json"
```

### Update a todo
```powershell
$body = @{
    Id = 1
    Title = "Updated from PowerShell"
    IsCompleted = $true
} | ConvertTo-Json

Invoke-RestMethod -Uri "https://gvzgfebzu5.execute-api.us-east-2.amazonaws.com/default/TodoLambdaFunction" `
    -Method Put `
    -Body $body `
    -ContentType "application/json"
```

### Delete all todos
```powershell
Invoke-RestMethod -Uri "https://gvzgfebzu5.execute-api.us-east-2.amazonaws.com/default/TodoLambdaFunction" -Method Delete
```

### Delete specific todo
```powershell
$body = @{
    Id = 1
} | ConvertTo-Json

Invoke-RestMethod -Uri "https://gvzgfebzu5.execute-api.us-east-2.amazonaws.com/default/TodoLambdaFunction" `
    -Method Delete `
    -Body $body `
    -ContentType "application/json"
```

## Testing with Postman

1. Import the following collection:

### GET Get Todos
- Method: `GET`
- URL: `https://gvzgfebzu5.execute-api.us-east-2.amazonaws.com/default/TodoLambdaFunction`

### POST Add Todo
- Method: `POST`
- URL: `https://gvzgfebzu5.execute-api.us-east-2.amazonaws.com/default/TodoLambdaFunction`
- Headers: `Content-Type: application/json`
- Body (raw, JSON):
```json
{
    "Title": "Test from Postman",
    "IsCompleted": false
}
```

### PUT Update Todo
- Method: `PUT`
- URL: `https://gvzgfebzu5.execute-api.us-east-2.amazonaws.com/default/TodoLambdaFunction`
- Headers: `Content-Type: application/json`
- Body (raw, JSON):
```json
{
    "Id": 1,
    "Title": "Updated from Postman",
    "IsCompleted": true
}
```

### DELETE Delete All Todos
- Method: `DELETE`
- URL: `https://gvzgfebzu5.execute-api.us-east-2.amazonaws.com/default/TodoLambdaFunction`

### DELETE Delete By Id
- Method: `DELETE`
- URL: `https://gvzgfebzu5.execute-api.us-east-2.amazonaws.com/default/TodoLambdaFunction`
- Headers: `Content-Type: application/json`
- Body (raw, JSON):
```json
{
    "Id": 1
}
```

## Local Development

1. Clone the repository:
```bash
git clone https://github.com/pilgrim-12/SimpleLambdaAPI.git
cd SimpleLambdaAPI
```

2. Navigate to the project:
```bash
cd TodoLambda/src/TodoLambda
```

3. Restore dependencies:
```bash
dotnet restore
```

4. Build the project:
```bash
dotnet build
```

5. Run tests (if available):
```bash
dotnet test
```

## Deployment

The project automatically deploys to AWS Lambda when you push to the main branch:

1. Make your changes
2. Commit and push:
```bash
git add .
git commit -m "Your changes"
git push
```

3. GitHub Actions will automatically:
   - Build the project
   - Package the Lambda function
   - Deploy to AWS Lambda
   - Update the function code

Monitor deployment: https://github.com/pilgrim-12/SimpleLambdaAPI/actions

## Manual Deployment

If you need to deploy manually:

1. Build for Lambda:
```bash
dotnet publish -c Release -r linux-x64
```

2. Package:
```bash
cd bin/Release/net8.0/linux-x64/publish
zip -r ../../../../../lambda-package.zip .
```

3. Deploy using AWS CLI:
```bash
aws lambda update-function-code \
    --function-name TodoLambdaFunction \
    --zip-file fileb://lambda-package.zip \
    --region us-east-2
```

## Configuration

### Required AWS Resources:
- **Lambda Function**: TodoLambdaFunction
  - Runtime: .NET 8
  - Memory: 256 MB
  - Timeout: 30 seconds
- **DynamoDB Table**: TodoItems
  - Partition Key: Id (Number)
  - Billing: Pay per request
- **API Gateway**: REST API
  - Resource: /TodoLambdaFunction
  - Methods: GET, POST, PUT, DELETE, OPTIONS
- **IAM Role**: TodoLambdaRole
  - AWSLambdaBasicExecutionRole
  - AmazonDynamoDBFullAccess

### GitHub Secrets Required:
- `AWS_ACCESS_KEY_ID` - Your AWS access key
- `AWS_SECRET_ACCESS_KEY` - Your AWS secret key
- `LAMBDA_ROLE_ARN` - ARN of the Lambda execution role

### Environment Variables:
- `TABLE_NAME`: TodoItems (set in Lambda configuration)

## Project Structure

```
SimpleLambdaAPI/
├── .github/
│   └── workflows/
│       └── deploy.yml          # GitHub Actions deployment
├── TodoLambda/
│   └── src/
│       └── TodoLambda/
│           ├── Function.cs     # Lambda handler
│           ├── TodoLambda.csproj
│           └── aws-lambda-tools-defaults.json
├── README.md
└── .gitignore
```

## Cost

This serverless architecture means you only pay for what you use:
- **Lambda**: First 1M requests/month are free
- **DynamoDB**: 25 GB storage and 25 provisioned capacity units free
- **API Gateway**: First 1M API calls/month free
- **CloudWatch Logs**: 5 GB ingestion and storage free

Estimated cost for low usage: **$0/month**

## Monitoring

View function logs and metrics:

1. CloudWatch Logs: https://console.aws.amazon.com/cloudwatch/
2. Lambda Monitoring: https://console.aws.amazon.com/lambda/
3. DynamoDB Metrics: https://console.aws.amazon.com/dynamodbv2/

## Troubleshooting

### 500 Internal Server Error
- Check CloudWatch logs for the Lambda function
- Verify DynamoDB permissions in IAM role
- Ensure table name matches in code and AWS

### 404 Not Found
- Verify API Gateway URL is correct
- Check if Lambda function is deployed
- Ensure API Gateway is configured properly

### Empty Response
- Check JSON property names match C# properties (case-sensitive)
- Verify Content-Type header is `application/json`
- Look at CloudWatch logs for serialization errors

## Future Improvements

- [ ] Add authentication with AWS Cognito
- [ ] Implement pagination for GET requests
- [ ] Add request validation
- [ ] Create a frontend application
- [ ] Add unit and integration tests
- [ ] Implement API versioning
- [ ] Add OpenAPI/Swagger documentation
- [ ] Add search and filtering capabilities
- [ ] Implement soft delete
- [ ] Add created/updated timestamps

## License

MIT License

Copyright (c) 2025 pilgrim-12

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.