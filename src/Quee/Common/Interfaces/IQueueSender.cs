#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace Quee;


/// <summary>
/// Sends a message of <typeparamref name="TMessage"/> into the queue for the specified message type
/// </summary>
/// <typeparam name="TMessage">Message to send to the queue</typeparam>
public interface IQueueSender<TMessage> where TMessage : class
{
    /// <summary>
    /// Send a message into the queue for the provided <typeparamref name="TMessage"/> object
    /// </summary>
    /// <param name="message">Message to send</param>
    /// <param name="cancellationToken">Process token</param>
    /// <param name="queueDelay">Delay added before the intial message is sent to the queue</param>
    Task SendMessageAsync(TMessage message, CancellationToken cancellationToken, TimeSpan? queueDelay = null);
}
