using SolutionTwo.Common.MaintenanceStatusAccessor.Enums;

namespace SolutionTwo.Common.MaintenanceStatusAccessor.Interfaces;

public interface IMaintenanceStatusSetter
{
    void SetMaintenanceStatus(MaintenanceStatus? maintenanceStatus);
}