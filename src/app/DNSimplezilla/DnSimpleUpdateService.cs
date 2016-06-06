using System;
using System.Threading.Tasks;
using System.Timers;
using DNSimple;

namespace DNSimplezilla
{
    public class DnSimpleUpdateService
    {
        private readonly ConfigurationProvider _configProvider;
        private readonly IEventLog _eventLog;
        private readonly Timer _timer;

        public DnSimpleUpdateService(ConfigurationProvider configProvider, IEventLog eventLog)
        {
            if (configProvider == null) throw new ArgumentNullException("configProvider");
            if (eventLog == null) throw new ArgumentNullException("eventLog");
            _configProvider = configProvider;
            _eventLog = eventLog;

            _timer = new Timer();
            _timer.Elapsed += TimerOnElapsed;
        }

        private async void TimerOnElapsed(object sender, ElapsedEventArgs elapsedEventArgs)
        {
            await UpdateDnsRecordsAsync();
        }

        public void Start()
        {
            var configuration = _configProvider.Load();
            _timer.Interval = TimeSpan.FromMinutes(configuration.UpdateInterval)
                                      .TotalMilliseconds;

            UpdateDnsRecordsAsync().Wait();
            _timer.Start();
        }

        private async void OnTimerTick(object state)
        {
        }

        private async Task UpdateDnsRecordsAsync()
        {
            try
            {
                _timer.Stop();
                var jsonIpRestClient = new MyExternalIpClient();
                var configuration = _configProvider.Load();
                var dnSimpleRestClient = new DNSimpleRestClient(configuration.Username, token: configuration.ApiToken);
                var recordUpdater = new DomainHostRecordUpdater(jsonIpRestClient, dnSimpleRestClient, configuration.Domains, _eventLog);

                await recordUpdater.UpdateAsync();
            }
            catch (Exception e)
            {
                _eventLog.Error("Failed to update DNS records", e);
            }
            finally
            {
                _timer.Start();
            }
        }

        private void LogError(Task task)
        {
            if (task == null) throw new ArgumentNullException("task");
            var ex = task.Exception != null ? task.Exception.Flatten() : null;
            _eventLog.Error(string.Format("An Exception occurred:\n {0}", ex), ex);
        }

        public void Stop()
        {
            _timer.Dispose();
        }
    }
}
