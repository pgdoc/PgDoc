// Copyright 2016 Flavien Charlon
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

namespace PgDoc.Serialization;

using System.Text.Json;
using System.Text.Json.Serialization;

public class DefaultJsonSerializer : IJsonSerializer
{
    private readonly JsonSerializerOptions _jsonSerializerOptions;

    public DefaultJsonSerializer(JsonSerializerOptions jsonSerializerOptions)
    {
        _jsonSerializerOptions = jsonSerializerOptions;
    }

    /// <inheritdoc />
    public T Deserialize<T>(string json)
    {
        using JsonDocument jsonDocument = JsonDocument.Parse(json);

        T? result = jsonDocument.Deserialize<T>(_jsonSerializerOptions);

        if (result != null)
            return result;
        else
            throw new JsonException("Unable to deserialize JSON.");
    }

    /// <inheritdoc />
    public string Serialize<T>(T value)
    {
        return JsonSerializer.Serialize<T>(value, _jsonSerializerOptions);
    }

    public static JsonSerializerOptions GetDefaultOptions()
    {
        JsonSerializerOptions result = new();
        result.Converters.Add(new UnixTimeConverter());
        result.Converters.Add(new ByteArrayConverter());
        result.Converters.Add(new EntityIdConverter());
        result.Converters.Add(new JsonStringEnumConverter());

        return result;
    }
}
