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
using System.Collections.Immutable;
using System.Text.RegularExpressions;
using Xunit;

namespace PgDoc.Serialization.Tests
{
    public class JsonEntityTests
    {
        private static readonly Guid _guid = Guid.Parse("f81428a9-0bd9-4d75-95bf-976225f24cf1");
        private static readonly EntityId _entityId = new EntityId(Guid.Parse("000a9d4a-78a1-4534-963a-37f1023a4022"));
        private static readonly byte[] _bytes = new byte[] { 10, 20, 30, 40, 50, 60, 70, 80 };

        [Fact]
        public void FromDocument_Success()
        {
            TestObject testObject = new TestObject()
            {
                StringValue = "value",
                Int64Value = 100,
                DecimalValue = 100.001m,
                ByteValue = _bytes,
                DateValue = new DateTime(2010, 6, 5, 4, 3, 2, DateTimeKind.Utc),
                NullableDateValue = new DateTime(2011, 9, 8, 7, 5, 4, DateTimeKind.Utc),
                EntityIdValue = _entityId,
                ListValue = ImmutableList<string>.Empty.Add("a").Add("b").Add("c")
            };

            JsonEntity<TestObject> entity = new JsonEntity<TestObject>(new EntityId(_guid), testObject, 10);

            Document document = entity.AsDocument();
            JsonEntity<TestObject> result = JsonEntity<TestObject>.FromDocument(document);

            const string expectedJson = @"{
                ""StringValue"":""value"",
                ""Int64Value"":100,
                ""DecimalValue"":100.001,
                ""ByteValue"":""ChQeKDI8RlA="",
                ""NullableDateValue"":1315465504,
                ""DateValue"":1275710582,
                ""EntityIdValue"":""000a9d4a-78a1-4534-963a-37f1023a4022"",
                ""ListValue"":[""a"",""b"",""c""]
                }";
            Assert.Equal(_guid, document.Id);
            Assert.Equal(10, document.Version);
            Assert.Equal(Regex.Replace(expectedJson, "\\s", ""), document.Body);

            Assert.Equal(_guid, result.Id.Value);
            Assert.Equal(10, result.Version);
            Assert.Equal("value", result.Entity.StringValue);
            Assert.Equal(100, result.Entity.Int64Value);
            Assert.Equal(100.001m, result.Entity.DecimalValue);
            Assert.Equal(_bytes, result.Entity.ByteValue);
            Assert.Equal(new DateTime(2010, 6, 5, 4, 3, 2), result.Entity.DateValue);
            Assert.Equal(DateTimeKind.Utc, result.Entity.DateValue.Kind);
            Assert.Equal(_entityId, result.Entity.EntityIdValue);
            Assert.Equal(new string[] { "a", "b", "c" }, result.Entity.ListValue);
        }

        [Fact]
        public void FromDocument_NullDocument()
        {
            JsonEntity<TestObject> entity = new JsonEntity<TestObject>(new EntityId(_guid), null, 10);

            Document document = entity.AsDocument();
            JsonEntity<TestObject> result = JsonEntity<TestObject>.FromDocument(document);

            Assert.Equal(_guid, document.Id);
            Assert.Equal(10, document.Version);
            Assert.Null(document.Body);

            Assert.Equal(_guid, result.Id.Value);
            Assert.Equal(10, result.Version);
            Assert.Null(result.Entity);
        }

        [Fact]
        public void FromDocument_NullValues()
        {
            TestObject testObject = new TestObject();

            JsonEntity<TestObject> entity = new JsonEntity<TestObject>(new EntityId(_guid), testObject, 10);

            Document document = entity.AsDocument();
            JsonEntity<TestObject> result = JsonEntity<TestObject>.FromDocument(document);

            const string expectedJson = @"{
                ""StringValue"":null,
                ""Int64Value"":0,
                ""DecimalValue"":0.0,
                ""ByteValue"":null,
                ""NullableDateValue"":null,
                ""DateValue"":-62135596800,
                ""EntityIdValue"":null,
                ""ListValue"":null
                }";
            Assert.Equal(_guid, document.Id);
            Assert.Equal(10, document.Version);
            Assert.Equal(Regex.Replace(expectedJson, "\\s", ""), document.Body);

            Assert.Equal(_guid, result.Id.Value);
            Assert.Equal(10, result.Version);
            Assert.Null(result.Entity.StringValue);
            Assert.Null(result.Entity.ByteValue);
            Assert.Null(result.Entity.NullableDateValue);
            Assert.Null(result.Entity.EntityIdValue);
            Assert.Null(result.Entity.ListValue);
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

            JsonEntity<TestObject> entity = new JsonEntity<TestObject>(new EntityId(_guid), testObject, 10);

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

            JsonEntity<TestObject> entity = new JsonEntity<TestObject>(new EntityId(_guid), testObject, 10);

            Document document = entity.AsDocument();
            JsonEntity<TestObject> result = JsonEntity<TestObject>.FromDocument(document);

            Assert.Equal(value, result.Entity.StringValue);
        }

        [Fact]
        public void Create_ExplicitType()
        {
            TestObject testObject = new TestObject()
            {
                Int64Value = 100
            };

            JsonEntity<TestObject> result = JsonEntity<TestObject>.Create(testObject, new EntityType(1));

            Assert.Equal(1, result.Id.Type.Value);
            Assert.Equal(testObject, result.Entity);
            Assert.Equal(0, result.Version);
        }

        [Fact]
        public void Create_UseAttribute()
        {
            TestObject testObject = new TestObject()
            {
                Int64Value = 100
            };

            JsonEntity<TestObject> result = JsonEntity<TestObject>.Create(testObject);

            Assert.Equal(5, result.Id.Type.Value);
            Assert.Equal(testObject, result.Entity);
            Assert.Equal(0, result.Version);
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

            JsonEntity<TestObject> initialValue = new JsonEntity<TestObject>(new EntityId(_guid), initialObject, 10);
            JsonEntity<TestObject> result = initialValue.Modify(newObject);

            Assert.Equal(new EntityId(_guid), result.Id);
            Assert.Equal(200, result.Entity.Int64Value);
            Assert.Equal(10, result.Version);
        }

        [Fact]
        public void Deconstruct_Success()
        {
            TestObject testObject = new TestObject()
            {
                StringValue = "abcd"
            };

            JsonEntity<TestObject> jsonEntity = new JsonEntity<TestObject>(new EntityId(_guid), testObject, 10);

            (EntityId id, TestObject entity, long version) = jsonEntity;

            Assert.Equal(new EntityId(_guid), id);
            Assert.Equal("abcd", entity.StringValue);
            Assert.Equal(10, version);
        }

        [JsonEntityType(5)]
        public class TestObject
        {
            public string StringValue { get; set; }

            public long Int64Value { get; set; }

            public decimal DecimalValue { get; set; }

            public byte[] ByteValue { get; set; }

            public DateTime? NullableDateValue { get; set; }

            public DateTime DateValue { get; set; }

            public EntityId EntityIdValue { get; set; }

            public ImmutableList<string> ListValue { get; set; }
        }
    }
}
