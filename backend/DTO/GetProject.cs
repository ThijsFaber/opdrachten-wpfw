namespace backend.DTO;

public record GetProject(
    int Id,
    string? Title,
    string? Beschrijving,
    string? Categorie,
    string GithubUrl,
    DateTime Datum
);