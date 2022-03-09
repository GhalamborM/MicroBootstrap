using Microsoft.Extensions.Options;

namespace MicroBootstrap.Messaging.Serialization.Newtonsoft
{
    public class NewtonsoftJsonUnSupportedTypeMatcher
    {
        protected NewtonsoftJsonOptions Options { get; }

        public NewtonsoftJsonUnSupportedTypeMatcher(IOptions<NewtonsoftJsonOptions> options)
        {
            Options = options.Value;
        }

        public virtual bool Match(Type type)
        {
            return Options.UnSupportedTypes.Contains(type);
        }
    }
}