using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;
using System.Text;

namespace beikeon.domain.user;

public class User
    : AbstractDomainEntity {
    private const int NumHashIterations = 100_000;
    private const int KeySize = 32;

    public User(string firstName, string lastName, string email, string rawPassword) {
        FirstName = firstName;
        LastName = lastName;
        Password = GetHashedAndSaltedPassword(rawPassword);
        Email = email;
    }

    public User() { }

    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }

    public string Password { get; set; }
    private byte[]? Salt { get; set; }

    public bool IsValidatedPassword(string testString) {
        return GetHashedPasswordWithCurrentSalt(testString).Equals(Password);
    }

    private string GetHashedAndSaltedPassword(string rawPassword) {
        SetNewSalt();
        return GetHashedPasswordWithCurrentSalt(rawPassword);
    }

    internal string GetHashedPasswordWithCurrentSalt(string password) {
        if (Salt == null) throw new ValidationException();

        var hash = Rfc2898DeriveBytes.Pbkdf2(
            Encoding.UTF8.GetBytes(password),
            Salt,
            NumHashIterations,
            HashAlgorithmName.SHA512,
            KeySize);

        return Convert.ToHexString(hash);
    }

    private void SetNewSalt() {
        Salt = RandomNumberGenerator.GetBytes(KeySize);
    }
}