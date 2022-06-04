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
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PgDoc.Core;

public class TestDocumentStore : IDocumentStore
{
    public IDictionary<Guid, Tuple<string, long>> Store { get; } = new Dictionary<Guid, Tuple<string, long>>();

    public bool Initialized { get; private set; } = false;

    public Task Initialize()
    {
        Initialized = true;
        return Task.FromResult(0);
    }

    public Task<IReadOnlyList<Document>> GetDocuments(IEnumerable<Guid> ids)
    {
        return Task.FromResult((IReadOnlyList<Document>)ids.Select(id =>
        {
            if (Store.TryGetValue(id, out Tuple<string, long> value))
                return new Document(id, value.Item1, value.Item2);
            else
                return new Document(id, null, 0);
        })
        .ToList()
        .AsReadOnly());
    }

    public async Task UpdateDocuments(IEnumerable<Document> updatedDocuments, IEnumerable<Document> checkedDocuments)
    {
        foreach (Document document in updatedDocuments.Concat(checkedDocuments))
        {
            IReadOnlyList<Document> existing = await GetDocuments(new[] { document.Id });

            if (!existing[0].Version.Equals(document.Version))
                throw new UpdateConflictException(document.Id, document.Version);
        }

        foreach (Document document in updatedDocuments)
        {
            Store[document.Id] = Tuple.Create(document.Body, document.Version + 1);
        }
    }

    public void Dispose()
    { }
}
