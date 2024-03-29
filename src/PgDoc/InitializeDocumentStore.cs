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

using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Filters;
using PgDoc.Core;

public class InitializeDocumentStore : IAsyncActionFilter
{
    private readonly IDocumentStore _documentStore;

    public InitializeDocumentStore(IDocumentStore documentStore)
    {
        _documentStore = documentStore;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        if (_documentStore != null)
            await _documentStore.Initialize();

        await next();
    }
}
