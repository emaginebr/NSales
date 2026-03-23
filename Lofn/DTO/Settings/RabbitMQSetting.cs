namespace Lofn.DTO.Settings
{
    public class RabbitMQSetting
    {
        public string HostName { get; set; }
        public int Port { get; set; } = 5672;
        public string UserName { get; set; }
        public string Password { get; set; }
        public string QueueName { get; set; }
    }
}
