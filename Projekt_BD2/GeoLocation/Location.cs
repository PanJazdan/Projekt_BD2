namespace UdtReaderApp.Models
{
    public class Location
    {
        public int Id { get; set; }
        public GeoLocation Loc { get; set; }

        public string Name { get; set; }
    }
}