﻿using System.ComponentModel.DataAnnotations;
using beikeon.domain.user;

namespace beikeon.web.security;

public class SecurityContext {
    private User? _actualUser;
    private bool _isInitialized;

    // This is a lazy getter so that it can be initialized with a function that does not need injected services
    private Lazy<Task<User>> _userGetter;

    public void Initialize(User user) {
        Initialize(() => Task.FromResult(user), user.Id, user.Email);
    }

    public async Task<User> GetUser() {
        ThrowIfNotSetup();
        _actualUser ??= await _userGetter.Value;
        return _actualUser;
    }

    private void ThrowIfNotSetup() {
        if (!_isInitialized) {
            throw new ValidationException("Security Context was not initialized!");
        }
    }

    /// <summary>
    /// Initializes the Security Context with the given user information.
    /// </summary>
    /// <param name="userGetter">A function that returns a user.  Separate so that it does not need injected services.</param>
    /// <param name="userId"></param>
    /// <param name="userEmail"></param>
    public void Initialize(Func<Task<User>> userGetter, long userId, string userEmail) {
        _userGetter = new Lazy<Task<User>>(userGetter);
        _isInitialized = true;
    }
}