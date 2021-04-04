using System;
using System.Collections.Generic;
using System.IO;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Torch.Core.Permissions
{
    public class PermissionService : IPermissionService
    {
        private readonly Dictionary<string, Dictionary<string, PermissionCollection>> _sections = new();
        private readonly Dictionary<string, PermissionModifier> _defaults = new();
        private readonly string _configFilePath;
        private readonly ISerializer _serializer;
        private readonly IDeserializer _deserializer;
        private PermissionModifier _default = PermissionModifier.Deny;

        public PermissionService(TorchEnvironment env)
        {
            _serializer = new SerializerBuilder().WithNamingConvention(CamelCaseNamingConvention.Instance).Build();
            _deserializer = new DeserializerBuilder().WithNamingConvention(CamelCaseNamingConvention.Instance).Build();
            _configFilePath = Path.Combine(env.UserDataPath, "config", "permissions.yaml");
            Load(_configFilePath);
        }
        
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

        public void SavePermissions()
        {
            Save(_configFilePath);
        }

        public void ReloadPermissions()
        {
            Load(_configFilePath);
        }

        private string[] SplitNode(string node)
        {
            // TODO sanitization
            return node.Split('.');
        }

        private void Load(string configFilePath)
        {
            if (!File.Exists(configFilePath))
                return;
            
            _sections.Clear();
            
            using var f = File.OpenRead(configFilePath);
            using var sr = new StreamReader(f);
            var data = _deserializer.Deserialize<Dictionary<string, Dictionary<string, SerializablePermissionCollection>>>(sr);

            foreach (var kv in data)
            {
                foreach (var kv2 in kv.Value)
                {
                    var collection = GetPermissions(kv.Key, kv2.Key);
                    foreach (var permission in kv2.Value.Permissions)
                        collection.AddExpression(new PermissionExpression(permission));

                    foreach (var inherits in kv2.Value.Inherits)
                    {
                        var sectionAndKey = inherits.Split('.');
                        collection.AddInherits(GetPermissions(sectionAndKey[0], sectionAndKey[1]));
                    }
                }
            }
        }

        private void Save(string configFilePath)
        {
            var serializablePermissions =
                new Dictionary<string, Dictionary<string, SerializablePermissionCollection>>();

            foreach (var kv in _sections)
            {
                var section = serializablePermissions[kv.Key] = new();
                foreach (var collection in _sections[kv.Key])
                    section[collection.Key] = new SerializablePermissionCollection(collection.Value);
            }

            using var f = File.Create(configFilePath);
            using var sw = new StreamWriter(f);
            _serializer.Serialize(sw, serializablePermissions);
        }
    }
}