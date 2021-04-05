using TorchSetup.InstallStrategies;

namespace TorchSetup
{
    internal static class Program
    {
        public static void Main(string[] args)
        {
            var installer = new ExistingSedsInstaller();
            installer.Install(installer.DefaultPath);
        }
    }
}
