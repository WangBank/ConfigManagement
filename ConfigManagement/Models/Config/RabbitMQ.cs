namespace ConfigManagement.Models.Config
{
    public class RabbitMQ
    {
        public string ServerUrl { get; set; }
        public string ServerPort { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string ExchangeName { get; set; }
        public string ExchangeType { get; set; }
        public string QueueName { get; set; }
        public string RoutingKey { get; set; }
        public string Durable { get; set; }
        public string Exclusive { get; set; }
        public string AutoDelete { get; set; }
    }
}
