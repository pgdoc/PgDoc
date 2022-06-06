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

namespace PgDoc.Tests;

using System;
using System.Text.RegularExpressions;
using PgDoc.Core;
using Xunit;

public class JsonSerializerExtensionsTests
{
    private readonly IJsonSerializer _serializer;

    private static readonly Guid _guid = Guid.Parse("f81428a9-0bd9-4d75-95bf-976225f24cf1");

    public JsonSerializerExtensionsTests()
    {
        _serializer = new DefaultJsonSerializer(DefaultJsonSerializer.GetDefaultOptions());
    }

    [Fact]
    public void FromDocument_NonNullBody()
    {
        TestObject testObject = new()
        {
            StringValue = "value"
        };

        IJsonEntity<TestObject> entity = new JsonEntity<TestObject>(new EntityId(_guid), testObject, 10);

        Document document = _serializer.ToDocument(entity);
        IJsonEntity<TestObject?> result = _serializer.FromDocument<TestObject>(document);

        const string expectedJson = """
            {
                "StringValue": "value"
            }
        """;

        Assert.Equal(_guid, document.Id);
        Assert.Equal(10, document.Version);
        Assert.Equal(Regex.Replace(expectedJson, "\\s", ""), document.Body);

        Assert.Equal(_guid, result.Id.Value);
        Assert.Equal(10, result.Version);
        Assert.Equal("value", result.Entity?.StringValue);
    }

    [Fact]
    public void FromDocument_NullBody()
    {
        IJsonEntity<TestObject?> entity = new JsonEntity<TestObject?>(new EntityId(_guid), null, 10);

        Document document = _serializer.ToDocument(entity);
        IJsonEntity<TestObject?> result = _serializer.FromDocument<TestObject>(document);

        Assert.Equal(_guid, document.Id);
        Assert.Equal(10, document.Version);
        Assert.Null(document.Body);

        Assert.Equal(_guid, result.Id.Value);
        Assert.Equal(10, result.Version);
        Assert.Null(result.Entity);
    }

    [Fact]
    public void FromExistingDocument_NonNullBody()
    {
        const string json = """
            {
                "StringValue": "value"
            }
        """;

        Document document = new(_guid, json, 10);
        IJsonEntity<TestObject> result = _serializer.FromExistingDocument<TestObject>(document);

        Assert.Equal(_guid, result.Id.Value);
        Assert.Equal(10, result.Version);
        Assert.Equal("value", result.Entity.StringValue);
    }

    [Fact]
    public void FromExistingDocument_NullBody()
    {
        Document document = new(_guid, null, 10);

        Assert.Throws<ArgumentException>(() =>
            _serializer.FromExistingDocument<TestObject>(document));
    }

    [JsonEntityType(5)]
    public class TestObject
    {
        public string? StringValue { get; set; }
    }
}
