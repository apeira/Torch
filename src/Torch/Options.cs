using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using CommandLine;
using JetBrains.Annotations;

namespace Torch
{
    /// <summary>
    /// Represents Torch's command line options.
    /// </summary>
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    [SuppressMessage("ReSharper", "SA1600", Justification = "Documentation is in HelpText.")]
    public class Options
    {
        [Option("userdata", HelpText = "Set the folder location to store user data.")]
        public string UserData { get; set; } = Path.Combine(Directory.GetCurrentDirectory(), "instance");

#if DEBUG
        [Option("load", HelpText = "(debug only) Forcibly load a DLL file as a plugin.")]
#endif
        public IEnumerable<string> PluginDlls { get; set; } = new List<string>();

#if DEBUG
        [Option("nolauncher", HelpText = "(debug only) Disable loading Torch into a new AppDomain.")]
#endif
        public bool NoLauncher { get; set; }
    }
}
