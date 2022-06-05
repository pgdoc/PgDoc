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

using System;
using System.Collections.Concurrent;
using System.Linq;

/// <summary>
/// Represents a value that identifies the type of a JSON entity.
/// </summary>
public readonly struct EntityType : IEquatable<EntityType>
{
    private static readonly ConcurrentDictionary<Type, EntityType> _entityTypeCache = new();

    public EntityType(int value)
    {
        Value = value;
    }

    public int Value { get; }

    public static EntityType GetEntityType<T>()
    {
        if (!_entityTypeCache.TryGetValue(typeof(T), out EntityType result))
        {
            object[] customAttributes = typeof(T).GetCustomAttributes(typeof(JsonEntityTypeAttribute), true);

            JsonEntityTypeAttribute attribute = customAttributes.OfType<JsonEntityTypeAttribute>().FirstOrDefault();

            if (attribute == null)
            {
                throw new ArgumentException(
                    $"The type {typeof(T).Name} does not have a JsonEntityType attribute.",
                    nameof(T));
            }

            result = new EntityType(attribute.EntityType);

            _entityTypeCache.TryAdd(typeof(T), result);
        }

        return result;
    }

    public bool Equals(EntityType other)
    {
        return Value == other.Value;
    }

    public override bool Equals(object other)
    {
        return (other is EntityType otherEntityType) && Equals(otherEntityType);
    }

    public override int GetHashCode()
    {
        return Value.GetHashCode();
    }

    public static bool operator ==(EntityType left, EntityType right)
    {
        return left.Value == right.Value;
    }

    public static bool operator !=(EntityType left, EntityType right)
    {
        return left.Value != right.Value;
    }
}
