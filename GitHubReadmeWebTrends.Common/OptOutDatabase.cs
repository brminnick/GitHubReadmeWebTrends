using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GitHubReadmeWebTrends.Common
{
    public class OptOutDatabase
    {
        readonly static string _connectionString = Environment.GetEnvironmentVariable("DatabaseConnectionString") ?? string.Empty;
        readonly ILogger _logger;

        public OptOutDatabase(ILogger<OptOutDatabase> logger) => _logger = logger;

        public List<OptOutModel> GetAllOptOutModels()
        {
            using var connection = new DatabaseContext();

            var optOutDatabaseModelList = connection.OptOutDatabaseModel?.ToList() ?? Enumerable.Empty<OptOutDatabaseModel>();

            return optOutDatabaseModelList.Select(x => OptOutDatabaseModel.ToOptOutModel(x)).ToList();
        }

        public async Task<OptOutModel> GetOptOutModel(string id)
        {
            var optOutDatabaseModel = await PerformDatabaseFunction(context => getOptOutModelFunction(context, id), _logger).ConfigureAwait(false);
            return OptOutDatabaseModel.ToOptOutModel(optOutDatabaseModel);

            static Task<OptOutDatabaseModel> getOptOutModelFunction(DatabaseContext dataContext, string id) => dataContext.OptOutDatabaseModel.SingleAsync(x => x.Id.Equals(id));
        }

        public async Task<OptOutModel> InsertOptOutModel(OptOutModel optOutModel)
        {
            var optOutDatabaseModel = await PerformDatabaseFunction(context => insertOptOutModelFunction(context, optOutModel), _logger).ConfigureAwait(false);

            return OptOutDatabaseModel.ToOptOutModel(optOutDatabaseModel);

            static async Task<OptOutDatabaseModel> insertOptOutModelFunction(DatabaseContext dataContext, OptOutModel optOutModel)
            {
                var optOutDatabaseModel = OptOutDatabaseModel.ToOptOutDatabaseModel(optOutModel);

                var entityEntry = await dataContext.AddAsync(optOutDatabaseModel).ConfigureAwait(false);

                return entityEntry.Entity;
            }
        }

        public async Task<OptOutModel> PatchOptOutModel(OptOutModel optOutModel)
        {
            var optOutDatabaseModel = await PerformDatabaseFunction(context => patchOptOutModelFunction(context, optOutModel), _logger).ConfigureAwait(false);

            return OptOutDatabaseModel.ToOptOutModel(optOutDatabaseModel);

            static OptOutDatabaseModel patchOptOutModelFunction(DatabaseContext dataContext, OptOutModel optOutModel)
            {
                var entityEntry = dataContext.Update(OptOutDatabaseModel.ToOptOutDatabaseModel(optOutModel));

                return entityEntry.Entity;
            }
        }

        internal async Task<OptOutModel> RemoveOptOutModel(string id)
        {
            var optOutDatabaseModel = await PerformDatabaseFunction(context => removeContactDatabaseFunction(context, id), _logger).ConfigureAwait(false);
            return OptOutDatabaseModel.ToOptOutModel(optOutDatabaseModel);

            static async Task<OptOutDatabaseModel> removeContactDatabaseFunction(DatabaseContext dataContext, string id)
            {
                var optOutDatabaseModel = await dataContext.OptOutDatabaseModel.SingleAsync(x => x.Id.Equals(id)).ConfigureAwait(false);

                var entityEntry = dataContext.Remove(optOutDatabaseModel);

                return entityEntry.Entity;
            }
        }

        static async Task<TResult> PerformDatabaseFunction<TResult>(Func<DatabaseContext, TResult> databaseFunction, ILogger logger) where TResult : class
        {
            using var connection = new DatabaseContext();

            try
            {
                var result = databaseFunction(connection);
                await connection.SaveChangesAsync().ConfigureAwait(false);

                return result;
            }
            catch (Exception e)
            {
                logger.LogError(e, e.Message);
                throw;
            }
        }

        static async Task<TResult> PerformDatabaseFunction<TResult>(Func<DatabaseContext, Task<TResult>> databaseFunction, ILogger logger) where TResult : class
        {
            using var connection = new DatabaseContext();

            try
            {
                var result = await databaseFunction(connection).ConfigureAwait(false);
                await connection.SaveChangesAsync().ConfigureAwait(false);

                return result;
            }
            catch (Exception e)
            {
                logger.LogError(e, e.Message);
                throw;
            }
        }

        class DatabaseContext : DbContext
        {
            public DatabaseContext() => Database.EnsureCreated();

            public DbSet<OptOutDatabaseModel>? OptOutDatabaseModel { get; set; }

            protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) => optionsBuilder.UseSqlServer(_connectionString);

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                modelBuilder.Entity<OptOutDatabaseModel>().Property(b => b.CreatedAt).HasDefaultValue(DateTimeOffset.UtcNow);
                modelBuilder.Entity<OptOutDatabaseModel>().Property(b => b.UpdatedAt).HasDefaultValue(DateTimeOffset.UtcNow);
            }
        }

        class OptOutDatabaseModel : IOptOutModel
        {
            [Key, DatabaseGenerat‌ed(DatabaseGeneratedOp‌​tion.Identity)]
            public Guid Id { get; set; }

            public string Alias { get; set; } = string.Empty;

            public bool HasOptedOut { get; set; }

            [DatabaseGenerat‌ed(DatabaseGeneratedOp‌​tion.Identity)]
            public DateTimeOffset CreatedAt { get; set; }

            [DatabaseGenerat‌ed(DatabaseGeneratedOp‌​tion.Computed)]
            public DateTimeOffset UpdatedAt { get; set; }

            public static OptOutModel ToOptOutModel(OptOutDatabaseModel optOutDatabaseModel) =>
                new OptOutModel(optOutDatabaseModel.Id, optOutDatabaseModel.Alias, optOutDatabaseModel.HasOptedOut, optOutDatabaseModel.CreatedAt, optOutDatabaseModel.UpdatedAt);

            public static OptOutDatabaseModel ToOptOutDatabaseModel(OptOutModel optOutModel) => new OptOutDatabaseModel
            {
                Id = optOutModel.Id,
                Alias = optOutModel.Alias,
                HasOptedOut = optOutModel.HasOptedOut,
                CreatedAt = optOutModel.CreatedAt,
                UpdatedAt = optOutModel.UpdatedAt
            };
        }
    }
}