# PgDoc.Serialization
[![PgDoc.Serialization](https://img.shields.io/nuget/v/PgDoc.Serialization.svg?style=flat-square&color=blue&logo=nuget)](https://www.nuget.org/packages/PgDoc.Serialization/)

PgDoc is a library for using PostgreSQL as a JSON document store. PgDoc.Serialization manages the mapping between .NET objects and JSON documents.

## Philosophy

The traditional approach when dealing with a relational SQL database is to keep a schema defined in SQL language using relational modelling, and then translate this schema into an object-oriented model in the application (usually using an ORM library). Trying to marry these two very different modelling approaches invariably leads to complex layers of abstraction, often treated as "magic" by the developer.

This leads to undesirable complexity, unclear performance characteristics, hard to troubleshoot bugs and maintainability overhead since two separate models have to be maintained.

PgDoc tries to eliminate altogether the need for object-relational mapping. Instead, entities are defined in the application as simple classes and are stored as-is in the database, via a straightforward JSON serialization and deserialization process.

The main benefits of using a RDBMS system such as PostgreSQL are still retained:
- Ability to use ACID transactions across multiple entities.
- Ability to use concurrency control to keep the data globally consistent.
- Ability to query and index complex JSON structures efficiently thanks to [GIN indexes](https://www.postgresql.org/docs/current/datatype-json.html#JSON-INDEXING).
- Decades of maturity.
- Extensive tooling.
- A permissive open source license.
- A wide variety of cloud vendors.

## Setup

Run the [`PgDoc/pgdoc.sql` SQL script](https://raw.githubusercontent.com/pgdoc/PgDoc/master/src/PgDoc/sql/pgdoc.sql) to create the required table and functions in the database.

## Concepts

PgDoc uses a single `document` table with just three columns:
- `id` (of type `uuid`) is used as the primary key of the table.
- `body` contains the JSON document itself.
- Finally, `version` is used for optimistic concurrency control. It is normally not directly manipulated by the users of PgDoc, but it is used behind the scenes to guarantee safety when using the read-modify-write pattern.

## Configuration

This package relies on the `Microsoft.Extensions.DependencyInjection` stack to manage dependencies.

PgDoc can be configured by using the `AddPgDoc` method at startup.

```csharp
WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddPgDoc("connection_string");
```

This will register several services in the dependency injection container including the `EntityStore` class.

In ASP.NET Core applications, it is also recommended to register the  `InitializeDocumentStore` filter globally to ensure the `IDocumentStore` object is automatically initialized. Without this, the `IDocumentStore.Initialize` method must be called explicitly to open and configure the underlying database connection.

```csharp
builder.Services.AddControllers(
    options =>
    {
        options.Filters.Add(typeof(InitializeDocumentStore));
    });
```

## Defining document types

Each document type in the database corresponds to a .NET type that defines its schema. The type is serialized to a JSON document before being stored in the database using the `System.Text.Json` API.

Here is an example of a document type:

```csharp
[JsonEntityType(1)]
public record Product
{
    public string Name { get; init; }
    public int Aisle { get; init; }
    public decimal Price { get; init; }
    public int StockQuantity { get; init; }
    public IReadOnlyList<string> Categories { get; init; }
}
```

Since PgDoc uses a single table to store all documents, which could be of heterogeneous types, the `[JsonEntityType]` attribute is used to help differenciate between the different types. The value specified through the attribute is stored as the first four bytes of the ID of the document. By simply looking at the first four bytes of the ID, it is therefore possible to tell which schema the document has.

## Creating a new document

In order to create a new document, the `JsonEntity<T>.Create` method is used.

```csharp
Product product = new Product()
{
    Name = "Vanilla Ice Cream",
    Aisle = 3,
    Price = 9.95m,
    StockQuantity = 140,
    Categories = new List<string>() { "Frozen Foods", "Organic" }
};

JsonEntity<Product> entity = JsonEntity<Product>.Create(product);
```

The ID of the entity will be automatically generated. It can be retrieved using `entity.Id`.

The `EntityStore` class must then be used to commit the new document to the database. It can be obtained via dependency injection.

```csharp
// Obtained via dependency injection
EntityStore store;

await store.UpdateEntities(entity);
```

This will serialize the object and store it in the database:

```json
{
  "Name": "Vanilla Ice Cream",
  "Aisle": 3,
  "Price": 9.95,
  "StockQuantity": 140,
  "Categories": [
    "Frozen Foods",
    "Organic"
  ]
}
```

## Modifying a document

PgDoc relies on the read-modify-write pattern, with mandatory optimistic concurrency control to ensure safe writes.

The document should first be retrieved from the database. One way to retrieve the document is by using its ID.

```csharp
EntityId id;

JsonEntity<Product> entity = await store.GetEntity<Product>(id);
```

The entity can then be modified by calling `entity.Modify`. This returns a new copy of the original entity with the same ID and version, but a modified body. The new entity is then used with `EntityStore.UpdateEntities` to commit the update.

```csharp
Product modifiedProduct = entity.Entity with
{
    Price = 4.95m
};

JsonEntity<Product> modifiedEntity = entity.Modify(modifiedProduct);

await store.UpdateEntities(modifiedEntity);
```

PgDoc will always make sure no update has been made to the document between the time it was read and the time the update was committed. If a conflicting update has been made during that time, an exception of type `UpdateConflictException` will be thrown at the moment of commiting the update.

## Advanced document queries

The full range of PostgreSQL's JSON operators can be used, along with PostgreSQL's JSON indexing capabilities.

Two objects are required: `ISqlDocumentStore` and `IJsonSerializer`. Both of these are automatically registered when `AddPgDoc` is called.

Reusing the `Product` record previously defined, the code below demonstrates how to define a method that will search for a specific value inside the `Categories` array of each document.

```csharp
public class DocumentQueries
{
    private readonly ISqlDocumentStore _documentStore;
    private readonly IJsonSerializer _serializer;

    public DocumentQueries(ISqlDocumentStore documentStore, IJsonSerializer serializer)
    {
        _documentStore = documentStore;
        _serializer = serializer;
    }

    public async Task<IList<JsonEntity<Product>>> FindByCategory(string category)
    {
        using NpgsqlCommand command = new NpgsqlCommand($@"
            SELECT id, body, version
            FROM document
            WHERE (body -> 'Categories') ? @category
        ");

        command.Parameters.AddWithValue("@category", category);

        return await _documentStore.ExecuteQuery(command)
            .Select(_serializer.FromDocument<Product>)
            .ToListAsync();
    }
}
```

This query can be optimized by defining a GIN index:

```sql
CREATE INDEX idx_categories ON document USING GIN ((body -> 'Categories'));
```

## Batch updates

There is often a need to atomically update multiple documents simulteanously. This can be achieved using the `BatchBuilder` class.

```csharp
// Obtained via dependency injection
EntityStore store;

BatchBuilder batchBuilder = store.CreateBatchBuilder();

Product modifedProduct1 = entity1.Entity with
{
    StockQuantity = 20
};

batchBuilder.Modify(entity1.Modify(modifedProduct1));

Product modifedProduct2 = entity2.Entity with
{
    StockQuantity = 30
};

batchBuilder.Modify(entity2.Modify(modifedProduct2));

await batchBuilder.Submit();
```

When `batchBuilder.Submit()` is called, both documents will be updated together as part of an ACID transaction. If any of the documents have been modified between the time they were read and the time `batchBuilder.Submit()` is called, an exception of type `UpdateConflictException` will be thrown, and none of the changes will be committed to the database

## License

Copyright 2016 Flavien Charlon

Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. You may obtain a copy of the License at

    http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and limitations under the License.
