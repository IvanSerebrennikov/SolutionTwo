using SolutionTwo.Common.MaintenanceStatusAccessor.Enums;

namespace SolutionTwo.Api.Attributes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public class InaccessibleWhenMaintenanceStatusAttribute : Attribute
{
    public InaccessibleWhenMaintenanceStatusAttribute(params MaintenanceStatus[] statuses)
    {
        Statuses = statuses;
    }

    public MaintenanceStatus[] Statuses { get; }
}