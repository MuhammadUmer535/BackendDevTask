using Dapper;
using Microsoft.Extensions.Configuration;
using PaymentMS.src.Entities;
using PaymentMS.src.Repositories;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace PaymenttMS.src.Repositories
{
    public class PaymentRepository : IPaymentRepository
    {
        // Make it possible to read a connection string from configuration
        private readonly IConfiguration _configuration;

        public PaymentRepository(IConfiguration configuration)
        {
            // Injecting Iconfiguration to the constructor of the Payment repository
            _configuration = configuration;
        }

        /// <summary>
        /// This method adds a new Payment to the database using Dapper
        /// </summary>
        /// <param name="entity"></param>
        /// <returns>int</returns>
        public async Task<int> AddAsync(Payment entity)
        {
            // Basic SQL statement to insert a Payment into the Payment table
            var sql = "Insert into Payment (RequestId,PaymentAmount,PaymentDate) VALUES (@RequestId,@PaymentAmount,@PaymentDate)";
            
            // Sing the Dapper Connection string we open a connection to the database
            using (var connection = new SqlConnection(_configuration.GetConnectionString("DapperConnection")))
            {
                connection.Open();

                // Pass the Payment object and the SQL statement into the Execute function (async)
                var result = await connection.ExecuteAsync(sql, entity);
                return result;
            }
        }
    }
}