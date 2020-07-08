namespace ConfigManagement.Models.Config
{
    public class PublicDataAdapters
    {
        public string DataAdapterAlias { get; set; }
        public string DataAdapterType { get; set; }
        public string DataAdapterInfo { get; set; }
        public string IsDefaultDataAdapter { get; set; } = "0";
        public string DataAdapterAccountName { get; set; }
    }
}
