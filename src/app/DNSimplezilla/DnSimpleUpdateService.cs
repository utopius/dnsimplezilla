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
            _timer.Elapsed += OnTimerElapsed;
        }

        private async void OnTimerElapsed(object sender, ElapsedEventArgs elapsedEventArgs)
        {
            try
            {
                await UpdateDnsRecordsAsync().ConfigureAwait(false);
            }
            catch (Exception e)
            {
                _eventLog.Error("Failed to update DNS records", e);
            }
        }

        public void Start()
        {
            var configuration = _configProvider.Load();
            _timer.Interval = TimeSpan.FromMinutes(configuration.UpdateInterval)
                                      .TotalMilliseconds;

            UpdateDnsRecordsAsync().Wait();
            _timer.Start();
        }

        private async Task UpdateDnsRecordsAsync()
        {
            try
            {
                _timer.Stop();
                var configuration = _configProvider.Load();
                var recordUpdater = new DomainHostRecordUpdater(new ICanHazIpClient(),
                    new DnSimple(new DNSimpleRestClient(configuration.Username, token: configuration.ApiToken)), _eventLog);

                await recordUpdater.UpdateAsync(configuration.Domains);
            }
            finally
            {
                _timer.Start();
            }
        }

        public void Stop()
        {
            _timer.Elapsed -= OnTimerElapsed;
            _timer.Dispose();
        }
    }
}
