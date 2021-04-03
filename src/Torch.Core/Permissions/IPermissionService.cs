namespace Torch.Core.Permissions
{
    /// <summary>
    /// Provides a centralized access control system with support for individual
    /// permissions as well as permission groups.
    /// </summary>
    public interface IPermissionService
    {
        /// <summary>
        /// Get the <see cref="IPermissionCollection"/> associated with the ID in the given section.
        /// </summary>
        /// <param name="section">The section to search for the ID in.</param>
        /// <param name="id">The ID to find.</param>
        /// <returns></returns>
        public IPermissionCollection GetPermissions(string section, string id);

        public void SetDefaultAccess(string node, PermissionModifier access);

        public void SetDefaultAccess(PermissionModifier access);

        public bool IsAllowed(string section, string id, string node);

        public bool IsDenied(string section, string id, string node);

        public void SavePermissions();

        public void ReloadPermissions();
    }
}
