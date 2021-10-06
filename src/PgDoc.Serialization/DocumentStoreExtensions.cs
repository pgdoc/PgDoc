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
using System.Linq;
using System.Threading.Tasks;

namespace PgDoc.Serialization
{
    public static class DocumentStoreExtensions
    {
        /// <summary>
        /// Updates atomically the body of multiple documents represented as <see cref="IJsonEntity{T}"/> objects.
        /// </summary>
        public static async Task<ByteString> UpdateEntities(
            this IDocumentStore documentStore,
            IEnumerable<IJsonEntity<object>> updatedDocuments,
            IEnumerable<IJsonEntity<object>> checkedDocuments)
        {
            return await documentStore.UpdateDocuments(
                updatedDocuments.Select(JsonEntityExtensions.AsDocument),
                checkedDocuments.Select(JsonEntityExtensions.AsDocument));
        }

        /// <summary>
        /// Updates atomically the body of multiple documents represented as <see cref="IJsonEntity{T}"/> objects.
        /// </summary>
        public static async Task<ByteString> UpdateEntities(
            this IDocumentStore documentStore,
            params IJsonEntity<object>[] updatedDocuments)
        {
            return await documentStore.UpdateEntities(updatedDocuments, Array.Empty<IJsonEntity<object>>());
        }

        /// <summary>
        /// Retrieves a document given its ID, represented as a <see cref="JsonEntity{T}"/> object.
        /// </summary>
        public static async Task<JsonEntity<T>> GetEntity<T>(this IDocumentStore documentStore, EntityId id)
            where T : class
        {
            Document result = await documentStore.GetDocument(id.Value);
            return JsonEntity<T>.FromDocument(result);
        }
    }
}
