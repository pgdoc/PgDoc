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

namespace PgDoc;

/// <summary>
/// Represents a document composed of a unique ID, a deserialized JSON body and a version number.
/// </summary>
/// <typeparam name="T">The type used to deserialize the JSON body of the document.</typeparam>
public class JsonEntity<T> : IJsonEntity<T>
    where T : class?
{
    public JsonEntity(EntityId id, T entity, long version)
    {
        Id = id;
        Entity = entity;
        Version = version;
    }

    /// <inheritdoc/>
    public EntityId Id { get; }

    /// <inheritdoc/>
    public T Entity { get; }

    /// <inheritdoc/>
    public long Version { get; }
}
