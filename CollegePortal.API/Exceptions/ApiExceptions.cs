namespace CollegePortal.API.Exceptions
{
    public class ApiException : Exception
    {
        public int StatusCode { get; }

        public ApiException(string message, int statusCode = 500) : base(message)
        {
            StatusCode = statusCode;
        }
    }

    public class UnauthorizedException : ApiException
    {
        public UnauthorizedException(string message = "Unauthorized access")
            : base(message, 401) { }
    }

    public class ForbiddenException : ApiException
    {
        public ForbiddenException(string message = "Access forbidden")
            : base(message, 403) { }
    }

    public class InvalidCredentialsException : ApiException
    {
        public InvalidCredentialsException(string message = "Invalid email or password")
            : base(message, 401) { }
    }

    public class UserAlreadyExistsException : ApiException
    {
        public UserAlreadyExistsException(string message = "User with this email already exists")
            : base(message, 409) { }
    }

    public class NotFoundException : ApiException
    {
        public NotFoundException(string message = "Resource not found")
            : base(message, 404) { }
    }

    public class ValidationException : ApiException
    {
        public ValidationException(string message = "Validation failed")
            : base(message, 400) { }
    }
}
