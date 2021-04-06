using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CommandLine;
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
            var spec1 = new PluginSpecification
            {
                Id = "test-plugin",
                Version = Version.Parse("1.2.3"),
            };
            var spec2 = new PluginSpecification
            {
                Id = "test-plugin",
                Version = Version.Parse("1.2.4"),
            };
            var spec3 = new PluginSpecification
            {
                Id = "test-plugin",
                Version = Version.Parse("1.2.5"),
            };
            var spec4 = new PluginSpecification
            {
                Id = "test-plugin",
                Version = Version.Parse("1.2.6"),
            };
            var spec5 = new PluginSpecification
            {
                Id = "other-plugin",
                Version = Version.Parse("1.0.0"),
            };

            var solver = new DependencySolver();
            solver.AddExclusiveConstraint(spec1, spec2, spec3, spec4);
            solver.AddRequirementConstraint(spec1, spec5);
            solver.AddSingleConstraint(spec1);
            solver.FindSolutions();
            Console.WriteLine("SOLUTIONS:");
            foreach (var solution in solver.Solutions)
                Console.WriteLine(string.Join(", ", solution.Select(x => x ? 'T' : 'F')));
        }
    }
}
