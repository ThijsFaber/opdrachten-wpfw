namespace backend.DTO;

public record PutProject (
    string? Title,
    string? Beschrijving,
    string? Categorie,
    string? GithubUrl
);