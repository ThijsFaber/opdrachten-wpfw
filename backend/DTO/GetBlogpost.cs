namespace backend.DTO;

public record GetBlogpost(
    int Id,
    string? Title,
    string? Inhoud,
    DateTime Publicatiedatum
);