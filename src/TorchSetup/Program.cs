using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CommandLine;
using SemVer;
using TorchSetup.InstallStrategies;
using TorchSetup.Options;
using TorchSetup.Plugins;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using Version = SemVer.Version;

namespace TorchSetup
{
    internal static class Program
    {
        private static readonly Dictionary<BaseGameType, IInstallStrategy> _installStrategies = new()
        {
            [BaseGameType.SpaceEngineersDedicated] = new SeDedicatedInstaller(),
        };

        public static void Main(string[] args)
        {
            TestDeps();
            return;
            
            var deser = new DeserializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();

            var spec = deser.Deserialize<PluginSpecification>(File.ReadAllText("exampleSpec.yaml"));

            Console.WriteLine(spec.Id);
            return;
            Parser.Default.ParseArguments<InstallOptions, object>(args)
                .WithParsed<InstallOptions>(Install);
        }

        private static void Install(InstallOptions opt)
        {
            if (_installStrategies.TryGetValue(opt.BaseGame, out var strategy))
                strategy.Install(opt);
        }

        private static void TestDeps()
        {
            var man = new PluginManager();
            for (var i = 0; i < 10; i++)
            {
                man.AddSpec(new PluginSpecification{Id = "test-plugin", Version = new Version(1, i, 0)});
                man.AddSpec(new PluginSpecification{Id = "other-plugin", Version = new Version(i, 0, 0)});
            }

            man.ExplicitlyInstalled["test-plugin"] = new Range("*");
            man.ExplicitlyInstalled["other-plugin"] = new Range("*");

            man.SolveDependencies();
        }
    }
}
