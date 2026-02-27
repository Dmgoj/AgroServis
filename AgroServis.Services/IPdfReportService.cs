using AgroServis.Services.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgroServis.Services
{
    public interface IPdfReportService
    {
        byte[] GenerateMaintenanceReport(IEnumerable<MaintenanceDto> items);
    }
}