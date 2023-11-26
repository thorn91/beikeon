using System.Runtime.CompilerServices;
using beikeon.data;
using beikeon.domain.exception;
using Microsoft.EntityFrameworkCore;

[assembly: InternalsVisibleTo("BackendTests")]

namespace beikeon.domain.user;

public interface IUserService {
    Task<User?> GetUserById(long id);

    Task<User> MustGetUserById(long id);

    Task<User?> GetUserByEmail(string email);

    Task<User> CreateNewUser(string email, string password, string firstName, string lastName);

    Task<bool> ExistsByEmail(string email);
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

    public async Task<User> CreateNewUser(string email, string password, string firstName, string lastName) {
        var user = new User(email: email, rawPassword: password, firstName: firstName, lastName: lastName);

        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();

        return user;
    }

    public async Task<bool> ExistsByEmail(string email) {
        return await GetUserByEmail(email) != null;
    }

    public async Task<User?> GetUserById(long id) {
        return await _dbContext.Users.FindAsync(id);
    }

    public async Task<User> MustGetUserById(long id) {
        var user = await GetUserById(id) ?? throw new NotFoundException<User>(id);
        return user;
    }
}