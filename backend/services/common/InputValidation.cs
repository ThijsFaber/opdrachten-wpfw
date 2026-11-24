namespace backend.services.common;

public static class InputValidation
{
    public static string? RequiredFields(
        string errorMessage,
        params string?[] values)
    {
        return values.Any(string.IsNullOrWhiteSpace)
            ? errorMessage
            : null;
    }
}