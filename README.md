# SolutionTwo main features

## DataBase related features

### Base Generic Repository
 - [x] Maximum flexibility (projection, filter, orderBy, includeMany, include, skip, take, withTracking)
 - [x] No Tracking by default
 - [x] No SaveChanges
 - [x] Update with possibility to set IsModified only to provided properties
 - [x] Async
 - [x] All collections returned as IReadOnlyList
 - [x] Abstract class that can be inherited and extended

### Base Unit Of Work
 - [x] SaveChangesAsync
 - [x] SaveChanges
 - [x] ExecuteInTransactionWithRetry
   -  `...TResult? ExecuteInTransactionWithRetry<TResult>(Func<TResult> funcToBeExecuted...`
 - [x] ExecuteInTransactionWithRetryAsync
   -  `...Task<TResult?> ExecuteInTransactionWithRetryAsync<TResult>(Func<Task<TResult>> funcToBeExecuted...`
 - [x] Transactions implemented with Context.Database.BeginTransaction with ability to set up isolation level, max retry count and delay between retries

### Base EF DB Context

 - [x] Aggregate Behaviors dictionary that populated during concrete Context (inherited from base Context) creation
   - each existing behavior executes as BeforeSaveChanges logic (for global write logic)...
   - ...and as AddGlobalQueryFilter during OnModelCreating (for global read logic)

### Features (aka possible Context behaviors)

 - [x] Audit
   - retrieve logged in user info with injected by DI ILoggedInUserGetter
   - BeforeSaveChanges: automatically set CreatedBy and CreatedDateTimeUtc properties for entities that implement IAuditableOnCreateEntity iterface
   - BeforeSaveChanges: automatically set LastModifiedBy and LastModifiedDateTimeUtc properties for entities that implement IAuditableOnUpdateEntity iterface
   - BeforeSaveChanges: do not set audit properties if all modified properties marked by [IgnoreAudit] attribute
 - [x] Multi Tenancy
   - retrieve tenant info (TenantId or AllTenantsAccessible) with injected by DI ITenantAccessGetter
   - AddGlobalQueryFilter: additionally filter all queries by user's TenantId for all entities that implement IOwnedByTenantEntity interface if not AllTenantsAccessible 
   - BeforeSaveChanges: automatically set TenantId property for entities that implement IOwnedByTenantEntity iterface if not AllTenantsAccessible 
 - [x] Optimistic Concurrency
   - BeforeSaveChanges: automatically set ConcurrencyVersion property for entities that implement IConcurrencyVersionedEntity iterface if any of modified properties marked by [ChangeConcurrencyVersionOnUpdate] attribute
   - realized from IConcurrencyVersionedEntity interface ConcurrencyVersion property should be marked by data annotation [ConcurrencyCheck] attribute
   - this solution is alternative concurrency handling solution and better from performance and application accessibility point of view then RepeatableRead transactions 
 - [x] Soft Deletion
   - retrieve logged in user info with injected by DI ILoggedInUserGetter
   - AddGlobalQueryFilter: additionally filter all queries by DeletedDateTimeUtc for all entities that implement ISoftDeletableEntity interface
   - BeforeSaveChanges: automatically set DeletedBy and DeletedDateTimeUtc properties for entities that implement ISoftDeletableEntity iterface

## API related features

### Middlewares
 - [x] GlobalErrorHandlingMiddleware 
   - ... 
 - [x] MaintenanceStatusCheckingMiddleware
   - ... 
 - [x] BasicAuthenticationMiddleware
   - ...
 - [x] TokenBasedAuthenticationMiddleware
   - ... 
 - [x] RoleBasedAuthorizationMiddleware
   - ... 
 - [x] TenantAccessSetupMiddleware
   - ... 

### Identity 
 - [x] One-way passwords hashing with salt 
   - ...
 - [x] JWT token based authentication 
   - ...
 - [x] Role based authorization 
   - ...
 - [x] Refresh tokens 
   - ...
 - [x] Automatic all user's refresh and access tokens deactivation in case of suspicion of refresh token stealing 
   - ...
 - [x] Ability for admin to deactivate all user's refresh and access tokens (log out user)
   - ...

## Dev Ops and App maintenance features

### YAML pipelines 
 - [x] Build pipeline 
   - ...
 - [ ] Release pipeline 
   - ...

### Maintenance
 - [ ] Azure timer-based function to remove expired refresh tokens 
   - ...
 - [x] Azure timer-based function to handle some business logic 
   - ...
 - [ ] Ability to denay access to some end-points some time before/after deployment/maintenance
   - ...

## Some helpers

 - [x] DashedLowercaseParameterTransformer
   - ...
 - [x] ValueAssertion
   - ...
 - [x] IConfiguration.GetSection
   - ...
 - [x] ModelBuilder.AppendGlobalQueryFilter
   - ...
