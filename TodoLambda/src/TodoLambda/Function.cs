using Amazon.Lambda.Core;
using Amazon.Lambda.APIGatewayEvents;
using System.Text.Json;
using System.Collections.Generic;
using System.Linq;
using System;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace TodoLambda
{
    public class Function
    {
        private static List<TodoItem> todos = new List<TodoItem>
        {
            new TodoItem { Id = 1, Title = "Learn AWS Lambda", IsCompleted = false },
            new TodoItem { Id = 2, Title = "Deploy to AWS", IsCompleted = false }
        };

        public APIGatewayProxyResponse FunctionHandler(APIGatewayProxyRequest request, ILambdaContext context)
        {
            // Детальное логирование для отладки
            context.Logger.LogLine($"=== Request Details ===");
            context.Logger.LogLine($"HttpMethod: '{request.HttpMethod}'");
            context.Logger.LogLine($"Path: '{request.Path}'");
            context.Logger.LogLine($"Resource: '{request.Resource}'");

            if (request.Headers != null)
            {
                foreach (var header in request.Headers)
                {
                    context.Logger.LogLine($"Header: {header.Key} = {header.Value}");
                }
            }

            context.Logger.LogLine($"Body: {request.Body}");
            context.Logger.LogLine($"===================");

            try
            {
                // Если метод пустой или null, пробуем GET по умолчанию
                var httpMethod = string.IsNullOrWhiteSpace(request.HttpMethod) ? "GET" : request.HttpMethod.ToUpper();

                context.Logger.LogLine($"Processing as method: {httpMethod}");

                return httpMethod switch
                {
                    "GET" => HandleGet(),
                    "POST" => HandlePost(request.Body),
                    "PUT" => HandlePut(request.Body),
                    "DELETE" => HandleDelete(),
                    "OPTIONS" => HandleOptions(),
                    _ => CreateResponse(405, JsonSerializer.Serialize(new
                    {
                        error = "Method not allowed",
                        method = httpMethod,
                        supportedMethods = new[] { "GET", "POST", "PUT", "DELETE", "OPTIONS" }
                    }))
                };
            }
            catch (Exception ex)
            {
                context.Logger.LogLine($"Error: {ex.Message}");
                context.Logger.LogLine($"StackTrace: {ex.StackTrace}");
                return CreateResponse(500, JsonSerializer.Serialize(new { error = ex.Message }));
            }
        }

        private APIGatewayProxyResponse HandleGet()
        {
            return CreateResponse(200, JsonSerializer.Serialize(todos));
        }

        private APIGatewayProxyResponse HandlePost(string? body)
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

                newTodo.Id = todos.Count > 0 ? todos.Max(t => t.Id) + 1 : 1;
                todos.Add(newTodo);
                return CreateResponse(201, JsonSerializer.Serialize(newTodo));
            }
            catch (JsonException ex)
            {
                return CreateResponse(400, JsonSerializer.Serialize(new
                {
                    error = "Invalid JSON format",
                    details = ex.Message
                }));
            }
        }

        private APIGatewayProxyResponse HandlePut(string? body)
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

                var existingTodo = todos.FirstOrDefault(t => t.Id == updatedTodo.Id);

                if (existingTodo != null)
                {
                    existingTodo.Title = updatedTodo.Title;
                    existingTodo.IsCompleted = updatedTodo.IsCompleted;
                    return CreateResponse(200, JsonSerializer.Serialize(existingTodo));
                }

                return CreateResponse(404, JsonSerializer.Serialize(new { error = "Todo not found" }));
            }
            catch (JsonException ex)
            {
                return CreateResponse(400, JsonSerializer.Serialize(new
                {
                    error = "Invalid JSON format",
                    details = ex.Message
                }));
            }
        }

        private APIGatewayProxyResponse HandleDelete()
        {
            todos.Clear();
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

    public class TodoItem
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public bool IsCompleted { get; set; }
    }
}