namespace IbraHabra.NET.Domain.Constants;

public enum ErrorType
{
    BusinessRule = 400,
    Validation = 400,
    Unauthorized = 401,
    Forbidden = 403,
    NotFound = 404,
    Conflict = 409,
    ServerError = 500
}