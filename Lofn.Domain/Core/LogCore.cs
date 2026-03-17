using System;
using Lofn.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace Lofn.Domain.Core
{
    public class LogCore : ILogCore
    {
        private readonly ILogger _logger;

        public LogCore(ILogger<LogCore> logger)
        {
            _logger = logger;
        }

        public void Log(string message, Levels level)
        {
            //if (System.Diagnostics.Debugger.IsAttached)
            //{
                switch (level)
                {
                    case Levels.Trace:
                        _logger.LogTrace(message);
                        break;
                    case Levels.Debug:
                        _logger.LogDebug(message);
                        break;
                    case Levels.Information:
                        _logger.LogInformation(message);
                        break;
                    case Levels.Warning:
                        _logger.LogWarning(message);
                        break;
                    case Levels.Error:
                        _logger.LogError(message);
                        break;
                    case Levels.Critical:
                        _logger.LogCritical(message);
                        break;
                    default:
                        _logger.LogTrace(message);
                        break;
                }
            //}
        }
    }
}
