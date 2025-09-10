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
            context.Logger.LogLine($"Request: {request.HttpMethod} {request.Path}");

            try
            {
                return request.HttpMethod switch
                {
                    "GET" => HandleGet(),
                    "POST" => HandlePost(request.Body),
                    "PUT" => HandlePut(request.Body),
                    _ => CreateResponse(405, "Method not allowed")
                };
            }
            catch (Exception ex)
            {
                context.Logger.LogLine($"Error: {ex.Message}");
                return CreateResponse(500, ex.Message);
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
                return CreateResponse(400, "Request body is required");
            }

            try
            {
                var newTodo = JsonSerializer.Deserialize<TodoItem>(body);
                if (newTodo == null)
                {
                    return CreateResponse(400, "Invalid todo item");
                }

                newTodo.Id = todos.Count > 0 ? todos.Max(t => t.Id) + 1 : 1;
                todos.Add(newTodo);
                return CreateResponse(201, JsonSerializer.Serialize(newTodo));
            }
            catch (JsonException)
            {
                return CreateResponse(400, "Invalid JSON format");
            }
        }

        private APIGatewayProxyResponse HandlePut(string? body)
        {
            if (string.IsNullOrEmpty(body))
            {
                return CreateResponse(400, "Request body is required");
            }

            try
            {
                var updatedTodo = JsonSerializer.Deserialize<TodoItem>(body);
                if (updatedTodo == null)
                {
                    return CreateResponse(400, "Invalid todo item");
                }

                var existingTodo = todos.FirstOrDefault(t => t.Id == updatedTodo.Id);

                if (existingTodo != null)
                {
                    existingTodo.Title = updatedTodo.Title;
                    existingTodo.IsCompleted = updatedTodo.IsCompleted;
                    return CreateResponse(200, JsonSerializer.Serialize(existingTodo));
                }

                return CreateResponse(404, "Todo not found");
            }
            catch (JsonException)
            {
                return CreateResponse(400, "Invalid JSON format");
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
                    { "Access-Control-Allow-Origin", "*" }
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