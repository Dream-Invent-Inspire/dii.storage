using dii.storage.cosmos.Models;
using dii.storage.cosmos.tests.Adapters;
using dii.storage.cosmos.tests.Models;
using dii.storage.cosmos.tests.Models.Interfaces;
using dii.storage.cosmos.tests.Utilities;
using dii.storage.Models;
using dii.storage.Models.Interfaces;
using Microsoft.Azure.Cosmos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using static dii.storage.cosmos.tests.Models.Enums;

namespace dii.storage.cosmos.tests.Fixtures
{
    /// <summary>
    /// A class to allow multiple tests within this test class to share a mock database context.
    /// </summary>
    public class ReadOnlyAdapterFixture : IDisposable
    {
        private readonly CosmosClient _client;
        private readonly Container _container;

        public Optimizer Optimizer;
        public INoSqlDatabaseConfig NoSqlDatabaseConfig;
        public IFakeReadOnlyAdapter<FakeEntity> FakeEntityReadOnlyAdapter;
        public List<FakeEntity> CreatedFakeEntities;

        public ReadOnlyAdapterFixture()
        {
            NoSqlDatabaseConfig = new FakeReadOnlyCosmosDatabaseConfig();
            var writeConfig = new CosmosDatabaseConfig
            {
                Uri = NoSqlDatabaseConfig.Uri,
                DatabaseConfig = new DatabaseConfig
                {
                    Key = "C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==",
                    DatabaseIds = new string[1]
                    {
                        NoSqlDatabaseConfig.ReadOnlyDatabaseConfig.DatabaseIds.FirstOrDefault()
                    },
                    AutoCreate = true,
                    MaxRUPerSecond = 4000,
                    AutoAdjustMaxRUPerSecond = true,
                    AutoScaling = true
                }
            };

            var initContextAndOptimizerTask = TestHelpers.InitContextAndOptimizerAsync(NoSqlDatabaseConfig, Optimizer, new[] { typeof(FakeEntity) });
            initContextAndOptimizerTask.Wait();

            Optimizer = initContextAndOptimizerTask.Result;

            // Need to use the read/write database config so the test builds the database.
            var databaseId = NoSqlDatabaseConfig.ReadOnlyDatabaseConfig.DatabaseIds.FirstOrDefault();

            if (FakeEntityReadOnlyAdapter == null)
            {
                FakeEntityReadOnlyAdapter = new FakeEntityReadOnlyAdapter(databaseId);

                _client = new CosmosClient(writeConfig.Uri, writeConfig.DatabaseConfig.Key, new CosmosClientOptions());

                var databaseCreatedTask = DoesDatabaseExistAsync(writeConfig);
                databaseCreatedTask.Wait();

                var tableName = databaseCreatedTask.Result;

                _container = _client.GetContainer(databaseId, tableName);

                CreatedFakeEntities ??= new List<FakeEntity>();

                var createEntitiesTask = AddEntitiesForTesting();
                createEntitiesTask.Wait();
            }
        }

        protected virtual void Dispose(bool doNotCleanUpNative)
        {

        }

        public void Dispose()
        {
            Dispose(true);

            GC.SuppressFinalize(this);
        }

        public async Task TeardownCosmosDbAsync(INoSqlDatabaseConfig noSqlDatabaseConfig)
        {
            var client = new CosmosClient(noSqlDatabaseConfig.Uri, noSqlDatabaseConfig.ReadOnlyDatabaseConfig.Key, new CosmosClientOptions());

            var db = client.GetDatabase(noSqlDatabaseConfig.ReadOnlyDatabaseConfig.DatabaseIds.FirstOrDefault());

            _ = await db.DeleteAsync().ConfigureAwait(false);
        }

