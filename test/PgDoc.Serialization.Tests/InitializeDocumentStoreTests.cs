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

using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Filters;
using Xunit;

public class InitializeDocumentStoreTests
{
    [Fact]
    public async Task InitializeDocumentStore_OnActionExecutionAsync()
    {
        TestDocumentStore store = new();
        bool nextCalled = false;
        ActionExecutionDelegate next = () =>
        {
            nextCalled = true;
            return Task.FromResult<ActionExecutedContext>(null);
        };

        InitializeDocumentStore initializer = new(store);
        await initializer.OnActionExecutionAsync(null, next);

        Assert.True(store.Initialized);
        Assert.True(nextCalled);
    }

    [Fact]
    public async Task InitializeDocumentStore_NoDocumentStore()
    {
        bool nextCalled = false;
        ActionExecutionDelegate next = () =>
        {
            nextCalled = true;
            return Task.FromResult<ActionExecutedContext>(null);
        };

        InitializeDocumentStore initializer = new(null);
        await initializer.OnActionExecutionAsync(null, next);

        Assert.True(nextCalled);
    }
}
