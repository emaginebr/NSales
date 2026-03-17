using System;
using System.Threading;
using System.Threading.Tasks;
using Lofn.BackgroundService;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using NCrontab;

namespace NoChainSwapBackgroundService
{
    public class Service : BackgroundService
    {
        /*
        private IConfiguration _configuration;

        private CrontabSchedule _schedule;
        private DateTime _nextRun;
        private readonly ScheduleTask _gwScheduleTask;

        public Service(ScheduleTask gwScheduleTask, IConfiguration configuration)
        {
            _configuration = configuration;
            //_schedule = CrontabSchedule.Parse(_configuration["Schedule:Cron"], new CrontabSchedule.ParseOptions { IncludingSeconds = true });
            _schedule = CrontabSchedule.Parse("* 0/5 * * * *", new CrontabSchedule.ParseOptions { IncludingSeconds = true });
            _nextRun = _schedule.GetNextOccurrence(DateTime.UtcNow);
            _gwScheduleTask = gwScheduleTask;
        }
        */
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            /*
            do
            {
                var now = DateTime.UtcNow;
                var nextrun = _schedule.GetNextOccurrence(now);

                if (now > _nextRun)
                {
                    //await _gwScheduleTask.DetectNewTransactions();
                    //await _gwScheduleTask.ProccessAllTransactions();
                    _nextRun = _schedule.GetNextOccurrence(DateTime.UtcNow);
                }
                
                await Task.Delay(60000, stoppingToken);
            }
            while (!stoppingToken.IsCancellationRequested); ;
            */
        }
    }
}
