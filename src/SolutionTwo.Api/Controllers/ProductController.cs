using Microsoft.AspNetCore.Mvc;
using SolutionTwo.Api.Attributes;
using SolutionTwo.Api.Controllers.Base;
using SolutionTwo.Business.Common.Constants;

namespace SolutionTwo.Api.Controllers;

// пусть при юзе продукта создается запись в базе (ProductUsage)
// с продукт айди, юзер айди (кто использует) и временем когда начал использовать 
// а при релизе в эту запись еще добавится релизд дейт тайм 

// один юзер не может дважды заюзать один и тот же продукт. 
// один продукт не может быть заюзан более чем определенным кол-вом юзеров (=определенное кол-во раз) одноврменно
// которое указано в одном из свойств продукта (MaxNumberOfSimultaneousUsages)

// надо добавить какой-то параметр в базу/конфиг/синглтон сервис, который будет выставляться 
// через колл на определенный ендпоинт через бейзик аус (с релиз пайплайна),
// плюс если можно добавить еще возможность бейзик ауса для сваггера чтобы можно было самому 
// постучать на этот ендпоинт потестить. 
// и если этот параметр выставлен, то продукт нельзя заюзать.
// реализовать желательно через какой-то атрибут ([MustBeInaccessibleBeforeMaintenance])
// который можно будет повесить на этот экшен и на любые подобные потом если надо будет,
// и новый мидлвеа будет чекать этот атрибут, и если он есть, то через новый сервис лезть в
// базу/конфиг/синглтон сервис и чекать этот параметр, и если он выствлен, то возвращать 
// соответствующее сообщение и статус, что сейчас ничего нельзя юзать так как скоро будет обновление приложения.
// проще и лучше с точки зрения перформанса будет сделать просто через синглтон сервис в апп проекте
// (плюс значение автоматом обнулится после рестарта прилаги при деплое) 
// но ендпоинт на отмену этого параметра лучше тоже на всякий добавить.
// плюс добавить ендпоинт который так же с бейзик аусом с пайплайна будет чекать есть ли сейчас заюзаные но 
// не зарелиженные продукты (мб с учетом времени когда был заюзан), и если есть, то не начинать деплой и снова
// чекать через какой-то интервал, в цикле или каким-то скедулинком или ретраем. 

// контроллер который будет содержать эти 3 новых ендпоинта кинуть в отдельную папку типо Internal, 
// и _дев тестинг туда же перенести, норм переименовать и бейзик аус на него повесить. 

// еще на примере этой фичи с продуктами и MaxNumberOfSimultaneousUsages 
// можно будет поиграться в имплемент транзакций с 3+ изолейшен левелом 
// тут скорее всего нужен будет 4 лвл, так как записи ProductUsage не только меняются, но и добавляются 

// на примере этого всего еще можно будет поиграться и сделать скедулер который будет релизить продукт юсейджи
// которые уже слишком давно заюзаны но так и не зарелизелись (допустим потенциально такое могло произойти из-за
// косяка самого юзера, стороннего сервиса, UI приложения, etc).
// но при релизе скедулером пусть еще проставляется параметр типо IsForceRelease = true

// еак же продукт сделать как IAuditableEntity и добавить для этого новый бехавиор контекста

/// <summary>
/// <see cref="UseProduct"/> and <see cref="ReleaseProduct"/> simulates some business flow
/// where if one action executed, another also should be 100% executed in the nearest future (after 0-1-2-5-10 minutes).
/// So it should be incorrect and can break business flow if start maintenance/deploy (stop Application)
/// while the first action has already been executed by some users,
/// but the associated second action has not yet been executed.
///
/// TODO: add more info about implementation (attribute, singleton service, middleware, scheduler)
/// TODO: after implementation will be finished
/// </summary>
[SolutionTwoAuthorize]
[Route("api/[controller]")]
[ApiController]
public class ProductController : ApiAuthorizedControllerBase
{
    /// <summary>
    /// Simulates some business flow that can be broken because of parallel user access.
    /// For example Product has MaxNumberOfSimultaneousUsages = 3, so if current User want to use provided Product,
    /// but 3 other Users already use it, current User should not pass ProductUsages checking and
    /// receive corresponding message.
    /// But if for example only 2 other Users already use this Product, user should successfully pass ProductUsages
    /// checking and new ProductUsage entry should be created for him. But if there will be 2 such requests in
    /// approximately the same moment, it can be possible if both 2 Users successfully pass ProductUsages checking and
    /// new ProductUsage entries will be created for both of them, and as result there will be 4 simultaneous usages
    /// for this product, that is incorrect.
    /// So need to handle this with DB transaction that will lock access to ProductUsages table during ProductUsages
    /// checking and new ProductUsage entry creation.
    ///
    /// TODO: тут скорее всего придется делать 4 уровень изоляции на лок всей ProductUsages таблицы. Может подумать
    /// TODO: над оптимизацией и сделать небольшую денормализацию и добавить еще в продукт св-во CurrentNumberOfSimultaneousUsages
    /// TODO: и тогда можно будет делать 3 уровень изоляции просто локая ту строку что начала читаться в продукт таблице.
    /// TODO: но тогда 3 уровень нужно будет делать еще и при релизе продукта, так как в один момент 2+ юзера могут захотеть
    /// TODO: релизнуть один и тот же продукт.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [SolutionTwoAuthorize(UserRoles.TenantAdmin, UserRoles.TenantUser)]
    [HttpPost("{id}/use")]
    public async Task<ActionResult> UseProduct(Guid id)
    {
        return Ok();
    }
    
    [SolutionTwoAuthorize(UserRoles.TenantAdmin, UserRoles.TenantUser)]
    [HttpPost("{id}/release")]
    public async Task<ActionResult> ReleaseProduct(Guid id)
    {
        return Ok();
    }
    
    [SolutionTwoAuthorize(UserRoles.TenantAdmin)]
    [HttpPost]
    public async Task<ActionResult> CreateProduct()
    {
        return Ok();
    }
}