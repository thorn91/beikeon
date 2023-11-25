using beikeon.domain.security;

namespace beikeon.web;

public static class AuthEndpointsExt {
    public const string Prefix = "/auth";

    public static RouteGroupBuilder MapAuthEndpoints(this RouteGroupBuilder builder) {
        builder.MapPost("/login",
            async (IAuthService authService, LoginDto dto) => await authService.Login(dto.Email, dto.Password));

        builder.MapPost("/register",
            async (IAuthService authService, RegisterDto dto) =>
                await authService.Register(dto.Email, dto.Password, dto.FirstName, dto.LastName));

        return builder;
    }
}

public record LoginDto(string Email, string Password);

public record RegisterDto(string Email, string Password, string FirstName, string LastName);