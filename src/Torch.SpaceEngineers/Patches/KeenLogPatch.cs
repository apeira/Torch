using System;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Threading;
using HarmonyLib;
using NLog;
using Torch.Core.Reflection;
using VRage.Utils;

namespace Torch.SpaceEngineers.Patches
{
    /// <summary>
    /// Patches the vanilla Space Engineers logger to redirect messages to NLog.
    /// </summary>
    [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1313", Justification = "Harmony patches use special parameter names.")]
    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Harmony patches use special parameter names.")]
    [SuppressMessage("ReSharper", "UnusedMember.Local", Justification = "Harmony patches aren't called directly.")]
    [SuppressMessage("ReSharper", "UnusedType.Global", Justification = "Harmony patches aren't instantiated directly.")]
    [HarmonyPatch(typeof(MyLog))]
    internal class KeenLogPatch
    {
        private static readonly Logger _log = LogManager.GetLogger("SpaceEngineers");
        private static readonly ThreadLocal<StringBuilder> _tmpStringBuilder = new ThreadLocal<StringBuilder>(() => new StringBuilder(32));
        private static readonly Func<MyLog, int> _getThreadId = MethodProxy.Create<Func<MyLog, int>>("GetThreadId");
        private static readonly Func<MyLog, int, int> _getIndentByThread = MethodProxy.Create<Func<MyLog, int, int>>("GetIdentByThread");

        [HarmonyPatch(nameof(MyLog.Log), typeof(MyLogSeverity), typeof(StringBuilder))]
        [HarmonyPrefix]
        private static bool PrefixLogStringBuilder(MyLog __instance, MyLogSeverity severity, StringBuilder builder)
        {
            _log.Log(LogLevelFor(severity), PrepareLog(__instance).Append(builder));
            return false;
        }

        [HarmonyPatch(nameof(MyLog.Log), typeof(MyLogSeverity), typeof(string), typeof(object[]))]
        [HarmonyPrefix]
        private static bool PrefixLogFormatted(MyLog __instance, MyLogSeverity severity, string format, object[]? args)
        {
            // Sometimes this is called with a pre-formatted string and no args
            // and causes a crash when the format string contains braces
            var sb = PrepareLog(__instance);
            if (args != null && args.Length > 0)
                sb.AppendFormat(format, args);
            else
                sb.Append(format);

            _log.Log(LogLevelFor(severity), sb);
            return false;
        }

        [HarmonyPatch(nameof(MyLog.WriteLine), typeof(string), typeof(LoggingOptions))]
        [HarmonyPrefix]
        private static bool PrefixWriteLineOptions(MyLog __instance, string message, LoggingOptions option)
        {
            if (__instance.LogFlag(option))
                _log.Info(PrepareLog(__instance).Append(message));
            return false;
        }

        [HarmonyPatch(nameof(MyLog.WriteLine), typeof(string))]
        [HarmonyPrefix]
        private static bool PrefixWriteLineString(MyLog __instance, string msg)
        {
            _log.Debug(PrepareLog(__instance).Append(msg));
            return false;
        }

        [HarmonyPatch(nameof(MyLog.WriteLine), typeof(Exception))]
        [HarmonyPrefix]
        private static bool PrefixWriteLineException(Exception ex)
        {
            _log.Error(ex);
            return false;
        }

        [HarmonyPatch(nameof(MyLog.WriteLineAndConsole), typeof(string))]
        [HarmonyPrefix]
        private static bool PrefixWriteLineAndConsole(MyLog __instance, string msg)
        {
            _log.Info(PrepareLog(__instance).Append(msg));
            return false;
        }

        [HarmonyPatch(nameof(MyLog.AppendToClosedLog), typeof(string))]
        [HarmonyPrefix]
        private static bool PrefixAppendToClosedLogString(MyLog __instance, string text)
        {
            _log.Info(PrepareLog(__instance).Append(text));
            return false;
        }

        [HarmonyPatch(nameof(MyLog.AppendToClosedLog), typeof(Exception))]
        [HarmonyPrefix]
        private static bool PrefixAppendToClosedLogException(Exception e)
        {
            _log.Error(e);
            return false;
        }

        private static LogLevel LogLevelFor(MyLogSeverity severity)
        {
            return severity switch
            {
                MyLogSeverity.Debug => LogLevel.Debug,
                MyLogSeverity.Info => LogLevel.Info,
                MyLogSeverity.Warning => LogLevel.Warn,
                MyLogSeverity.Error => LogLevel.Error,
                MyLogSeverity.Critical => LogLevel.Fatal,
                _ => LogLevel.Info
            };
        }

        private static StringBuilder PrepareLog(MyLog log)
        {
            var v = _tmpStringBuilder.Value;
            v.Clear();
            var i = _getThreadId(log);

            var t = 0;
            try
            {
                t = _getIndentByThread(log, i);
            }
            catch (NullReferenceException)
            {
                _log.Trace("Failed to get per-thread indent, defaulting to 0");
            }

            v.Append(' ', t * 3);
            return v;
        }
    }
}
