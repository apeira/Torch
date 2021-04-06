using TorchSetup.Options;

namespace TorchSetup.InstallStrategies
{
    public interface IInstallStrategy
    {
        /// <summary>
        /// Gets the default location that this strategy can install to.
        /// </summary>
        string DefaultPath { get; }

        /// <summary>
        /// Installs Torch using this strategy.
        /// </summary>
        /// <param name="basePath">The base path of the installation.</param>
        void Install(InstallOptions options);
    }
}
