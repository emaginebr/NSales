using System.Threading.Tasks;

namespace Lofn.Infra.Interfaces.AppService
{
    public interface IRabbitMQAppService
    {
        Task PublishAsync<T>(T message);
    }
}
