using System.Text.Json;

namespace FitApp.Application.Features.Sync;

internal static class SyncJson
{
    public static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web);

    public static T Deserialize<T>(JsonElement? payload)
    {
        if (payload is null)
            throw new InvalidOperationException("Payload boş olamaz.");
        return payload.Value.Deserialize<T>(Options)
            ?? throw new InvalidOperationException("Payload çözümlenemedi.");
    }

    public static JsonElement ToElement<T>(T value) =>
        JsonSerializer.SerializeToElement(value, Options);
}
