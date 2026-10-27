namespace AcmHouse.Application.Exceptions;

/// <summary>The requested resource does not exist (HTTP 404).</summary>
public class NotFoundException(string message) : Exception(message);

/// <summary>The request conflicts with the current state, e.g. the dates are taken (HTTP 409).</summary>
public class ConflictException(string message) : Exception(message);

/// <summary>The request breaks a business rule (HTTP 400).</summary>
public class BusinessRuleException(string message) : Exception(message);

/// <summary>Wrong email or password (HTTP 401).</summary>
public class InvalidCredentialsException() : Exception("Invalid email or password.");

/// <summary>The employee is signed in but may not perform this action (HTTP 403).</summary>
public class ForbiddenException(string message) : Exception(message);
