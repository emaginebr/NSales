using System;
using Lofn.Infra.Interfaces;
using Lofn.Domain.Core;
using Lofn.Domain.Interfaces;
using Microsoft.EntityFrameworkCore.Storage;

namespace Lofn.Infra
{
    public class TransactionDisposable : ITransaction
    {
        private readonly ILogCore _log;
        private readonly IDbContextTransaction _transaction;

        public TransactionDisposable(ILogCore log, IDbContextTransaction transaction)
        {
            _log = log;
            _transaction = transaction;
        }

        public void Commit()
        {
            _log.Log("Finalizando bloco de transação.", Levels.Trace);
            _transaction.Commit();
        }

        public void Dispose()
        {
            _log.Log("Liberando transação da memória.", Levels.Trace);
            _transaction.Dispose();
        }

        public void Rollback()
        {
            _log.Log("Rollback do bloco de transação.", Levels.Trace);
            _transaction.Rollback();

        }
    }
}
