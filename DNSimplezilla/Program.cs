using Topshelf;

namespace DNSimplezilla
{
    class Program
    {
        static int Main(string[] args)
        {
            log4net.Config.XmlConfigurator.Configure();
            var eventLog = new EventLog();

            var exitCode = HostFactory.Run(x =>
                {
                    x.Service<DnSimpleUpdateService>(s =>
                    {
                        s.ConstructUsing(hostSettings =>
                        {
                            eventLog.Info("DNSimplezilla preparing for start...");

                            var configProvider = ConfigurationProvider.Create(eventLog);
                            return new DnSimpleUpdateService(configProvider,eventLog);
                        });
                        s.WhenStarted((service, host) =>
                            {
                                eventLog.Info("DNSimplezilla starting...");

                                service.Start();

                                eventLog.Info("DNSimplezilla started.");
                                return true;
                            });
                        s.WhenStopped((service, host) =>
                            {
                                eventLog.Info("DNSimplezilla stopping...");

                                service.Stop();

                                eventLog.Info("DNSimplezilla stopped.");
                                return true;
                            });
                    });
                    x.SetServiceName("DNSimplezilla");
                    x.SetDisplayName("DNSimplezilla");
                    x.SetDescription("Updates DNSimple Records");
                    x.DependsOnEventLog();
                    x.RunAsLocalSystem();
                });

            return (int)exitCode;
        }
    }
}
