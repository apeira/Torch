using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Reflection;
using HarmonyLib;
using NLog;
using Sandbox;
using Sandbox.Game;
using Sandbox.Game.World;
using SpaceEngineers.Game;
using Torch.Core;
using VRage;
using VRage.Dedicated;
using VRage.Game;
using VRage.Game.SessionComponents;
using VRage.Platform.Windows;

namespace Torch.SpaceEngineers
{
    [ExcludeFromCodeCoverage]
    // ReSharper disable once InconsistentNaming
    public class SEDediCore : ITorchCore
    {
        private static readonly Logger _log = LogManager.GetCurrentClassLogger();

        private readonly TorchEnvironment _config;
        private CoreState _state;

        public event Action<CoreState>? StateChanged;

        public SEDediCore(TorchEnvironment config)
        {
            AppDomain.CurrentDomain.AssemblyResolve += RedirectComponentModelAnnotations;

            _config = config;
        }
        
        public CoreState State
        {
            get => _state;
            private set
            {
                _state = value;
                StateChanged?.Invoke(_state);
            }
        }

        /// <summary>
        /// Works around Space Engineers requesting an old version of the System.ComponentModel.Annotations library.
        /// </summary>
        private Assembly? RedirectComponentModelAnnotations(object sender, ResolveEventArgs args)
        {
            if (args.Name == "System.ComponentModel.Annotations, Version=4.2.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")
                return Assembly.LoadFrom("System.ComponentModel.Annotations.dll");

            return null;
        }

        /// <inheritdoc />
        public void Run()
        {
            _log.Info("Starting server.");
            State = CoreState.BeforeStart;

            // Initial constants setup from MyProgram.Main
            Sandbox.Engine.Platform.Game.IsDedicated = true;
            SpaceEngineersGame.SetupBasicGameInfo();
            SpaceEngineersGame.SetupPerGameSettings();
            MyPerServerSettings.GameName = MyPerGameSettings.GameName;
            MyPerServerSettings.GameNameSafe = MyPerGameSettings.GameNameSafe;
            MyPerServerSettings.GameDSName = MyPerServerSettings.GameNameSafe + "Dedicated";
            MyPerServerSettings.GameDSDescription = "Your place for space engineering, destruction and exploring.";
            MyPerGameSettings.SendLogToKeen = DedicatedServer.SendLogToKeen;
            MySessionComponentExtDebug.ForceDisable = true;
            MyPerServerSettings.AppId = 244850U;
            MyFinalBuildConstants.APP_VERSION = MyPerGameSettings.BasicGameInfo.GameVersion;

            MyVRageWindows.Init(MyPerGameSettings.BasicGameInfo.ApplicationName, MySandboxGame.Log, string.Empty, false);
            MySandboxGame.InitMultithreading();

            // Set DedicatedServer.InitializeServices to MyProgram.InitializeServices. This way if Keen changes
            // the service configuration it won't require a Torch update.
            Assembly.LoadFrom("SpaceEngineersDedicated.exe");
            var initServicesMethod = Delegate.CreateDelegate(
                typeof(Action<bool>),
                AccessTools.Method("SpaceEngineersDedicated.MyProgram:InitializeServices"));
            AccessTools.PropertySetter(typeof(DedicatedServer), nameof(DedicatedServer.InitializeServices))
                .Invoke(null, new object[] { initServicesMethod });

            // Necessary bits from DedicatedServer.RunMain
            MySimpleProfiler.ENABLE_SIMPLE_PROFILER = false;
            MyInitializer.InvokeBeforeRun(
                MyPerServerSettings.AppId,
                MyPerServerSettings.GameDSName,
                Path.Combine(_config.UserDataPath, "Instance"),
                DedicatedServer.AddDateToLog);

            MySession.OnLoading += MySessionOnOnLoading;
            var runInternal = AccessTools.Method("VRage.Dedicated.DedicatedServer:RunInternal");
            do
            {
                // The argument is the instance name.
                runInternal.Invoke(null, new object[] { "Torch" });
            }
            while (MySandboxGame.IsReloading);

            MyVRage.Done();
            State = CoreState.AfterStop;
        }

        /// <inheritdoc />
        public void SignalStop(Action<ITorchCore>? callback)
        {
            _log.Info("Signalling game to stop.");

            MySandboxGame.Static.Invoke(
                () =>
                {
                    State = CoreState.BeforeStop;
                    MySandboxGame.Static.Exit();
                    ParallelTasks.Parallel.Scheduler.WaitForTasksToFinish(TimeSpan.FromSeconds(10));
                    callback?.Invoke(this); 
                }, nameof(SEDediCore));
        }

        private void MySessionOnOnLoading()
        {
            MySession.Static.OnReady += OnSessionReady;
            MySession.OnLoading -= MySessionOnOnLoading;
        }

        private void OnSessionReady()
        {
            State = CoreState.AfterStart;
            MySession.Static.OnReady -= OnSessionReady;
        }
    }
}
