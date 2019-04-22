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

namespace PgDoc.Serialization.Tests
{
    public class TestDocumentStore : IDocumentStore
    {
        private byte _currentVersion = 0;

        public IDictionary<Guid, Tuple<string, ByteString>> Store { get; } = new Dictionary<Guid, Tuple<string, ByteString>>();

        public Task Initialize()
        {
            return Task.FromResult(0);
        }

        public Task<IReadOnlyList<Document>> GetDocuments(IEnumerable<Guid> ids)
        {
            return Task.FromResult((IReadOnlyList<Document>)ids.Select(id =>
            {
                if (Store.TryGetValue(id, out Tuple<string, ByteString> value))
                    return new Document(id, value.Item1, value.Item2);
                else
                    return new Document(id, null, ByteString.Empty);
            })
            .ToList()
            .AsReadOnly());
        }

        public async Task<ByteString> UpdateDocuments(IEnumerable<Document> updatedDocuments, IEnumerable<Document> checkedDocuments)
        {
            foreach (Document document in updatedDocuments.Concat(checkedDocuments))
            {
                IReadOnlyList<Document> existing = await GetDocuments(new[] { document.Id });

                if (!existing[0].Version.Equals(document.Version))
                    throw new UpdateConflictException(document.Id, document.Version);
            }

            ByteString newVersion = GetVersionNumber(_currentVersion++);

            foreach (Document document in updatedDocuments)
            {
                Store[document.Id] = Tuple.Create(document.Body, newVersion);
            }

            return newVersion;
        }

        public void Dispose()
        { }

        public static ByteString GetVersionNumber(byte i)
        {
            return new ByteString(new byte[] { 0, i });
        }
    }
}
