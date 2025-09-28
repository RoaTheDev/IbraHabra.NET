namespace IbraHabra.NET.Domain.SharedKernel.Interface;

public interface IEntity<out TKey>
{
    TKey Id { get; }
}; 