namespace beikeon.domain.exception;

public abstract class DomainException : Exception {
    protected DomainException(string s) : base(s) { }

    protected DomainException(string s, Guid type) : base(s) {
        Type = type;
    }

    public Guid? Type { get; private set; }
}

public class NotFoundException<T> : DomainException {
    public NotFoundException(long id) : base($"Entity Not Found: {id} :: {typeof(T).Name}") { }
}