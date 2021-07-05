using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace GitHubReadmeWebTrends.Common
{
    public class OptOutDatabase
    {
        readonly OptOutDbContext _dbContext;

        public OptOutDatabase(OptOutDbContext optOutDbContext) => _dbContext = optOutDbContext;

        public async Task<IReadOnlyList<OptOutModel>> GetAllOptOutModels() =>
            await _dbContext.OptOutDatabaseModel.ToListAsync().ConfigureAwait(false);

        public async Task<OptOutModel?> GetOptOutModel(string alias) =>
            await _dbContext.OptOutDatabaseModel.SingleOrDefaultAsync(x => x.Alias == alias).ConfigureAwait(false);

        public async Task<OptOutModel> InsertOptOutModel(OptOutModel optOutModel)
        {
            var currentTimeStamp = DateTimeOffset.UtcNow;

            optOutModel = optOutModel with
            {
                UpdatedAt = currentTimeStamp,
                CreatedAt = currentTimeStamp
            };

            var entityEntry = await _dbContext.AddAsync(optOutModel).ConfigureAwait(false);
            await _dbContext.SaveChangesAsync().ConfigureAwait(false);

            return entityEntry.Entity;
        }

        public async Task<OptOutModel> PatchOptOutModel(OptOutModel optOutModel)
        {
            optOutModel = optOutModel with { UpdatedAt = DateTimeOffset.UtcNow };

            var entityEntry = _dbContext.Update(optOutModel);
            await _dbContext.SaveChangesAsync().ConfigureAwait(false);

            return entityEntry.Entity;
        }

        internal async Task<OptOutModel> RemoveOptOutModel(string id)
        {
            var optOutDatabaseModel = await _dbContext.OptOutDatabaseModel.SingleAsync(x => x.Alias.Equals(id)).ConfigureAwait(false);

            var entityEntry = _dbContext.Remove(optOutDatabaseModel);
            await _dbContext.SaveChangesAsync().ConfigureAwait(false);

            return entityEntry.Entity;
        }
    }
}