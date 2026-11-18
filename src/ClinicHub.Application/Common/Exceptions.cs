namespace ClinicHub.Application.Common;

public class NotFoundException(string entity, Guid id) : Exception($"{entity} '{id}' was not found.");

public class ConflictException(string message) : Exception(message);
