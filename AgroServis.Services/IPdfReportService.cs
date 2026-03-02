using AgroServis.Services.DTO;

using AgroServis.Services.DTO;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgroServis.Services
{
    public interface IPdfReportService
    {
        byte[] GenerateMaintenanceReport(
        IReadOnlyList<MaintenanceDto> data,
        MaintenanceReportOptionsDto options
    );
    }
}