using backend.data;
using backend.domain.user;
using Microsoft.EntityFrameworkCore;

namespace backend.api;

public static class UserApi {
    public static void MapUserApi(this WebApplication app) {
        app.MapGet("/users", async (BeikeonDbContext context) => {
            var users = await context.Users.ToListAsync();
            return users;
        });
        
        app.MapPost("/users", async (BeikeonDbContext context, User user) => {
            await context.Users.AddAsync(user);
            await context.SaveChangesAsync();
            return user;
        });
    }

}