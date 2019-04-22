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
        private static readonly EntityId[] _entityIds = Enumerable.Range(0, 20).Select(i => EntityId.New(new EntityType(1))).ToArray();
        private static readonly ByteString[] _versions = Enumerable.Range(0, 20).Select(i => new ByteString(new byte[] { 255, (byte)i })).ToArray();

        private readonly TestDocumentStore _store;
        private readonly BatchBuilder _builder;

        public BatchBuilderTests()
        {
            _store = new TestDocumentStore();
            _builder = new BatchBuilder(_store);
        }

        [Fact]
        public void Constructor_Exception()
        {
            Assert.Throws<ArgumentNullException>(
                () => new BatchBuilder(null));
        }

        [Fact]
        public async Task Submit_Success()
        {
            Check(_entityIds[0], ByteString.Empty);
            Modify(_entityIds[1], ByteString.Empty);

            ByteString version = await _builder.Submit();

            Assert.Equal(1, _store.Store.Count);
            Assert.Equal(_store.Store[_entityIds[1].Value].Item2, version);
        }

        [Fact]
        public async Task MultipleModify_Error()
        {
            _builder.Modify(new Document(_entityIds[0].Value, "{'abc':'def'}", ByteString.Empty));

            Assert.Throws<InvalidOperationException>(() =>
                _builder.Modify(
                    new Document(_entityIds[1].Value, "{'abc':'def'}", _versions[0]),
                    new Document(_entityIds[0].Value, "{'abc':'def'}", _versions[1])));

            // The modification of the other document should not occur and no conflict should be raised by the data store
            ByteString version = await _builder.Submit();

            Assert.Equal(1, _store.Store.Count);
            Assert.Equal(_store.Store[_entityIds[0].Value].Item2, version);
        }

        [Fact]
        public async Task MultipleCheck_Error()
        {
            _builder.Modify(new Document(_entityIds[0].Value, "{'abc':'def'}", ByteString.Empty));

            Assert.Throws<InvalidOperationException>(() =>
                _builder.Check(
                    new Document(_entityIds[1].Value, "{'abc':'def'}", _versions[0]),
                    new Document(_entityIds[0].Value, "{'abc':'def'}", _versions[1])));

            // The version check on the other document should not occur and no conflict should be raised by the data store
            ByteString version = await _builder.Submit();

            Assert.Equal(1, _store.Store.Count);
            Assert.Equal(_store.Store[_entityIds[0].Value].Item2, version);
        }

        [Fact]
        public async Task CheckCheck_Success()
        {
            Check(_entityIds[0], ByteString.Empty);
            Check(_entityIds[0], ByteString.Empty);

            _ = await _builder.Submit();

            Assert.Equal(0, _store.Store.Count);
        }

        [Fact]
        public void CheckCheck_Error()
        {
            Check(_entityIds[0], ByteString.Empty);

            InvalidOperationException exception = Assert.Throws<InvalidOperationException>(() => Check(_entityIds[0], _versions[0]));
            Assert.Equal($"A different version of document {_entityIds[0].Value} is already being checked.", exception.Message);
        }

        [Fact]
        public async Task CheckModify_Success()
        {
            Check(_entityIds[0], ByteString.Empty);
            Modify(_entityIds[0], ByteString.Empty);

            ByteString version = await _builder.Submit();

            Assert.Equal(1, _store.Store.Count);
            Assert.Equal(_store.Store[_entityIds[0].Value].Item2, version);
        }

        [Fact]
        public void CheckModify_Error()
        {
            Check(_entityIds[0], ByteString.Empty);

            InvalidOperationException exception = Assert.Throws<InvalidOperationException>(() => Modify(_entityIds[0], _versions[0]));
            Assert.Equal($"A different version of document {_entityIds[0].Value} is already being checked.", exception.Message);
        }

        [Fact]
        public async Task ModifyCheck_Success()
        {
            Modify(_entityIds[0], ByteString.Empty);
            Check(_entityIds[0], ByteString.Empty);

            ByteString version = await _builder.Submit();

            Assert.Equal(1, _store.Store.Count);
            Assert.Equal(_store.Store[_entityIds[0].Value].Item2, version);
        }

        [Fact]
        public void ModifyCheck_Error()
        {
            Modify(_entityIds[0], ByteString.Empty);

            InvalidOperationException exception = Assert.Throws<InvalidOperationException>(() => Check(_entityIds[0], _versions[0]));
            Assert.Equal($"A different version of document {_entityIds[0].Value} is already being modified.", exception.Message);
        }

        [Fact]
        public void ModifyModify_Error()
        {
            Modify(_entityIds[0], ByteString.Empty);

            InvalidOperationException exception = Assert.Throws<InvalidOperationException>(() => Modify(_entityIds[0], ByteString.Empty));
            Assert.Equal($"Document {_entityIds[0].Value} is already being modified.", exception.Message);
        }

        private void Modify(EntityId entityId, ByteString version)
        {
            _builder.Modify(new JsonEntity<TestObject>(entityId, new TestObject("abc"), version));
        }

        private void Check(EntityId entityId, ByteString version)
        {
            _builder.Check(new JsonEntity<TestObject>(entityId, new TestObject("def"), version));
        }

        public class TestObject
        {
            public TestObject(string value)
            {
                Value = value;
            }

            public string Value { get; set; }
        }
    }
}
