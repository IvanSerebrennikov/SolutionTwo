using Microsoft.AspNetCore.Http.Features;
using SolutionTwo.Api.Attributes;
using SolutionTwo.Api.Models;
using SolutionTwo.Common.MaintenanceStatusAccessor.Enums;
using SolutionTwo.Common.MaintenanceStatusAccessor.Interfaces;

namespace SolutionTwo.Api.Middlewares;

public class MaintenanceStatusCheckingMiddleware
{
    private readonly RequestDelegate _next;
    
    private readonly ILogger<MaintenanceStatusCheckingMiddleware> _logger;

    public MaintenanceStatusCheckingMiddleware(
        RequestDelegate next,
        ILogger<MaintenanceStatusCheckingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, IMaintenanceStatusGetter maintenanceStatusGetter)
    {
        if (!MaintenanceStatusShouldBeChecked(context, out var statuses) || statuses == null || statuses.Length == 0)
        {
            await _next(context);
            return;
        }

        var inaccessible = maintenanceStatusGetter.MaintenanceStatus.HasValue &&
                           statuses.Any(x => x == maintenanceStatusGetter.MaintenanceStatus.Value);
        if (inaccessible)
        {
            await HandleInaccessibilityAsync(context);
            return;
        }

        await _next(context);
    }

    private static bool MaintenanceStatusShouldBeChecked(HttpContext context, out MaintenanceStatus[]? statuses)
    {
        var endpoint = context.Features.Get<IEndpointFeature>()?.Endpoint;
        var maintenanceStatusAttribute =
            endpoint?.Metadata.GetMetadata<InaccessibleWhenMaintenanceStatusAttribute>();

        statuses = maintenanceStatusAttribute?.Statuses;

        return maintenanceStatusAttribute != null;
    }
    
    private async Task HandleInaccessibilityAsync(HttpContext context)
    {
        _logger.LogWarning(
            "Someone is trying to request end-point that inaccessible because of maintenance or deployment");

        context.Response.StatusCode = StatusCodes.Status503ServiceUnavailable;

        var traceId = context.TraceIdentifier;
        var errorResponse =
            new ErrorResponse("Requested end-point temporary inaccessible because of maintenance or deployment.",
                traceId);
        await context.Response.WriteAsJsonAsync(errorResponse);
    }
}