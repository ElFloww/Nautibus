This project has been initialized from the `AmetisAPI` template available [here](TODO_URL)

# Features
This template includes as a base a clean architecture using Cortex.Mediator and Autofac.

The API startup project (`AmetisAPI.API`) provides:
- ASP.NET Core Web API with API versioning (`Asp.Versioning.Mvc.ApiExplorer`) and Swagger (`Swashbuckle.AspNetCore`)
- Health checks (`AspNetCore.HealthChecks.*`, including SQL Server)
- Structured logging with Serilog (file sink, `Serilog.Exceptions` enrichment)
- Request validation with FluentValidation
- Object mapping with Mapster

Data storage: EF Core on SQL Server (`Microsoft.EntityFrameworkCore.SqlServer`), with HierarchyId and NetTopologySuite support. Access goes through the `IUnitOfWork` interface.

Other packages used by the layers:
- `Ardalis.GuardClauses` for argument guards
- `LinqKit.Core` for composable query predicates

Not part of this API: Dapper, Azure Tables, Azure Insights and Azure AD authentication.
