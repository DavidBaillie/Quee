using Quee.Interfaces;

namespace Quee.Services.Common;

internal sealed class QueueConfigurator : IQueueConfigurator
{
    public IQueueConfigurator AddQueueConsumer<TMessage, TConsumer>(string queueName)
        where TMessage : class
        where TConsumer : IConsumer<TMessage>
    {
        throw new NotImplementedException();
    }

    public IQueueConfigurator AddQueueProcessors<TMessage, TConsumer>(string queueName)
        where TMessage : class
        where TConsumer : IConsumer<TMessage>
    {
        throw new NotImplementedException();
    }

    public IQueueConfigurator AddQueueSender<TMessage>(string queueName) where TMessage : class
    {
        throw new NotImplementedException();
    }
}
