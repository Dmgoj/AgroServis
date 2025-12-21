namespace AgroServis.Services.Exceptions
{
    public class DuplicateEntityException : Exception
    {
        public string SerialNumber { get; }

        public DuplicateEntityException(string serialNumber)
            : base($"Equipment with serial number '{serialNumber}' already exists.")
        {
            SerialNumber = serialNumber;
        }

        public DuplicateEntityException(string serialNumber, string message)
            : base(message)
        {
            SerialNumber = serialNumber;
        }

        public DuplicateEntityException(string serialNumber, string message, Exception innerException)
            : base(message, innerException)
        {
            SerialNumber = serialNumber;
        }
    }
}