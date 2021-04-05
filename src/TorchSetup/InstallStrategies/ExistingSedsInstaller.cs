using System;
using System.IO;
using System.Linq;

namespace TorchSetup.InstallStrategies
{
    /// <summary>
    /// Installs Torch to an existing Space Engineers Dedicated installation.
    /// </summary>
    public class ExistingSedsInstaller : IInstallStrategy
    {
        private string? _basePath;

        /// <inheritdoc/>
        public string DefaultPath { get; } =
            @"C:\Program Files (x86)\Steam\steamapps\common\SpaceEngineersDedicatedServer";

        /// <inheritdoc/>
        public void Install(string basePath)
        {
            var testPath = Path.Combine(basePath, "DedicatedServer64", "SpaceEngineersDedicated.exe");
            if (!File.Exists(testPath))
                throw new InvalidOperationException("The base path doesn't look like a SEDS installation.");

            _basePath = basePath;
            CleanBaseDir();
            CopyTorchFiles();
        }

        /// <summary>
        /// Deletes stuff in the base directory that shouldn't be there.
        /// </summary>
        private void CleanBaseDir()
        {
            var dirsToKeep = new []
            {
                "_CommonRedist",
                "Content",
                "DedicatedServer64",
                "TempContent",
                "instance",
            };

            foreach (var dir in Directory.GetDirectories(_basePath))
            {
                var info = new DirectoryInfo(dir);
                if (dirsToKeep.Contains(info.Name))
                    continue;

                Directory.Delete(dir, true);
            }

            foreach (var file in Directory.GetFiles(_basePath))
            {
                File.Delete(file);
            }
        }

        private void CopyTorchFiles()
        {
            var torchDir = Path.Combine(_basePath, "bin-torch");
            if (Directory.Exists(torchDir))
                Directory.Delete(torchDir, true);

            Directory.CreateDirectory(torchDir);

            foreach (var file in Directory.GetFiles(Directory.GetCurrentDirectory()))
            {
                var info = new FileInfo(file);

                if (info.Extension == ".exe" || info.Extension == ".dll")
                {
                    var dest = Path.Combine(torchDir, info.Name);
                    File.Copy(file, dest);
                }
            }

            const string domainOptions = "..\\\nbin-torch;DedicatedServer64";
            File.WriteAllText(Path.Combine(torchDir, "domain"), domainOptions);

            Shortcut.Create(
                Path.Combine(_basePath, "Torch.lnk"),
                Path.Combine(torchDir, "Torch.exe"),
                "--load Torch.SpaceEngineers.dll",
                torchDir,
                "Launch Torch",
                string.Empty,
                string.Empty);
        }
    }
}
