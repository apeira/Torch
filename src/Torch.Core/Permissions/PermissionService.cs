using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using YamlDotNet.RepresentationModel;

namespace Torch.Core.Permissions
{
    public class PermissionService : IPermissionService
    {
        private readonly Dictionary<string, Dictionary<string, PermissionCollection>> _sections = new();
        private readonly Dictionary<string, PermissionModifier> _defaults = new();
        private PermissionModifier _default = PermissionModifier.Deny;
        private string _configFilePath;

        public PermissionService(TorchEnvironment env)
        {
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
            
            // Need to resolve inheritance after all collections are created.
            var inheritsMap = new Dictionary<string, List<string>>();
            
            using var f = File.OpenRead(configFilePath);
            using var sr = new StreamReader(f);
            var yaml = new YamlStream();
            yaml.Load(sr);
            var root = (YamlMappingNode)yaml.Documents[0].RootNode;

            foreach (var node in root.Children)
            {
                var section = ((YamlScalarNode)node.Key).Value;

                foreach (var collection in (YamlMappingNode)node.Value)
                {
                    var key = ((YamlScalarNode)collection.Key).Value;
                    var value = (YamlMappingNode)collection.Value;
                    var obj = GetPermissions(section, key);

                    foreach (var permission in (YamlSequenceNode)value["permissions"])
                    {
                        var val = ((YamlScalarNode)permission).Value;
                        obj.AddExpression(new PermissionExpression(val));
                    }

                    var inheritsList = inheritsMap[$"{section}.{key}"] = new List<string>();
                    foreach (var inherits in (YamlSequenceNode)value["inherits"])
                    {
                        inheritsList.Add(((YamlScalarNode)inherits).Value);
                    }
                }
            }

            foreach (var pair in inheritsMap)
            {
                var split1 = pair.Key.Split('.');
                var collection = GetPermissions(split1[0], split1[1]);

                foreach (var inherit in pair.Value)
                {
                    var split2 = inherit.Split('.');
                    collection.AddInherits(GetPermissions(split2[0], split2[1]));
                }
            }
        }

        private void Save(string configFilePath)
        {
            var fullMap = new YamlMappingNode();

            foreach (var section in _sections)
            {
                var sectionMap = new YamlMappingNode();

                foreach (var collection in section.Value.Values)
                {
                    var collectionMap = new YamlMappingNode
                    {
                        {
                            "permissions", 
                            new YamlSequenceNode(collection.Expressions.Select(x =>
                                new YamlScalarNode(x.ToString())))
                        },
                        {
                            "inherits", 
                            new YamlSequenceNode(collection.Inherits.Select(x =>
                                new YamlScalarNode($"{x.Section}.{x.Id}")))
                        }
                    };
                    
                    sectionMap.Add(collection.Id, collectionMap);
                }
                
                fullMap.Add(section.Key, sectionMap);
            }

            using var f = File.Create(configFilePath);
            using var sw = new StreamWriter(f);
            new YamlStream(new YamlDocument(fullMap)).Save(sw);
        }
    }
}