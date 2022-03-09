using MicroBootstrap.Abstractions.Types;
using MicroBootstrap.Core.Types.Collections;
using Newtonsoft.Json;

namespace MicroBootstrap.Messaging.Serialization.Newtonsoft
{
    public class NewtonsoftJsonOptions
    {
        public IList<JsonConverter> Converters { get; set; }
        public ITypeList UnSupportedTypes { get; }

        public NewtonsoftJsonOptions()
        {
            Converters = new List<JsonConverter>();
            UnSupportedTypes = new TypeList();
        }
    }
}
