namespace Dependo;

/// <summary>
/// Provides dependo helper.
/// </summary>
public static class DependoHelper
{
    /// <summary>
    /// Stringifies the key.
    /// </summary>
    /// <param name="key">The key.</param>
    /// <returns>The key.</returns>
    public static string StringifyKey(object key) => key switch
    {
        string s => s,
        int i => i.ToString(),
        Enum e => e.ToString(),
        Guid g => g.ToString(),
        null => throw new ArgumentNullException(nameof(key)),
        _ => $"{key.GetType().FullName}:{key.GetHashCode()}"
    };
}