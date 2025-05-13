namespace Dependo;

public static class DependoHelper
{
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