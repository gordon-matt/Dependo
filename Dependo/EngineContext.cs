using System.Runtime.CompilerServices;

namespace Dependo;

public static class EngineContext
{
    public static IEngine Current { get; private set; }

    [MethodImpl(MethodImplOptions.Synchronized)]
    public static IEngine Create(IEngine engine) => Current ??= engine;
}