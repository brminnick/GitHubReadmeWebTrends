﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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

        public async Task<IReadOnlyList<OptOutModel>> GetAllOptOutModels()
        {
            using var connection = new DatabaseContext();

            var optOutDatabaseModelList = await connection.OptOutDatabaseModel.ToListAsync().ConfigureAwait(false);

            return optOutDatabaseModelList.Select(x => OptOutDatabaseModel.ToOptOutModel(x)).ToList();
        }

        public async Task<OptOutModel?> GetOptOutModel(string alias)
        {
            using var connection = new DatabaseContext();

            var optOutDatabaseModel = await connection.OptOutDatabaseModel.SingleOrDefaultAsync(x => x.Alias == alias).ConfigureAwait(false);

            if (optOutDatabaseModel is null)
                return null;

            return OptOutDatabaseModel.ToOptOutModel(optOutDatabaseModel);
        }

        public async Task<OptOutModel> InsertOptOutModel(OptOutModel optOutModel)
        {
            var optOutDatabaseModel = await PerformDatabaseFunction(context => insertOptOutModelFunction(context, optOutModel), _logger).ConfigureAwait(false);

            return OptOutDatabaseModel.ToOptOutModel(optOutDatabaseModel);

            static async Task<OptOutDatabaseModel> insertOptOutModelFunction(DatabaseContext dataContext, OptOutModel optOutModel)
            {
                var optOutDatabaseModel = OptOutDatabaseModel.ToOptOutDatabaseModel(optOutModel);
                optOutDatabaseModel.UpdatedAt = optOutDatabaseModel.CreatedAt = DateTimeOffset.UtcNow;

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
                var optOutDatabaseModel = OptOutDatabaseModel.ToOptOutDatabaseModel(optOutModel);
                optOutDatabaseModel.UpdatedAt = DateTimeOffset.UtcNow;

                var entityEntry = dataContext.Update(optOutDatabaseModel);

                return entityEntry.Entity;
            }
        }

        internal async Task<OptOutModel> RemoveOptOutModel(string id)
        {
            var optOutDatabaseModel = await PerformDatabaseFunction(context => removeContactDatabaseFunction(context, id), _logger).ConfigureAwait(false);
            return OptOutDatabaseModel.ToOptOutModel(optOutDatabaseModel);

            static async Task<OptOutDatabaseModel> removeContactDatabaseFunction(DatabaseContext dataContext, string alias)
            {
                var optOutDatabaseModel = await dataContext.OptOutDatabaseModel.SingleAsync(x => x.Alias.Equals(alias)).ConfigureAwait(false);

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
        }

        class OptOutDatabaseModel : IOptOutModel
        {
            [Key]
            public string Alias { get; set; } = string.Empty;

            public bool HasOptedOut { get; set; }

            public DateTimeOffset CreatedAt { get; set; }

            public DateTimeOffset UpdatedAt { get; set; }

            public static OptOutModel ToOptOutModel(OptOutDatabaseModel optOutDatabaseModel) =>
                new OptOutModel(optOutDatabaseModel.Alias, optOutDatabaseModel.HasOptedOut, optOutDatabaseModel.CreatedAt, optOutDatabaseModel.UpdatedAt);

            public static OptOutDatabaseModel ToOptOutDatabaseModel(OptOutModel optOutModel) => new OptOutDatabaseModel
            {
                Alias = optOutModel.Alias,
                HasOptedOut = optOutModel.HasOptedOut,
                CreatedAt = optOutModel.CreatedAt,
                UpdatedAt = optOutModel.UpdatedAt
            };
        }
    }
}