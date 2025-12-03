#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace Quee;

/// <summary>
/// Defines the message a developer will receive when a consumer is processing a stream from the queue. 
/// Contains the source message <typeparamref name="T"/> as well as any additional information about the consumption of the message.
/// </summary>
/// <typeparam name="T">Message being consumed from the queue</typeparam>
public sealed class Message<T> where T : class
{
    public required T Payload { get; set; }
}
