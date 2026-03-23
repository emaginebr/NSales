using System;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Lofn.DTO.Settings;
using Lofn.Infra.Interfaces.AppService;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace Lofn.Infra.AppService
{
    public class RabbitMQAppService : IRabbitMQAppService, IAsyncDisposable
    {
        private readonly RabbitMQSetting _settings;
        private readonly ILogger<RabbitMQAppService> _logger;
        private IConnection _connection;
        private IChannel _channel;

        public RabbitMQAppService(
            IOptions<RabbitMQSetting> settings,
            ILogger<RabbitMQAppService> logger)
        {
            _settings = settings.Value;
            _logger = logger;
        }

        public async Task PublishAsync<T>(T message)
        {
            await EnsureConnectionAsync();

            var json = JsonSerializer.Serialize(message);
            var body = Encoding.UTF8.GetBytes(json);

            await _channel.BasicPublishAsync(
                exchange: string.Empty,
                routingKey: _settings.QueueName,
                body: body);

            _logger.LogInformation("Message published to queue {QueueName}", _settings.QueueName);
        }

        private async Task EnsureConnectionAsync()
        {
            if (_connection != null && _connection.IsOpen)
                return;

            var factory = new ConnectionFactory
            {
                HostName = _settings.HostName,
                Port = _settings.Port,
                UserName = _settings.UserName,
                Password = _settings.Password
            };

            _connection = await factory.CreateConnectionAsync();
            _channel = await _connection.CreateChannelAsync();

            await _channel.QueueDeclareAsync(
                queue: _settings.QueueName,
                durable: true,
                exclusive: false,
                autoDelete: false);
        }

        public async ValueTask DisposeAsync()
        {
            if (_channel != null)
                await _channel.CloseAsync();
            if (_connection != null)
                await _connection.CloseAsync();
        }
    }
}
