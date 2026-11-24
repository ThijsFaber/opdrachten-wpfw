namespace backend.DTO;

public record RegisterRequest(
    string? Email,
    string? Password);

public record LoginRequest(
    string? Email,
    string? Password);

public record GetUser(
    string Id,
    string Email,
    IList<string> Roles);