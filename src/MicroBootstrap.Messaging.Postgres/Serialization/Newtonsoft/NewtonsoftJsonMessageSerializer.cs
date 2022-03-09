using System.Reflection;
using MicroBootstrap.Abstractions.Messaging.Serialization;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace MicroBootstrap.Messaging.Serialization.Newtonsoft;

public class NewtonsoftJsonMessageSerializer : IMessageSerializer
{
    private readonly NewtonsoftJsonUnSupportedTypeMatcher _newtonsoftJsonUnSupportedTypeMatcher;
    private readonly IList<JsonConverter> _converters;

    public NewtonsoftJsonMessageSerializer(
        IOptions<NewtonsoftJsonOptions> options,
        NewtonsoftJsonUnSupportedTypeMatcher newtonsoftJsonUnSupportedTypeMatcher)
    {
        _newtonsoftJsonUnSupportedTypeMatcher = newtonsoftJsonUnSupportedTypeMatcher;
        _converters = options.Value.Converters;
    }

    public bool CanHandle(Type type)
    {
        return !_newtonsoftJsonUnSupportedTypeMatcher.Match(type);
    }

    public string Serialize(object obj, bool camelCase = true, bool indented = true)
    {
        return JsonConvert.SerializeObject(obj, CreateSerializerSettings(camelCase, indented));
    }

    public T? Deserialize<T>(string payload, bool camelCase = true)
    {
        return JsonConvert.DeserializeObject<T>(payload, CreateSerializerSettings(camelCase));
    }

    public object? Deserialize(string payload, Type type, bool camelCase = true)
    {
        return JsonConvert.DeserializeObject(payload, type, CreateSerializerSettings(camelCase));
    }

    protected virtual JsonSerializerSettings? CreateSerializerSettings(bool camelCase = true, bool indented = false)
    {
        var settings = new JsonSerializerSettings();

        ((List<JsonConverter>)settings.Converters).AddRange(_converters);

        settings.ContractResolver = new ContractResolverWithPrivate();

        if (indented)
        {
            settings.Formatting = Formatting.Indented;
        }

        // for handling private constructor
        settings.ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor;
        settings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;

        return settings;
    }

    private class ContractResolverWithPrivate : CamelCasePropertyNamesContractResolver
    {
        // http://danielwertheim.se/json-net-private-setters/
        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            var prop = base.CreateProperty(member, memberSerialization);

            if (!prop.Writable)
            {
                var property = member as PropertyInfo;
                if (property != null)
                {
                    var hasPrivateSetter = property.GetSetMethod(true) != null;
                    prop.Writable = hasPrivateSetter;
                }
            }

            return prop;
        }
    }
}
