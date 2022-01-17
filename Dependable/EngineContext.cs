using System.Runtime.CompilerServices;

namespace Dependable
{
    public static class EngineContext
    {
        public static IEngine Current { get; private set; }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public static IEngine Create(IEngine engine)
        {
            if (Current == null)
            {
                Current = engine;
            }

            return Current;
        }
    }
}