namespace VendorMdm.Core.Framework.Logging;

/// <summary>
/// Core structured logging service.
/// Provides consistent logging across all MDM applications with contextual properties.
/// </summary>
public interface IStructuredLogger
{
    /// <summary>
    /// Logs an informational message with contextual properties.
    /// </summary>
    void LogInformation(string message, params (string key, object value)[] properties);

    /// <summary>
    /// Logs a warning message with contextual properties.
    /// </summary>
    void LogWarning(string message, params (string key, object value)[] properties);

    /// <summary>
    /// Logs an error with exception and contextual properties.
    /// </summary>
    void LogError(Exception ex, string message, params (string key, object value)[] properties);

    /// <summary>
    /// Logs a debug message with contextual properties.
    /// </summary>
    void LogDebug(string message, params (string key, object value)[] properties);

    /// <summary>
    /// Logs a critical error with exception and contextual properties.
    /// </summary>
    void LogCritical(Exception ex, string message, params (string key, object value)[] properties);

    /// <summary>
    /// Begins a logging scope with contextual properties.
    /// All logs within the scope will include these properties.
    /// </summary>
    IDisposable BeginScope(params (string key, object value)[] properties);

    /// <summary>
    /// Begins a logging scope for an operation.
    /// Automatically logs operation start/end with duration.
    /// </summary>
    IDisposable BeginOperation(string operationName, params (string key, object value)[] properties);
}
