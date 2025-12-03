#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace Quee;

/// <summary>
/// Defines the exception state where the system attempted to send a message into the Queue Provider but failed to deliver the message.
/// In effect this means the information about the request was lost and cannot be recovered. 
/// </summary>
public class TransmissionFailureException(string message = "", Exception? inner = null)
    : Exception(message, inner);
