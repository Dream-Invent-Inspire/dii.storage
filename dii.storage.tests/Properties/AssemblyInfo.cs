using dii.storage.tests.Orderer;
using Xunit;

[assembly: TestCollectionOrderer(TestCollectionPriorityOrderer.FullName, TestCollectionPriorityOrderer.AssemblyName)]
[assembly: CollectionBehavior(DisableTestParallelization = true)]