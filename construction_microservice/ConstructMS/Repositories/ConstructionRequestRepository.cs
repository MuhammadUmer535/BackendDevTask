using Dapper;
using Microsoft.Extensions.Configuration;
using ConstructMS.Entities;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace ConstructMS.Repositories
{
    public class ConstructionRequestRepository : IConstructionRequestRepository
    {
        // Make it possible to read a connection string from configuration
        private readonly IConfiguration _configuration;

        public ConstructionRequestRepository(IConfiguration configuration)
        {
            // Injecting Iconfiguration to the constructor of the ConstructionRequest repository
            _configuration = configuration;
        }

        /// <summary>
        /// This method adds a new ConstructionRequest to the database using Dapper
        /// </summary>
        /// <param name="entity"></param>
        /// <returns>int</returns>
        public async Task<int> AddAsync(ConstructionRequest entity)
        {
            // Basic SQL statement to insert a ConstructionRequest into the ConstructionRequest table
            var sql = "Insert into ConstructionRequest (ClientName,ClientEmail,ClientPhone,ConstructionServiceId,RequestDate,Status) VALUES (@ClientName,@ClientEmail,@ClientPhone,@ConstructionServiceId,@RequestDate,@Status)";
            
            // Sing the Dapper Connection string we open a connection to the database
            using (var connection = new SqlConnection(_configuration.GetConnectionString("DapperConnection")))
            {
                connection.Open();

                // Pass the ConstructionRequest object and the SQL statement into the Execute function (async)
                var result = await connection.ExecuteAsync(sql, entity);
                return result;
            }
        }

        /// <summary>
        /// This method deleted a ConstructionRequest specified by an ID
        /// </summary>
        /// <param name="id"></param>
        /// <returns>int</returns>
        public async Task<int> DeleteAsync(int id)
        {
            var sql = "DELETE FROM ConstructionRequest WHERE RequestId = @Id";
            using (var connection = new SqlConnection(_configuration.GetConnectionString("DapperConnection")))
            {
                connection.Open();
                var result = await connection.ExecuteAsync(sql, new { Id = id });
                return result;
            }
        }

        /// <summary>
        /// This method returns all ConstructionRequests in database in a list object
        /// </summary>
        /// <returns>IEnumerable ConstructionRequest</returns>
        public async Task<IReadOnlyList<ConstructionRequest>> GetAllAsync()
        {
            var sql = "SELECT * FROM ConstructionRequest";
            using (var connection = new SqlConnection(_configuration.GetConnectionString("DapperConnection")))
            {
                connection.Open();

                // Map all ConstructionRequests from database to a list of type ConstructionRequest defined in Models.
                // this is done by using Async method which is also used on the GetByIdAsync
                var result = await connection.QueryAsync<ConstructionRequest>(sql);
                return result.ToList();
            }
        }

        /// <summary>
        /// This method returns a single ConstructionRequest specified by an ID
        /// </summary>
        /// <param name="id"></param>
        /// <returns>ConstructionRequest</returns>
        public async Task<ConstructionRequest> GetByIdAsync(int id)
        {
            var sql = "SELECT * FROM ConstructionRequest WHERE RequestId = @Id";
            using (var connection = new SqlConnection(_configuration.GetConnectionString("DapperConnection")))
            {
                connection.Open();
                var result = await connection.QuerySingleOrDefaultAsync<ConstructionRequest>(sql, new { Id = id });
                return result;
            }
        }

        /// <summary>
        /// This method updates a ConstructionRequest specified by an ID. Added column won't be touched.
        /// </summary>
        /// <param name="entity"></param>
        /// <returns>int</returns>
        public async Task<int> UpdateAsync(ConstructionRequest entity)
        {
            var sql = "UPDATE ConstructionRequest SET ClientName = @ClientName, ClientEmail = @ClientEmail, ClientPhone = @ClientPhone, ConstructionServiceId = @ConstructionServiceId, RequestDate = @RequestDate, Status = @Status WHERE RequestId = @Id";
            using (var connection = new SqlConnection(_configuration.GetConnectionString("DapperConnection")))
            {
                connection.Open();
                var result = await connection.ExecuteAsync(sql, entity);
                return result;
            }
        }
    }
}