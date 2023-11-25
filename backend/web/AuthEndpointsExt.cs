using beikeon.domain.security;

namespace beikeon.web;

public static class AuthEndpointsExt {
    private static readonly string Prefix = "/login";

    public static RouteGroupBuilder MapAuthEndpoints(this RouteGroupBuilder builder) {
        builder.MapPost("",
            async (IAuthService authService, LoginDto dto) => await authService.Login(dto.Email, dto.Password));

        return builder;
    }
}

public record LoginDto(string Email, string Password);