using Topshelf;

namespace DNSimple.UpdateService
{
    class Program
    {
        static int Main(string[] args)
        {
            log4net.Config.XmlConfigurator.Configure();

            var exitCode = HostFactory.Run(x =>
                {
                    x.Service<DnSimpleUpdateService>(s =>
                    {
                        s.ConstructUsing(hostSettings =>
                        {
                            var configProvider = ConfigurationProvider.Load();
                            return new DnSimpleUpdateService(configProvider.Configuration);
                        });
                        s.WhenStarted((service, host) =>
                            {
                                EventLog.Info("DNSimpleUpdateService starting...");

                                service.Start();

                                EventLog.Info("DNSimpleUpdateService started.");
                                return true;
                            });
                        s.WhenStopped((service, host) =>
                            {
                                EventLog.Info("DNSimpleUpdateService stopping...");

                                service.Stop();

                                EventLog.Info("DNSimpleUpdateService stopped.");
                                return true;
                            });
                    });
                    x.SetServiceName("DNSimpleUpdateService");
                    x.SetDisplayName("DNSimple.UpdateService");
                    x.SetDescription("Updates DNSimple Records");
                    x.DependsOnEventLog();
                    x.RunAsLocalSystem();
                });

            return (int)exitCode;
        }
    }
}
