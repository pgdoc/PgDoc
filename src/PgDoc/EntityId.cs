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
using System.Security.Cryptography;
using System.Text;
using System.Threading;

/// <summary>
/// Represents a <see cref="Guid"/> object whose first 32 bits are used to represent an entity type.
/// </summary>
public class EntityId : IEquatable<EntityId?>
{
    private static readonly ThreadLocal<RandomNumberGenerator> _random =
        new(() => RandomNumberGenerator.Create());

    public EntityId(Guid id)
    {
        Value = id;
        byte[] byteArray = id.ToByteArray();
        Type = new EntityType((byteArray[3] << 24) | (byteArray[2] << 16) | (byteArray[1] << 8) | byteArray[0]);
    }

    /// <summary>
    /// Gets the <see cref="Guid"/> representation of this <see cref="EntityId"/> object.
    /// </summary>
    public Guid Value { get; }

    /// <summary>
    /// Gets the type of the entity represented by this <see cref="EntityId"/> object.
    /// </summary>
    public EntityType Type { get; }

    /// <summary>
    /// Generates a random <see cref="EntityId"/> value with the specified entity type.
    /// </summary>
    public static EntityId New(EntityType type)
    {
        return New(type.Value);
    }

    /// <summary>
    /// Generates a random <see cref="EntityId"/> value with the specified entity type.
    /// </summary>
    public static EntityId New(int type)
    {
        byte[] data = new byte[16];
        _random.Value.GetBytes(data);

        data[0] = (byte)(type & 0xFF);
        data[1] = (byte)((type >> 8) & 0xFF);
        data[2] = (byte)((type >> 16) & 0xFF);
        data[3] = (byte)((type >> 24) & 0xFF);

        return new EntityId(new Guid(data));
    }

    /// <summary>
    /// Parses an <see cref="EntityId"/> object from a string value.
    /// </summary>
    public static EntityId Parse(string input)
    {
        return new EntityId(Guid.Parse(input));
    }

    /// <summary>
    /// Returns a copy of this <see cref="EntityId"/> object with a different entity type.
    /// </summary>
    public EntityId WithType(EntityType type)
    {
        byte[] data = Value.ToByteArray();

        data[0] = (byte)(type.Value & 0xFF);
        data[1] = (byte)((type.Value >> 8) & 0xFF);
        data[2] = (byte)((type.Value >> 16) & 0xFF);
        data[3] = (byte)((type.Value >> 24) & 0xFF);

        return new EntityId(new Guid(data));
    }

    /// <summary>
    /// Returns a copy of this <see cref="EntityId"/> object with a different entity type.
    /// </summary>
    public EntityId WithType<T>()
    {
        return WithType(EntityType.GetEntityType<T>());
    }

    /// <summary>
    /// Generates an <see cref="EntityId"/> object from a string value and a type.
    /// </summary>
    /// <param name="type"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public static EntityId FromString(EntityType type, string value)
    {
        using (MD5 md5 = MD5.Create())
        {
            byte[] data = md5.ComputeHash(Encoding.UTF8.GetBytes(value));
            return new EntityId(new Guid(data)).WithType(type);
        }
    }

    public bool Equals(EntityId? other)
    {
        return other != null && Value.Equals(other.Value);
    }

    public override bool Equals(object obj)
    {
        return Equals(obj as EntityId);
    }

    public override int GetHashCode()
    {
        return Value.GetHashCode();
    }

    public override string ToString()
    {
        return Value.ToString();
    }
}
