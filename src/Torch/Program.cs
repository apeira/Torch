using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using HarmonyLib;
using Microsoft.Extensions.DependencyInjection;
using NLog;
using Torch.Core;
using Torch.Core.Plugins;

namespace Torch
{
    internal static class Program
    {
        private static Logger Log;
        private static readonly PluginService _pluginService = new();

        public static void Main(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += LogUnhandledException;

            var assemblyDir = Directory.GetParent(typeof(Program).Assembly.Location).FullName;
            var dataPathOptionIndex = Array.IndexOf(args, "--userdata");
            var dataPath = dataPathOptionIndex > 0 ? args[dataPathOptionIndex + 1] : "instance";
            var environment = new TorchEnvironment(assemblyDir, dataPath, args);

            environment.Services.AddSingleton<IPluginService>(_pluginService);
            _pluginService.LoadPluginDebug(typeof(TestPlugin.TestPlugin).Assembly, environment);
            _pluginService.LoadPlugins(environment.Configuration.Plugins, environment);

            LogManager.Configuration = environment.Logging;
            Log = LogManager.GetLogger("Torch");
            Log.Info("Logging initialized.");

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

            Log.Debug(sb);

            var provider = environment.Services.BuildServiceProvider();
            _pluginService.ActivatePlugins(provider);

            Log.Info("Plugins loaded.");
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
            Log.Fatal(sb);
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
