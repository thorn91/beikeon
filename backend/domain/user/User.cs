namespace api.domain.user;

public class User : AbstractDomainEntity {
    public User(string firstName, string lastName, string email, string password) {
        FirstName = firstName;
        LastName = lastName;
        Email = email;
        Password = password;
    }

    public string? FirstName { get; protected set; }
    public string? LastName { get; protected set; }
    public string? Email { get; protected set; }
    public string? Password { get; protected set; }
}