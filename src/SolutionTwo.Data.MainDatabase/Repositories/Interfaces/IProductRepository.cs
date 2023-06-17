using SolutionTwo.Data.Common.Repositories.Interfaces;
using SolutionTwo.Data.MainDatabase.Entities;

namespace SolutionTwo.Data.MainDatabase.Repositories.Interfaces;

public interface IProductRepository : IBaseRepository<ProductEntity, Guid>
{
    
}