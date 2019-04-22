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

using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading;

namespace PgDoc.Serialization
{
    public class EntityId : IEquatable<EntityId?>
    {
        private static readonly ThreadLocal<RandomNumberGenerator> _random =
            new ThreadLocal<RandomNumberGenerator>(() => RandomNumberGenerator.Create());

        public EntityId(Guid id)
        {
            Value = id;
            byte[] byteArray = id.ToByteArray();
            Type = new EntityType((short)((byteArray[3] << 8) | byteArray[2]));
        }

        public Guid Value { get; }

        public EntityType Type { get; }

        public static EntityId New(EntityType type)
        {
            return New(type.Value);
        }

        public static EntityId New(short type)
        {
            byte[] data = new byte[16];
            _random.Value.GetBytes(data);

            data[2] = (byte)(type & 0xFF);
            data[3] = (byte)(type >> 8);

            return new EntityId(new Guid(data));
        }

        public EntityId WithType(EntityType type)
        {
            byte[] data = Value.ToByteArray();

            data[2] = (byte)(type.Value & 0xFF);
            data[3] = (byte)(type.Value >> 8);

            return new EntityId(new Guid(data));
        }

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
}
