using Torch.Core.Commands;
using Xunit;

namespace Torch.Core.Tests.Commands
{
    /// <summary>
    /// Tests for <see cref="CommandService"/>.
    /// </summary>
    public class CommandServiceTests
    {
        /// <summary>
        /// Tests that command processors are called in the order they were added.
        /// </summary>
        [Fact]
        public void ProcessorsCalledInRightOrder()
        {
            var commands = new CommandService(null!);
            commands.RegisterDelegate("test", ctx => ctx.Respond("exec"));
            commands.AddProcessorStep((ctx, next) =>
            {
                ctx.Respond("1");
                next(ctx);
                ctx.Respond("6");
            });
            commands.AddProcessorStep((ctx, next) =>
            {
                ctx.Respond("2");
                next(ctx);
                ctx.Respond("5");
            });
            commands.AddProcessorStep((ctx, next) =>
            {
                ctx.Respond("3");
                next(ctx);
                ctx.Respond("4");
            });
            var response = string.Empty;
            commands.Execute(null!, "test", x => response += x);
            Assert.Equal("123exec456", response);
        }
    }
}
