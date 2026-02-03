using Serilog;
using Serilog.Context;
using System.Diagnostics;

namespace VendorMdm.Core.Framework.Logging;

/// <summary>
/// Serilog implementation of IStructuredLogger.
/// </summary>
public class SerilogStructuredLogger : IStructuredLogger
{
    private readonly ILogger _logger;
    private readonly string _appName;

    public SerilogStructuredLogger(ILogger logger, string appName)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _appName = appName ?? throw new ArgumentNullException(nameof(appName));
    }

    public void LogInformation(string message, params (string key, object value)[] properties)
    {
        using (PushProperties(properties))
        {
            _logger.Information(message);
        }
    }

    public void LogWarning(string message, params (string key, object value)[] properties)
    {
        using (PushProperties(properties))
        {
            _logger.Warning(message);
        }
    }

    public void LogError(Exception ex, string message, params (string key, object value)[] properties)
    {
        using (PushProperties(properties))
        {
            _logger.Error(ex, message);
        }
    }

    public void LogDebug(string message, params (string key, object value)[] properties)
    {
        using (PushProperties(properties))
        {
            _logger.Debug(message);
        }
    }

    public void LogCritical(Exception ex, string message, params (string key, object value)[] properties)
    {
        using (PushProperties(properties))
        {
            _logger.Fatal(ex, message);
        }
    }

    public IDisposable BeginScope(params (string key, object value)[] properties)
    {
        return PushProperties(properties);
    }

    public IDisposable BeginOperation(string operationName, params (string key, object value)[] properties)
    {
        return new OperationScope(_logger, operationName, properties);
    }

    private IDisposable PushProperties((string key, object value)[] properties)
    {
        var disposables = new List<IDisposable>
        {
            LogContext.PushProperty("AppName", _appName)
        };

        foreach (var (key, value) in properties)
        {
            disposables.Add(LogContext.PushProperty(key, value));
        }

        return new CompositeDisposable(disposables);
    }

    private class OperationScope : IDisposable
    {
        private readonly ILogger _logger;
        private readonly string _operationName;
        private readonly Stopwatch _stopwatch;
        private readonly IDisposable _scope;

        public OperationScope(ILogger logger, string operationName, (string key, object value)[] properties)
        {
            _logger = logger;
            _operationName = operationName;
            _stopwatch = Stopwatch.StartNew();

            var disposables = new List<IDisposable>
            {
                LogContext.PushProperty("OperationName", operationName)
            };

            foreach (var (key, value) in properties)
            {
                disposables.Add(LogContext.PushProperty(key, value));
            }

            _scope = new CompositeDisposable(disposables);

            _logger.Information($"Operation started: {operationName}");
        }

        public void Dispose()
        {
            _stopwatch.Stop();
            _logger.Information(
                $"Operation completed: {_operationName} (Duration: {_stopwatch.ElapsedMilliseconds}ms)");
            _scope.Dispose();
        }
    }

    private class CompositeDisposable : IDisposable
    {
        private readonly List<IDisposable> _disposables;

        public CompositeDisposable(List<IDisposable> disposables)
        {
            _disposables = disposables;
        }

        public void Dispose()
        {
            foreach (var disposable in _disposables)
            {
                disposable.Dispose();
            }
        }
    }
}
