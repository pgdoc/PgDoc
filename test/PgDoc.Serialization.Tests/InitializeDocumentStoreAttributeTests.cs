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
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

public class InitializeDocumentStoreAttributeTests
{
    private readonly TestDocumentStore _store = new();

    [Fact]
    public async Task InitializeDocumentStore_OnActionExecutionAsync()
    {
        InitializeDocumentStore initializer = new(_store);
        await initializer.OnActionExecutionAsync(
            null,
            () => Task.FromResult<ActionExecutedContext>(null));

        Assert.True(_store.Initialized);
    }

    [Fact]
    public async Task InitializeDocumentStore_NoDocumentStore()
    {
        InitializeDocumentStore initializer = new(null);
        await initializer.OnActionExecutionAsync(
            null,
            () => Task.FromResult<ActionExecutedContext>(null));

        Assert.False(_store.Initialized);
    }
}
