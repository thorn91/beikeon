namespace beikeon.web.middleware;

public class AuthMiddleware(RequestDelegate next) {
    public const string AuthCookieName = "access_token";

    public async Task Invoke(HttpContext context) {
        var token = context.Request.Cookies[AuthCookieName];
        if (token != null) context.Request.Headers.Append("Authorization", $"Bearer {token}");

        await next.Invoke(context);

        if (context.Response.StatusCode == 401) context.Response.Cookies.Delete(AuthCookieName);
    }
}