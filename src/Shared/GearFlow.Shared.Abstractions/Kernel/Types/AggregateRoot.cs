
namespace GearFlow.Shared.Abstractions.Kernel.Types;

public abstract class AggregateRoot
{
    public uint Version { get; protected set; }
}
