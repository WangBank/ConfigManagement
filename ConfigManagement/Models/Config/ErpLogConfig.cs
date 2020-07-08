namespace ConfigManagement.Models.Config
{
    public class ErpLogConfig
    {

        public Mqconnmodel MQConnModel { get; set; }

        public class Mqconnmodel
        {
            public Connectionmodel ConnectionModel { get; set; }
            public string MQType { get; set; }
            public Consumeinfo[] ConsumeInfo { get; set; }
        }

        public class Connectionmodel
        {
            public string Host { get; set; }
            public string Port { get; set; }
            public string UserName { get; set; }
            public string Password { get; set; }
            public string VirtualHost { get; set; }
        }

        public class Consumeinfo
        {
            public string ExchangeName { get; set; }
            public string[] QueueName { get; set; }
        }

    }
}
