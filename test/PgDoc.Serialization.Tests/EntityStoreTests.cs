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

using System.Threading.Tasks;
using Xunit;

public class EntityStoreTests
{
    private readonly EntityId _entityId = EntityId.New(new EntityType(1));
    private readonly TestDocumentStore _store;
    private readonly EntityStore _entityStore;

    public EntityStoreTests()
    {
        _store = new TestDocumentStore();
        _entityStore = new EntityStore(_store, new DefaultJsonConverter(new JsonConverterSettings()));
    }

    [Fact]
    public async Task UpdateEntities_Update()
    {
        await _entityStore.UpdateEntities(new JsonEntity<string>(_entityId, "Value", 0));

        JsonEntity<string> entity = await _entityStore.GetEntity<string>(_entityId);

        Assert.Equal(_entityId, entity.Id);
        Assert.Equal("Value", entity.Entity);
        Assert.Equal(1, entity.Version);
    }

    [Fact]
    public async Task UpdateEntities_Check()
    {
        await _store.UpdateDocuments(new Document(_entityId.Value, "'Value'", 0));

        await _entityStore.UpdateEntities(
            new IJsonEntity<object>[0],
            new[] { new JsonEntity<string>(_entityId, null, 1) });

        JsonEntity<string> entity = await _entityStore.GetEntity<string>(_entityId);

        Assert.Equal(_entityId, entity.Id);
        Assert.Equal("Value", entity.Entity);
        Assert.Equal(1, entity.Version);
    }

    [Fact]
    public async Task UpdateEntities_Multiple()
    {
        JsonEntity<string> entity1 = JsonEntity<string>.Create("Value", new EntityType(1));
        JsonEntity<int[]> entity2 = JsonEntity<int[]>.Create(new[] { 1, 2, 3 }, new EntityType(2));

        await _entityStore.UpdateEntities(entity1, entity2);

        JsonEntity<string> result1 = await _entityStore.GetEntity<string>(entity1.Id);
        JsonEntity<int[]> result2 = await _entityStore.GetEntity<int[]>(entity2.Id);

        Assert.Equal(entity1.Id, result1.Id);
        Assert.Equal("Value", result1.Entity);
        Assert.Equal(1, result1.Version);
        Assert.Equal(entity2.Id, result2.Id);
        Assert.Equal(new int[] { 1, 2, 3 }, result2.Entity);
        Assert.Equal(1, result2.Version);
    }

    [Fact]
    public async Task GetEntity_Found()
    {
        await _store.UpdateDocuments(new Document(_entityId.Value, "'Value'", 0));

        JsonEntity<string> entity = await _entityStore.GetEntity<string>(_entityId);

        Assert.Equal(_entityId, entity.Id);
        Assert.Equal("Value", entity.Entity);
        Assert.Equal(1, entity.Version);
    }

    [Fact]
    public async Task GetEntity_NotFound()
    {
        JsonEntity<string> entity = await _entityStore.GetEntity<string>(_entityId);

        Assert.Equal(_entityId, entity.Id);
        Assert.Null(entity.Entity);
        Assert.Equal(0, entity.Version);
    }
}
