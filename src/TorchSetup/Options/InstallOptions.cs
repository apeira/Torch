using System.Collections.Generic;
using CommandLine;

namespace TorchSetup.Options
{
    [Verb("install")]
    public class InstallOptions
    {
        [Option('b', "basegame", HelpText = "The base game to install Torch on.")]
        public BaseGameType BaseGame { get; set; }

        [Option('t', "target", HelpText = "The location to install Torch.")]
        public string InstallLocation { get; set; } = string.Empty;

        [Option('p', "plugin", HelpText = "A plugin to install.")]
        public IEnumerable<string> Plugins { get; set; } = new List<string>();
    }
}
