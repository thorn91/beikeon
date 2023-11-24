// ReSharper disable NonReadonlyMemberInGetHashCode

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

#pragma warning disable CS0472 // The result of the expression is always the same since a value of this type is never equal to 'null'

namespace api.domain;

internal interface IAbstractDomainEntity {
    public bool Equals(object? other);

    public int GetHashCode();
}

public abstract class AbstractDomainEntity {
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; internal set; }

    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public DateTime CreatedTime { get; internal set; }

    public override bool Equals(object? other) {
        if (other == null) return false;

        return other.GetType() == GetType() && ((AbstractDomainEntity)other).Id.Equals(Id);
    }

    public override int GetHashCode() {
        return Id == null ? 0 : Id.GetHashCode();
    }
}