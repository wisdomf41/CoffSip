namespace CoffSip.Application.MenuItems;

// Carries field-level errors to the API without leaking implementation details.
public sealed class MenuItemValidationException : Exception
{
    public IReadOnlyDictionary<string, string[]> Errors { get; }

    public MenuItemValidationException(IReadOnlyDictionary<string, string[]> errors)
        : base("One or more validation errors occurred.")
    {
        Errors = errors;
    }
}
