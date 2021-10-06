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

using System.Threading.Tasks;
using Xunit;

namespace PgDoc.Serialization.Tests
{
    public class DocumentStoreExtensionsTests
    {
        private readonly EntityId _entityId = EntityId.New(new EntityType(1));
        private readonly TestDocumentStore _store;

        public DocumentStoreExtensionsTests()
        {
            _store = new TestDocumentStore();
        }

        [Fact]
        public async Task UpdateEntities_Update()
        {
            ByteString version = await _store.UpdateEntities(new JsonEntity<string>(_entityId, "Value", ByteString.Empty));

            JsonEntity<string> entity = await _store.GetEntity<string>(_entityId);

            Assert.Equal(_entityId, entity.Id);
            Assert.Equal("Value", entity.Entity);
            Assert.Equal(version, entity.Version);
        }

        [Fact]
        public async Task UpdateEntities_Check()
        {
            ByteString version = await _store.UpdateDocuments(new Document(_entityId.Value, "'Value'", ByteString.Empty));

            await _store.UpdateEntities(
                new IJsonEntity<object>[0],
                new[] { new JsonEntity<string>(_entityId, null, version) });

            JsonEntity<string> entity = await _store.GetEntity<string>(_entityId);

            Assert.Equal(_entityId, entity.Id);
            Assert.Equal("Value", entity.Entity);
            Assert.Equal(version, entity.Version);
        }

        [Fact]
        public async Task UpdateEntities_Multiple()
        {
            JsonEntity<string> entity1 = JsonEntity<string>.Create("Value", new EntityType(1));
            JsonEntity<int[]> entity2 = JsonEntity<int[]>.Create(new[] { 1, 2, 3 }, new EntityType(2));

            ByteString version = await _store.UpdateEntities(entity1, entity2);

            JsonEntity<string> result1 = await _store.GetEntity<string>(entity1.Id);
            JsonEntity<int[]> result2 = await _store.GetEntity<int[]>(entity2.Id);

            Assert.Equal(entity1.Id, result1.Id);
            Assert.Equal("Value", result1.Entity);
            Assert.Equal(version, result1.Version);
            Assert.Equal(entity2.Id, result2.Id);
            Assert.Equal(new int[] { 1, 2, 3 }, result2.Entity);
            Assert.Equal(version, result2.Version);
        }

        [Fact]
        public async Task GetEntity_Found()
        {
            ByteString version = await _store.UpdateDocuments(new Document(_entityId.Value, "'Value'", ByteString.Empty));

            JsonEntity<string> entity = await _store.GetEntity<string>(_entityId);

            Assert.Equal(_entityId, entity.Id);
            Assert.Equal("Value", entity.Entity);
            Assert.Equal(version, entity.Version);
        }

        [Fact]
        public async Task GetEntity_NotFound()
        {
            JsonEntity<string> entity = await _store.GetEntity<string>(_entityId);

            Assert.Equal(_entityId, entity.Id);
            Assert.Null(entity.Entity);
            Assert.Equal(ByteString.Empty, entity.Version);
        }
    }
}
