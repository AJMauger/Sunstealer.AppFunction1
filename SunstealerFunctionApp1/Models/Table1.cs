using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using System.ComponentModel.DataAnnotations;

namespace Sunstealer.FunctionApp1.Models;

/// <summary>
/// 
/// </summary>
public class Table1
{
    /// <summary>
    /// 
    /// </summary>
    [Key]
    public int UUID { get; set; }
    /// <summary>
    /// 
    /// </summary>
    [OpenApiProperty(Description = "<i>description</i>: Encrypted1 description")]
    [MaxLength(64)]
    [MinLength(1)]
    [Required]
    [OpenApiExtensionBool(key: "Unique", value: true) ]
    [OpenApiExtensionInt(key: "MyInt", value: 667)]
    [OpenApiExtensionString(key: "MyString", value: "Revelation Space")]
    public string Encrypted1 { get; set; } = string.Empty;
    /// <summary>
    /// 
    /// </summary>
    public DateTime? Date1 { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public int Number1 { get; set; }
    /// <summary>
    /// 
    /// </summary>

    [OpenApiProperty(Description = "<i>description</i>: Text1 description")]
    [MaxLength(64)]
    [MinLength(1)]
    [Required]
    [OpenApiExtensionBool(key: "Unique", value: false)]
    [OpenApiExtensionInt(key: "MyInt", value: 667)]
    [OpenApiExtensionString(key: "MyString", value: "Sunstealer")]
    public string Text1 { get; set; } = string.Empty;
}
