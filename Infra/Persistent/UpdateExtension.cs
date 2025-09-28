using Microsoft.EntityFrameworkCore.Query;

namespace IbraHabra.NET.Infra.Persistent;

public static class UpdateExtension
{
    public static SetPropertyCalls<TEntity> SetStrPropIfNotNullOrEmpty<TEntity>(
        this SetPropertyCalls<TEntity> calls, Func<TEntity, string> propertiesEx, string? val)
    {
        if (string.IsNullOrWhiteSpace(val))
            return calls;

        var trimmed = val.Trim();
        if (trimmed.Length is 0)
            return calls;

        return calls.SetProperty(propertiesEx, trimmed);
    }

    public static SetPropertyCalls<TEntity> SetPropIfNotNull<TEntity, TProperty>(
        this SetPropertyCalls<TEntity> calls, Func<TEntity, TProperty> propertiesEx, TProperty val)
    {
        if (val is null)
            return calls;

        return calls.SetProperty(propertiesEx, val);
    }
}