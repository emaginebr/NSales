using System;
using Lofn.Infra.Interfaces;
using Lofn.Infra.Context;
using Lofn.Domain.Core;
using Lofn.Domain.Interfaces;

namespace Lofn.Infra
{
    public class UnitOfWork : IUnitOfWork
    {

        private readonly LofnContext _ccsContext;
        private readonly ILogCore _log;

        public UnitOfWork(ILogCore log, LofnContext ccsContext)
        {
            this._ccsContext = ccsContext;
            _log = log;
        }

        public ITransaction BeginTransaction()
        {
            try
            {
                _log.Log("Iniciando bloco de transação.", Levels.Trace);
                return new TransactionDisposable(_log, _ccsContext.Database.BeginTransaction());
            }
            catch(Exception e)
            {
                throw e;
            }
        }
    }
}
