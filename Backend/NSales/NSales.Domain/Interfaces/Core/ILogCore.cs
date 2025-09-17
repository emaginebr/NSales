using System;
using NSales.Domain.Impl.Core;
using Microsoft.Extensions.Logging;

namespace NSales.Domain.Interfaces.Core
{
    public interface ILogCore
    {
        void Log(string message, Levels level);
    }
}
