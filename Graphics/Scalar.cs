using System.Runtime.CompilerServices;

namespace Reflections.Graphics
{
    public static class Scalar
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static float UnitClamp(float x) => x < 0.0 ? 0.0F : x > 1.0 ? 1.0F : x;
    }
}