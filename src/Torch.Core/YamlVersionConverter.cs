using System;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;
using Version = SemVer.Version;

namespace Torch.Core
{
    public class YamlVersionConverter : IYamlTypeConverter
    {
        public bool Accepts(Type type) => type == typeof(Version);

        public object ReadYaml(IParser parser, Type type)
        {
            return new Version(parser.Consume<Scalar>().Value);
        }

        public void WriteYaml(IEmitter emitter, object value, Type type)
        {
            var str = ((Version)value).ToString();
            emitter.Emit(new Scalar(str));
        }
    }
}
