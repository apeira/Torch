using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using CommandLine;
using HarmonyLib;
using Microsoft.Extensions.DependencyInjection;
using NLog;
using Torch.Core;
using Torch.Core.Plugins;

namespace Torch
{
    /// <summary>
    /// Contains the entry point for Torch.
    /// </summary>
    internal static class Program
    {
        private static readonly PluginService _pluginService = new();
        private static Logger? _log;

        /// <summary>
        /// Executable entry point.
        /// </summary>
        /// <param name="args">The arguments passed to the executable.</param>
        public static void Main(string[] args)
        {
            Options? opt = null;
            var parser = new Parser(settings => settings.AutoVersion = false);
            parser.ParseArguments<Options>(args).WithParsed(o => opt = o);

            if (opt == null)
                return;

            if (Launcher.IsLaunched || opt.NoLauncher)
                Run(opt);
            else
                Launcher.Launch(args);
        }

        private static void Run(Options options)
        {
            AppDomain.CurrentDomain.UnhandledException += LogUnhandledException;

            var assemblyDir = Directory.GetParent(typeof(Program).Assembly.Location).FullName;
            var environment = new TorchEnvironment(assemblyDir, options.UserData, null!);

            environment.Services.AddSingleton<IPluginService>(_pluginService);
            foreach (var loadPlugin in options.PluginDlls)
                _pluginService.LoadPluginDebug(Assembly.LoadFrom(loadPlugin), environment);
            _pluginService.LoadPlugins(environment.Configuration.Plugins, environment);

            LogManager.Configuration = environment.Logging;
            _log = LogManager.GetLogger("Torch");
            _log.Info("Logging initialized.");

            var sb = new StringBuilder();
            sb.AppendLine("PATCH SUMMARY:");
            foreach (var patcher in _pluginService.Patchers.Values)
            {
                sb.AppendLine($"  {patcher.Id}");
                foreach (var method in patcher.GetPatchedMethods())
                {
                    sb.AppendLine($"    {method.FullDescription()}");
                }
            }

            _log.Debug(sb);

            var provider = environment.Services.BuildServiceProvider();
            _pluginService.ActivatePlugins(provider);

            _log.Info("Plugins loaded.");
            var core = provider.GetService<ITorchCore>();
            if (core == null)
                throw new Exception("No active plugin provided an implementation of ITorchCore.");

            core.Run();
            LogManager.Shutdown();
        }

        private static void LogUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var ex = (Exception)e.ExceptionObject;

            var sb = new StringBuilder();
            sb.AppendLine("A fatal runtime error has occured. Please include the entire section below when requesting help.");
            sb.AppendLine("----- ERROR START -----");
            LogException(ex, sb, 2);
            sb.AppendLine("----- ERROR END -------");
            _log?.Fatal(sb);
            LogManager.Shutdown();
            Process.GetCurrentProcess().Kill();
        }

        private static void LogException(Exception e, StringBuilder sb, int indentLevel)
        {
            var indent = new string(' ', indentLevel);
            var lines = $"{indent}{e.GetType()}: {e.Message}\n{e.StackTrace}".Split('\n');
            sb.AppendLine(string.Join($"\n{indent}", lines));

            indentLevel += 2;
            switch (e)
            {
                case ReflectionTypeLoadException re:
                    sb.AppendLine($"{indent}TYPE LOAD EXCEPTIONS:");
                    foreach (var inner in re.LoaderExceptions)
                        LogException(inner, sb, indentLevel);
                    break;
                case AggregateException ae:
                    sb.AppendLine($"{indent}INNER EXCEPTIONS:");
                    foreach (var inner in ae.InnerExceptions)
                        LogException(inner, sb, indentLevel);
                    break;
                default:
                    if (e.InnerException != null)
                    {
                        sb.AppendLine($"{indent}INNER EXCEPTION:");
                        LogException(e.InnerException, sb, indentLevel);
                    }

                    break;
            }
        }
    }
}
