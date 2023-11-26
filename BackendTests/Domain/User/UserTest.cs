using Xunit.Abstractions;

namespace BackendTests.Domain.User;

public class UserTest(ITestOutputHelper testOutputHelper) {
    private readonly beikeon.domain.user.User _user = new("firstName", "lastName", "email@email.com", "1234");

    [Fact]
    public void GetHashedPassword() {
        var hashedPassword = _user.GetHashedPasswordWithCurrentSalt("1234");
        Assert.Equal(hashedPassword, _user.Password);
    }
}