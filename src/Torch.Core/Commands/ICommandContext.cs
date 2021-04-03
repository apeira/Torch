using System.Collections.Generic;

namespace Torch.Core.Commands
{
    public interface ICommandContext
    {
        public object Sender { get; set; }

        public string CommandName { get; set; }

        public string[] Args { get; set; }

        public void Respond(string message);

        public IDictionary<string, object> OtherData { get; }
    }
}