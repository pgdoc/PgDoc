﻿// Copyright 2016 Flavien Charlon
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
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using PgDoc.Core;

using Configurator = System.Action<System.IServiceProvider, PgDocOptions>;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddPgDoc(this IServiceCollection serviceCollection, string connectionString)
    {
        return serviceCollection.AddPgDoc(_ => new PgDocOptions()
        {
            ConnectionString = connectionString
        });
    }

    public static IServiceCollection AddPgDoc(this IServiceCollection serviceCollection, PgDocOptions options)
    {
        return serviceCollection.AddPgDoc(_ => options);
    }

    public static IServiceCollection AddPgDoc(this IServiceCollection serviceCollection, Action<PgDocOptions> configureOptions)
    {
        return serviceCollection.AddPgDoc(services =>
        {
            PgDocOptions options = new();
            configureOptions(options);
            return options;
        });
    }

    public static IServiceCollection AddPgDoc(this IServiceCollection serviceCollection, Func<IServiceProvider, PgDocOptions> createOptions)
    {
        serviceCollection.AddSingleton<PgDocOptions>(services =>
        {
            PgDocOptions options = createOptions(services);

            IEnumerable<Configurator> configurators = services.GetServices<Configurator>();

            foreach (Configurator configurator in configurators)
                configurator(services, options);

            return options;
        });

        serviceCollection.AddSingleton<IJsonSerializer>(services =>
        {
            PgDocOptions options = services.GetRequiredService<PgDocOptions>();
            return new DefaultJsonSerializer(options.JsonSerializerOptions);
        });

        serviceCollection.AddScoped<NpgsqlConnection>(services =>
        {
            PgDocOptions options = services.GetRequiredService<PgDocOptions>();
            return new NpgsqlConnection(options.ConnectionString);
        });

        serviceCollection.AddScoped<ISqlDocumentStore, SqlDocumentStore>();
        serviceCollection.AddScoped<IDocumentStore, SqlDocumentStore>();
        serviceCollection.AddScoped<EntityStore>();

        return serviceCollection;
    }

    public static IServiceCollection ConfigurePgDoc(this IServiceCollection serviceCollection, Configurator configurator)
    {
        serviceCollection.AddSingleton<Configurator>(configurator);
        return serviceCollection;
    }
}
