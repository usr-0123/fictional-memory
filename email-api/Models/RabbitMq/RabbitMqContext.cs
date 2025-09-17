using RabbitMQ.Client;

namespace background_api.Models.RabbitMq;

public class RabbitMqContext(IConnection connection, IModel channel)
{
    public IConnection Connection { get; } = connection;
    public IModel Channel { get; } = channel;

    public void Dispose()
    {
        Channel.Dispose();
        Connection.Dispose();
    }
}