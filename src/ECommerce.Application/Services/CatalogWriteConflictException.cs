namespace ECommerce.Application.Services;

public sealed class CatalogWriteConflictException(string message) : Exception(message);
