using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Domain
{
    public interface IUnitOfWork
    {
        ITransaction BeginTransaction();
    }
    public interface ITransaction : IDisposable
    {
        void Commit();
        void Rollback();
    }
}
