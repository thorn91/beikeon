using beikeon.domain.user;
using beikeon.web.security;
using Microsoft.AspNetCore.Mvc;

namespace beikeon.domain.security;

public class AuthService(IUserService userService, ISecurityContext securityContext) : IAuthService {
    public async Task<IActionResult> Login(string email, string password) {
        var user = await userService.GetUserByEmail(email);

        if (user is null || !user.IsValidatedPassword(password)) return new UnauthorizedResult();

        securityContext.Initialize(user);
        var token = TokenGenerator.CreateTokenForUser(user, null);
        return new OkObjectResult(token);
    }

    public async Task<IActionResult> Register(string email, string password, string firstName, string lastName) {
        if (await userService.ExistsByEmail(email))
            return new BadRequestObjectResult(new {
                message = $"User with email {email} already exists!"
            });

        var newUser = await userService.CreateNewUser(email, password, firstName, lastName);
        securityContext.Initialize(newUser);
        var token = TokenGenerator.CreateTokenForUser(newUser, null);
        return new OkObjectResult(token);
    }
}

public interface IAuthService {
    /// <summary>
    ///     Attempts to login the user with the given email and password and returns a generated Token if successful.
    /// </summary>
    /// <param name="email"></param>
    /// <param name="password"></param>
    /// <returns>
    ///     <see cref="OkObjectResult" /> with the generated token if successful, <see cref="UnauthorizedResult" /> if
    ///     the credentials are invalid.
    /// </returns>
    Task<IActionResult> Login(string email, string password);

    /// <summary>
    ///     Attempts to register a new user with the given details and returns a generated Token if successful.
    /// </summary>
    /// <param name="email"></param>
    /// <param name="password"></param>
    /// <param name="firstName"></param>
    /// <param name="lastName"></param>
    /// <returns>
    ///     <see cref="OkObjectResult" /> with the generated token if successful, <see cref="BadRequestObjectResult" /> if
    ///     the email is already in use.
    /// </returns>
    Task<IActionResult> Register(string email, string password, string firstName, string lastName);
}