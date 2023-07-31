# dii.storage.cosmos

A .NET 6 framework for managing CosmosDB entities with [dii.storage](https://www.nuget.org/packages/dii.storage).

## Features

* **Infrastructure Auto-Creation** — Allows the automatic creation of CosmosDB Database and Container at startup.

* **Abstract Adapter** — Provides an abstract adapter optimized to take advantage of dii.storage.

  * Features common access patterns with Get, Create, Replace, Upsert, Patch and Delete APIs.

  * Fully functional Bulk variants of all access patterns.

## Dependencies

_**net5.0**_
* For use with .NET 5, use up to [v1.2.0](https://www.nuget.org/packages/dii.storage.cosmos/1.2.0):
  * [dii.storage](https://www.nuget.org/packages/dii.storage/1.2.0) (<= 1.2.0)

_**net6.0**_
* For use with .NET 6, use [latest](https://www.nuget.org/packages/dii.storage.cosmos).
  * [dii.storage](https://www.nuget.org/packages/dii.storage) (>= 1.2.1)
  * [Microsoft.Azure.Cosmos](https://www.nuget.org/packages/Microsoft.Azure.Cosmos/) (>= 3.27.2)

## Getting Started with dii.storage.cosmos

To get started with dii.storage.cosmos, please check out the example code in the [dii.storage.cosmos.examples](https://github.com/Dream-Invent-Inspire/dii.storage/tree/main/dii.storage.cosmos.examples) project.

## Questions? Need Help?

If you've got questions about setup, features, or just want to chat with the developer, please feel free to [start a thread in our Discussions tab](https://github.com/Dream-Invent-Inspire/dii.storage/discussions)!

## Found a bug?

[Submit an issue](https://github.com/Dream-Invent-Inspire/dii.storage/issues). Also feel free to submit pull requests with bug fixes or changes to the `dev` branch.

## Contributors

The core optimizer of [dii.storage](https://www.nuget.org/packages/dii.storage) was originally developed by [Andrew Beers](https://github.com/aquamoogle). Additional features and maintenance by Andrew and [Pat MacMannis](https://github.com/pmac627). Both original contributors are active with the project today.
