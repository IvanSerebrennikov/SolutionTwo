using SolutionTwo.Data.Common.Repositories;
using SolutionTwo.Data.MainDatabase.Context;
using SolutionTwo.Data.MainDatabase.Entities;
using SolutionTwo.Data.MainDatabase.Repositories.Interfaces;

namespace SolutionTwo.Data.MainDatabase.Repositories;

public class ProductRepository : BaseRepository<ProductEntity, Guid>, IProductRepository
{
    public ProductRepository(MainDatabaseContext context) : base(context)
    {
    }
}