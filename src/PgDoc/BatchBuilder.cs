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
using System.Threading.Tasks;
using PgDoc.Core;

/// <summary>
/// Represents a batch of updates to be performed atomically.
/// </summary>
public class BatchBuilder
{
    private readonly IDocumentStore _documentStore;
    private readonly IJsonSerializer _jsonSerializer;
    private readonly Dictionary<Guid, Document> _checkedDocuments = new();
    private readonly Dictionary<Guid, Document> _modifiedDocuments = new();

    public BatchBuilder(IDocumentStore documentStore, IJsonSerializer jsonSerializer)
    {
        _documentStore = documentStore ?? throw new ArgumentNullException(nameof(documentStore));
        _jsonSerializer = jsonSerializer ?? throw new ArgumentNullException(nameof(jsonSerializer));
    }

    public void Check(params Document[] documents)
    {
        List<Document> addCheckDocuments = new();

        // Validation phase
        foreach (Document document in documents)
        {
            if (_checkedDocuments.TryGetValue(document.Id, out Document existingDocument))
            {
                if (existingDocument.Version.Equals(document.Version))
                    continue;
                else
                    throw new InvalidOperationException($"A different version of document {document.Id} is already being checked.");
            }
            else if (_modifiedDocuments.TryGetValue(document.Id, out existingDocument))
            {
                if (existingDocument.Version.Equals(document.Version))
                    continue;
                else
                    throw new InvalidOperationException($"A different version of document {document.Id} is already being modified.");
            }

            addCheckDocuments.Add(document);
        }

        foreach (Document document in addCheckDocuments)
            _checkedDocuments.Add(document.Id, document);
    }

    public void Check<T>(IJsonEntity<T> document)
    {
        Check(_jsonSerializer.ToDocument(document));
    }

    public void Modify(params Document[] documents)
    {
        List<Guid> removeCheckedDocuments = new();
        List<Document> addModifyDocuments = new();

        // Validation phase
        foreach (Document document in documents)
        {
            if (_checkedDocuments.TryGetValue(document.Id, out Document existingDocument))
            {
                if (existingDocument.Version.Equals(document.Version))
                    removeCheckedDocuments.Add(document.Id);
                else
                    throw new InvalidOperationException($"A different version of document {document.Id} is already being checked.");
            }
            else if (_modifiedDocuments.TryGetValue(document.Id, out existingDocument))
            {
                throw new InvalidOperationException($"Document {document.Id} is already being modified.");
            }

            addModifyDocuments.Add(document);
        }

        foreach (Guid remove in removeCheckedDocuments)
            _checkedDocuments.Remove(remove);

        foreach (Document document in addModifyDocuments)
            _modifiedDocuments.Add(document.Id, document);
    }

    public void Modify<T>(IJsonEntity<T> document)
    {
        Modify(_jsonSerializer.ToDocument(document));
    }

    public async Task Submit()
    {
        await _documentStore.UpdateDocuments(_modifiedDocuments.Values, _checkedDocuments.Values);

        _checkedDocuments.Clear();
        _modifiedDocuments.Clear();
    }
}
