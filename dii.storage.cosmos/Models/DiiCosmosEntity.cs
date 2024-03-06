using dii.storage.Attributes;
using dii.storage.Models;
using dii.storage.Models.Interfaces;
using MessagePack;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace dii.storage.cosmos.Models
{
    /// <summary>
    /// The abstract implementation to ensure clean interaction with <see cref="Optimizer"/>.
    /// </summary>
    public class DiiCosmosEntity : DiiBasicEntity
    {
    }
}