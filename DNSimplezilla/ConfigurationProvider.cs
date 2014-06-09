using System;
using System.Configuration;
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
        private const string ConfigFilename = "config.xml";
        private static FileInfo _defaultConfigFile;
        private static FileInfo _configFile;

        public Configuration Configuration { get; private set; }

        private ConfigurationProvider(Configuration configuration)
        {
            Configuration = configuration;
        }

        public static ConfigurationProvider Load()
        {
            var fileVersionInfo = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location);

            string programDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), fileVersionInfo.ProductName);
            if (!Directory.Exists(programDataPath))
            {
                Directory.CreateDirectory(programDataPath);
            }

            var executableDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            _defaultConfigFile = new FileInfo(Path.Combine(executableDirectory, ConfigFilename));
            _configFile = new FileInfo(Path.Combine(programDataPath, ConfigFilename));

            if (!_configFile.Exists)
            {
                var message =
                    string.Format(
                        "No configuration file found, the default configuration file was created: {0}. You have to add sane values to the file before starting the service.",
                        _configFile.FullName);
                EventLog.Warn(message);
                _defaultConfigFile.CopyTo(_configFile.FullName);
                throw new ServiceConfigurationException(message);
            }
            
            if (File.ReadAllLines(_defaultConfigFile.FullName)
                .SequenceEqual(File.ReadAllLines(_configFile.FullName)))
            {
                var message =
                    string.Format(
                        "The configuration file '{0}' contains default values. You have to add sane values to the file before starting the service.",
                        _configFile.FullName);
                EventLog.Warn(message);
                throw new ServiceConfigurationException(message);
            }
            var configuration = LoadConfiguration();

            return new ConfigurationProvider(configuration);
        }

        private static Configuration LoadConfiguration()
        {
            EventLog.Info(string.Format("Loading configuration from {0}...", _configFile.FullName));

            try
            {
                using (var configurationFileStream = _configFile.OpenRead())
                {
                    var serializer = XmlSerializer.FromTypes(new[] { typeof(Configuration) }).
                                                   First();

                    var configuration = (Configuration)serializer.Deserialize(configurationFileStream);

                    EventLog.Info(string.Format("Configuration loaded from {0}.", _configFile.FullName));
                    return configuration;
                }
            }
            catch (Exception e)
            {
                EventLog.Error("Configuration data corrupted and cannot be restored.", e);
                throw;
            }
        }

        public void Save()
        {
            Save(Configuration);
        }

        private static void Save(Configuration configuration)
        {
            EventLog.Info(string.Format("Saving Configuration to '{0}'...", _configFile.FullName));
            try
            {
                using (var fileStream = _configFile.OpenWrite())
                {
                    var serializer = new XmlSerializer(typeof(Configuration));
                    serializer.Serialize(fileStream, configuration);
                    EventLog.Info(string.Format("Configuration saved to {0}.", _configFile.FullName));
                }
            }
            catch (Exception e)
            {
                EventLog.Error("Error saving Configuration! File may be corrupted!", e);
            }
        }
    }

    public class ServiceConfigurationException : Exception
    {
        public ServiceConfigurationException(string message)
            : base(message)
        {
        }
    }
}