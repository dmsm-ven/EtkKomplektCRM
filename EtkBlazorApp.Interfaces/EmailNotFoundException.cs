using System.Runtime.Serialization;

namespace EtkBlazorApp.Core;
[Serializable]
public class EmailNotFoundException : Exception
{
    public EmailNotFoundException() : base("Письмо по заданным характеристикам не найдено")
    {
    }

    public EmailNotFoundException(string message, Exception innerException) : base(message, innerException)
    {
    }

    protected EmailNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}