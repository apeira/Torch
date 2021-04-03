using System;
using System.Collections.Generic;

namespace Torch.Core.Permissions
{
    public class PermissionService : IPermissionService
    {
        private readonly Dictionary<string, Dictionary<string, PermissionCollection>> _sections = new();
        private readonly Dictionary<string, PermissionModifier> _defaults = new();
        private PermissionModifier _default = PermissionModifier.Deny;
        
        public IPermissionCollection GetPermissions(string section, string id)
        {
            if (!_sections.TryGetValue(section, out var dict))
                dict = _sections[section] = new Dictionary<string, PermissionCollection>();
            if (!dict.TryGetValue(id, out var collection))
                collection = dict[id] = new PermissionCollection(section, id);

            return collection;
        }

        public void SetDefaultAccess(PermissionModifier access)
        {
            if (access == PermissionModifier.Default)
                throw new ArgumentException("Default access must be Allow or Deny.");

            _default = access;
        }
        
        public void SetDefaultAccess(string node, PermissionModifier access)
        {
            if (access == PermissionModifier.Default)
                throw new ArgumentException("Default access must be Allow or Deny.");

            _defaults[node] = access;
        }

        public bool IsAllowed(string section, string id, string node)
        {
            var result = GetPermissions(section, id).Evaluate(SplitNode(node));
            if (result == PermissionModifier.Default)
                if (!_defaults.TryGetValue(node, out result))
                    result = _default;

            return result switch
            {
                PermissionModifier.Allow => true,
                PermissionModifier.Deny => false,
                _ => throw new InvalidOperationException("Permission modifier should not be default at this point.")
            };
        }

        public bool IsDenied(string section, string id, string node)
        {
            var result = GetPermissions(section, id).Evaluate(SplitNode(node));
            if (result == PermissionModifier.Default)
                if (!_defaults.TryGetValue(node, out result))
                    result = _default;

            return result switch
            {
                PermissionModifier.Allow => false,
                PermissionModifier.Deny => true,
                _ => throw new InvalidOperationException("Permission modifier should not be default at this point.")
            };
        }

        private string[] SplitNode(string node)
        {
            // TODO sanitization
            return node.Split('.');
        }

        private void Load(string configFilePath)
        {
            throw new NotImplementedException();
        }

        private void Save(string configFilePath)
        {
            throw new NotImplementedException();
        }
    }
}