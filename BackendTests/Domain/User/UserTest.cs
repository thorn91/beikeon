using System.ComponentModel.DataAnnotations;
using Xunit.Abstractions;

namespace BackendTests.Domain.User;

public class UserTest(ITestOutputHelper testOutputHelper) {
    private readonly beikeon.domain.user.User _user = new("firstName", "lastName", "email@email.com", "1234");

    [Fact]
    public void GetHashedPassword() {
        var hashedPassword = _user.GetHashedPasswordWithCurrentSalt("1234");
        Assert.Equal(hashedPassword, _user.Password);
        Assert.NotNull(_user.Salt);
    }

    [Fact]
    public void ThrowIfInvalidPassword() {
        var passwordLength = beikeon.domain.user.User.PasswordLengthMin;

        var invalidPasswords = new[] {
            "a",
            "ab",
            "abc",
        };
        
        foreach (var invalidPassword in invalidPasswords) {
            Assert.True(invalidPassword.Length < passwordLength);
            Assert.Throws<ValidationException>(() => beikeon.domain.user.User.ThrowIfInvalidPassword(invalidPassword));
        }
    }

    [Fact]
    public void CreateUser_EmailTrimmed() {
        var untrimmedEmail = " test@test.com     ";
        var trimmedEmail = untrimmedEmail.Trim();

        var user = new beikeon.domain.user.User("test", "test", untrimmedEmail, "test");
        Assert.Equal(trimmedEmail, user.Email);
    }
    
    [Fact]
    public void ThrowIfInvalidEmail_InvalidEmail() {
        var invalidEmails = new[] {
            "missing@domain",
            "missingAtSign.com",
            "missingDomain@",
            "missingDomain@.com",
            "",
            "missingDomain@.com",
        };
        
        foreach (var invalidEmail in invalidEmails) {
            Assert.Throws<ValidationException>(() => beikeon.domain.user.User.ThrowIfInvalidEmail(invalidEmail));
        }
    }
}