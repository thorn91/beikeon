namespace beikeon.domain.user;

public class User(string firstName, string lastName, string email, string password)
    : AbstractDomainEntity {
    public string? FirstName { get; protected set; } = firstName;
    public string? LastName { get; protected set; } = lastName;
    public string Email { get; protected set; } = email;
    public string? Password { get; protected set; } = password;

    public bool IsValidatedPassword(string s) {
        return s == Password;
    }
}