using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TableDependency.SqlClient;
using TableDependency.SqlClient.Base.EventArgs;

namespace Services
{
    public class DataService : IAsyncDisposable
    {
        private readonly AppDbContext dbContext;
        private readonly IConfiguration _configuration; // Inject IConfiguration to get connection string
        private HubConnection? hubConnection;
      

        // Event to notify when data is updated
        public event Action? OnDataUpdated;

        // Constructor accepts AppDbContext and IConfiguration
        public DataService(AppDbContext dbContext, IConfiguration configuration)
        {
            this.dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        /// <summary>
        /// Initializes the SignalR connection and fetches the initial data.
        /// </summary>
        /// <param name="hubUrl">The SignalR hub URL.</param>
        /// <param name="minutesCutoff">The cutoff in minutes to fetch initial data.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task InitializeAsync(string hubUrl, int minutesCutoff)
        {
            if (string.IsNullOrWhiteSpace(hubUrl))
                throw new ArgumentException("Hub URL cannot be null or empty.", nameof(hubUrl));
            if (minutesCutoff <= 0)
                throw new ArgumentException("Minutes cutoff must be greater than zero.", nameof(minutesCutoff));

         
        }

        /// <summary>
        /// Event handler for database changes from TableDependency.
        /// </summary>
       

        /// <summary>
        /// Fetches data from the database based on the cutoff time.
        /// </summary>
        /// <param name="minutesCutoff">The cutoff time in minutes.</param>
        /// <returns>A task representing the fetched data.</returns>
     

        /// <summary>
        /// Stops the SignalR connection and disposes of resources.
        /// </summary>
        public async ValueTask DisposeAsync()
        {
           

           
        }
    }
}
