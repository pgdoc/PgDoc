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

public static class JsonEntity
{
    /// <summary>
    /// Returns a copy of this <see cref="IJsonEntity{T}"/> object with the same ID and version, but replaces the
    /// body with a new one.
    /// </summary>
    public static IJsonEntity<T> Modify<T>(this IJsonEntity<T> entity, T newValue)
        where T : class?
    {
        return new JsonEntity<T>(entity.Id, newValue, entity.Version);
    }

    /// <summary>
    /// Creates a new <see cref="IJsonEntity{T}"/> object with a new random ID and a version set to zero.
    /// </summary>
    public static IJsonEntity<T> Create<T>(T value, EntityType type)
        where T : class?
    {
        return new JsonEntity<T>(EntityId.New(type), value, 0);
    }

    /// <summary>
    /// Creates a new <see cref="IJsonEntity{T}"/> object with a new random ID and a version set to zero.
    /// </summary>
    public static IJsonEntity<T> Create<T>(T value)
        where T : class?
    {
        return Create(value, EntityType.GetEntityType<T>());
    }

    /// <summary>
    /// Deconstructs the ID, body and version of this <see cref="IJsonEntity{T}"/> object.
    /// </summary>
    public static void Deconstruct<T>(this IJsonEntity<T> source, out EntityId id, out T entity, out long version)
        where T : class?
    {
        id = source.Id;
        entity = source.Entity;
        version = source.Version;
    }
}
