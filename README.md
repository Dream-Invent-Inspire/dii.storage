# dii.storage

A .NET 5+ framework for managing entities in storage.

## Features

### dii.storage

* **Object Compression** — Specify which properties to compress into the stored object. dii.storage handles packing and unpacking of the compressed properties so you don't have to!

* **Property Name Abbreviation** — Add unique property names used for stored objects while using clear names on your .NET properties.

* **Auto-Detect Types** — Allows auto-detection of types used for storage. Don't worry, you can explicitly pass in types too!

* **Runtime Type Registration** — Add new types to the Optimizer without requiring a system restart.

### dii.storage.cosmos

* **Infrastructure Auto-Creation** — Allows the automatic creation of CosmosDB Database and Container at startup.

* **Abstract Adapter** — Provides an abstract adapter optimized to take advantage of dii.storage.

  * Features common access patterns with Get, Create, Replace, Upsert, Patch and Delete APIs.

  * Fully functional Bulk variants of all access patterns.

## Getting Started with dii.storage

To get started with dii.storage, please check out the [Getting Started](https://github.com/Dream-Invent-Inspire/dii.storage/wiki#getting-started)
section in our [wiki](https://github.com/Dream-Invent-Inspire/dii.storage/wiki). You can also view example code in the [dii.storage.cosmos.examples](https://github.com/Dream-Invent-Inspire/dii.storage/tree/rc/1.2.0/dii.storage.cosmos.examples) project.

## Questions? Need Help?

If you've got questions about setup, features, or just want to chat with the developer, please feel free to [start a thread in our Discussions tab](https://github.com/Dream-Invent-Inspire/dii.storage/discussions)!

## Found a bug?

[Submit an issue](https://github.com/Dream-Invent-Inspire/dii.storage/issues). Also feel free to submit pull requests with bug fixes or changes to the `dev` branch.

## Contributors

The core optimizer of dii.storage was originally developed by [Andrew Beers](https://github.com/aquamoogle). Additional features and maintenance by Andrew and [Pat MacMannis](https://github.com/pmac627). Both original contributors are active with the project today.