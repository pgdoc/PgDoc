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
    public class JsonEntityTests
    {
        private static readonly Guid guid = Guid.Parse("f81428a9-0bd9-4d75-95bf-976225f24cf1");

        [Fact]
        public void Constructor_Exception()
        {
            Assert.Throws<ArgumentNullException>(
                () => new JsonEntity<string>(new EntityId(guid), "abc", null));
        }

        [Fact]
        public void FromDocument_Success()
        {
            TestObject testObject = new TestObject()
            {
                StringValue = "value",
                Int64Value = 100,
                DecimalValue = 100.001m,
                ByteValue = ByteString.Parse("abcdef0123456789"),
                DateValue = new DateTime(2010, 6, 5, 4, 3, 2, DateTimeKind.Utc)
            };

            JsonEntity<TestObject> entity = new JsonEntity<TestObject>(new EntityId(guid), testObject, ByteString.Parse("abcd"));

            Document document = entity.AsDocument();
            JsonEntity<TestObject> result = JsonEntity<TestObject>.FromDocument(document);

            Assert.Equal(guid, document.Id);
            Assert.Equal(ByteString.Parse("abcd"), document.Version);
            Assert.Equal(@"{""StringValue"":""value"",""Int64Value"":100,""DecimalValue"":100.001,""ByteValue"":""q83vASNFZ4k="",""DateValue"":1275710582}", document.Body);

            Assert.Equal(guid, result.Id.Value);
            Assert.Equal(ByteString.Parse("abcd"), result.Version);
            Assert.Equal("value", result.Entity.StringValue);
            Assert.Equal(100, result.Entity.Int64Value);
            Assert.Equal(100.001m, result.Entity.DecimalValue);
            Assert.Equal(ByteString.Parse("abcdef0123456789"), result.Entity.ByteValue);
            Assert.Equal(new DateTime(2010, 6, 5, 4, 3, 2), result.Entity.DateValue);
        }

        [Fact]
        public void FromDocument_Null()
        {
            JsonEntity<TestObject> entity = new JsonEntity<TestObject>(new EntityId(guid), null, ByteString.Parse("abcd"));

            Document document = entity.AsDocument();
            JsonEntity<TestObject> result = JsonEntity<TestObject>.FromDocument(document);

            Assert.Equal(guid, document.Id);
            Assert.Equal(ByteString.Parse("abcd"), document.Version);
            Assert.Null(document.Body);

            Assert.Equal(guid, result.Id.Value);
            Assert.Equal(ByteString.Parse("abcd"), result.Version);
            Assert.Null(result.Entity);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(long.MaxValue)]
        [InlineData(long.MinValue)]
        public void FromDocument_SerializeInt64(long value)
        {
            TestObject testObject = new TestObject()
            {
                Int64Value = value
            };

            JsonEntity<TestObject> entity = new JsonEntity<TestObject>(new EntityId(guid), testObject, ByteString.Parse("abcd"));

            Document document = entity.AsDocument();
            JsonEntity<TestObject> result = JsonEntity<TestObject>.FromDocument(document);

            Assert.Equal(value, result.Entity.Int64Value);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("value")]
        public void FromDocument_SerializeString(string value)
        {
            TestObject testObject = new TestObject()
            {
                StringValue = value
            };

            JsonEntity<TestObject> entity = new JsonEntity<TestObject>(new EntityId(guid), testObject, ByteString.Parse("abcd"));

            Document document = entity.AsDocument();
            JsonEntity<TestObject> result = JsonEntity<TestObject>.FromDocument(document);

            Assert.Equal(value, result.Entity.StringValue);
        }

        [Fact]
        public void Create_Success()
        {
            TestObject testObject = new TestObject()
            {
                Int64Value = 100
            };

            JsonEntity<TestObject> result = JsonEntity<TestObject>.Create(testObject, new EntityType(1));

            Assert.Equal(1, result.Id.Type.Value);
            Assert.Equal(testObject, result.Entity);
            Assert.Equal(ByteString.Empty, result.Version);
        }

        [Fact]
        public void Modify_Success()
        {
            TestObject initialObject = new TestObject()
            {
                Int64Value = 100
            };

            TestObject newObject = new TestObject()
            {
                Int64Value = 200
            };

            JsonEntity<TestObject> initialValue = new JsonEntity<TestObject>(new EntityId(guid), initialObject, ByteString.Parse("abcd"));
            JsonEntity<TestObject> result = initialValue.Modify(newObject);

            Assert.Equal(new EntityId(guid), result.Id);
            Assert.Equal(200, result.Entity.Int64Value);
            Assert.Equal(ByteString.Parse("abcd"), result.Version);
        }

        public class TestObject
        {
            public string StringValue { get; set; }

            public long Int64Value { get; set; }

            public decimal DecimalValue { get; set; }

            public ByteString ByteValue { get; set; }

            public DateTime? DateValue { get; set; }
        }
    }
}
