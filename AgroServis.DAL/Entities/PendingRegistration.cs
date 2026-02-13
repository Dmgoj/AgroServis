using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgroServis.DAL.Entities
{
    public class PendingRegistration
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Position { get; set; }
        public string PasswordHash { get; set; }
        public DateTime RequestedAt { get; set; }
        public bool IsProcessed { get; set; } = false;
        public string ApprovalToken { get; set; }
        public DateTime TokenExpiresAt { get; set; }
    }
}