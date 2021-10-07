namespace PgDoc.Serialization
{
    /// <summary>
    ///  Represents a document composed of a unique ID, a deserialized JSON body and a version.
    /// </summary>
    /// <typeparam name="T">The type used to deserialize the JSON body of the document.</typeparam>
    public interface IJsonEntity<out T>
        where T : class
    {
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
    }
}