        private async Task<string> DoesDatabaseExistAsync(INoSqlDatabaseConfig noSqlDatabaseConfig)
        {
            var config = noSqlDatabaseConfig.DatabaseConfig;
            var databaseId = config.DatabaseIds.FirstOrDefault();

            if (config.AutoCreate)
            {
                var throughputProperties = config.AutoScaling ?
                            ThroughputProperties.CreateAutoscaleThroughput(config.MaxRUPerSecond)
                            : ThroughputProperties.CreateManualThroughput(config.MaxRUPerSecond);

                var response = await _client.CreateDatabaseIfNotExistsAsync(databaseId, throughputProperties).ConfigureAwait(false);

                if (response.StatusCode == HttpStatusCode.Created)
                {
                    var db = _client.GetDatabase(databaseId);
                    var tableMetaData = Optimizer.Tables.FirstOrDefault();

                    _ = await db.CreateContainerIfNotExistsAsync(new ContainerProperties(tableMetaData.TableName, tableMetaData.PartitionKeyPath)).ConfigureAwait(false);

                    return tableMetaData.TableName;
                }
            }

            // In this fixture, the database should always be new.
            return null;
        }

        private async Task<List<FakeEntity>> CreateBulkAsync(IReadOnlyList<FakeEntity> diiEntities, ItemRequestOptions requestOptions = null, CancellationToken cancellationToken = default)
        {
            var unpackedEntities = default(List<FakeEntity>);

            if (diiEntities == null || !diiEntities.Any())
            {
                return unpackedEntities;
            }

            var packedEntities = diiEntities.Select(x => new
            {
                PartitionKey = Optimizer.GetPartitionKey(x),
                Entity = Optimizer.ToEntity(x)
            });

            var concurrentTasks = new List<Task<ItemResponse<object>>>();

            foreach (var packedEntity in packedEntities)
            {
                var task = _container.CreateItemAsync(packedEntity.Entity, new PartitionKey(packedEntity.PartitionKey), requestOptions, cancellationToken);
                concurrentTasks.Add(task);
            }

            var itemResponses = await Task.WhenAll(concurrentTasks).ConfigureAwait(false);

            var returnResult = requestOptions == null || !requestOptions.EnableContentResponseOnWrite.HasValue || requestOptions.EnableContentResponseOnWrite.Value;

            if (!returnResult)
            {
                return unpackedEntities;
            }

            unpackedEntities = itemResponses.Select(x => Optimizer.FromEntity<FakeEntity>(x.Resource)).ToList();

            return unpackedEntities;
        }

