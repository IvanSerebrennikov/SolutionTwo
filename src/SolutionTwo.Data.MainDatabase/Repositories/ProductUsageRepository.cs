using SolutionTwo.Data.Common.Repositories;
using SolutionTwo.Data.MainDatabase.Context;
using SolutionTwo.Data.MainDatabase.Entities;
using SolutionTwo.Data.MainDatabase.Repositories.Interfaces;

namespace SolutionTwo.Data.MainDatabase.Repositories;

public class ProductUsageRepository : BaseRepository<ProductUsageEntity, Guid>, IProductUsageRepository
{
    public ProductUsageRepository(MainDatabaseContext context) : base(context)
    {
    }
}