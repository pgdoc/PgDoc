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

namespace PgDoc;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PgDoc.Core;

public class EntityStore
{
    private readonly IDocumentStore _documentStore;
    private readonly IJsonSerializer _jsonSerializer;

    public EntityStore(IDocumentStore documentStore, IJsonSerializer jsonSerializer)
    {
        _documentStore = documentStore ?? throw new ArgumentNullException(nameof(documentStore));
        _jsonSerializer = jsonSerializer ?? throw new ArgumentNullException(nameof(jsonSerializer));
    }

    /// <summary>
    /// Updates atomically the body of multiple documents represented as <see cref="IJsonEntity{T}"/> objects.
    /// </summary>
    /// <exception cref="UpdateConflictException">Thrown when attempting to modify a document using the wrong
    /// base version.</exception>
    public async Task UpdateEntities(
        IEnumerable<IJsonEntity<object>> updatedDocuments,
        IEnumerable<IJsonEntity<object>> checkedDocuments)
    {
        await _documentStore.UpdateDocuments(
            updatedDocuments.Select(_jsonSerializer.ToDocument),
            checkedDocuments.Select(_jsonSerializer.ToDocument));
    }

    /// <summary>
    /// Updates atomically the body of multiple documents represented as <see cref="IJsonEntity{T}"/> objects.
    /// </summary>
    /// <exception cref="UpdateConflictException">Thrown when attempting to modify a document using the wrong
    /// base version.</exception>
    public async Task UpdateEntities(params IJsonEntity<object>[] updatedDocuments)
    {
        await UpdateEntities(updatedDocuments, Array.Empty<IJsonEntity<object>>());
    }

    /// <summary>
    /// Retrieves a document given its ID, represented as a <see cref="JsonEntity{T}"/> object.
    /// </summary>
    public async Task<JsonEntity<T>> GetEntity<T>(EntityId id)
        where T : class
    {
        Document result = await _documentStore.GetDocument(id.Value);
        return _jsonSerializer.FromDocument<T>(result);
    }

    /// <summary>
    /// Returns a new <see cref="BatchBuilder"/> object that can be used to update multiple documents atomically.
    /// </summary>
    public BatchBuilder CreateBatchBuilder()
    {
        return new BatchBuilder(_documentStore, _jsonSerializer);
    }
}
