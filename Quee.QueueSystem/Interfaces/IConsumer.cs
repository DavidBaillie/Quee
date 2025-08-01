namespace Quee.Interfaces;

/// <summary>
/// Defines a class that will consume messages of <typeparamref name="T"/> from the queue and handle a fault when the message cannot be consumed 
/// </summary>
/// <typeparam name="T">Message being sent in the queue</typeparam>
public interface IConsumer<T> where T : class
{
    /// <summary>
    /// Consumes the base message from the queue
    /// </summary>
    /// <param name="message">Message sent to queue</param>
    /// <param name="cancellationToken">Process token</param>
    Task ConsumeAsync(T message, CancellationToken cancellationToken);

    /// <summary>
    /// Consumes the base message when an exception occurred in the <see cref="ConsumeAsync(T, CancellationToken)"/> method
    /// </summary>
    /// <param name="fault"></param>
    /// <param name="cancellationToken"></param>
    Task ConsumeFaultAsync(IFault<T> fault, CancellationToken cancellationToken);
}
