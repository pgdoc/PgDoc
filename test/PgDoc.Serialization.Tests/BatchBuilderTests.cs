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
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace PgDoc.Serialization.Tests
{
    public class BatchBuilderTests
    {
        private static readonly EntityId[] transactionGuids = Enumerable.Range(0, 20).Select(i => EntityId.New(new EntityType(1))).ToArray();
        private static readonly ByteString[] versions = Enumerable.Range(0, 20).Select(i => new ByteString(new byte[] { 255, (byte)i })).ToArray();

        private readonly TestDocumentStore store;
        private readonly BatchBuilder builder;

        public BatchBuilderTests()
        {
            store = new TestDocumentStore();
            builder = new BatchBuilder(store);
        }

        [Fact]
        public async Task Submit_Success()
        {
            Check(transactionGuids[0], ByteString.Empty);
            Modify(transactionGuids[1], ByteString.Empty);

            ByteString version = await builder.Submit();

            Assert.Equal(1, store.Store.Count);
            Assert.Equal(store.Store[transactionGuids[1].Value].Item2, version);
        }

        [Fact]
        public async Task CheckCheck_Success()
        {
            Check(transactionGuids[0], ByteString.Empty);
            Check(transactionGuids[0], ByteString.Empty);

            ByteString version = await builder.Submit();

            Assert.Equal(0, store.Store.Count);
        }

        [Fact]
        public void CheckCheck_Error()
        {
            Check(transactionGuids[0], ByteString.Empty);
            Assert.Throws<InvalidOperationException>(() => Check(transactionGuids[0], versions[0]));
        }

        [Fact]
        public async Task CheckModify_Success()
        {
            Check(transactionGuids[0], ByteString.Empty);
            Modify(transactionGuids[0], ByteString.Empty);

            ByteString version = await builder.Submit();

            Assert.Equal(1, store.Store.Count);
            Assert.Equal(store.Store[transactionGuids[0].Value].Item2, version);
        }

        [Fact]
        public void CheckModify_Error()
        {
            Check(transactionGuids[0], ByteString.Empty);
            Assert.Throws<InvalidOperationException>(() => Modify(transactionGuids[0], versions[0]));
        }

        [Fact]
        public async Task ModifyCheck_Success()
        {
            Modify(transactionGuids[0], ByteString.Empty);
            Check(transactionGuids[0], ByteString.Empty);

            ByteString version = await builder.Submit();

            Assert.Equal(1, store.Store.Count);
            Assert.Equal(store.Store[transactionGuids[0].Value].Item2, version);
        }

        [Fact]
        public void ModifyCheck_Error()
        {
            Modify(transactionGuids[0], ByteString.Empty);
            Assert.Throws<InvalidOperationException>(() => Check(transactionGuids[0], versions[0]));
        }

        [Fact]
        public void ModifyModify_Error()
        {
            Modify(transactionGuids[0], ByteString.Empty);
            Assert.Throws<InvalidOperationException>(() => Modify(transactionGuids[0], ByteString.Empty));
        }

        private void Modify(EntityId entityId, ByteString version)
        {
            this.builder.Modify(new Document(entityId.Value, "{'abc':'def'}", version));
        }

        private void Check(EntityId entityId, ByteString version)
        {
            this.builder.Check(new Document(entityId.Value, "{'abc':'def'}", version));
        }
    }
}
