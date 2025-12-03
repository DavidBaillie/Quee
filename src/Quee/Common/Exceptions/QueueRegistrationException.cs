#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace Quee;

/// <summary>
/// Represents the exception state that the user has attempted to register a sender/receiver more than once. An exception is raised to prevent 
/// unexpected behaviour that arises from having multiple consumers or senders present in the system for the same message.
/// </summary>
/// <param name="message"></param>
/// <param name="inner"></param>
public class QueueRegistrationException(string message = "", Exception? inner = null) 
    : Exception(message, inner);
