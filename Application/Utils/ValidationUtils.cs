namespace IbraHabra.NET.Application.Utils;

public static  class ValidationUtils
{
    public static bool BeValidUrl(string? uriString)
    {
        return Uri.TryCreate(uriString, UriKind.Absolute, out _);
    }
}