
//using System.Collections.Generic;
//using System.Threading.Tasks;

//using PipServices.Commons.Data;
//using PipServices.Data.Persistence;

//namespace PipServices.Data.Persistence
//{
//    public class MockDummyPersistence : AbstractMockDbPersistence<Dummy>, IDummyPersistence
//    {
//        public Task DeleteByFilterAsync(string correlationId, FilterDefinition<Dummy> filterDefinition)
//        {
//            throw new System.NotImplementedException();
//        }

//        public async Task<Dummy> DeleteByIdAsync(string correlationId, string id)
//        {
//            return await DeleteAsync(correlationId, id);
//        }

//        public Task DeleteByIdsAsync(string correlationId, string[] ids)
//        {
//            throw new System.NotImplementedException();
//        }

//        public Task<List<Dummy>> GetListByFilterAsync(string correlationId, FilterDefinition<Dummy> filterDefinition, SortDefinition<Dummy> sortDefinition = null)
//        {
//            throw new System.NotImplementedException();
//        }

//        public Task<List<Dummy>> GetListByIdsAsync(string correlationId, string[] ids)
//        {
//            throw new System.NotImplementedException();
//        }

//        public async Task<Dummy> GetOneByIdAsync(string correlationId, string id)
//        {
//            return await base.GetByIdAsync(correlationId, id);
//        }

//        public async Task<object> GetOneByIdAsync(string correlationId, string id, ProjectionParams projection)
//        {
//            return await base.GetByIdAsync(correlationId, id, projection);
//        }

//        public Task<Dummy> GetOneRandomAsync(string correlationId, FilterDefinition<Dummy> filterDefinition)
//        {
//            throw new System.NotImplementedException();
//        }

//        public Task<DataPage<object>> GetPageByFilterAndProjectionAsync(string correlationId, FilterDefinition<Dummy> filterDefinition, PagingParams paging = null, SortDefinition<Dummy> sortDefinition = null, ProjectionParams projection = null)
//        {
//            throw new System.NotImplementedException();
//        }

//        public Task<DataPage<Dummy>> GetPageByFilterAsync(string correlationId, FilterDefinition<Dummy> filterDefinition, PagingParams paging = null, SortDefinition<Dummy> sortDefinition = null)
//        {
//            throw new System.NotImplementedException();
//        }

//        public Task<Dummy> ModifyAsync(string correlationId, FilterDefinition<Dummy> filterDefinition, UpdateDefinition<Dummy> updateDefinition)
//        {
//            throw new System.NotImplementedException();
//        }

//        public Task<Dummy> ModifyByIdAsync(string correlationId, string id, UpdateDefinition<Dummy> updateDefinition)
//        {
//            throw new System.NotImplementedException();
//        }

//        public Task<Dummy> SetAsync(string correlationId, Dummy item)
//        {
//            throw new System.NotImplementedException();
//        }
//    }
//}
