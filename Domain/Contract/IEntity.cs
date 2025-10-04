namespace IbraHabra.NET.Domain.Contract;

public interface IEntity<out TKey>
{
    TKey Id { get; }
}; 