namespace Torch.Core.Commands
{
    public delegate bool CommandCanExecuteDel(ICommandContext context, ref string reason);
}