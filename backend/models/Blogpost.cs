namespace backend.models;

public class Blogpost
{
    public int Id { get; set;}

    public string Title { get; set;} = string.Empty;

    public string Inhoud { get; set;} = string.Empty;

    public DateTime Publicatiedatum { get; set;}
}