using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using beikeon.domain.user;
using Microsoft.IdentityModel.Tokens;

namespace beikeon.domain.security;

public static class TokenGenerator {
    private static readonly int DefaultExpiryMins = 15;
    private static string? _jwtKey;
    private static int? _expiryMins;

    public static void Initialize(string jwtKey, int expiryMins) {
        _jwtKey = jwtKey;
        _expiryMins = expiryMins;
    }

    public static string CreateTokenForUser(User user, DateTime? expireTime) {
        if (_jwtKey is null) throw new ValidationException("JWT Token Provider was not initialized during configuration!");

        var handler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(_jwtKey);
        var claimsIdentity = GetClaimsFromUser(user);
        expireTime ??= DateTime.UtcNow.AddMinutes(_expiryMins.GetValueOrDefault(DefaultExpiryMins));

        var tokenDescriptor = new SecurityTokenDescriptor {
            Subject = claimsIdentity,
            Expires = expireTime,
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var token = handler.CreateToken(tokenDescriptor);
        return handler.WriteToken(token);
    }

    private static ClaimsIdentity GetClaimsFromUser(User user) {
        return new ClaimsIdentity(new List<Claim> {
            new(ClaimTypes.Name, user.Email),
            new(ClaimTypes.NameIdentifier, user.Id.ToString())
        });
    }
}