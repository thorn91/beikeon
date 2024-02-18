using beikeon.domain.Security;
using beikeon.domain.user;
using Microsoft.AspNetCore.Mvc;

namespace beikeon.domain.security;

public class AuthService(IUserService userService, ISecurityContext securityContext) : IAuthService {
    public async Task<string> Login(string email, string password) {
        var user = await userService.GetUserByEmail(email);

        if (user is null || !user.IsValidatedPassword(password))  throw new UnauthorizedAccessException("Invalid credentials!");

        securityContext.Initialize(user);
        return TokenGenerator.CreateTokenForUser(user, null);
    }

    public async Task<string> Register(string email, string password, string firstName, string lastName) {
        if (await userService.ExistsByEmail(email)) return $"User with email {email} already exists!";

        var newUser = await userService.CreateNewUser(email, password, firstName, lastName);
        securityContext.Initialize(newUser);
        return TokenGenerator.CreateTokenForUser(newUser, null);
    }
}

public interface IAuthService {
    /// <summary>
    ///     Attempts to login the user with the given email and password and returns a generated Token if successful.
    /// </summary>
    /// <param name="email"></param>
    /// <param name="password"></param>
    /// <returns>
    ///     
    /// </returns>
    Task<string> Login(string email, string password);

    /// <summary>
    ///     Attempts to register a new user with the given details and returns a generated Token if successful.
    /// </summary>
    /// <param name="email"></param>
    /// <param name="password"></param>
    /// <param name="firstName"></param>
    /// <param name="lastName"></param>
    /// <returns>
    /// </returns>
    Task<string> Register(string email, string password, string firstName, string lastName);
}