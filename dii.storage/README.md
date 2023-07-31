# dii.storage

A .NET 6 framework for managing entities in storage.

## Features

* **Object Compression** — Specify which properties to compress into the stored object. dii.storage handles packing and unpacking of the compressed properties so you don't have to!

* **Property Name Abbreviation** — Add unique property names used for stored objects while using clear names on your .NET properties.

* **Auto-Detect Types** — Allows auto-detection of types used for storage. Don't worry, you can explicitly pass in types too!

* **Runtime Type Registration** — Add new types to the Optimizer without requiring a system restart.

## Extension Packages

* [dii.storage.cosmos](https://www.nuget.org/packages/dii.storage.cosmos) - A .NET 6 framework for managing CosmosDB entities with dii.storage.

## Dependencies

_**net5.0**_
* For use with .NET 5, use up to [v1.2.0](https://www.nuget.org/packages/dii.storage/1.2.0):
  * [MessagePack](https://www.nuget.org/packages/MessagePack/) (>= 2.3.85)
  * [Microsoft.Azure.Cosmos](https://www.nuget.org/packages/Microsoft.Azure.Cosmos/) (>= 3.27.2)

_**net6.0**_
* For use with .NET 6, use [latest](https://www.nuget.org/packages/dii.storage).
  * [MessagePack](https://www.nuget.org/packages/MessagePack/) (>= 2.4.35)
  * [Newtonsoft.Json](https://www.nuget.org/packages/Newtonsoft.Json/) (>= 13.0.1)

## Getting Started with dii.storage

To get started with dii.storage, please check out the [Getting Started](https://github.com/Dream-Invent-Inspire/dii.storage/wiki/Getting-Started)
section in our [wiki](https://github.com/Dream-Invent-Inspire/dii.storage/wiki).

## Questions? Need Help?

If you've got questions about setup, features, or just want to chat with the developer, please feel free to [start a thread in our Discussions tab](https://github.com/Dream-Invent-Inspire/dii.storage/discussions)!

## Found a bug?

[Submit an issue](https://github.com/Dream-Invent-Inspire/dii.storage/issues). Also feel free to submit pull requests with bug fixes or changes to the `dev` branch.

## Contributors

The core optimizer of dii.storage was originally developed by [Andrew Beers](https://github.com/aquamoogle). Additional features and maintenance by Andrew and [Pat MacMannis](https://github.com/pmac627). Both original contributors are active with the project today.
