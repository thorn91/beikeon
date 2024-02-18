using beikeon.data;
using beikeon.domain.user;
using Microsoft.EntityFrameworkCore;

namespace beikeon.api;

public static class UserEndpoints {
    // TODO: DTOS
    
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