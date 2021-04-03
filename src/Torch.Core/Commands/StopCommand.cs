namespace Torch.Core.Commands
{
    public class StopCommand : ICommand
    {
        private ITorchCore _core;
        
        public StopCommand(ITorchCore core)
        {
            _core = core;
        }
        
        public void Execute(ICommandContext context)
        {
            _core.SignalStop(_ => context.Respond("Core stopped successfully."));
        }

        public bool CanExecute(ICommandContext context, ref string reason)
        {
            if (_core.State == CoreState.AfterStart)
                return true;

            reason = "The core is not running.";
            return false;
        }
    }
}