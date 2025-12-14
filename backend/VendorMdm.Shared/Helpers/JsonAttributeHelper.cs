using System.Text.Json;

namespace VendorMdm.Shared.Helpers;

/// <summary>
/// Helper class for working with JSON attributes in SQL entities.
/// Provides type-safe access to semi-structured data stored in nvarchar(max) columns.
/// </summary>
public static class JsonAttributeHelper
{
    /// <summary>
    /// Gets a strongly-typed value from JSON attributes.
    /// </summary>
    /// <typeparam name="T">Type to deserialize to</typeparam>
    /// <param name="json">JSON string from Attributes column</param>
    /// <param name="key">Key to retrieve</param>
    /// <returns>Deserialized value or default</returns>
    public static T? GetAttribute<T>(string? json, string key)
    {
        if (string.IsNullOrWhiteSpace(json) || json == "{}")
            return default;

        try
        {
            using var doc = JsonDocument.Parse(json);
            if (doc.RootElement.TryGetProperty(key, out var element))
            {
                return JsonSerializer.Deserialize<T>(element.GetRawText());
            }
        }
        catch (JsonException)
        {
            // Log error in production
            return default;
        }
        return default;
    }

    /// <summary>
    /// Sets a value in JSON attributes.
    /// </summary>
    /// <typeparam name="T">Type of value to set</typeparam>
    /// <param name="json">Current JSON string</param>
    /// <param name="key">Key to set</param>
    /// <param name="value">Value to set</param>
    /// <returns>Updated JSON string</returns>
    public static string SetAttribute<T>(string? json, string key, T value)
    {
        var dict = new Dictionary<string, object>();
        
        if (!string.IsNullOrWhiteSpace(json) && json != "{}")
        {
            try
            {
                dict = JsonSerializer.Deserialize<Dictionary<string, object>>(json) 
                    ?? new Dictionary<string, object>();
            }
            catch (JsonException)
            {
                // Start fresh if JSON is invalid
                dict = new Dictionary<string, object>();
            }
        }

        dict[key] = value!;
        return JsonSerializer.Serialize(dict);
    }

    /// <summary>
    /// Removes a key from JSON attributes.
    /// </summary>
    /// <param name="json">Current JSON string</param>
    /// <param name="key">Key to remove</param>
    /// <returns>Updated JSON string</returns>
    public static string RemoveAttribute(string? json, string key)
    {
        if (string.IsNullOrWhiteSpace(json) || json == "{}")
            return "{}";

        try
        {
            var dict = JsonSerializer.Deserialize<Dictionary<string, object>>(json);
            dict?.Remove(key);
            return JsonSerializer.Serialize(dict ?? new Dictionary<string, object>());
        }
        catch (JsonException)
        {
            return "{}";
        }
    }

    /// <summary>
    /// Deserializes entire attributes JSON to a strongly-typed object.
    /// </summary>
    /// <typeparam name="T">Type to deserialize to</typeparam>
    /// <param name="json">JSON string from Attributes column</param>
    /// <returns>Deserialized object or default</returns>
    public static T? DeserializeAttributes<T>(string? json)
    {
        if (string.IsNullOrWhiteSpace(json) || json == "{}")
            return default;

        try
        {
            return JsonSerializer.Deserialize<T>(json);
        }
        catch (JsonException)
        {
            return default;
        }
    }

    /// <summary>
    /// Serializes an entire object to JSON attributes string.
    /// </summary>
    /// <typeparam name="T">Type to serialize</typeparam>
    /// <param name="attributes">Object to serialize</param>
    /// <returns>JSON string</returns>
    public static string SerializeAttributes<T>(T attributes)
    {
        if (attributes == null)
            return "{}";

        try
        {
            return JsonSerializer.Serialize(attributes);
        }
        catch (JsonException)
        {
            return "{}";
        }
    }
}
