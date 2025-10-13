using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace GameOfLife.Configuration;

public class SwaggerOperationFilter : IOperationFilter
{
    private static readonly (Type Type, string StatusCode)[] NonGenericResultMap = new[]
    {
        (typeof(Accepted), "202"),
        (typeof(Ok), "200"),
        (typeof(NoContent), "204"),
        (typeof(BadRequest), "400"),
        (typeof(NotFound), "404"),
    };

    private static readonly (string GenericTypeName, string StatusCode)[] GenericResultMap = new[]
    {
        ("BadRequest`1", "400"),
        ("Ok`1", "200"),
        ("Created`1", "201"),
        ("Accepted`1", "202"),
        ("NotFound`1", "404"),
    };

    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var mi = context.MethodInfo;
        var returnType = mi.ReturnType;

        if (returnType.IsGenericType &&
            (returnType.GetGenericTypeDefinition() == typeof(Task<>) ||
             returnType.GetGenericTypeDefinition() == typeof(ValueTask<>)))
        {
            returnType = returnType.GetGenericArguments()[0];
        }

        if (returnType.IsGenericType && returnType.Name.StartsWith("Results`"))
        {
            var args = returnType.GetGenericArguments();
            foreach (var arg in args)
            {
                ProcessResultType(arg, operation, context);
            }

            foreach (var arg in args)
            {
                if (arg.IsGenericType && arg.GetGenericTypeDefinition().Name == "Ok`1")
                {
                    var payloadType = arg.GetGenericArguments()[0];
                    AddResponse(operation, context, "200", payloadType);
                }
            }
        }
        else
        {
            ProcessResultType(returnType, operation, context);
        }
    }

    private void ProcessResultType(Type resultType, OpenApiOperation operation, OperationFilterContext context)
    {
        var nonGen = NonGenericResultMap.FirstOrDefault(m => m.Type == resultType);
        if (nonGen.Type != null)
        {
            if (!operation.Responses.ContainsKey(nonGen.StatusCode))
                operation.Responses[nonGen.StatusCode] = new OpenApiResponse { Description = GetDescription(nonGen.StatusCode) };
            return;
        }

        if (resultType.IsGenericType)
        {
            var genDef = resultType.GetGenericTypeDefinition();
            var gdName = genDef.Name;
            var map = GenericResultMap.FirstOrDefault(m => m.GenericTypeName == gdName);
            if (!string.IsNullOrEmpty(map.GenericTypeName))
            {
                var status = map.StatusCode;
                if (!operation.Responses.ContainsKey(status))
                {
                    var payloadType = resultType.GetGenericArguments()[0];
                    AddResponse(operation, context, status, payloadType);
                }
                return;
            }
        }

        if (!IsHttpResultType(resultType))
        {
            const string status200 = "200";
            if (!operation.Responses.ContainsKey(status200))
                AddResponse(operation, context, status200, resultType);
        }
    }

    private static void AddResponse(OpenApiOperation operation, OperationFilterContext context, string statusCode, Type payloadType)
    {
        var response = new OpenApiResponse { Description = GetDescription(statusCode) };
        var schema = context.SchemaGenerator.GenerateSchema(payloadType, context.SchemaRepository);
        response.Content = new Dictionary<string, OpenApiMediaType>
        {
            ["application/json"] = new OpenApiMediaType { Schema = schema }
        };
        operation.Responses[statusCode] = response;
    }

    private static bool IsHttpResultType(Type t)
    {
        if (t == null) return false;
        var ns = t.Namespace ?? string.Empty;
        return ns.StartsWith("Microsoft.AspNetCore.Http", StringComparison.OrdinalIgnoreCase)
               || ns.StartsWith("Microsoft.AspNetCore.Mvc", StringComparison.OrdinalIgnoreCase);
    }

    private static string GetDescription(string statusCode)
    {
        return statusCode switch
        {
            "200" => "OK",
            "201" => "Created",
            "202" => "Accepted",
            "204" => "No Content",
            "400" => "Bad Request",
            "404" => "Not Found",
            _ => "Response"
        };
    }
}
