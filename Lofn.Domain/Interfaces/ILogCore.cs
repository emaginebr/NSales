using System;
using Lofn.Domain.Core;
using Microsoft.Extensions.Logging;

namespace Lofn.Domain.Interfaces
{
    public interface ILogCore
    {
        void Log(string message, Levels level);
    }
}
