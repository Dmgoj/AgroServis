using AgroServis.DAL.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgroServis.Services
{
    public interface IEmailService
    {
        Task SendAdminApprovalNotificationAsync(PendingRegistration registration);
        Task SendApprovalConfirmationAsync(string email, string firstName);
        Task SendRejectionNotificationAsync(string email, string firstName);
    }
}