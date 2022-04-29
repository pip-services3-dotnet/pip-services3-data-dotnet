using System;
using System.Threading;

using Xunit;

using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

using PipServices3.Commons.Data;

namespace PipServices3.Data.Persistence
{
    public class PersistenceFixture2
    {
        private readonly Dummy _dummy1 = new Dummy
        {
            Key = "Key 1",
            Content = "Content 1",
            CreateTimeUtc = DateTime.UtcNow,
            InnerDummy = new InnerDummy()
            {
                Description = "Inner Dummy Description"
            },
            DummyType = DummyType.NotDummy,
            InnerDummies = new List<InnerDummy>()
            {
                new InnerDummy
                {
                    Id = "1",
                    Name = "InnerDummy #1"
                },
                new InnerDummy
                {
                    Id = "2",
                    Name = "InnerDummy #2"
                },
                new InnerDummy
                {
                    Id = "3",
                    Name = "InnerDummy #3",
                    InnerInnerDummies = new List<InnerDummy>()
                    {
                        new InnerDummy()
                        {
                            Id = "100",
                            Name = "InnerInner Dummy#1"
                        }
                    }
                }
            }
        };

        private readonly Dummy _dummy2 = new Dummy
        {
            Key = "Key 2",
            Content = "Content 2",
            CreateTimeUtc = DateTime.UtcNow,
            DummyType = DummyType.Dummy
        };

        private readonly Dummy _dummy3 = new Dummy
        {
            Key = "Key 3",
            Content = "Content 3",
            CreateTimeUtc = DateTime.UtcNow,
            InnerDummy = new InnerDummy()
            {
                Description = "Inner Dummy #3 Description"
            },
            DummyType = DummyType.Dummy,
            InnerDummies = new List<InnerDummy>()
            {
                new InnerDummy
                {
                    Id = "3",
                    Name = "InnerDummy #3",
                    InnerInnerDummies = new List<InnerDummy>()
                    {
                        new InnerDummy()
                        {
                            Id = "100",
                            Name = "InnerInner Dummy# 3.1"
                        },
                        new InnerDummy
                        {
                            Id = "200",
                            Name = "InnerInner Dummy #3.2"
                        },
                        new InnerDummy
                        {
                            Id = "300",
                            Name = "InnerInner Dummy #3.3"
                        }
                    }
                }
            }
        };

        private readonly IDummyPersistence _persistence;

        public PersistenceFixture2(IDummyPersistence persistence)
        {
            Assert.NotNull(persistence);

            _persistence = persistence;
        }

        public async Task TestCrudOperationsAsync()
        {
            // Create one dummy
            var dummy1 = await _persistence.CreateAsync(null, _dummy1);

            Assert.NotNull(dummy1);
            Assert.NotNull(dummy1.Id);
            Assert.Equal(_dummy1.Key, dummy1.Key);
            Assert.Equal(_dummy1.Content, dummy1.Content);

            // Create another dummy
            var dummy2 = await _persistence.CreateAsync(null, _dummy2);

            Assert.NotNull(dummy2);
            Assert.NotNull(dummy2.Id);
            Assert.Equal(_dummy2.Key, dummy2.Key);
            Assert.Equal(_dummy2.Content, dummy2.Content);

            //// Get all dummies
            //var dummies = await _get.GetAllAsync(null);
            //Assert.NotNull(dummies);
            //Assert.Equal(2, dummies.Count());

            // Update the dummy
            dummy1.Content = "Updated Content 1";
            var dummy = await _persistence.UpdateAsync(null, dummy1);

            Assert.NotNull(dummy);
            Assert.Equal(dummy1.Id, dummy.Id);
            Assert.Equal(dummy1.Key, dummy.Key);
            Assert.Equal(dummy1.Content, dummy.Content);

            // Update partially the dummy
            dummy = await _persistence.UpdatePartiallyAsync(null, dummy1.Id, AnyValueMap.FromTuples(
                "content", "Partially Updated Content 1"
            ));

            Assert.NotNull(dummy);
            Assert.Equal(dummy1.Id, dummy.Id);
            Assert.Equal(dummy1.Key, dummy.Key);
            Assert.Equal("Partially Updated Content 1", dummy.Content);

            // Delete the dummy
            await _persistence.DeleteByIdAsync(null, dummy1.Id);

            // Try to get deleted dummy
            dummy = await _persistence.GetOneByIdAsync(null, dummy1.Id);
            Assert.Null(dummy);
        }

