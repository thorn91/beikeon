using beikeon.data;
using beikeon.domain.exception;
using Microsoft.EntityFrameworkCore;

namespace beikeon.domain.user;

public interface IUserService {
    Task<User?> GetUserById(long id);

    Task<User> MustGetUserById(long id);

    Task<User?> GetUserByEmail(string email);
}

public class UserService : IUserService {
    private readonly BeikeonDbContext _dbContext;
    private readonly ILogger<UserService> _logger;

    public UserService(BeikeonDbContext dbContext, ILogger<UserService> logger) {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<User?> GetUserByEmail(string email) {
        return await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task<User?> GetUserById(long id) {
        return await _dbContext.Users.FindAsync(id);
    }

    public async Task<User> MustGetUserById(long id) {
        var user = await GetUserById(id) ?? throw new NotFoundException<User>(id);
        return user;
    }
}