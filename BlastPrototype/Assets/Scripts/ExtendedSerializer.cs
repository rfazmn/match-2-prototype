using Newtonsoft.Json;

public static class ExtendedSerializer
{
    private static readonly JsonSerializerSettings SerializerSettings = new JsonSerializerSettings
    {
        TypeNameHandling = TypeNameHandling.Auto,
        Formatting = Formatting.None,
        NullValueHandling = NullValueHandling.Ignore,
        DefaultValueHandling = DefaultValueHandling.Ignore,
    };

    public static string Serialize<T>(this T source)
    {
        return JsonConvert.SerializeObject(source, SerializerSettings);
    }

    public static T Deserialize<T>(this string source)
    {
        return JsonConvert.DeserializeObject<T>(source, SerializerSettings);
    }
}