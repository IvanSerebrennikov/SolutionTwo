using SolutionTwo.Common.MaintenanceStatusAccessor.Enums;
using SolutionTwo.Common.MaintenanceStatusAccessor.Interfaces;

namespace SolutionTwo.Common.MaintenanceStatusAccessor;

public class MaintenanceStatusAccessor : IMaintenanceStatusGetter, IMaintenanceStatusSetter
{
    public MaintenanceStatus? MaintenanceStatus { get; private set; }
    
    public void SetMaintenanceStatus(MaintenanceStatus? maintenanceStatus)
    {
        MaintenanceStatus = maintenanceStatus;
    }
}