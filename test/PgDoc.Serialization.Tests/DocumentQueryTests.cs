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

using System.Collections.Generic;
using System.Threading.Tasks;
using Npgsql;
using Xunit;

public class DocumentQueryTests
{
    private readonly DocumentQuery _documentQuery;

    private readonly NpgsqlConnection _connection;
    private readonly SqlDocumentStore _store;
    private readonly EntityStore _entityStore;

    public DocumentQueryTests()
    {
        _connection = new NpgsqlConnection(ConfigurationManager.GetSetting("connection_string"));

        _store = new SqlDocumentStore(_connection);
        _store.Initialize().Wait();

        NpgsqlCommand command = _connection.CreateCommand();
        command.CommandText = @"TRUNCATE TABLE document;";
        command.ExecuteNonQuery();

        DefaultJsonSerializer jsonSerializer = new(DefaultJsonSerializer.GetDefaultSettings());
        _entityStore = new EntityStore(_store, jsonSerializer);
        _documentQuery = new DocumentQuery(jsonSerializer);
    }

    [Fact]
    public async Task ExecuteList_Success()
    {
        JsonEntity<TestObject> entity = JsonEntity<TestObject>.Create(new TestObject() { Value = "abcd" });
        await _entityStore.UpdateEntities(entity);

        _connection.CreateCommand();
        NpgsqlCommand command = _connection.CreateCommand();
        command.CommandText = "SELECT id, body, version FROM document";

        IReadOnlyList<JsonEntity<TestObject>> result = await _documentQuery.ExecuteList<TestObject>(command);

        Assert.Equal(1, result.Count);
        Assert.Equal(entity.Id, result[0].Id);
        Assert.Equal("abcd", result[0].Entity.Value);
        Assert.Equal(1, result[0].Version);
    }

    [JsonEntityType(1)]
    public class TestObject
    {
        public string Value { get; set; }
    }
}
