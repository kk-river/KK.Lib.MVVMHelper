using System.Runtime.Serialization;

namespace KK.Lib.MVVMHelper.DependencyInjection;

public class ContainerException : InvalidOperationException
{
    public ContainerException()
    {
    }

    public ContainerException(string? message) : base(message)
    {
    }

    public ContainerException(string? message, Exception? innerException) : base(message, innerException)
    {
    }

    protected ContainerException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}
