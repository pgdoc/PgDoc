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
        public async Task GetEntity_Found()
        {
            ByteString version = await _store.UpdateDocument(_entityId.Value, "'Value'", ByteString.Empty);

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
