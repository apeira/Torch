using System;
using System.IO;

namespace Torch
{
    /// <summary>
    /// Executes Torch in a new AppDomain with the correct paths to resolve assemblies.
    /// </summary>
    public static class Launcher
    {
        private const string APP_DOMAIN = "TorchAppDomain";

        /// <summary>
        /// Gets a value indicating whether Torch is currently executing within the launcher AppDomain.
        /// </summary>
        public static bool IsLaunched => AppDomain.CurrentDomain.FriendlyName == APP_DOMAIN;

        /// <summary>
        /// Expects a file named "domain" next to Torch.exe where the first line is the base path
        /// relative to Torch.exe and the second line is a semicolon-delimited list of private bin paths.
        /// </summary>
        /// <param name="args">The arguments to pass to the launched Torch assembly.</param>
        public static void Launch(string[]? args)
        {
            var assemblyDir = new FileInfo(typeof(Launcher).Assembly.Location).DirectoryName!;

            var options = File.ReadAllText(Path.Combine(assemblyDir, "domain"))
                .Split(new[] { "\n", Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

            var basePath = Path.Combine(assemblyDir, options[0]);
            var setup = new AppDomainSetup();
            setup.ApplicationBase = basePath;
            if (options.Length > 1)
                setup.PrivateBinPath = options[1];
            var domain = AppDomain.CreateDomain(APP_DOMAIN, null, setup);
            Directory.SetCurrentDirectory(basePath);
            domain.ExecuteAssemblyByName(typeof(Launcher).Assembly.FullName, args);
        }
    }
}
