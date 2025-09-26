namespace IbraHabra.NET.Domain.Interface;

public interface IEntity<out TKey>
{
    TKey Id { get; }
}; 