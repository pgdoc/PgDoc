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
using System.Threading.Tasks;

namespace PgDoc.Serialization
{
    public class BatchBuilder
    {
        private readonly IDocumentStore dataStore;
        private readonly Dictionary<Guid, Document> checkedDocuments = new Dictionary<Guid, Document>();
        private readonly Dictionary<Guid, Document> modifiedDocuments = new Dictionary<Guid, Document>();

        public BatchBuilder(IDocumentStore store)
        {
            this.dataStore = store ?? throw new ArgumentNullException(nameof(store));
        }

        public void Check(params Document[] documents)
        {
            List<Document> addCheckDocuments = new List<Document>();

            // Validation phase
            foreach (Document document in documents)
            {
                if (checkedDocuments.TryGetValue(document.Id, out Document existingDocument))
                {
                    if (existingDocument.Version.Equals(document.Version))
                        continue;
                    else
                        throw new InvalidOperationException($"A different version of document {document.Id} is already being checked.");
                }

                if (modifiedDocuments.TryGetValue(document.Id, out existingDocument))
                {
                    if (existingDocument.Version.Equals(document.Version))
                        continue;
                    else
                        throw new InvalidOperationException($"A different version of document {document.Id} is already being modified.");
                }

                addCheckDocuments.Add(document);
            }

            foreach (Document document in addCheckDocuments)
                this.checkedDocuments.Add(document.Id, document);
        }

        public void Check<T>(JsonEntity<T> document)
            where T : class
        {
            Check(document.AsDocument());
        }

        public void Modify(params Document[] documents)
        {
            List<Guid> removeCheckedDocuments = new List<Guid>();
            List<Document> addModifyDocuments = new List<Document>();

            // Validation phase
            foreach (Document document in documents)
            {
                if (checkedDocuments.TryGetValue(document.Id, out Document existingDocument))
                {
                    if (existingDocument.Version.Equals(document.Version))
                        removeCheckedDocuments.Add(document.Id);
                    else
                        throw new InvalidOperationException($"A different version of document {document.Id} is already being checked.");
                }

                if (modifiedDocuments.TryGetValue(document.Id, out existingDocument))
                {
                    throw new InvalidOperationException($"Document {document.Id} is already being modified.");
                }

                addModifyDocuments.Add(document);
            }

            foreach (Guid remove in removeCheckedDocuments)
                this.checkedDocuments.Remove(remove);

            foreach (Document document in addModifyDocuments)
                this.modifiedDocuments.Add(document.Id, document);
        }

        public void Modify<T>(JsonEntity<T> document)
            where T : class
        {
            Modify(document.AsDocument());
        }

        public async Task<ByteString> Submit()
        {
            ByteString result = await this.dataStore.UpdateDocuments(modifiedDocuments.Values, checkedDocuments.Values);

            checkedDocuments.Clear();
            modifiedDocuments.Clear();

            return result;
        }
    }
}
