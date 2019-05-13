namespace PgDoc.Serialization
{
    public interface IJsonEntity<out T>
        where T : class
    {
        public EntityId Id { get; }

        public ByteString Version { get; }

        public T? Entity { get; }
    }
}
