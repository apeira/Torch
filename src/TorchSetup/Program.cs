using System;
using System.Collections.Generic;
using System.IO;
using CommandLine;
using TorchSetup.InstallStrategies;
using TorchSetup.Options;
using TorchSetup.Plugins;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

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
    }
}
