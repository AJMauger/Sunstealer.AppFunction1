namespace Sunstealer.FunctionApp1.Models;

/// <summary>
/// 
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class OpenApiExtensionBoolAttribute : Attribute
{
    /// <summary>
    /// 
    /// </summary>
    public string Key { get; }

    /// <summary>
    /// 
    /// </summary>
    public bool Value { get; }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    public OpenApiExtensionBoolAttribute(string key, bool value)
    {
        Key = key;
        Value = value;
    }
}

/// <summary>
/// 
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class OpenApiExtensionIntAttribute : Attribute
{
    /// <summary>
    /// 
    /// </summary>
    public string Key { get; }

    /// <summary>
    /// 
    /// </summary>
    public int Value { get; }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    public OpenApiExtensionIntAttribute(string key, int value)
    {
        Key = key;
        Value = value;
    }
}

/// <summary>
/// 
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class OpenApiExtensionStringAttribute : Attribute
{
    /// <summary>
    /// 
    /// </summary>
    public string Key { get; }
    
    /// <summary>
    /// 
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    public OpenApiExtensionStringAttribute(string key, string value)
    {
        Key = key;
        Value = value;
    }
}