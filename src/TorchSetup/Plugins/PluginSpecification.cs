using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using SemVer;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using Version = SemVer.Version;

namespace TorchSetup.Plugins
{
    /// <summary>
    /// Represents a plugin specification file.
    /// </summary>
    public class PluginSpecification
    {
        private string _id;

        [YamlMember(Alias = "id")]
        public string Id
        {
            get => _id;
            set => SetId(value);
        }

        [YamlIgnore]
        public Version Version { get; set; }

        [YamlMember(Alias = "version")]
        public string VersionSerializable
        {
            get => Version.ToString();
            set => Version = Version.Parse(value);
        }

        [YamlMember(Alias = "name", DefaultValuesHandling = DefaultValuesHandling.OmitNull)]
        public string? Name { get; set; }

        [YamlMember(Alias = "author")]
        public string Author { get; set; }

        [YamlMember(Alias = "desc")]
        public string Description { get; set; }

        [YamlMember(Alias = "website", DefaultValuesHandling = DefaultValuesHandling.OmitNull)]
        public string? Website { get; set; }

        [YamlMember(Alias = "entryPoint")]
        public string EntryPoint { get; set; }

        [YamlIgnore]
        public List<(string Id, Range Range)>? Requires { get; set; }

        [YamlMember(Alias = "requires", DefaultValuesHandling = DefaultValuesHandling.OmitNull)]
        public IEnumerable<string>? RequiresSerializable
        {
            get => Requires?.Select(x => $"{x.Id} {x.Range}");
            set => SetRequires(value);
        }

        [YamlIgnore]
        public List<(string Id, Range Range)>? Conflicts { get; set; }

        [YamlMember(Alias = "conflicts", DefaultValuesHandling = DefaultValuesHandling.OmitNull)]
        public IEnumerable<string>? ConflictsSerializable
        {
            get => Conflicts?.Select(x => $"{x.Id} {x.Range}");
            set => SetConflicts(value);
        }

        [YamlMember(Alias = "load-after", DefaultValuesHandling = DefaultValuesHandling.OmitNull)]
        public List<string>? LoadAfter { get; set; }

        private void SetId(string id)
        {
            ValidateId(id);
            _id = id;
        }

        private IEnumerable<(string Id, Range Range)> Parse(IEnumerable<string>? collection)
        {
            if (collection is null)
                yield break;

            foreach (var str in collection)
            {
                var delimIndex = str.IndexOf(' ');

                if (delimIndex > 0)
                {
                    var id = str.Substring(0, delimIndex);
                    var range = str.Substring(delimIndex + 1);
                    ValidateId(id);
                    yield return (id, new Range(range));
                }
                else
                {
                    yield return (str, new Range("*"));
                }
            }
        }

        private void SetConflicts(IEnumerable<string>? conflicts)
        {
            if (conflicts is null)
            {
                Conflicts = null;
                return;
            }

            Conflicts = Parse(conflicts).ToList();
        }

        private void SetRequires(IEnumerable<string>? requires)
        {
            if (requires == null)
            {
                Requires = null;
                return;
            }

            Requires = Parse(requires).ToList();
        }

        private void ValidateId(string id)
        {
            const string allowedChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-";
            if (id.Any(x => !allowedChars.Contains(x)))
                throw new ArgumentException($"The ID '{id}' contains invalid characters.");
        }

        public static PluginSpecification Load(string file)
        {
            var deser = new DeserializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();

            return deser.Deserialize<PluginSpecification>(File.ReadAllText("exampleSpec.yaml"));
        }
    }
}
