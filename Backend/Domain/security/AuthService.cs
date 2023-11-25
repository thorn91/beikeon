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
    Task<IActionResult> Login(string email, string password);

    Task<IActionResult> Register(string email, string password, string firstName, string lastName);
}