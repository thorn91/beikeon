namespace beikeon.web.middleware;

public class AuthMiddleware(RequestDelegate next) {
    public const string AuthCookieName = "access_token";

    public async Task Invoke(HttpContext context) {
        AddAuthCookieAsHeader(context);
        await next.Invoke(context);
        DeleteCookieFrom401Response(context);
    }

    private static void AddAuthCookieAsHeader(HttpContext context) {
        var token = context.Request.Cookies[AuthCookieName];
        if (token != null) context.Request.Headers.Append("Authorization", $"Bearer {token}");
    }

    private static void DeleteCookieFrom401Response(HttpContext context) {
        if (context.Response.StatusCode == 401) context.Response.Cookies.Delete(AuthCookieName);
    }
}