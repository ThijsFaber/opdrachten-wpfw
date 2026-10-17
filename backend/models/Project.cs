namespace backend.models;

public class Project
{
    public int Id { get; set;}

    public string Title { get; set;} = string.Empty;

    public string Beschrijving {get; set;} = string.Empty;

    public string Categorie { get; set;} = string.Empty;

    public string GithubUrl { get; set;} = string.Empty;

    public DateTime Datum { get; set;}
}