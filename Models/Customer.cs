namespace ISDN.Models
{
    public class Customer
    {
        public int customer_id { get; set; }
        public string first_name { get; set; }
        public string last_name { get; set; }
        public string street_address { get; set; }
        public string city { get; set; }
        public string zip_code { get; set; } // Zip Code
        public string phone_number { get; set; }
    }
}