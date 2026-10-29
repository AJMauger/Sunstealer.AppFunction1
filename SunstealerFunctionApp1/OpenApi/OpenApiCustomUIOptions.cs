using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Abstractions;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Configurations;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Interfaces;
using Microsoft.OpenApi.Models;
using Sunstealer.FunctionApp1.Models;
using System.Reflection;

namespace Sunstealer.FunctionApp1.OpenApi;

/// <summary>
/// 
/// </summary>
public class DocumentFilter : IDocumentFilter
{
    // ajm: ---------------------------------------------------------------------------------------
    /// <summary>
    /// 
    /// </summary>
    /// <param name="req"></param>
    /// <param name="document"></param>
    public void Apply(IHttpRequestDataObject req, OpenApiDocument document)
    {
        var schemas = document.Components.Schemas;
        foreach (var schema in schemas)
        {
            if (schema.Key == "table1")
            {
                schema.Value.Example = new OpenApiArray() {
                    new OpenApiObject()
                    {
                        ["UUID"] = new OpenApiInteger(1),
                        ["Encrypted1"] = new OpenApiString("DecryptedText"),
                        ["Date1"] = new OpenApiDateTime(DateTime.Now),
                        ["Number1"] = new OpenApiInteger(1),
                        ["Text1"] = new OpenApiString("ClearText")
                    }
                };
            }
        }

        var targetedTypes = Assembly.GetExecutingAssembly().GetTypes().Where(t => t.IsClass);
        foreach (var type in targetedTypes)
        {
            string schemaName = type.Name.ToLower();
            if (document.Components.Schemas.TryGetValue(schemaName, out var openApiSchema))
            {
                var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
                foreach (var property in properties)
                {
                    var attributeBool = property.GetCustomAttribute<OpenApiExtensionBoolAttribute>();
                    if (attributeBool != null)
                    {
                        string openApiPropName = property.Name.Substring(0, 1).ToLower() + property.Name.Substring(1);
                        if (openApiSchema.Properties.TryGetValue(openApiPropName, out var openApiProperty))
                        {
                            openApiProperty.Extensions[attributeBool.Key.ToLower()] = new OpenApiBoolean(attributeBool.Value);
                        }
                    }
                    var attributeInt = property.GetCustomAttribute<OpenApiExtensionIntAttribute>();
                    if (attributeInt != null)
                    {
                        string openApiPropName = property.Name.Substring(0, 1).ToLower() + property.Name.Substring(1);
                        if (openApiSchema.Properties.TryGetValue(openApiPropName, out var openApiProperty))
                        {
                            openApiProperty.Extensions[attributeInt.Key.ToLower()] = new OpenApiInteger(attributeInt.Value);
                        }
                    }
                    var attributeString = property.GetCustomAttribute<OpenApiExtensionStringAttribute>();
                    if (attributeString != null)
                    {
                        string openApiPropName = property.Name.Substring(0, 1).ToLower() + property.Name.Substring(1);
                        if (openApiSchema.Properties.TryGetValue(openApiPropName, out var openApiProperty))
                        {
                            openApiProperty.Extensions[attributeString.Key.ToLower()] = new OpenApiString(attributeString.Value);
                        }
                    }
                }
            }
        }
    }
}

// ajm: ---------------------------------------------------------------------------------------
/// <summary>
/// 
/// </summary>
public class OpenApiConfigurationOptions : DefaultOpenApiConfigurationOptions
{
    /// <summary>
    /// 
    /// </summary>
    public OpenApiConfigurationOptions()
    {
        DocumentFilters.Add(new DocumentFilter());
    }

    /// <summary>
    /// 
    /// </summary>
    public override OpenApiInfo Info { get; set; } = new OpenApiInfo()
    {
        Version = "1.0.0",
        Title = "Azure Function API",
        Description = @"Sunstealer Azure Function API"
    };

    /// <summary>
    /// 
    /// </summary>
    public override OpenApiVersionType OpenApiVersion { get; set; } = OpenApiVersionType.V3;
}