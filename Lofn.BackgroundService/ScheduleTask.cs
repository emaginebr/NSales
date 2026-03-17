using System;
using System.Linq;
using System.Threading.Tasks;
using Lofn.Domain.Interfaces;

namespace Lofn.BackgroundService
{
    public class ScheduleTask
    {
        //private ITransactionService _transactionService;

        /*
        public ScheduleTask(ITransactionService transactionService)
        {
            _transactionService = transactionService;
        }

        public async Task<bool> DetectNewTransactions()
        {
            bool ret = false;
            try
            {
                Console.WriteLine(DateTime.Now.ToString("yyyy-MM-dd h:mm:ss") + " - Start detecting new transactions");
                ret = await _transactionService.DetectAllTransaction();
                Console.WriteLine(DateTime.Now.ToString("yyyy-MM-dd h:mm:ss") + " - Detect transaction terminated");
            }
            catch (Exception e)
            {
                Console.WriteLine(DateTime.Now.ToString("yyyy-MM-dd h:mm:ss") + " - Error on detecting transactions:\n" + e.Message);
            }
            return ret;
        }

        public async Task<bool> ProccessAllTransactions()
        {
            bool ret = false;
            try
            {
                Console.WriteLine(DateTime.Now.ToString("yyyy-MM-dd h:mm:ss") + " - Start processing all transactions");
                ret = await _transactionService.ProcessAllTransaction();
                Console.WriteLine(DateTime.Now.ToString("yyyy-MM-dd h:mm:ss") + " - All transaction proccess terminated");
            } catch(Exception e)
            {
                Console.WriteLine(DateTime.Now.ToString("yyyy-MM-dd h:mm:ss") + " - Error on proccessing transactions:\n" + e.Message);
            }
            return ret;
        }
        */
    }
}
