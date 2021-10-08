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
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Threading.Tasks;
using Npgsql;

namespace PgDoc.Serialization
{
    public static class DocumentQuery
    {
        /// <summary>
        /// Executes a SQL query and converts the result into a list of <see cref="JsonEntity{T}"/> objects. The query
        /// must return the id, body and version columns.
        /// </summary>
        public static async Task<IReadOnlyList<JsonEntity<T>>> Execute<T>(NpgsqlCommand command)
            where T : class
        {
            List<JsonEntity<T>> result = new List<JsonEntity<T>>();

            using (DbDataReader reader = await command.ExecuteReaderAsync(CommandBehavior.Default | CommandBehavior.SingleResult))
            {
                while (await reader.ReadAsync())
                {
                    Document document = new Document(
                        (Guid)reader["id"],
                        (string)reader["body"],
                        (long)reader["version"]);

                    result.Add(JsonEntity<T>.FromDocument(document));
                }
            }

            return result.AsReadOnly();
        }
    }
}
