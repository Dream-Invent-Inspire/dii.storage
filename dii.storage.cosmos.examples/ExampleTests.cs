using dii.storage.cosmos.examples.Attributes;
using dii.storage.cosmos.examples.Fixtures;
using dii.storage.cosmos.examples.Models;
using dii.storage.cosmos.examples.Orderer;
using Microsoft.Azure.Cosmos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace dii.storage.cosmos.examples
{
    [Collection(nameof(ExampleTests))]
    [TestCaseOrderer(TestPriorityOrderer.FullName, TestPriorityOrderer.AssemblyName)]
    public class ExampleTests : IClassFixture<ExamplePersonAdapterFixture>
    {
        private readonly ExamplePersonAdapterFixture _fixture;

        public ExampleTests(ExamplePersonAdapterFixture fixture)
        {
            _fixture = fixture ?? throw new ArgumentNullException(nameof(fixture));
        }

        [Fact, TestPriorityOrder(100)]
		public async Task RunExample1()
		{
            // Some quick dummy data.
            var person1 = new Person
            {
                ClientId = Guid.NewGuid().ToString(),
                PersonId = Guid.NewGuid().ToString(),
                Name = "Jimbo",
                Age = 37,
                OtherData = "Comments daily on the site.",
                Address = new Address
                {
                    ZipCode = "90210",
                    OtherData = "325 Hemlock Way",
                    PhoneNumber = new PhoneNumber
                    {
                        FullPhoneNumber = "412-555-2340",
                        OtherData = "Carrier: Verizon"
                    }
                }
            };


            // Create the entity with our adapter.
            var createdPerson = await _fixture.PersonAdapter.CreateAsync(person1).ConfigureAwait(false);

            Assert.NotNull(createdPerson);
            Assert.Equal(person1.ClientId, createdPerson.ClientId);
            Assert.Equal(person1.PersonId, createdPerson.PersonId);
            Assert.Equal(person1.Name, createdPerson.Name);
            Assert.Equal(person1.Age, createdPerson.Age);
            Assert.Equal(person1.Address.ZipCode, createdPerson.Address.ZipCode);
            Assert.Equal(person1.Address.PhoneNumber.FullPhoneNumber, createdPerson.Address.PhoneNumber.FullPhoneNumber);

            _fixture.People.Add(createdPerson);
		}

		[Fact, TestPriorityOrder(200)]
		public async Task RunExample2()
		{
			var persistedPerson1 = _fixture.People[0];

			// Fetch the person with our adapter.
			var fetchedPerson = await _fixture.PersonAdapter.FetchAsync(persistedPerson1.PersonId, persistedPerson1.ClientId).ConfigureAwait(false);

			Assert.NotNull(fetchedPerson);
			Assert.Equal(persistedPerson1.ClientId, fetchedPerson.ClientId);
			Assert.Equal(persistedPerson1.PersonId, fetchedPerson.PersonId);
			Assert.Equal(persistedPerson1.Name, fetchedPerson.Name);
			Assert.Equal(persistedPerson1.Age, fetchedPerson.Age);
			Assert.Equal(persistedPerson1.Address.ZipCode, fetchedPerson.Address.ZipCode);
			Assert.Equal(persistedPerson1.Address.PhoneNumber.FullPhoneNumber, fetchedPerson.Address.PhoneNumber.FullPhoneNumber);

			// Add a year to the person's age with our adapter.
			await _fixture.PersonAdapter.AddYearToAgeAsync(persistedPerson1.PersonId, persistedPerson1.ClientId).ConfigureAwait(false);

			// Fetch all people now older than the original age of the person.
			// This should return the newly updated person.

			var olderPeople = await _fixture.PersonAdapter.GetByAgeComparisonAsync(persistedPerson1.Age, Enums.ComparisonType.GreaterThan).ConfigureAwait(false);

			var nowOlderPerson = olderPeople[0];

			Assert.NotNull(nowOlderPerson);
			Assert.Equal(persistedPerson1.ClientId, nowOlderPerson.ClientId);
			Assert.Equal(persistedPerson1.PersonId, nowOlderPerson.PersonId);
			Assert.Equal(persistedPerson1.Name, nowOlderPerson.Name);
			Assert.Equal(persistedPerson1.Age + 1, nowOlderPerson.Age);
			Assert.Equal(persistedPerson1.Address.ZipCode, nowOlderPerson.Address.ZipCode);
			Assert.Equal(persistedPerson1.Address.PhoneNumber.FullPhoneNumber, nowOlderPerson.Address.PhoneNumber.FullPhoneNumber);

			_fixture.People[0].Age += 1;
		}

		[Fact, TestPriorityOrder(300)]
		public async Task RunExample3()
		{
			var persistedPerson1 = _fixture.People[0];

			// Some additional quick dummy data.
			var person2 = new Person
			{
				ClientId = persistedPerson1.ClientId,
				PersonId = Guid.NewGuid().ToString(),
				Name = "Sally",
				Age = 56L,
				OtherData = "Doesn't like to sleep on the bus.",
				Address = new Address
				{
					ZipCode = "15642",
					OtherData = "23424 Brokenhearts Blvd",
					PhoneNumber = new PhoneNumber
					{
						FullPhoneNumber = "431-555-7944",
						OtherData = "Carrier: Nextel"
					}
				}
			};

			var person3 = new Person
			{
				ClientId = Guid.NewGuid().ToString(),
				PersonId = Guid.NewGuid().ToString(),
				Name = "Arizona",
				Age = 19L,
				OtherData = "N/A",
				Address = new Address
				{
					ZipCode = "45630",
					OtherData = "1 Endless Street",
					PhoneNumber = new PhoneNumber
					{
						FullPhoneNumber = "798-555-2358",
						OtherData = "Carrier: Nextel"
					}
				}
			};

			// Create the entities with our adapter.
			var createdPerson2 = await _fixture.PersonAdapter.UpsertAsync(person2).ConfigureAwait(false);

			Assert.NotNull(createdPerson2);
			Assert.Equal(person2.ClientId, createdPerson2.ClientId);
			Assert.Equal(person2.PersonId, createdPerson2.PersonId);
			Assert.Equal(person2.Name, createdPerson2.Name);
			Assert.Equal(person2.Age, createdPerson2.Age);
			Assert.Equal(person2.Address.ZipCode, createdPerson2.Address.ZipCode);
			Assert.Equal(person2.Address.PhoneNumber.FullPhoneNumber, createdPerson2.Address.PhoneNumber.FullPhoneNumber);

			_fixture.People.Add(createdPerson2);

			var createdPerson3 = await _fixture.PersonAdapter.UpsertAsync(person3).ConfigureAwait(false);

			Assert.NotNull(createdPerson3);
			Assert.Equal(person3.ClientId, createdPerson3.ClientId);
			Assert.Equal(person3.PersonId, createdPerson3.PersonId);
			Assert.Equal(person3.Name, createdPerson3.Name);
			Assert.Equal(person3.Age, createdPerson3.Age);
			Assert.Equal(person3.Address.ZipCode, createdPerson3.Address.ZipCode);
			Assert.Equal(person3.Address.PhoneNumber.FullPhoneNumber, createdPerson3.Address.PhoneNumber.FullPhoneNumber);

			_fixture.People.Add(createdPerson3);

			// Fetch the new people with our adapter.
			var fetchedNewPeople = await _fixture.PersonAdapter.GetManyByPersonIdsAsync(new List<string> { person2.PersonId, person3.PersonId }).ConfigureAwait(false);

			Assert.NotNull(fetchedNewPeople);
			Assert.Equal(2, fetchedNewPeople.Count);

			// Fetch people by client Id to cross reference any overlap between both sets of people.
			var fetchedPeopleByClientId = await _fixture.PersonAdapter.GetManyByClientIdAsync(persistedPerson1.ClientId).ConfigureAwait(false);

			Assert.NotNull(fetchedPeopleByClientId);
			Assert.Equal(2, fetchedPeopleByClientId.Count);

			var matchingPeople = fetchedNewPeople.Where(x => fetchedPeopleByClientId.Any(y => y.PersonId == x.PersonId));

			Assert.NotNull(matchingPeople);
			Assert.Single(matchingPeople);
		}

		[Fact, TestPriorityOrder(400)]
		public async Task RunExample4()
		{
			var persistedPerson1 = _fixture.People[0];
			var persistedPerson2 = _fixture.People[1];
			var persistedPerson3 = _fixture.People[2];

			// Fetch the person with our adapter.
			var fetchedPeopleFrom90210Zip = await _fixture.PersonAdapter.SearchByZipCodeAsync(persistedPerson1.ClientId, "90210").ConfigureAwait(false);

			Assert.NotNull(fetchedPeopleFrom90210Zip);

			var fetchedPerson1 = fetchedPeopleFrom90210Zip.FirstOrDefault();

			Assert.Equal(persistedPerson1.ClientId, fetchedPerson1.ClientId);
			Assert.Equal(persistedPerson1.PersonId, fetchedPerson1.PersonId);
			Assert.Equal(persistedPerson1.Name, fetchedPerson1.Name);
			Assert.Equal(persistedPerson1.Age, fetchedPerson1.Age);
			Assert.Equal(persistedPerson1.Address.ZipCode, fetchedPerson1.Address.ZipCode);
			Assert.Equal(persistedPerson1.Address.PhoneNumber.FullPhoneNumber, fetchedPerson1.Address.PhoneNumber.FullPhoneNumber);

			// Fetch the person with our adapter.
			var fetchedPeopleFrom798AreaCode = await _fixture.PersonAdapter.SearchByAreaCodeAsync(persistedPerson3.ClientId, "798").ConfigureAwait(false);

			Assert.NotNull(fetchedPeopleFrom798AreaCode);

			var fetchedPerson3 = fetchedPeopleFrom798AreaCode.FirstOrDefault();

			Assert.Equal(persistedPerson3.ClientId, fetchedPerson3.ClientId);
			Assert.Equal(persistedPerson3.PersonId, fetchedPerson3.PersonId);
			Assert.Equal(persistedPerson3.Name, fetchedPerson3.Name);
			Assert.Equal(persistedPerson3.Age, fetchedPerson3.Age);
			Assert.Equal(persistedPerson3.Address.ZipCode, fetchedPerson3.Address.ZipCode);
			Assert.Equal(persistedPerson3.Address.PhoneNumber.FullPhoneNumber, fetchedPerson3.Address.PhoneNumber.FullPhoneNumber);

			// Delete all of the people from the database.
			_ = await _fixture.PersonAdapter.DeleteBulkAsync(new List<Person> { persistedPerson1, persistedPerson2, persistedPerson3 }).ConfigureAwait(false);

			// Ensure they are deleted.
			var fetchedDeletedPeople = await _fixture.PersonAdapter.GetManyByPersonIdsAsync(new List<string> { persistedPerson1.PersonId, persistedPerson2.PersonId, persistedPerson3.PersonId }).ConfigureAwait(false);

			Assert.Empty(fetchedDeletedPeople);

			_fixture.People.Clear();
		}

        #region Teardown
        [Fact, TestPriorityOrder(int.MaxValue)]
        public async Task Teardown()
		{
            try
            {
                Optimizer.Clear();
                DiiCosmosContext.Reset();
                //var context = DiiCosmosContext.Get();

                //if (context.Dbs != null)
                //{
                //    foreach (var db in context.Dbs)
                //    {
                //        _ = await db.DeleteAsync().ConfigureAwait(false);
                //    }
                //}
            }
            catch (Exception ex)
            {
            }
            
		}
		#endregion
	}
}