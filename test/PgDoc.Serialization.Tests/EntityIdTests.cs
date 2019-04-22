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
using Xunit;

namespace PgDoc.Serialization.Tests
{
    public class EntityIdTests
    {
        private readonly string _guid = "d31b6b50-fda9-11e8-b568-0800200c9a66";

        [Theory]
        [InlineData((short)0)]
        [InlineData((short)1)]
        [InlineData((short)-1)]
        [InlineData(short.MaxValue)]
        [InlineData(short.MinValue)]
        [InlineData((short)255)]
        [InlineData((short)256)]
        public void Constructor_Success(short value)
        {
            EntityId entityId = EntityId.New(value);
            Assert.Equal(value, entityId.Type.Value);
        }

        [Fact]
        public void FromString_Success()
        {
            EntityId value1 = EntityId.FromString(new EntityType(1), "a");
            EntityId value2 = EntityId.FromString(new EntityType(1), "b");
            EntityId value3 = EntityId.FromString(new EntityType(2), "b");
            EntityId value4 = EntityId.FromString(new EntityType(1), "b");

            Assert.NotEqual(value1, value2);
            Assert.NotEqual(value1, value3);
            Assert.NotEqual(value2, value3);
            Assert.Equal(value2, value4);
        }

        [Fact]
        public void ToString_Success()
        {
            EntityId value = new EntityId(Guid.Parse(_guid));

            Assert.Equal(_guid, value.ToString());
        }

        [Fact]
        public void Equals_Success()
        {
            EntityId value1 = EntityId.FromString(new EntityType(1), "a");
            EntityId value2 = EntityId.FromString(new EntityType(1), "b");

            Assert.True(value1.Equals(value1));
            Assert.False(value1.Equals(value2));
            Assert.False(value1.Equals("string"));
            Assert.False(value1.Equals(null));
        }

        [Fact]
        public void GetHashCode_Success()
        {
            EntityId value1 = EntityId.FromString(new EntityType(1), "a");
            EntityId value2 = EntityId.FromString(new EntityType(1), "b");
            EntityId value3 = EntityId.FromString(new EntityType(1), "a");

            Assert.Equal(value1.GetHashCode(), value3.GetHashCode());
            Assert.NotEqual(value1.GetHashCode(), value2.GetHashCode());
        }
    }
}
