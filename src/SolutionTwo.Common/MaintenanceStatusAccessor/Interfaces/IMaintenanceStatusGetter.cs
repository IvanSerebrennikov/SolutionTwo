using SolutionTwo.Common.MaintenanceStatusAccessor.Enums;

namespace SolutionTwo.Common.MaintenanceStatusAccessor.Interfaces;

public interface IMaintenanceStatusGetter
{
    MaintenanceStatus? MaintenanceStatus { get; }
}