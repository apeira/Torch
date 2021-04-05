using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using JetBrains.Annotations;
using YamlDotNet.Serialization;

namespace Torch.Core.Permissions
{
    [DataContract]
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    internal class SerializablePermissionCollection
    {
        [DataMember]
        public List<string> Permissions { get; set; }

        [DataMember]
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