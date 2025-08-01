namespace Quee.Exceptions;

public class TransmissionFailureException(string message = "", Exception? inner = null)
    : Exception(message, inner);
