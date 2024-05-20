namespace WebHook.DTOs
{
    public class StaffIntegrationDTO
    {
        public List<Datum> data { get; set; }
    }

    public class Datum
    {
        public string email { get; set; }
        public string first_name { get; set; }
        public string integration_uid { get; set; }
        public string last_name { get; set; }
    }
}
