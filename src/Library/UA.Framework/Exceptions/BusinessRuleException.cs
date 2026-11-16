namespace UA.Framework.Exceptions;
public class BusinessRuleException(string message) : Exception(message);
public class ForbiddenException(string message = "Access denied") : Exception(message);
public class NotFoundException(string message = "Record not found") : Exception(message);
