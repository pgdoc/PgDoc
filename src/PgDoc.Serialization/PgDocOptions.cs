namespace PgDoc.Serialization;

using Newtonsoft.Json;

public class PgDocOptions
{
    public string? ConnectionString { get; set; }

    public JsonSerializerSettings JsonSerializerSettings { get; set; } = DefaultJsonSerializer.GetDefaultSettings();
}
