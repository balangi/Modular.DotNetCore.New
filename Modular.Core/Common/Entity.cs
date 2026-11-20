#nullable enable

namespace Modular.Core.Common;

/// <summary>Base entity with strongly typed identity and audit timestamps.</summary>
public abstract class Entity<TId> : IEquatable<Entity<TId>> where TId : notnull
{
    protected Entity(TId id)
    {
        if (EqualityComparer<TId>.Default.Equals(id, default!))
            throw new ArgumentException("Entity id cannot be the default value.", nameof(id));

        Id = id;
        CreatedAtUtc = DateTimeOffset.UtcNow;
        UpdatedAtUtc = CreatedAtUtc;
    }

    public TId Id { get; }
    public DateTimeOffset CreatedAtUtc { get; private set; }
    public DateTimeOffset UpdatedAtUtc { get; private set; }

    protected void Touch(DateTimeOffset? timestamp = null)
    {
        var value = timestamp ?? DateTimeOffset.UtcNow;
        if (value < CreatedAtUtc)
            throw new ArgumentOutOfRangeException(nameof(timestamp), "Updated timestamp cannot precede creation timestamp.");

        UpdatedAtUtc = value;
    }

    public bool Equals(Entity<TId>? other) => other is not null && EqualityComparer<TId>.Default.Equals(Id, other.Id);
    public override bool Equals(object? obj) => obj is Entity<TId> other && Equals(other);
    public override int GetHashCode() => EqualityComparer<TId>.Default.GetHashCode(Id);
    public static bool operator ==(Entity<TId>? left, Entity<TId>? right) => Equals(left, right);
    public static bool operator !=(Entity<TId>? left, Entity<TId>? right) => !Equals(left, right);
}

public abstract class Entity : Entity<Guid>
{
    protected Entity(Guid id) : base(id) { }
}
