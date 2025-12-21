using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgroServis.Services.Exceptions
{
    public class EntityNotFoundException : Exception
    {
        public object EntityId { get; }
        public string EntityName { get; }

        public EntityNotFoundException(string entityName, object entityId)
            : base($"{entityName} with ID {entityId} not found.")
        {
            EntityName = entityName;
            EntityId = entityId;
        }

        public EntityNotFoundException()
        {
        }
        public EntityNotFoundException(string message) : base(message)
        {
        }
        public EntityNotFoundException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}