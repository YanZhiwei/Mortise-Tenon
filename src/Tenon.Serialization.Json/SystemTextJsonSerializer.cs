using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using Tenon.Serialization.Abstractions;
using SystemJsonSerializer = System.Text.Json.JsonSerializer;

namespace Tenon.Serialization.Json;

public sealed class SystemTextJsonSerializer(JsonSerializerOptions? jsonSerializerOptions = null) : ISerializer
{
    public static readonly JsonSerializerOptions DefaultOptions;
    private readonly JsonSerializerOptions _jsonSerializerOption = jsonSerializerOptions ?? DefaultOptions;

    static SystemTextJsonSerializer()
    {
        DefaultOptions = new JsonSerializerOptions
        {
            Converters =
            {
                new DateTimeConverter(),
                new DateTimeNullableConverter(),
                new JsonStringEnumConverter()
            },
            ReadCommentHandling = JsonCommentHandling.Skip,
            PropertyNameCaseInsensitive = true,
            DictionaryKeyPolicy = JsonNamingPolicy.CamelCase,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            WriteIndented = true
        };
    }

    public byte[] Serialize<T>(T value)
    {
        return SystemJsonSerializer.SerializeToUtf8Bytes(value, _jsonSerializerOption);
    }

    public string SerializeObject(object value)
    {
        return SystemJsonSerializer.Serialize(value, _jsonSerializerOption);
    }

    public T Deserialize<T>(byte[] bytes)
    {
        return SystemJsonSerializer.Deserialize<T>(bytes, _jsonSerializerOption);
    }

    public T DeserializeObject<T>(string value)
    {
        return SystemJsonSerializer.Deserialize<T>(value, _jsonSerializerOption);
    }
}