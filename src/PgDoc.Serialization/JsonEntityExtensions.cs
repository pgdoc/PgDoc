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

using Newtonsoft.Json;

public static class JsonEntityExtensions
{
    /// <summary>
    /// Converts a <see cref="IJsonEntity{T}"/> object to a <see cref="Document"/> object by serializing its body
    /// to JSON.
    /// </summary>
    public static Document AsDocument<T>(this IJsonEntity<T> jsonEntity)
        where T : class
    {
        return new Document(
            jsonEntity.Id.Value,
            jsonEntity.Entity == null ? null : JsonConvert.SerializeObject(jsonEntity.Entity, JsonSettings.Settings),
            jsonEntity.Version);
    }
}
