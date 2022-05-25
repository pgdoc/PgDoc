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

using Microsoft.Extensions.DependencyInjection;
using Xunit;

public class ServiceCollectionExtensionsTests
{
    private readonly ServiceCollection _serviceCollection = new();

    [Fact]
    public void AddPgDoc_Success()
    {
        _serviceCollection.AddPgDoc(options =>
        {
            options.ConnectionString = "Host=localhost";
        });

        ServiceProvider serviceProvider = _serviceCollection.BuildServiceProvider();

        EntityStore entityStore = serviceProvider.GetRequiredService<EntityStore>();
        DocumentQuery documentQuery = serviceProvider.GetRequiredService<DocumentQuery>();

        Assert.NotNull(entityStore);
        Assert.NotNull(documentQuery);
    }
}
