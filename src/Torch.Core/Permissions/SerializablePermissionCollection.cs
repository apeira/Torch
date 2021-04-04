using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using YamlDotNet.Serialization;

namespace Torch.Core.Permissions
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    internal class SerializablePermissionCollection
    {
        [YamlMember(Alias = "permissions")]
        public List<string> Permissions { get; set; }
        
        [YamlMember(Alias = "inherits")]
        public List<string> Inherits { get; set; }

        public SerializablePermissionCollection(IPermissionCollection collection)
        {
            Permissions = collection.Expressions.Select(x => x.ToString()).ToList();
            Inherits = collection.Inherits.Select(x => $"{x.Section}.{x.Id}").ToList();
        }

        public SerializablePermissionCollection()
        {
            Permissions = new List<string>();
            Inherits = new List<string>();
        }
    }
}