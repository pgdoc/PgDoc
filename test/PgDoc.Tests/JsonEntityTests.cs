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
using Xunit;

public class JsonEntityTests
{
    private static readonly Guid _guid = Guid.Parse("f81428a9-0bd9-4d75-95bf-976225f24cf1");

    [Fact]
    public void Create_ExplicitType()
    {
        TestObject testObject = new()
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
        TestObject testObject = new()
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
        TestObject initialObject = new()
        {
            Int64Value = 100
        };

        TestObject newObject = new()
        {
            Int64Value = 200
        };

        JsonEntity<TestObject> initialValue = new(new EntityId(_guid), initialObject, 10);
        JsonEntity<TestObject> result = initialValue.Modify(newObject);

        Assert.Equal(new EntityId(_guid), result.Id);
        Assert.Equal(200, result.Entity.Int64Value);
        Assert.Equal(10, result.Version);
    }

    [Fact]
    public void Deconstruct_Success()
    {
        TestObject testObject = new()
        {
            StringValue = "abcd"
        };

        JsonEntity<TestObject> jsonEntity = new(new EntityId(_guid), testObject, 10);

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
    }
}
