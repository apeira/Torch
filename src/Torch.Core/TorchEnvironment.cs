using System.Diagnostics;
using System.IO;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using NLog;
using NLog.Config;
using NLog.Fluent;
using NLog.Layouts;
using NLog.Targets;
using Torch.Core.Commands;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Torch.Core
{
    public sealed class TorchEnvironment
    {
        public string BinPath { get; }

        public string UserDataPath { get; set; }

        public string[] GameArgs { get; }

        public ServiceCollection Services { get; }

        public LoggingConfiguration Logging { get; }
        
        public TorchConfiguration Configuration { get; }

        public TorchEnvironment(string binPath, string userDataPath, string[] gameArgs)
        {
            BinPath = binPath;
            UserDataPath = userDataPath;
            GameArgs = gameArgs;
            Services = new ServiceCollection();
            
            Directory.CreateDirectory(userDataPath);
            Directory.CreateDirectory(Path.Combine(userDataPath, "config"));

            Configuration = LoadConfig(Path.Combine(userDataPath, "config", "torch.yaml"));
            
            var nlogConfigPath = Path.Combine(userDataPath, "config", "NLog.config");
            if (File.Exists(nlogConfigPath))
            {
                Logging = new XmlLoggingConfiguration(nlogConfigPath);   
            }
            else
            {
                Logging = new LoggingConfiguration();
                AddDefaultLogging();
                if (Configuration.LogToJson)
                    AddJsonLogging();
            }
        }

        private void AddJsonLogging()
        {
            var jsonTarget = new FileTarget("json")
            {
                Encoding = Encoding.UTF8,
                Layout = new JsonLayout
                {
                    Attributes =
                    {
                        new JsonAttribute("time", "${longdate}"),
                        new JsonAttribute("level", "${level}"),
                        new JsonAttribute("logger", "${logger}"),
                        new JsonAttribute("message", "${message:withException=true")
                    }
                },
                FileName = Path.Combine(UserDataPath, "logs", "torch.json"),
                ArchiveOldFileOnStartup = true,
                MaxArchiveFiles = 5,
            };
            Logging.AddTarget(jsonTarget);
            Logging.AddRule(LogLevel.Debug, LogLevel.Fatal, jsonTarget);
        }
        
        /// <summary>
        /// Creates a default logging configuration that will log >=Info to console and >=Debug to torch.log.
        /// </summary>
        /// <returns></returns>
        private void AddDefaultLogging()
        {
            const string logLayout = " ${logger:shortname=true}: ${message:withException=true}";
            var consoleTarget = new ColoredConsoleTarget("console")
            {
                Encoding = Encoding.UTF8,
                Layout = "${date:format=HH\\:mm\\:ss.ff} [${level:uppercase=true:truncate=1}]" + logLayout,
            };
            var fileTarget = new FileTarget("file")
            {
                Encoding = Encoding.UTF8,
                Layout = "${longdate} | ${pad:padding=-5:inner=${level:uppercase=true}} |" + logLayout,
                FileName = Path.Combine(UserDataPath, "logs", "torch.log"),
                ArchiveOldFileOnStartup = true,
                MaxArchiveFiles = 5,
            };

            Logging.AddTarget(consoleTarget);
            Logging.AddTarget(fileTarget);
            Logging.AddRule(LogLevel.Info, LogLevel.Fatal, consoleTarget);
            Logging.AddRule(LogLevel.Debug, LogLevel.Fatal, fileTarget);
        }
        
        private static TorchConfiguration LoadConfig(string configFilePath)
        {
            CreateConfig(configFilePath);
            
            if (!File.Exists(configFilePath))
                throw new FileNotFoundException($"The Torch configuration file cannot be found at '{configFilePath}'.");

            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();
            using var f = File.OpenRead(configFilePath);
            using var reader = new StreamReader(f);
            return deserializer.Deserialize<TorchConfiguration>(reader);
        }

        [Conditional("DEBUG")]
        private static void CreateConfig(string configFilePath)
        {
            if (File.Exists(configFilePath))
                return;

            var serializer = new SerializerBuilder().WithNamingConvention(CamelCaseNamingConvention.Instance).Build();
            using var f = File.Create(configFilePath);
            using var writer = new StreamWriter(f);
            serializer.Serialize(writer, new TorchConfiguration());
        }
    }
}
