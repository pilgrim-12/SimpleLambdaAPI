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
            // Детальное логирование
            context.Logger.LogLine($"Request Method: {request.HttpMethod}");
            context.Logger.LogLine($"Request Path: {request.Path}");
            context.Logger.LogLine($"Request Resource: {request.Resource}");
            context.Logger.LogLine($"Request Body: {request.Body}");

            try
            {
                return request.HttpMethod?.ToUpper() switch
                {
                    "GET" => HandleGet(),
                    "POST" => HandlePost(request.Body),
                    "PUT" => HandlePut(request.Body),
                    "DELETE" => CreateResponse(200, "[]"),
                    "OPTIONS" => CreateResponse(200, ""), // Для CORS preflight
                    _ => CreateResponse(405, $"Method '{request.HttpMethod}' not allowed. Supported methods: GET, POST, PUT, DELETE")
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
            catch (JsonException)
            {
                return CreateResponse(400, JsonSerializer.Serialize(new { error = "Invalid JSON format" }));
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
            catch (JsonException)
            {
                return CreateResponse(400, JsonSerializer.Serialize(new { error = "Invalid JSON format" }));
            }
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