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

namespace PgDoc.Serialization.Tests;

using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Xunit;

public class ServiceCollectionExtensionsTests
{
    private readonly ServiceCollection _serviceCollection = new();

    [Fact]
    public void AddPgDoc_ConnectionString()
    {
        _serviceCollection.AddPgDoc("Host=host");

        ServiceProvider serviceProvider = _serviceCollection.BuildServiceProvider();

        AssertServices(serviceProvider, "Host=host");
    }

    [Fact]
    public void AddPgDoc_Options()
    {
        _serviceCollection.AddPgDoc(new PgDocOptions()
        {
            ConnectionString = "Host=host"
        });

        ServiceProvider serviceProvider = _serviceCollection.BuildServiceProvider();

        AssertServices(serviceProvider, "Host=host");
    }

    [Fact]
    public void AddPgDoc_ActionOption()
    {
        _serviceCollection.AddPgDoc(options =>
        {
            options.ConnectionString = "Host=host";
        });

        ServiceProvider serviceProvider = _serviceCollection.BuildServiceProvider();

        AssertServices(serviceProvider, "Host=host");
    }

    [Fact]
    public void AddPgDoc_OptionsBuilder()
    {
        _serviceCollection.AddPgDoc(services =>
        {
            return new PgDocOptions()
            {
                ConnectionString = "Host=host"
            };
        });

        ServiceProvider serviceProvider = _serviceCollection.BuildServiceProvider();

        AssertServices(serviceProvider, "Host=host");
    }

    [Fact]
    public void AddPgDoc_ConfigureBeforeAdd()
    {
        _serviceCollection.ConfigurePgDoc((_, options) =>
        {
            options.ConnectionString = "Host=host1";
        });

        _serviceCollection.AddPgDoc("Host=host2");

        ServiceProvider serviceProvider = _serviceCollection.BuildServiceProvider();

        AssertServices(serviceProvider, "Host=host1");
    }

    [Fact]
    public void AddPgDoc_ConfigureAfterAdd()
    {
        _serviceCollection.AddPgDoc("Host=host1");

        _serviceCollection.ConfigurePgDoc((_, options) =>
        {
            options.ConnectionString = "Host=host2";
        });

        ServiceProvider serviceProvider = _serviceCollection.BuildServiceProvider();

        AssertServices(serviceProvider, "Host=host2");
    }

    [Fact]
    public void AddPgDoc_ConfigureMultipleTimes()
    {
        _serviceCollection.ConfigurePgDoc((_, options) =>
        {
            options.ConnectionString += ";Username=admin";
        });

        _serviceCollection.AddPgDoc("Host=host");

        _serviceCollection.ConfigurePgDoc((_, options) =>
        {
            options.ConnectionString += ";Port=5432";
        });

        ServiceProvider serviceProvider = _serviceCollection.BuildServiceProvider();

        AssertServices(serviceProvider, "Host=host;Username=admin;Port=5432");
    }

    private static void AssertServices(ServiceProvider serviceProvider, string connectionString)
    {
        Assert.Single(serviceProvider.GetService<IEnumerable<PgDocOptions>>());
        Assert.Single(serviceProvider.GetService<IEnumerable<NpgsqlConnection>>());
        Assert.Single(serviceProvider.GetService<IEnumerable<EntityStore>>());
        Assert.Single(serviceProvider.GetService<IEnumerable<ISqlDocumentStore>>());
        Assert.Single(serviceProvider.GetService<IEnumerable<IDocumentStore>>());
        Assert.Single(serviceProvider.GetService<IEnumerable<IJsonSerializer>>());
        Assert.Equal(connectionString, serviceProvider.GetService<NpgsqlConnection>().ConnectionString);
    }
}
