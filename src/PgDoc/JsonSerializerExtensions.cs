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
using PgDoc.Core;

public static class JsonSerializerExtensions
{
    /// <summary>
    /// Converts a <see cref="IJsonEntity{T}"/> object to a <see cref="Document"/> object by serializing its body
    /// to JSON.
    /// </summary>
    public static Document ToDocument<T>(this IJsonSerializer serializer, IJsonEntity<T?> jsonEntity)
    {
        return new Document(
            id: jsonEntity.Id.Value,
            body: jsonEntity.Entity != null
                ? serializer.Serialize(jsonEntity.Entity)
                : null,
            version: jsonEntity.Version);
    }

    /// <summary>
    /// Converts a <see cref="Document"/> object to a <see cref="IJsonEntity{T}"/> by deserializing its JSON body.
    /// </summary>
    public static IJsonEntity<T?> FromDocument<T>(this IJsonSerializer serializer, Document document)
        where T : class?
    {
        return new JsonEntity<T?>(
            id: new EntityId(document.Id),
            entity: document.Body != null
                ? serializer.Deserialize<T>(document.Body)
                : null,
            version: document.Version);
    }

    /// <summary>
    /// Converts a <see cref="Document"/> object to a <see cref="IJsonEntity{T}"/> by deserializing its JSON body.
    /// The document must have a non-null body, or an <see cref="ArgumentException"/> will be thrown.
    /// </summary>
    public static IJsonEntity<T> FromExistingDocument<T>(this IJsonSerializer serializer, Document document)
    {
        if (document.Body != null)
        {
            return new JsonEntity<T>(
                id: new EntityId(document.Id),
                entity: serializer.Deserialize<T>(document.Body),
                version: document.Version);
        }
        else
        {
            throw new ArgumentException("The body of the document must not be null.", nameof(document));
        }
    }
}
