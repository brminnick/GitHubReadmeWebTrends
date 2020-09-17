using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace GitHubReadmeWebTrends.Common
{
    public class OptOutDatabase
    {
        readonly static string _connectionString = Environment.GetEnvironmentVariable("DatabaseConnectionString") ?? string.Empty;

        public List<OptOutModel> GetAllOptOutModels(Func<OptOutModel, bool> wherePredicate)
        {
            using var connection = new DatabaseContext();

            return connection.OptOutDatabaseModel?.Where(wherePredicate).ToList() ?? new List<OptOutModel>();
        }

        public List<OptOutModel> GetAllOptOutModels() => GetAllOptOutModels(x => true);

        public Task<OptOutModel> GetOptOutModel(string id)
        {
            return PerformDatabaseFunction(getOptOutModelFunction);

            Task<OptOutModel> getOptOutModelFunction(DatabaseContext dataContext) => dataContext.OptOutDatabaseModel.SingleAsync(x => x.Id.Equals(id));
        }

        public Task<OptOutModel> InsertOptOutModel(OptOutModel contact)
        {
            return PerformDatabaseFunction(insertOptOutModelFunction);

            async Task<OptOutModel> insertOptOutModelFunction(DatabaseContext dataContext)
            {
                if (string.IsNullOrWhiteSpace(contact.Id))
                    contact.Id = Guid.NewGuid().ToString();

                contact.CreatedAt = DateTimeOffset.UtcNow;
                contact.UpdatedAt = DateTimeOffset.UtcNow;

                await dataContext.AddAsync(contact).ConfigureAwait(false);

                return contact;
            }
        }

        public OptOutModel PatchOptOutModel(OptOutModel optOutModel)
        {
            return PerformDatabaseFunction(patchOptOutModelFunction);

            OptOutModel patchOptOutModelFunction(DatabaseContext dataContext)
            {
                dataContext.Update(optOutModel);

                return optOutModel;
            }
        }

        internal Task<OptOutModel> RemoveOptOutModel(string id)
        {
            return PerformDatabaseFunction(removeContactDatabaseFunction);

            async Task<OptOutModel> removeContactDatabaseFunction(DatabaseContext dataContext)
            {
                var answerModelFromDatabase = await dataContext.OptOutDatabaseModel.SingleAsync(x => x.Id.Equals(id)).ConfigureAwait(false);

                dataContext.Remove(answerModelFromDatabase);

                return answerModelFromDatabase;
            }
        }

        static async Task<TResult> PerformDatabaseFunction<TResult>(Func<DatabaseContext, Task<TResult>> databaseFunction) where TResult : class
        {
            using var connection = new DatabaseContext();

            try
            {
                var result = await databaseFunction.Invoke(connection).ConfigureAwait(false);
                await connection.SaveChangesAsync().ConfigureAwait(false);

                return result;
            }
            catch (Exception e)
            {
                Debug.WriteLine("");
                Debug.WriteLine(e.Message);
                Debug.WriteLine(e.ToString());
                Debug.WriteLine("");

                throw;
            }
        }

        class DatabaseContext : DbContext
        {
            public DatabaseContext() => Database.EnsureCreated();

            public DbSet<OptOutDatabaseModel>? OptOutDatabaseModel { get; set; }

            protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) => optionsBuilder.UseSqlServer(_connectionString);
        }

        class OptOutDatabaseModel : IOptOutModel
        {
            public Guid Id { get; set; }
            public string Alias { get; set; } = string.Empty;
            public bool HasOptedOut { get; set; }
            public DateTimeOffset CreatedAt { get; set; }
            public DateTimeOffset UpdatedAt { get; set; }
        }
    }
}