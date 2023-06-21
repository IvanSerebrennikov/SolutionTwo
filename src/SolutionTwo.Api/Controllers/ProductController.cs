using Microsoft.AspNetCore.Mvc;
using SolutionTwo.Api.Attributes;
using SolutionTwo.Api.Controllers.Base;
using SolutionTwo.Business.Common.Constants;
using SolutionTwo.Business.Core.Models.Product.Incoming;
using SolutionTwo.Business.Core.Models.Product.Outgoing;
using SolutionTwo.Business.Core.Services.Interfaces;
using SolutionTwo.Common.MaintenanceStatusAccessor.Enums;

namespace SolutionTwo.Api.Controllers;

// плюс добавить ендпоинт который так же с бейзик аусом с пайплайна будет чекать есть ли сейчас заюзаные но 
// не зарелиженные продукты (мб с учетом времени когда был заюзан), и если есть, то не начинать деплой и снова
// чекать через какой-то интервал, в цикле или каким-то скедулинком/ретраем в пайплайне. 

// контроллер который будет содержать эти 3 новых ендпоинта кинуть в отдельную папку типо Internal, 
// и _дев тестинг туда же перенести, норм переименовать и бейзик аус на него повесить. 

// на примере этого всего еще можно будет поиграться и сделать скедулер который будет релизить продукт юсейджи
// которые уже слишком давно заюзаны но так и не зарелизелись (допустим потенциально такое могло произойти из-за
// косяка самого юзера, стороннего сервиса, UI приложения, etc).
// но при релизе скедулером пусть еще проставляется параметр типо IsForceRelease = true

// заодно скедулер для удаления старых рефреш токенов можно сделать за компанию

// скедулер делать как таймер-бейсд ажур функцию

/// <summary>
/// <see cref="UseProduct"/> and <see cref="ReleaseProduct"/> simulates some business flow
/// where if one action executed, another also should be 100% executed in the nearest future (after 0-1-2-5-10 minutes).
/// So it should be incorrect and can break business flow if start maintenance/deploy (stop Application)
/// while the first action has already been executed by some users,
/// but the associated second action has not yet been executed.
/// To prevent first action requesting before deployment - next components used:
/// InaccessibleWhenMaintenanceStatusAttribute,
/// MaintenanceStatusCheckingMiddleware,
/// MaintenanceStatusAccessor,
/// ApplicationManagingController.Set/ResetMaintenanceStatus methods (that should be called from Deploy pipeline)
/// To prevent deployment if first action was requested by any users but related second wasn't - next components used:
/// ApplicationManagingController.CheckProductsThatAreCurrentlyInActiveUse (that should be called from Deploy pipeline
/// after ApplicationManagingController.SetMaintenanceStatus was called)
///
/// Also simulates some business flow that can be broken because of parallel user access.
/// For example Product has MaxActiveUsagesCount = 3, so if current User want to use provided Product,
/// but 3 other Users are already using it, current User should not pass ProductUsages checking and
/// receive corresponding message.
/// But if only 2 other Users are already using this Product, user should successfully pass ProductUsages
/// checking and new ProductUsage entry should be created for him. But if there will be 2 such requests in
/// approximately the same moment, it can be possible if both 2 Users successfully pass ProductUsages checking and
/// new ProductUsage entries will be created for both of them, and as result there will be 4 simultaneous usages
/// for this product, that is incorrect.
/// So need to handle this with DB transaction that will lock write access to ProductUsages table during
/// ProductUsages checking and new ProductUsage entry creation.
/// Because of there are new DB rows could be created, need to lock whole table and use Serializable isolation level,
/// but it may significantly affects performance and API accessibility, so added some de-normalization with new
/// column/property CurrentActiveUsagesCount to the ProductEntity and make it possible to use RepeatableRead or Snapshot
/// isolation level that will lock only one row in Products table instead of full ProductUsages table lock.
/// Also now with this de-normalization there is possible to use optimistic concurrency solution
/// with ConcurrencyVersion column/property in ProductEntity instead of RepeatableRead/Snapshot transaction,
/// and avoid any DB locks at all.
/// Check SolutionTwo.Data.Common.Features.OptimisticConcurrency.OptimisticConcurrencyContextBehavior, it's usages
/// and ProductService.UpdateProductAsync/UseProductAsync/ReleaseProduct methods to see how it works.
/// </summary>
[SolutionTwoAuthorize]
[Route("api/[controller]")]
[ApiController]
public class ProductController : ApiAuthorizedControllerBase
{
    private readonly IProductService _productService;

    public ProductController(IProductService productService)
    {
        _productService = productService;
    }
    
    [InaccessibleWhenMaintenanceStatus(MaintenanceStatus.LessThen2MinBeforeDeployment)]
    [SolutionTwoAuthorize(UserRoles.TenantAdmin, UserRoles.TenantUser)]
    [HttpPost("{id}/use")]
    public async Task<ActionResult> UseProduct(Guid id)
    {
        var result = await _productService.UseProductAsync(id);

        if (!result.IsSucceeded)
            return BadRequest(result.Message);

        return Ok();
    }
    
    [SolutionTwoAuthorize(UserRoles.TenantAdmin, UserRoles.TenantUser)]
    [HttpPost("{id}/release")]
    public async Task<ActionResult> ReleaseProduct(Guid id)
    {
        var result = await _productService.ReleaseProductAsync(id);

        if (!result.IsSucceeded)
            return BadRequest(result.Message);

        return Ok();
    }
    
    [SolutionTwoAuthorize(UserRoles.TenantAdmin)]
    [HttpPost]
    public async Task<ActionResult> CreateProduct(CreateProductModel createProductModel)
    {
        var serviceResult = await _productService.CreateProductAsync(createProductModel);
        
        if (!serviceResult.IsSucceeded || serviceResult.Data == null)
            return BadRequest(serviceResult);
        
        var productModel = serviceResult.Data;

        return CreatedAtAction(nameof(GetById), new { id = productModel.Id }, productModel);
    }
    
    [SolutionTwoAuthorize(UserRoles.TenantAdmin)]
    [HttpPut]
    public async Task<ActionResult> UpdateProduct(UpdateProductModel updateProductModel)
    {
        var serviceResult = await _productService.UpdateProductAsync(updateProductModel);
        
        if (!serviceResult.IsSucceeded)
            return BadRequest(serviceResult);
        
        return Ok();
    }
    
    [SolutionTwoAuthorize(UserRoles.TenantAdmin)]
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ProductWithActiveUsagesModel>>> GetAll()
    {
        var productModels = await _productService.GetAllProductsWithActiveUsagesAsync();

        return Ok(productModels);
    }

    [SolutionTwoAuthorize(UserRoles.TenantAdmin)]
    [HttpGet("{id}")]
    public async Task<ActionResult<ProductWithActiveUsagesModel>> GetById(Guid id)
    {
        var productModel = await _productService.GetProductWithActiveUsagesByIdAsync(id);

        if (productModel == null)
            return NotFound();
        
        return Ok(productModel);
    }

    [SolutionTwoAuthorize(UserRoles.TenantAdmin)]
    [HttpDelete]
    public async Task<ActionResult> DeleteUser(Guid id)
    {
        var result = await _productService.DeleteProductAsync(id);

        if (!result.IsSucceeded)
            return BadRequest(result.Message);

        return Ok();
    }
}