using Microsoft.Extensions.Logging;

namespace VendorMdm.Infrastructure.Utilities
{
    /// <summary>
    /// Centralized utility for marking simulation/mock mode in logs.
    /// Complies with Golden Rules: Simulation-First pattern.
    /// </summary>
    public static class SimulationLogger
    {
        private const string SimulationPrefix = "[SIMULATION MODE]";

        public static void LogSimulation(ILogger logger, string message, params object[] args)
        {
            logger.LogInformation($"{SimulationPrefix} {message}", args);
        }

        public static void LogSimulationWarning(ILogger logger, string message, params object[] args)
        {
            logger.LogWarning($"{SimulationPrefix} {message}", args);
        }

        public static string AddPrefix(string message)
        {
            return $"{SimulationPrefix} {message}";
        }
    }
}
