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

using Newtonsoft.Json;

namespace PgDoc.Serialization
{
    /// <summary>
    ///  Represents a document comprised of a unique ID, a deserialized JSON body and a version.
    /// </summary>
    /// <typeparam name="T">The type used to deserialize the JSON body of the document.</typeparam>
    public class JsonEntity<T> : IJsonEntity<T>
        where T : class
    {
        public JsonEntity(EntityId id, T? entity, ByteString version)
        {
            Id = id;
            Entity = entity;
            Version = version;
        }

        /// <summary>
        /// Gets the unique identifier of the document.
        /// </summary>
        public EntityId Id { get; }

        /// <summary>
        /// Gets the body of the document deserialized into an object, or null if the document does not exist.
        /// </summary>
        public T? Entity { get; }

        /// /// <summary>
        /// Gets the current version of the document.
        /// </summary>
        public ByteString Version { get; }

        /// <summary>
        /// Converts a <see cref="Document"/> object to a <see cref="JsonEntity{T}"/> by deserializing its JSON body.
        /// </summary>
        public static JsonEntity<T> FromDocument(Document document)
        {
            return new JsonEntity<T>(
                new EntityId(document.Id),
                document.Body != null ? (T?)JsonConvert.DeserializeObject(document.Body, typeof(T), JsonSettings.Settings) : null,
                document.Version);
        }

        /// <summary>
        /// Returns a copy of this <see cref="JsonEntity{T}"/> object with the same ID and version, but replaces the
        /// body with a new one.
        /// </summary>
        public JsonEntity<T> Modify(T newValue)
        {
            return new JsonEntity<T>(Id, newValue, Version);
        }

        /// <summary>
        /// Creates a new <see cref="JsonEntity{T}"/> object with a new random ID and a version set to zero.
        /// </summary>
        public static JsonEntity<T> Create(T value, EntityType type)
        {
            return new JsonEntity<T>(EntityId.New(type), value, ByteString.Empty);
        }

        /// <summary>
        /// Deconstructs the ID, body and version of the <see cref="JsonEntity{T}"/> object.
        /// </summary>
        public void Deconstruct(out EntityId id, out T? entity, out ByteString version)
        {
            id = Id;
            entity = Entity;
            version = Version;
        }
    }
}
