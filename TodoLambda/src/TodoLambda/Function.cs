using Amazon.Lambda.Core;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using System.Text.Json;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Threading.Tasks;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace TodoLambda
{
    public class Function
    {
        private readonly IAmazonDynamoDB _dynamoDbClient;
        private const string TableName = "TodoItems";

        public Function()
        {
            _dynamoDbClient = new AmazonDynamoDBClient();
        }

        public async Task<APIGatewayProxyResponse> FunctionHandler(APIGatewayProxyRequest request, ILambdaContext context)
        {
            context.Logger.LogLine($"=== FULL REQUEST START ===");
            context.Logger.LogLine($"Full request JSON: {JsonSerializer.Serialize(request)}");
            context.Logger.LogLine($"Request is null: {request == null}");
            context.Logger.LogLine($"HttpMethod: '{request?.HttpMethod}'");
            context.Logger.LogLine($"Path: '{request?.Path}'");
            context.Logger.LogLine($"Body: '{request?.Body}'");
            context.Logger.LogLine($"=== FULL REQUEST END ===");

            try
            {
                if (request == null)
                {
                    context.Logger.LogLine("ERROR: Request is null!");
                    return CreateResponse(400, JsonSerializer.Serialize(new { error = "Request is null" }));
                }

                var httpMethod = string.IsNullOrWhiteSpace(request.HttpMethod) ? "GET" : request.HttpMethod.ToUpper();
                context.Logger.LogLine($"Processing as method: {httpMethod}");

                return httpMethod switch
                {
                    "GET" => await HandleGet(),
                    "POST" => await HandlePost(request.Body),
                    "PUT" => await HandlePut(request.Body),
                    "DELETE" => await HandleDelete(request.Body),
                    "OPTIONS" => HandleOptions(),
                    _ => CreateResponse(405, JsonSerializer.Serialize(new { error = "Method not allowed" }))
                };
            }
            catch (Exception ex)
            {
                context.Logger.LogLine($"ERROR: {ex.GetType().Name}: {ex.Message}");
                context.Logger.LogLine($"StackTrace: {ex.StackTrace}");
                return CreateResponse(500, JsonSerializer.Serialize(new { error = ex.Message, type = ex.GetType().Name }));
            }
        }

        private async Task<APIGatewayProxyResponse> HandleGet()
        {
            var scanRequest = new ScanRequest
            {
                TableName = TableName
            };

            var response = await _dynamoDbClient.ScanAsync(scanRequest);
            var todos = new List<TodoItem>();

            foreach (var item in response.Items)
            {
                todos.Add(new TodoItem
                {
                    Id = int.Parse(item["Id"].N),
                    Title = item["Title"].S,
                    IsCompleted = item["IsCompleted"].BOOL
                });
            }

            return CreateResponse(200, JsonSerializer.Serialize(todos.OrderBy(t => t.Id)));
        }

        private async Task<APIGatewayProxyResponse> HandlePost(string? body)
        {
            if (string.IsNullOrEmpty(body))
            {
                return CreateResponse(400, JsonSerializer.Serialize(new { error = "Request body is required" }));
            }

            try
            {
                var newTodo = JsonSerializer.Deserialize<TodoItem>(body);
                if (newTodo == null)
                {
                    return CreateResponse(400, JsonSerializer.Serialize(new { error = "Invalid todo item" }));
                }

                // Get next ID
                var scanRequest = new ScanRequest { TableName = TableName };
                var scanResponse = await _dynamoDbClient.ScanAsync(scanRequest);
                var maxId = scanResponse.Items.Count > 0
                    ? scanResponse.Items.Max(item => int.Parse(item["Id"].N))
                    : 0;

                newTodo.Id = maxId + 1;

                // Save to DynamoDB
                var putRequest = new PutItemRequest
                {
                    TableName = TableName,
                    Item = new Dictionary<string, AttributeValue>
                    {
                        ["Id"] = new AttributeValue { N = newTodo.Id.ToString() },
                        ["Title"] = new AttributeValue { S = newTodo.Title },
                        ["IsCompleted"] = new AttributeValue { BOOL = newTodo.IsCompleted }
                    }
                };

                await _dynamoDbClient.PutItemAsync(putRequest);
                return CreateResponse(201, JsonSerializer.Serialize(newTodo));
            }
            catch (JsonException ex)
            {
                return CreateResponse(400, JsonSerializer.Serialize(new { error = "Invalid JSON format", details = ex.Message }));
            }
        }

        private async Task<APIGatewayProxyResponse> HandlePut(string? body)
        {
            if (string.IsNullOrEmpty(body))
            {
                return CreateResponse(400, JsonSerializer.Serialize(new { error = "Request body is required" }));
            }

            try
            {
                var updatedTodo = JsonSerializer.Deserialize<TodoItem>(body);
                if (updatedTodo == null)
                {
                    return CreateResponse(400, JsonSerializer.Serialize(new { error = "Invalid todo item" }));
                }

                // Check if item exists
                var getRequest = new GetItemRequest
                {
                    TableName = TableName,
                    Key = new Dictionary<string, AttributeValue>
                    {
                        ["Id"] = new AttributeValue { N = updatedTodo.Id.ToString() }
                    }
                };

                var getResponse = await _dynamoDbClient.GetItemAsync(getRequest);
                if (getResponse.Item == null || getResponse.Item.Count == 0)
                {
                    return CreateResponse(404, JsonSerializer.Serialize(new { error = "Todo not found" }));
                }

                // Update item
                var updateRequest = new UpdateItemRequest
                {
                    TableName = TableName,
                    Key = new Dictionary<string, AttributeValue>
                    {
                        ["Id"] = new AttributeValue { N = updatedTodo.Id.ToString() }
                    },
                    AttributeUpdates = new Dictionary<string, AttributeValueUpdate>
                    {
                        ["Title"] = new AttributeValueUpdate
                        {
                            Action = AttributeAction.PUT,
                            Value = new AttributeValue { S = updatedTodo.Title }
                        },
                        ["IsCompleted"] = new AttributeValueUpdate
                        {
                            Action = AttributeAction.PUT,
                            Value = new AttributeValue { BOOL = updatedTodo.IsCompleted }
                        }
                    }
                };

                await _dynamoDbClient.UpdateItemAsync(updateRequest);
                return CreateResponse(200, JsonSerializer.Serialize(updatedTodo));
            }
            catch (JsonException ex)
            {
                return CreateResponse(400, JsonSerializer.Serialize(new { error = "Invalid JSON format", details = ex.Message }));
            }
        }

        private async Task<APIGatewayProxyResponse> HandleDelete(string? body)
        {
            if (!string.IsNullOrEmpty(body))
            {
                try
                {
                    var todoToDelete = JsonSerializer.Deserialize<TodoItem>(body);
                    if (todoToDelete != null && todoToDelete.Id > 0)
                    {
                        var deleteRequest = new DeleteItemRequest
                        {
                            TableName = TableName,
                            Key = new Dictionary<string, AttributeValue>
                            {
                                ["Id"] = new AttributeValue { N = todoToDelete.Id.ToString() }
                            }
                        };

                        await _dynamoDbClient.DeleteItemAsync(deleteRequest);
                        return CreateResponse(200, JsonSerializer.Serialize(new { message = "Todo deleted" }));
                    }
                }
                catch { }
            }

            // Delete all
            var scanRequest = new ScanRequest { TableName = TableName };
            var scanResponse = await _dynamoDbClient.ScanAsync(scanRequest);

            foreach (var item in scanResponse.Items)
            {
                var deleteRequest = new DeleteItemRequest
                {
                    TableName = TableName,
                    Key = new Dictionary<string, AttributeValue>
                    {
                        ["Id"] = new AttributeValue { N = item["Id"].N }
                    }
                };
                await _dynamoDbClient.DeleteItemAsync(deleteRequest);
            }

            return CreateResponse(200, JsonSerializer.Serialize(new { message = "All todos deleted" }));
        }

        private APIGatewayProxyResponse HandleOptions()
        {
            return CreateResponse(200, "");
        }

        private APIGatewayProxyResponse CreateResponse(int statusCode, string body)
        {
            return new APIGatewayProxyResponse
            {
                StatusCode = statusCode,
                Headers = new Dictionary<string, string>
                {
                    { "Content-Type", "application/json" },
                    { "Access-Control-Allow-Origin", "*" },
                    { "Access-Control-Allow-Methods", "GET,POST,PUT,DELETE,OPTIONS" },
                    { "Access-Control-Allow-Headers", "Content-Type,X-Amz-Date,Authorization,X-Api-Key,X-Amz-Security-Token" }
                },
                Body = body
            };
        }
    }
}