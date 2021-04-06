using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using SemVer;
using YamlDotNet.Serialization;
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

        [YamlMember(Alias = "entry")]
        public string EntryPoint { get; set; }

        [YamlIgnore]
        public List<(string Id, Range Range)>? Requires { get; set; }

        [YamlMember(Alias = "requires", DefaultValuesHandling = DefaultValuesHandling.OmitNull)]
        public IEnumerable<string>? RequiresSerializable
        {
            get => Requires?.Select(x => $"{x.Id} {x.Range}");
            set => SetRequires(value);
        }

        private void SetId(string id)
        {
            ValidateId(id);
            _id = id;
        }

        private void SetRequires(IEnumerable<string>? requires)
        {
            if (requires == null)
            {
                Requires = null;
                return;
            }

            Requires = new List<(string Id, Range Range)>();
            foreach (var str in requires)
            {
                var delimIndex = str.IndexOf(' ');

                if (delimIndex > 0)
                {
                    var id = str.Substring(0, delimIndex);
                    var range = str.Substring(delimIndex + 1);
                    ValidateId(id);
                    Requires.Add((id, new Range(range)));
                }
                else
                {
                    Requires.Add((str, new Range("*")));
                }
            }
        }

        private void ValidateId(string id)
        {
            const string allowedChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-";
            if (id.Any(x => !allowedChars.Contains(x)))
                throw new ArgumentException($"The ID '{id}' contains invalid characters.");
        }
    }
}
