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

namespace PgDoc.Serialization;

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Npgsql;

public class DocumentQuery
{
    private readonly IJsonConverter _jsonConverter;

    public DocumentQuery(IJsonConverter jsonConverter)
    {
        _jsonConverter = jsonConverter;
    }

    /// <summary>
    /// Executes a SQL query and converts the result into an asynchronous stream of <see cref="Document"/> objects.
    /// The query must return the id, body and version columns.
    /// </summary>
    public async IAsyncEnumerable<Document> Execute(
        NpgsqlCommand command,
        [EnumeratorCancellation] CancellationToken cancel = default)
    {
        using (DbDataReader reader = await command.ExecuteReaderAsync(
            CommandBehavior.Default | CommandBehavior.SingleResult,
            cancel))
        {
            while (await reader.ReadAsync(cancel))
            {
                Document document = new(
                    (Guid)reader["id"],
                    (string)reader["body"],
                    (long)reader["version"]);

                yield return document;
            }
        }
    }

    /// <summary>
    /// Executes a SQL query and converts the result into an asynchronous stream of <see cref="JsonEntity{T}"/>
    /// objects. The query must return the id, body and version columns.
    /// </summary>
    public IAsyncEnumerable<JsonEntity<T>> Execute<T>(
        NpgsqlCommand command,
        CancellationToken cancel = default)
        where T : class
    {
        return Execute(command, cancel).Select(_jsonConverter.FromDocument<T>);
    }

    /// <summary>
    /// Executes a SQL query and converts the result into a list of <see cref="JsonEntity{T}"/> objects. The query
    /// must return the id, body and version columns.
    /// </summary>
    public async Task<IReadOnlyList<JsonEntity<T>>> ExecuteList<T>(
        NpgsqlCommand command,
        CancellationToken cancel = default)
        where T : class
    {
        return (await Execute<T>(command, cancel).ToListAsync(cancel)).AsReadOnly();
    }
}
