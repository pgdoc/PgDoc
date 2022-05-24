namespace PgDoc.Serialization;

using System;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddPgDoc(this IServiceCollection serviceCollection, Action<PgDocOptions> configure)
    {
        PgDocOptions options = new();
        configure(options);

        serviceCollection.AddSingleton<IJsonConverter>(new DefaultJsonConverter(options.JsonSerializerSettings));

        serviceCollection.AddScoped<IDocumentStore, SqlDocumentStore>();

        serviceCollection.AddScoped<EntityStore>();

        serviceCollection.AddSingleton<DocumentQuery>();

        if (options.ConnectionString != null)
        {
            string connectionString = options.ConnectionString;
            serviceCollection.AddScoped<NpgsqlConnection>(serviceProvider => new NpgsqlConnection(connectionString));
        }

        return serviceCollection;
    }
}
