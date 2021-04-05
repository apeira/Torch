using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Reflection;
using HarmonyLib;
using NLog;
using Sandbox;
using Sandbox.Engine.Networking;
using Sandbox.Engine.Utils;
using Sandbox.Game;
using Sandbox.Game.World;
using SpaceEngineers.Game;
using Torch.Core;
using VRage;
using VRage.Dedicated;
using VRage.Dedicated.RemoteAPI;
using VRage.FileSystem;
using VRage.Game;
using VRage.Game.SessionComponents;
using VRage.Platform.Windows;
using VRageRender;

namespace Torch.SpaceEngineers
{
    /// <summary>
    /// Runs a Space Engineers Dedicated server.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class SpaceDediCore : ITorchCore
    {
        private static readonly Logger _log = LogManager.GetCurrentClassLogger();

        private readonly TorchEnvironment _config;
        private CoreState _state;

        /// <summary>
        /// Initializes a new instance of the <see cref="SpaceDediCore"/> class.
        /// </summary>
        /// <param name="config">The current Torch environment.</param>
        public SpaceDediCore(TorchEnvironment config)
        {
            AppDomain.CurrentDomain.AssemblyResolve += RedirectAssemblyBinding;
            _config = config;
        }

        /// <inheritdoc />
        public event Action<CoreState>? StateChanged;

        /// <inheritdoc />
        public CoreState State
        {
            get => _state;
            private set
            {
                _state = value;
                StateChanged?.Invoke(_state);
            }
        }

        /// <inheritdoc />
        public void Run()
        {
            _log.Info("Starting server.");
            State = CoreState.BeforeStart;

            // Required for Steam networking to initialize properly.
            if (!File.Exists("steam_appid.txt"))
                File.WriteAllText("steam_appid.txt", "244850");

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
            Assembly.LoadFrom(@"DedicatedServer64\SpaceEngineersDedicated.exe");
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
                _config.UserDataPath,
                DedicatedServer.AddDateToLog);

            MySession.OnLoading += MySessionOnOnLoading;
            RunInternal();

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
                }, nameof(SpaceDediCore));
        }

        /// <summary>
        /// Adapted from <see cref="DedicatedServer.RunInternal"/>.
        /// </summary>
        private void RunInternal()
        {
            MySandboxGame.IsReloading = false;
            MyFileSystem.ExePath = new FileInfo(typeof(DedicatedServer).Assembly.Location).DirectoryName;
            MyFileSystem.RootPath = Directory.GetCurrentDirectory();
            MyFileSystem.InitUserSpecific(null, "saves");
            MyRenderProxy.Initialize(new MyNullRender());
            MyNetworkMonitor.Init();

            var configPath = Path.Combine(_config.UserDataPath, "SpaceEngineers-Dedicated.cfg");
            MySandboxGame.ConfigDedicated = new MyConfigDedicated<MyObjectBuilder_SessionSettings>(configPath);
            if (File.Exists(configPath))
                MySandboxGame.ConfigDedicated.Load();
            else
                MySandboxGame.ConfigDedicated.Save(configPath);

            if (MySandboxGame.ConfigDedicated.AutoRestartEnabled)
            {
                _log.Error("Torch does not support 'AutoRestartEnabled'. Please disable this setting.");
                return;
            }

            if (MySandboxGame.ConfigDedicated.AutoUpdateEnabled)
            {
                _log.Error("Torch does not support 'AutoUpdateEnabled'. Please disable this setting.");
                return;
            }

            // This depends on enabling console compatibility in the DS config file.
            DedicatedServer.InitConsoleCompatibility();

            DedicatedServer.InitializeServices(true);

            // TODO pass args to MySandboxGame
            using var game = new MySandboxGame(new string[0]);

            if (!MyGameService.HasGameServer)
                _log.Error($"{MyGameService.Networking.ServiceName} is not running!");

            _log.Info($"Server name: {MySandboxGame.ConfigDedicated.ServerName}");
            _log.Info($"World name: {MySandboxGame.ConfigDedicated.WorldName}");

            // VRage Remote API setup
            MyRemoteServer? remoteApi = null;
            var remoteApiPort = MySandboxGame.ConfigDedicated.RemoteApiPort;
            var remoteApiKey = MySandboxGame.ConfigDedicated.RemoteSecurityKey;
            if (MySandboxGame.ConfigDedicated.RemoteApiEnabled && !string.IsNullOrEmpty(remoteApiKey) && remoteApiPort != 0)
                remoteApi = new MyRemoteServer(remoteApiPort, remoteApiKey);

            game.Run();

            if (remoteApi != null && remoteApi.IsRunning)
                AccessTools.Method(typeof(MyRemoteServer), "Stop").Invoke(remoteApi, new object[0]);
            MyGameService.ShutDown();
        }

        /// <summary>
        /// Works around Space Engineers requesting the wrong versions of certain assemblies since we aren't
        /// using its app.config.
        /// </summary>
        private Assembly? RedirectAssemblyBinding(object sender, ResolveEventArgs args)
        {
            if (args.Name == "System.ComponentModel.Annotations, Version=4.2.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")
                return Assembly.LoadFrom(@"DedicatedServer64\System.ComponentModel.Annotations.dll");

            if (args.Name == "System.Runtime.CompilerServices.Unsafe, Version=4.0.4.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")
                return Assembly.LoadFrom(@"DedicatedServer64\System.Runtime.CompilerServices.Unsafe.dll");

            return null;
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
