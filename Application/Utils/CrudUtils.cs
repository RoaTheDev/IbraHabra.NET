using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Query;
using static System.Linq.Expressions.Expression;
namespace IbraHabra.NET.Application.Utils;

public static class CrudUtils
{
    public static Expression<Func<SetPropertyCalls<T>, SetPropertyCalls<T>>> Append<T>(
        Expression<Func<SetPropertyCalls<T>, SetPropertyCalls<T>>> current,
        Expression<Func<SetPropertyCalls<T>, SetPropertyCalls<T>>> next)
    {
        var param = Parameter(typeof(SetPropertyCalls<T>), "s");
        var body = Invoke(next, Invoke(current, param));
        return Lambda<Func<SetPropertyCalls<T>, SetPropertyCalls<T>>>(body, param);
    }
}