        public async Task TestMultithreading()
        {
            const int itemNumber = 50;

            var dummies = new List<Dummy>();

            for (var i = 0; i < itemNumber; i++)
            {
                dummies.Add(new Dummy() { Id = i.ToString(), Key = "Key " + i, Content = "Content " + i });
            }

            var count = 0;
            dummies.AsParallel().ForAll(async x =>
            {
                await _persistence.CreateAsync(null, x);
                Interlocked.Increment(ref count);
            });

            while (count < itemNumber)
            {
                await Task.Delay(TimeSpan.FromMilliseconds(10));
            }

            //var dummiesResponce = await _get.GetAllAsync(null);
            //Assert.NotNull(dummies);
            //Assert.Equal(itemNumber, dummiesResponce.Count());
            //Assert.Equal(itemNumber, dummiesResponce.Total);

            dummies.AsParallel().ForAll(async x =>
            {
                var updatedContent = "Updated Content " + x.Id;

                // Update the dummy
                x.Content = updatedContent;
                var dummy = await _persistence.UpdateAsync(null, x);

                Assert.NotNull(dummy);
                Assert.Equal(x.Id, dummy.Id);
                Assert.Equal(x.Key, dummy.Key);
                Assert.Equal(updatedContent, dummy.Content);
            });

            var taskList = new List<Task>();
            foreach (var dummy in dummies)
            {
                taskList.Add(AssertDelete(dummy));
            }

            Task.WaitAll(taskList.ToArray(), CancellationToken.None);

            //count = 0;
            //dummies.AsParallel().ForAll(async x =>
            //{
            //    // Delete the dummy
            //    await _write.DeleteByIdAsync(null, x.Id);

            //    // Try to get deleted dummy
            //    var dummy = await _get.GetOneByIdAsync(null, x.Id);
            //    Assert.Null(dummy);

            //    Interlocked.Increment(ref count);
            //});

            //while (count < itemNumber)
            //{
            //    await Task.Delay(TimeSpan.FromMilliseconds(10));
            //}

            //dummiesResponce = await _get.GetAllAsync(null);
            //Assert.NotNull(dummies);
            //Assert.Equal(0, dummiesResponce.Count());
            //Assert.Equal(0, dummiesResponce.Total);
        }

        public async Task TestGetByWrongIdAndProjection()
        {
            // arrange
            var dummy = await _persistence.CreateAsync(null, _dummy1);
            var projection = ProjectionParams.FromValues("inner_dummy.description", "content", "key");

            // act
            dynamic result = await _persistence.GetOneByIdAsync(null, "wrong_id", projection);

            // assert
            Assert.Null(result);
        }

        public async Task TestGetByIdAndProjection()
        {
            // arrange
            var dummy = await _persistence.CreateAsync(null, _dummy1);
            var projection = ProjectionParams.FromValues("inner_dummy.description", "content", "key", "create_time_utc", "dummy_type");

            // act
            dynamic result = await _persistence.GetOneByIdAsync(null, dummy.Id, projection);

            // assert
            Assert.NotNull(result);
            Assert.Equal(dummy.Key, result.key);
            Assert.Equal(dummy.Content, result.content);
            Assert.Equal(dummy.InnerDummy.Description, result.inner_dummy.description);
            Assert.Equal(dummy.CreateTimeUtc.ToString(), result.create_time_utc.ToString());
            Assert.Equal(dummy.DummyType.ToString(), result.dummy_type.ToString());
        }

        public async Task TestGetByIdAndWrongProjection()
        {
            // arrange
            var dummy = await _persistence.CreateAsync(null, _dummy1);
            var projection = ProjectionParams.FromValues("Wrong_Key", "Wrong_Content");

            // act
            dynamic result = await _persistence.GetOneByIdAsync(null, dummy.Id, projection);

            // assert
            Assert.Null(result);
        }

        public async Task TestGetByIdAndNullProjection()
        {
            // arrange
            var dummy = await _persistence.CreateAsync(null, _dummy1);

            // act
            dynamic result = await _persistence.GetOneByIdAsync(null, dummy.Id, null);

            // assert
            Assert.NotNull(result);

            if (result is Dummy)
            {
                Assert.Equal(dummy.Id, (result as Dummy).Id);
                Assert.Equal(dummy.Key, (result as Dummy).Key);
                Assert.Equal(dummy.Content, (result as Dummy).Content);
                Assert.Equal(dummy.InnerDummy.Description, (result as Dummy).InnerDummy.Description);
            }
            else
            {
                Assert.Equal(dummy.Id, result.id);
                Assert.Equal(dummy.Key, result.key);
                Assert.Equal(dummy.Content, result.content);
                Assert.Equal(dummy.InnerDummy.Description, result.inner_dummy.description);
            }
        }

        public async Task TestGetByIdAndIdProjection()
        {
            // arrange
            var dummy = await _persistence.CreateAsync(null, _dummy1);
            var projection = ProjectionParams.FromValues("id");

            // act
            dynamic result = await _persistence.GetOneByIdAsync(null, dummy.Id, projection);

            // assert
            Assert.NotNull(result);
            Assert.Equal(dummy.Id, result.id);
        }

        private async Task AssertDelete(Dummy dummy)
        {
            await _persistence.DeleteByIdAsync(null, dummy.Id);

            // Try to get deleted dummy
            var result = await _persistence.GetOneByIdAsync(null, dummy.Id);
            Assert.Null(result);
        }

        private FilterParams ExtractFilterParams(string query)
        {
            FilterParams filterParams = new FilterParams();

            foreach (var filterParameter in query.Split(','))
            {
                var keyValue = filterParameter.Split(':');

                if (keyValue.Length == 2)
                {
                    filterParams[keyValue[0]] = keyValue[1];
                }
            }

            return filterParams;
        }
    }
}
