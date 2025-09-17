using System;
using Core.Domain;
using NSales.Domain.Impl.Core;
using NSales.Domain.Interfaces.Core;
using Microsoft.EntityFrameworkCore.Storage;

namespace DB.Infra
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
