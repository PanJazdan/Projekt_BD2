
namespace UdtReaderApp.Models
{
    public class User
    {
        public int Id { get; set; }
        public Email ContactEmail { get; set; } // Używamy naszego typu CLR UDT Email
    }
}