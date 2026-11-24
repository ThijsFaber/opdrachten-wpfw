namespace backend.security;

public static class AppRoles
{
    public const string Gast = "Gast";
    public const string Editor = "Editor";
    public const string Admin = "Admin";
}

public static class AppPolicies
{
    public const string WriteContent = "WriteContent";
}