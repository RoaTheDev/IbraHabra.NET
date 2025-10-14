using IbraHabra.NET.Domain.Constants;

namespace IbraHabra.NET.Domain.Contract;

public record ApiError(
    string Code,
    string Message,
    ErrorType Type,
    string? Field = null
);