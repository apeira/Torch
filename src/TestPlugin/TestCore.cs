using System;
using System.Threading;
using HarmonyLib;
using NLog;
using Torch.Core;

namespace TestPlugin
{
    public class TestCore : ITorchCore
    {
        private static readonly Logger _log = LogManager.GetCurrentClassLogger();
        private readonly ManualResetEvent _mre = new ManualResetEvent(false);
        public event Action<CoreState> StateChanged;

        private CoreState _state;

        public CoreState State
        {
            get => _state;
            private set
            {
                _state = value;
                StateChanged?.Invoke(_state);
            }
        }

        public void Run()
        {
            _log.Info(GetType().FullDescription() + "::" + nameof(Run));
            State = CoreState.BeforeStart;
            State = CoreState.AfterStart;
            _mre.WaitOne();
        }

        public void SignalStop(Action<ITorchCore> callback = null)
        {
            _log.Info(GetType().FullDescription() + "::" + nameof(SignalStop));
            _mre.Set();
            State = CoreState.BeforeStop;
            State = CoreState.AfterStop;
            callback?.Invoke(this);
        }
    }
}