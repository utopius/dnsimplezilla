using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Serialization;

namespace DNSimple.UpdateService
{
    /// <summary>
    /// Provides access to accounts and the according messageUID-files.
    /// </summary>
    public class ConfigurationProvider
    {
        private static readonly string ConfigurationFilePath;

        static ConfigurationProvider()
        {
            var fileVersionInfo = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location);

            string programDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), fileVersionInfo.ProductName);
            string configurationFile = string.Format("{0}.xml", fileVersionInfo.ProductName);
            if (!Directory.Exists(programDataPath))
            {
                Directory.CreateDirectory(programDataPath);
            }
            ConfigurationFilePath = Path.Combine(programDataPath, configurationFile);
        }

        private ConfigurationProvider(Configuration configuration)
        {
            Configuration = configuration;
        }

        public static ConfigurationProvider Load()
        {
            var configuration = LoadConfiguration(ConfigurationFilePath);

            return new ConfigurationProvider(configuration);
        }

        private static Configuration LoadConfiguration(string path)
        {
            EventLog.Info(string.Format("Loading configuration from {0}...", ConfigurationFilePath));

            var configuration = new Configuration { Domain = "domain", DomainToken = "domainToken", UpdateIntervalInMinutes = 15, RecordId = 0 };

            if (!File.Exists(path))
            {
                EventLog.Warn(string.Format("No configuration file found. A blank configuration file was created: {0}", ConfigurationFilePath));
                Save(configuration);
                throw new Exception(string.Format("No configuration file found. A blank configuration file was created: {0}", ConfigurationFilePath));
            }

            try
            {
                using (var configurationFileStream = new FileStream(path, FileMode.Open))
                {
                    var serializer = XmlSerializer.FromTypes(new[] { typeof(Configuration) }).
                                                   First();

                    configuration = (Configuration)serializer.Deserialize(configurationFileStream) ??
                                    new Configuration();
                    EventLog.Info(string.Format("Configuration loaded from {0}.", ConfigurationFilePath));
                }
            }
            catch (Exception e)
            {
                EventLog.Error("Configuration data corrupted and cannot be restored.", e);
                throw;
            }
            return configuration;
        }

        public void Save()
        {
            Save(Configuration);
        }

        private static void Save(Configuration configuration)
        {
            EventLog.Info(string.Format("Saving Configuration to '{0}'...", ConfigurationFilePath));
            try
            {
                using (var fileStream = new FileStream(ConfigurationFilePath, FileMode.Create))
                {
                    var serializer = new XmlSerializer(typeof(Configuration));
                    serializer.Serialize(fileStream, configuration);
                    EventLog.Info(string.Format("Configuration saved to {0}.", ConfigurationFilePath));
                }
            }
            catch (Exception e)
            {
                EventLog.Error("Error saving Configuration! File may be corrupted!", e);
            }
        }

        public Configuration Configuration { get; private set; }
    }
}