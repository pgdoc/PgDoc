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

namespace PgDoc.Serialization.Tests;

using System;
using System.Collections.Immutable;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Xunit;

public class DefaultJsonConverterTests
{
    private readonly IJsonConverter _converter;

    private static readonly EntityId _entityId = new(Guid.Parse("000a9d4a-78a1-4534-963a-37f1023a4022"));
    private static readonly byte[] _bytes = new byte[] { 10, 20, 30, 40, 50, 60, 70, 80 };

    public DefaultJsonConverterTests()
    {
        _converter = new DefaultJsonConverter(DefaultJsonConverter.GetDefaultSettings());
    }

    [Fact]
    public void ToJson_Success()
    {
        TestObject testObject = new()
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

        string json = _converter.ToJson(testObject);
        TestObject result = _converter.FromJson<TestObject>(json);

        const string expectedJson = """
            {
                "StringValue": "value",
                "Int64Value": 100,
                "DecimalValue": 100.001,
                "ByteValue": "ChQeKDI8RlA=",
                "NullableDateValue": 1315465504,
                "DateValue": 1275710582,
                "EntityIdValue": "000a9d4a-78a1-4534-963a-37f1023a4022",
                "ListValue": ["a", "b", "c"]
            }
        """;

        Assert.Equal(Regex.Replace(expectedJson, "\\s", ""), json);
        Assert.Equal("value", result.StringValue);
        Assert.Equal(100, result.Int64Value);
        Assert.Equal(100.001m, result.DecimalValue);
        Assert.Equal(_bytes, result.ByteValue);
        Assert.Equal(new DateTime(2010, 6, 5, 4, 3, 2), result.DateValue);
        Assert.Equal(DateTimeKind.Utc, result.DateValue.Kind);
        Assert.Equal(_entityId, result.EntityIdValue);
        Assert.Equal(new string[] { "a", "b", "c" }, result.ListValue);
    }

    [Fact]
    public void ToJson_NullValues()
    {
        TestObject testObject = new();

        string json = _converter.ToJson(testObject);
        TestObject result = _converter.FromJson<TestObject>(json);

        const string expectedJson = """
            {
                "StringValue": null,
                "Int64Value": 0,
                "DecimalValue": 0.0,
                "ByteValue": null,
                "NullableDateValue": null,
                "DateValue": -62135596800,
                "EntityIdValue": null,
                "ListValue": null
            }
        """;

        Assert.Equal(Regex.Replace(expectedJson, "\\s", ""), json);
        Assert.Equal(testObject, result);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(long.MaxValue)]
    [InlineData(long.MinValue)]
    public void ToJson_SerializeInt64(long value)
    {
        TestObject testObject = new()
        {
            Int64Value = value
        };

        string json = _converter.ToJson(testObject);
        TestObject result = _converter.FromJson<TestObject>(json);

        Assert.Equal(testObject, result);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("value")]
    public void ToJson_SerializeString(string value)
    {
        TestObject testObject = new()
        {
            StringValue = value
        };

        string json = _converter.ToJson(testObject);
        TestObject result = _converter.FromJson<TestObject>(json);

        Assert.Equal(testObject, result);
    }

    [Fact]
    public void ToJson_JsonSerializerSettings()
    {
        IJsonConverter converter = new DefaultJsonConverter(
            new JsonSerializerSettings()
            {
                ContractResolver = new DefaultContractResolver()
                {
                    NamingStrategy = new SnakeCaseNamingStrategy()
                },
                NullValueHandling = NullValueHandling.Ignore,
                DefaultValueHandling = DefaultValueHandling.Ignore
            });

        TestObject testObject = new()
        {
            StringValue = "value"
        };

        string json = converter.ToJson(testObject);
        TestObject result = converter.FromJson<TestObject>(json);

        const string expectedJson = """
            {
                "string_value": "value"
            }
        """;

        Assert.Equal(Regex.Replace(expectedJson, "\\s", ""), json);
        Assert.Equal(testObject, result);
    }

    [Theory]
    [InlineData("")]
    [InlineData("{")]
    public void FromJson_Exception(string json)
    {
        Assert.ThrowsAny<JsonException>(
            () => _converter.FromJson<TestObject>(json));
    }

    [JsonEntityType(5)]
    public record TestObject
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
