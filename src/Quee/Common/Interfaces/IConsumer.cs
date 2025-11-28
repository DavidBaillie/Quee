#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace Quee;

/// <summary>
/// Defines the structure which consumes messages of <typeparamref name="T"/> from the queue and handle faults when the message cannot be consumed
/// </summary>
/// <typeparam name="T">Message being sent in the queue</typeparam>
public interface IConsumer<T> where T : class
{
    /// <summary>
    /// Consumes the base message from the queue.
    /// </summary>
    /// <param name="message">Message sent to queue</param>
    /// <param name="cancellationToken">Process token</param>
    Task ConsumeAsync(Message<T> message, CancellationToken cancellationToken);

    /// <summary>
    /// Consumes the base message when an exception occurred in the <see cref="ConsumeAsync(T, CancellationToken)"/> method
    /// </summary>
    /// <param name="fault"></param>
    /// <param name="cancellationToken"></param>
    Task ConsumeFaultAsync(FaultMessage<T> fault, CancellationToken cancellationToken);
}
