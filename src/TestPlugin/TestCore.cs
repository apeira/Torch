using System;
using System.Threading;
using HarmonyLib;
using NLog;
using Torch.Core;

namespace TestPlugin
{
    /// <summary>
    /// Test core implementation.
    /// </summary>
    public class TestCore : ITorchCore
    {
        private static readonly Logger _log = LogManager.GetCurrentClassLogger();
        private readonly ManualResetEvent _mre = new ManualResetEvent(false);
        private CoreState _state;

        /// <inheritdoc/>
        public event Action<CoreState>? StateChanged;

        /// <inheritdoc/>
        public CoreState State
        {
            get => _state;
            private set
            {
                _state = value;
                StateChanged?.Invoke(_state);
            }
        }

        /// <inheritdoc/>
        public void Run()
        {
            _log.Info(GetType().FullDescription() + "::" + nameof(Run));
            State = CoreState.BeforeStart;
            State = CoreState.AfterStart;
            _mre.WaitOne();
        }

        /// <inheritdoc/>
        public void SignalStop(Action<ITorchCore>? callback = null)
        {
            _log.Info(GetType().FullDescription() + "::" + nameof(SignalStop));
            _mre.Set();
            State = CoreState.BeforeStop;
            State = CoreState.AfterStop;
            callback?.Invoke(this);
        }
    }
}
