using System.Collections;
using System.Linq.Expressions;
using System.Reflection;

namespace MicroBootstrap.Core.Utils.Reflections;

public static class ReflectionHelpers
{
    public static IEnumerable<Type> GetAllTypesImplementingInterface<TInterface>(params Assembly[] assemblies)
    {
        var inputAssemblies = assemblies.Any() ? assemblies : AppDomain.CurrentDomain.GetAssemblies();
        return inputAssemblies.SelectMany(GetAllTypesImplementingInterface<TInterface>);
    }

    private static IEnumerable<Type> GetAllTypesImplementingInterface<TInterface>(Assembly assembly = null)
    {
        var inputAssembly = assembly ?? Assembly.GetExecutingAssembly();
        return inputAssembly.GetTypes()
            .Where(type => typeof(TInterface).IsAssignableFrom(type) && !type.IsInterface && !type.IsAbstract &&
                           type.IsClass);
    }

    public static IEnumerable<string> GetPropertyNames<T>(params Expression<Func<T, object>>[] propertyExpressions)
    {
        var retVal = new List<string>();
        foreach (var propertyExpression in propertyExpressions)
        {
            retVal.Add(GetPropertyName(propertyExpression));
        }

        return retVal;
    }

    public static string GetPropertyName<T>(Expression<Func<T, object>> propertyExpression)
    {
        string retVal = null;
        if (propertyExpression != null)
        {
            var lambda = (LambdaExpression)propertyExpression;
            MemberExpression memberExpression;
            if (lambda.Body is UnaryExpression unaryExpression)
            {
                memberExpression = (MemberExpression)unaryExpression.Operand;
            }
            else
            {
                memberExpression = (MemberExpression)lambda.Body;
            }

            retVal = memberExpression.Member.Name;
        }

        return retVal;
    }


    // https://riptutorial.com/csharp/example/15938/creating-an-instance-of-a-type
    public static bool IsHaveAttribute(this PropertyInfo propertyInfo, Type attribute)
    {
        return propertyInfo.GetCustomAttributes(attribute, true).Any();
    }

    public static T[] GetFlatObjectsListWithInterface<T>(this object obj, IList<T> resultList = null)
    {
        var retVal = new List<T>();

        resultList ??= new List<T>();

        // Ignore cycling references
        if (!resultList.Any(x => ReferenceEquals(x, obj)))
        {
            var objectType = obj.GetType();

            if (objectType.GetInterface(typeof(T).Name) != null)
            {
                retVal.Add((T)obj);
                resultList.Add((T)obj);
            }

            var properties = objectType.GetProperties(BindingFlags.Public | BindingFlags.Instance);

            var objects = properties.Where(x => x.PropertyType.GetInterface(typeof(T).Name) != null)
                .Select(x => (T)x.GetValue(obj)).ToList();

            // Recursive call for single properties
            retVal.AddRange(objects.Where(x => x != null)
                .SelectMany(x => x.GetFlatObjectsListWithInterface(resultList)));

            // Handle collection and arrays
            var collections = properties.Where(p => p.GetIndexParameters().Length == 0)
                .Select(x => x.GetValue(obj, null))
                .Where(x => x is IEnumerable && !(x is string))
                .Cast<IEnumerable>();

            foreach (var collection in collections)
            {
                foreach (var collectionObject in collection)
                {
                    if (collectionObject is T)
                    {
                        retVal.AddRange(collectionObject.GetFlatObjectsListWithInterface<T>(resultList));
                    }
                }
            }
        }

        return retVal.ToArray();
    }


    // https://stackoverflow.com/a/39679855/581476
    public static Task<dynamic> InvokeAsync(
        MethodInfo methodInfo,
        object obj,
        params object[] parameters)
    {
        dynamic awaitable = methodInfo.Invoke(obj, parameters);
        return awaitable;
    }

    public static T CastTo<T>(this object o) => (T)o;

    // https://stackoverflow.com/a/55852845/581476
    public static dynamic CastToReflected(this object o, Type type)
    {
        var methodInfo =
            typeof(ReflectionHelper).GetMethod(nameof(CastTo), BindingFlags.Static | BindingFlags.Public);
        var genericArguments = new[] { type };
        var genericMethodInfo = methodInfo?.MakeGenericMethod(genericArguments);
        return genericMethodInfo?.Invoke(null, new[] { o });
    }

    private static bool GenericParametersMatch(
        IReadOnlyList<Type> parameters,
        IReadOnlyList<Type> interfaceArguments)
    {
        if (parameters.Count != interfaceArguments.Count)
        {
            return false;
        }

        for (var i = 0; i < parameters.Count; i++)
        {
            if (parameters[i] != interfaceArguments[i])
            {
                return false;
            }
        }

        return true;
    }

    public static string GetModuleName(this object value)
        => value?.GetType().GetModuleName() ?? string.Empty;

    /// <summary>
    /// Iterates recursively over each public property of object to gather member values.
    /// </summary>
    public static IEnumerable<KeyValuePair<string, object>> TraverseObjectGraph(this object original)
    {
        foreach (var result in original.TraverseObjectGraphRecursively(new List<object>(), original.GetType().Name))
        {
            yield return result;
        }
    }

    private static IEnumerable<KeyValuePair<string, object>> TraverseObjectGraphRecursively(
        this object obj,
        List<object> visited,
        string memberPath)
    {
        yield return new KeyValuePair<string, object>(memberPath, obj);
        if (obj != null)
        {
            var typeOfOriginal = obj.GetType();
            if (!IsPrimitive(typeOfOriginal) && !visited.Any(x => ReferenceEquals(obj, x)))
            {
                visited.Add(obj);
                if (obj is IEnumerable objEnum)
                {
                    var originalEnumerator = objEnum.GetEnumerator();
                    var iIdx = 0;
                    while (originalEnumerator.MoveNext())
                    {
                        foreach (var result in originalEnumerator.Current.TraverseObjectGraphRecursively(
                                     visited,
                                     $@"{memberPath}[{iIdx++}]"))
                        {
                            yield return result;
                        }
                    }
                }
                else
                {
                    foreach (var propInfo in typeOfOriginal.GetProperties(BindingFlags.Instance |
                                                                          BindingFlags.Public))
                    {
                        foreach (var result in propInfo.GetValue(obj)
                                     .TraverseObjectGraphRecursively(visited, $@"{memberPath}.{propInfo.Name}"))
                        {
                            yield return result;
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// Check if type is a value-type, primitive type  or string
    /// </summary>
    public static bool IsPrimitive(this object obj)
    {
        return obj == null || obj.GetType().IsPrimitive();
    }

    public static Type? GetTypeFromAnyReferencingAssembly(string typeName)
    {
        var referencedAssemblies = Assembly.GetEntryAssembly()?
            .GetReferencedAssemblies()
            .Select(a => a.FullName);

        if (referencedAssemblies == null)
            return null;

        return AppDomain.CurrentDomain.GetAssemblies()
            .Where(a => referencedAssemblies.Contains(a.FullName))
            .SelectMany(a => a.GetTypes().Where(x => x.FullName == typeName || x.Name == typeName))
            .FirstOrDefault();
    }

    public static Type? GetFirstMatchingTypeFromCurrentDomainAssembly(string typeName)
    {
        return AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(a => a.GetTypes().Where(x => x.FullName == typeName || x.Name == typeName))
            .FirstOrDefault();
    }
}
