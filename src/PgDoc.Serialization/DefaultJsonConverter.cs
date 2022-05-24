﻿// Copyright 2016 Flavien Charlon
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

using System.Collections.Generic;
using Newtonsoft.Json;

public class DefaultJsonConverter : IJsonConverter
{
    private readonly JsonSerializerSettings _jsonSerializerSettings;

    public DefaultJsonConverter(JsonSerializerSettings jsonSerializerSettings)
    {
        _jsonSerializerSettings = jsonSerializerSettings;
    }

    /// <inheritdoc />
    public T FromJson<T>(string json)
    {
        T? result = JsonConvert.DeserializeObject<T>(json, _jsonSerializerSettings);

        if (result != null)
            return result;
        else
            throw new JsonException("Unable to deserialize JSON.");
    }

    /// <inheritdoc />
    public string ToJson<T>(T value)
    {
        return JsonConvert.SerializeObject(value, _jsonSerializerSettings);
    }

    public static JsonSerializerSettings GetDefaultSettings()
    {
        List<JsonConverter> converters = new()
        {
            new UnixTimeConverter(),
            new ByteArrayConverter(),
            new EntityIdConverter()
        };

        return new JsonSerializerSettings()
        {
            Converters = converters,
            Formatting = Formatting.None
        };
    }
}