        private async Task AddEntitiesForTesting()
        {
            var fakeEntities = new List<FakeEntity>
            {
                new FakeEntity
                {
                    FakeEntityId = DateTime.Now.Ticks.ToString(),
                    SearchableIntegerValue = 2,
                    SearchableDecimalValue = 2.02m,
                    SearchableStringValue = $"fakeEntity200: {nameof(FakeEntity.SearchableStringValue)}",
                    SearchableGuidValue = Guid.NewGuid(),
                    SearchableListValue = new List<string>
                    {
                        $"fakeEntity200: {nameof(FakeEntity.SearchableListValue)}[0]",
                        $"fakeEntity200: {nameof(FakeEntity.SearchableListValue)}[1]"
                    },
                    SearchableDateTimeValue = DateTime.UtcNow.AddDays(2),
                    SearchableEnumValue = FakeEnum.Second,
                    CompressedPackedEntity = new FakeMessagePackEntity
                    {
                        PackedIntegerValue = 200,
                        PackedDecimalValue = 200.02m,
                        PackedStringValue = $"fakeEntity200: {nameof(FakeEntity.CompressedPackedEntity.PackedStringValue)}",
                        PackedGuidValue = Guid.NewGuid(),
                        PackedListValue = new List<string>
                        {
                            $"fakeEntity200: {nameof(FakeEntity.CompressedPackedEntity.PackedListValue)}[0]",
                            $"fakeEntity200: {nameof(FakeEntity.CompressedPackedEntity.PackedListValue)}[1]"
                        },
                        PackedDateTimeValue = DateTime.UtcNow.AddDays(200),
                        PackedEnumValue = FakeEnum.Fourth
                    },
                    CompressedIntegerValue = 20,
                    CompressedDecimalValue = 20.02m,
                    CompressedStringValue = $"fakeEntity200: {nameof(FakeEntity.CompressedStringValue)}",
                    CompressedGuidValue = Guid.NewGuid(),
                    CompressedListValue = new List<string>
                    {
                        $"fakeEntity200: {nameof(FakeEntity.CompressedListValue)}[0]",
                        $"fakeEntity200: {nameof(FakeEntity.CompressedListValue)}[1]"
                    },
                    CompressedDateTimeValue = DateTime.UtcNow.AddDays(20),
                    CompressedEnumValue = FakeEnum.Third,
                    ComplexSearchable = new FakeSearchableEntity
                    {
                        SearchableStringValue = $"fakeEntity200: {nameof(FakeEntity.ComplexSearchable)}.{nameof(FakeEntity.ComplexSearchable.SearchableStringValue)}",
                        CompressedStringValue = $"fakeEntity200: {nameof(FakeEntity.ComplexSearchable)}.{nameof(FakeEntity.ComplexSearchable.CompressedStringValue)}",
                        ComplexSearchable = new FakeSearchableEntityTwo
                        {
                            SearchableStringValue = $"fakeEntity200: {nameof(FakeEntity.ComplexSearchable)}.{nameof(FakeEntity.ComplexSearchable.ComplexSearchable)}.{nameof(FakeEntity.ComplexSearchable.ComplexSearchable.SearchableStringValue)}",
                            CompressedStringValue = $"fakeEntity200: {nameof(FakeEntity.ComplexSearchable)}.{nameof(FakeEntity.ComplexSearchable.ComplexSearchable)}.{nameof(FakeEntity.ComplexSearchable.ComplexSearchable.CompressedStringValue)}"
                        }
                    }
                },
                new FakeEntity
                {
                    FakeEntityId = DateTime.Now.Ticks.ToString(),
                    SearchableIntegerValue = 3,
                    SearchableDecimalValue = 3.03m,
                    SearchableStringValue = $"fakeEntity201: {nameof(FakeEntity.SearchableStringValue)}",
                    SearchableGuidValue = Guid.NewGuid(),
                    SearchableListValue = new List<string>
                    {
                        $"fakeEntity201: {nameof(FakeEntity.SearchableListValue)}[0]",
                        $"fakeEntity201: {nameof(FakeEntity.SearchableListValue)}[1]"
                    },
                    SearchableDateTimeValue = DateTime.UtcNow.AddDays(3),
                    SearchableEnumValue = FakeEnum.Third,
                    CompressedPackedEntity = new FakeMessagePackEntity
                    {
                        PackedIntegerValue = 300,
                        PackedDecimalValue = 300.03m,
                        PackedStringValue = $"fakeEntity201: {nameof(FakeEntity.CompressedPackedEntity.PackedStringValue)}",
                        PackedGuidValue = Guid.NewGuid(),
                        PackedListValue = new List<string>
                        {
                            $"fakeEntity201: {nameof(FakeEntity.CompressedPackedEntity.PackedListValue)}[0]",
                            $"fakeEntity201: {nameof(FakeEntity.CompressedPackedEntity.PackedListValue)}[1]"
                        },
                        PackedDateTimeValue = DateTime.UtcNow.AddDays(300),
                        PackedEnumValue = FakeEnum.Fifth
                    },
                    CompressedIntegerValue = 30,
                    CompressedDecimalValue = 30.03m,
                    CompressedStringValue = $"fakeEntity201: {nameof(FakeEntity.CompressedStringValue)}",
                    CompressedGuidValue = Guid.NewGuid(),
                    CompressedListValue = new List<string>
                    {
                        $"fakeEntity201: {nameof(FakeEntity.CompressedListValue)}[0]",
                        $"fakeEntity201: {nameof(FakeEntity.CompressedListValue)}[1]"
                    },
                    CompressedDateTimeValue = DateTime.UtcNow.AddDays(30),
                    CompressedEnumValue = FakeEnum.Fourth,
                    ComplexSearchable = new FakeSearchableEntity
                    {
                        SearchableStringValue = $"fakeEntity201: {nameof(FakeEntity.ComplexSearchable)}.{nameof(FakeEntity.ComplexSearchable.SearchableStringValue)}",
                        CompressedStringValue = $"fakeEntity201: {nameof(FakeEntity.ComplexSearchable)}.{nameof(FakeEntity.ComplexSearchable.CompressedStringValue)}",
                        ComplexSearchable = new FakeSearchableEntityTwo
                        {
                            SearchableStringValue = $"fakeEntity201: {nameof(FakeEntity.ComplexSearchable)}.{nameof(FakeEntity.ComplexSearchable.ComplexSearchable)}.{nameof(FakeEntity.ComplexSearchable.ComplexSearchable.SearchableStringValue)}",
                            CompressedStringValue = $"fakeEntity201: {nameof(FakeEntity.ComplexSearchable)}.{nameof(FakeEntity.ComplexSearchable.ComplexSearchable)}.{nameof(FakeEntity.ComplexSearchable.ComplexSearchable.CompressedStringValue)}"
                        }
                    }
                },
                new FakeEntity
                {
                    FakeEntityId = DateTime.Now.Ticks.ToString(),
                    SearchableIntegerValue = 4,
                    SearchableDecimalValue = 4.04m,
                    SearchableStringValue = $"fakeEntity202: {nameof(FakeEntity.SearchableStringValue)}",
                    SearchableGuidValue = Guid.NewGuid(),
                    SearchableListValue = new List<string>
                    {
                        $"fakeEntity202: {nameof(FakeEntity.SearchableListValue)}[0]",
                        $"fakeEntity202: {nameof(FakeEntity.SearchableListValue)}[1]"
                    },
                    SearchableDateTimeValue = DateTime.UtcNow.AddDays(4),
                    SearchableEnumValue = FakeEnum.Fourth,
                    CompressedPackedEntity = new FakeMessagePackEntity
                    {
                        PackedIntegerValue = 400,
                        PackedDecimalValue = 400.04m,
                        PackedStringValue = $"fakeEntity202: {nameof(FakeEntity.CompressedPackedEntity.PackedStringValue)}",
                        PackedGuidValue = Guid.NewGuid(),
                        PackedListValue = new List<string>
                        {
                            $"fakeEntity202: {nameof(FakeEntity.CompressedPackedEntity.PackedListValue)}[0]",
                            $"fakeEntity202: {nameof(FakeEntity.CompressedPackedEntity.PackedListValue)}[1]"
                        },
                        PackedDateTimeValue = DateTime.UtcNow.AddDays(400),
                        PackedEnumValue = FakeEnum.Sixth
                    },
                    CompressedIntegerValue = 40,
                    CompressedDecimalValue = 40.04m,
                    CompressedStringValue = $"fakeEntity202: {nameof(FakeEntity.CompressedStringValue)}",
                    CompressedGuidValue = Guid.NewGuid(),
                    CompressedListValue = new List<string>
                    {
                        $"fakeEntity202: {nameof(FakeEntity.CompressedListValue)}[0]",
                        $"fakeEntity202: {nameof(FakeEntity.CompressedListValue)}[1]"
                    },
                    CompressedDateTimeValue = DateTime.UtcNow.AddDays(40),
                    CompressedEnumValue = FakeEnum.Fifth,
                    ComplexSearchable = new FakeSearchableEntity
                    {
                        SearchableStringValue = $"fakeEntity202: {nameof(FakeEntity.ComplexSearchable)}.{nameof(FakeEntity.ComplexSearchable.SearchableStringValue)}",
                        CompressedStringValue = $"fakeEntity202: {nameof(FakeEntity.ComplexSearchable)}.{nameof(FakeEntity.ComplexSearchable.CompressedStringValue)}",
                        ComplexSearchable = new FakeSearchableEntityTwo
                        {
                            SearchableStringValue = $"fakeEntity202: {nameof(FakeEntity.ComplexSearchable)}.{nameof(FakeEntity.ComplexSearchable.ComplexSearchable)}.{nameof(FakeEntity.ComplexSearchable.ComplexSearchable.SearchableStringValue)}",
                            CompressedStringValue = $"fakeEntity202: {nameof(FakeEntity.ComplexSearchable)}.{nameof(FakeEntity.ComplexSearchable.ComplexSearchable)}.{nameof(FakeEntity.ComplexSearchable.ComplexSearchable.CompressedStringValue)}"
                        }
                    }
                }
            };

            var savedFakeEntities = await CreateBulkAsync(fakeEntities).ConfigureAwait(false);

            CreatedFakeEntities.AddRange(savedFakeEntities);
        }
    }
}