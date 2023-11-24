// ReSharper disable NonReadonlyMemberInGetHashCode

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

#pragma warning disable CS0472 // The result of the expression is always the same since a value of this type is never equal to 'null'

namespace api.domain;

public abstract class AbstractDomainEntity {
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; internal set; }

    public DateTime CreatedTime { get; internal set; }
    public DateTime LastUpdateTime { get; internal set; }

    public override bool Equals(object? other) {
        if (other == null) return false;

        return other.GetType() == GetType() && ((AbstractDomainEntity)other).Id.Equals(Id);
    }

    public override int GetHashCode() {
        return Id == null ? 0 : Id.GetHashCode();
    }
}