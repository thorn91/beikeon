using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace beikeon.domain.user;

public class User
    : AbstractDomainEntity {
    private const int NumHashIterations = 100_000;
    private const int KeySize = 32;
    private const int EmailLengthMin = 5;
    private const string ValidEmailRegex = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";
    internal const int PasswordLengthMin = 4;

    public User(string firstName, string lastName, string email, string rawPassword) {
        var cleanedEmail = email.Trim().ToLower();
        
        ThrowIfInvalidEmail(cleanedEmail);
        ThrowIfInvalidPassword(rawPassword);
        
        FirstName = firstName;
        LastName = lastName;
        Password = GetHashedAndSaltedPassword(rawPassword);
        Email = cleanedEmail;
    }

    public User() { }

    public string FirstName { get; protected set; } = null!;
    public string LastName { get; protected set; } = null!;
    public string Email { get; protected set; } = null!;

    public string Password { get; protected set; } = null!;

    [Required]
    public byte[]? Salt { get; private set; }

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

    internal static void ThrowIfInvalidPassword(string password) {
        if (!IsValidPassword(password)) throw new ValidationException("Invalid password");
    }
    
    internal static void ThrowIfInvalidEmail(string email) {
        if (!IsValidEmail(email)) throw new ValidationException("Invalid email address");
    }
    
    private static bool IsValidPassword(string? password) {
        return password?.Length >= PasswordLengthMin;
    }

    private static bool IsValidEmail(string? email) {
        return email?.Trim().Length >= EmailLengthMin && Regex.IsMatch(email, ValidEmailRegex);
    }
}