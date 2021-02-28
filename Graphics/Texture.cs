using System.Runtime.CompilerServices;

namespace Reflections.Graphics
{
    public sealed class Texture
    {
        public static readonly Texture Blank = new(new byte[512 * 512 * 4]);

        private readonly int[] _data;

        public Texture(in byte[] data)
        {
            _data = new int[data.Length >> 2];
            for (var i = 0; i < data.Length; i += 4)
            {
                _data[i >> 2] = data[i + 0] << 16 | data[i + 1] << 8 | data[i + 2];
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public int Sample(in int u, in int v) => _data[(v << 9) + u];
    }
}