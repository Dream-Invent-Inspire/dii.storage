using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using dii.cosmos;
using dii.cosmos.tests.Models;
using System.Threading;
using dii.cosmos.Models;
using Microsoft.Azure.Cosmos;

namespace dii.cosmos.tests.Adapters
{
    //This goes in BLL
    public interface IFakeEntityAdapter
	{
        Task<FakeEntity> GetByIdsAsync(string id, string fakeId, CancellationToken cancellationToken = default);

    }

    //This goes in cosmos.infra
    public class FakeEntitySampleAdapter : DiiCosmosAdapter<FakeEntity>, IFakeEntityAdapter
    {
        //This would be an example of ClientId and EntityId
        //ClientId (for example) would be your partition key and fakeId is your EntityId.
        public async Task<FakeEntity> GetByIdsAsync(string id, string fakeId, CancellationToken cancellationToken = default)
		{
            return await base.GetAsync(id, fakeId, cancellationToken).ConfigureAwait(false);
		}
    }


    //This would belong in your onion BLL
    public interface IFakeEntityTwoAdapter
	{
        Task<PagedList<FakeEntityTwo>> GetByIdsAsync(params string[] ids);

    }

    //This would belong in your infra for Cosmos implementing the interface.
    //DI in app start.
    public class FakeEntityTwoSampleAdapter : DiiCosmosAdapter<FakeEntityTwo>, IFakeEntityTwoAdapter
    {
        public async Task<PagedList<FakeEntityTwo>> GetByIdsAsync(params string[] ids)
        {
            var keysDict = new Dictionary<string, string>();
            for (var i = 0; i < ids.Length; i++)
            {
                keysDict.Add($"@id{i}", ids[i]);
            }
            var queryDefinition = new QueryDefinition($"SELECT * FROM fakeentitytwo fet WHERE fet.id IN ({string.Join(", ", keysDict.Keys)})");
            foreach (var id in keysDict)
            {
                queryDefinition.WithParameter(id.Key, id.Value);
            }

            return await base.GetPagedAsync(queryDefinition).ConfigureAwait(false);
        }
    }
}
