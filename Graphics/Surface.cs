using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;

namespace Reflections.Graphics
{
    public sealed class Surface : IDisposable
    {
        private readonly int _totalBytes;
        private readonly IntPtr _colorBufferPtr;
        private readonly IntPtr _depthBufferPtr;

        private readonly Gradients _gradients = new();
        private readonly Edge[] _edges = {new(), new(), new()};

        public readonly int Width;
        public readonly float CenterX;
        public readonly float CenterY;
        public Texture Texture = Texture.Blank;

        public Surface(int width, int height)
        {
            Width = width;
            CenterX = width / 2.0F;
            CenterY = height / 2.0F;
            _totalBytes = width * height * 4;
            _depthBufferPtr = Marshal.AllocHGlobal(_totalBytes);
            _colorBufferPtr = Marshal.AllocHGlobal(_totalBytes);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
        {
            Fill(_colorBufferPtr, _totalBytes, 0);
            Fill(_depthBufferPtr, _totalBytes, float.PositiveInfinity);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public void Transfer(int[] dst) => Marshal.Copy(_colorBufferPtr, dst, 0, dst.Length);

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public unsafe void Render(in Vertex v0, in Vertex v1, in Vertex v2)
        {
            if (IsBackFacing(in v0, in v1, in v2)) return;
            _gradients.Configure(in v0, in v1, in v2);

            int leftIndex;
            if (v0.Position.Y < v1.Position.Y)
            {
                if (v2.Position.Y < v0.Position.Y)
                {
                    _edges[0].Configure(v2, v1, _gradients, 8);
                    _edges[1].Configure(v2, v0, _gradients, 8);
                    _edges[2].Configure(v0, v1, _gradients, 0);
                    leftIndex = 0;
                }
                else
                {
                    if (v1.Position.Y < v2.Position.Y)
                    {
                        _edges[0].Configure(v0, v2, _gradients, 0);
                        _edges[1].Configure(v0, v1, _gradients, 0);
                        _edges[2].Configure(v1, v2, _gradients, 4);
                        leftIndex = 0;
                    }
                    else
                    {
                        _edges[0].Configure(v0, v1, _gradients, 0);
                        _edges[1].Configure(v0, v2, _gradients, 0);
                        _edges[2].Configure(v2, v1, _gradients, 8);
                        leftIndex = 1;
                    }
                }
            }
            else
            {
                if (v2.Position.Y < v1.Position.Y)
                {
                    _edges[0].Configure(v2, v0, _gradients, 8);
                    _edges[1].Configure(v2, v1, _gradients, 8);
                    _edges[2].Configure(v1, v0, _gradients, 4);
                    leftIndex = 1;
                }
                else
                {
                    if (v0.Position.Y < v2.Position.Y)
                    {
                        _edges[0].Configure(v1, v2, _gradients, 4);
                        _edges[1].Configure(v1, v0, _gradients, 4);
                        _edges[2].Configure(v0, v2, _gradients, 0);
                        leftIndex = 1;
                    }
                    else
                    {
                        _edges[0].Configure(v1, v0, _gradients, 4);
                        _edges[1].Configure(v1, v2, _gradients, 4);
                        _edges[2].Configure(v2, v0, _gradients, 8);
                        leftIndex = 0;
                    }
                }
            }

            var rightIndex = 1 - leftIndex;
            var height = _edges[1].Height;
            var total = _edges[0].Height;
            var y = _edges[0].Y * Width;

            var color = (Color*) _colorBufferPtr;
            var depth = (float*) _depthBufferPtr;
            while (total > 0)
            {
                total -= height;
                var left = _edges[leftIndex];
                var right = _edges[rightIndex];
                while (--height >= 0)
                {
                    var xStart = (int) Math.Ceiling(left.X);
                    var scan = (int) Math.Ceiling(right.X) - xStart;
                    var xPreStep = xStart - left.X;
                    var uOverZ = left.OverZ[0] + xPreStep * _gradients.Dxdu;
                    var vOVerZ = left.OverZ[1] + xPreStep * _gradients.Dxdv;
                    var sOverZ = left.OverZ[2] + xPreStep * _gradients.Dxds;
                    var oOverZ = left.OverZ[3] + xPreStep * _gradients.Dxd1;
                    var mem = y + xStart;
                    while (scan-- > 0)
                    {
                        var z = 1.0F / oOverZ;
                        if (z < depth[mem])
                        {
                            depth[mem] = z;
                            var u = (int) (uOverZ * z * 256.0F + 256.0F);
                            var v = (int) (vOVerZ * z * 256.0F + 256.0F);
                            var s = (int) (sOverZ * z * 256.0F);
                            var spec = SpecularMap[s];
                            var env = Texture.Sample(u, v);
                            var envP = (byte*) &env;
                            var red = (s * envP[0] >> 8) + spec;
                            var grn = (s * envP[1] >> 8) + spec;
                            var blu = (s * envP[2] >> 8) + spec;
                            color[mem].R = red < 255 ? (byte) red : 255;
                            color[mem].G = grn < 255 ? (byte) grn : 255;
                            color[mem].B = blu < 255 ? (byte) blu : 255;
                            color[mem].A = 255;
                        }

                        uOverZ += _gradients.Dxdu;
                        vOVerZ += _gradients.Dxdv;
                        sOverZ += _gradients.Dxds;
                        oOverZ += _gradients.Dxd1;
                        ++mem;
                    }

                    left.Step();
                    right.Step();
                    y += Width;
                }

                height = _edges[2].Height;
                leftIndex <<= 1;
                rightIndex <<= 1;
            }
        }

        public void Dispose()
        {
            Marshal.FreeHGlobal(_colorBufferPtr);
            Marshal.FreeHGlobal(_depthBufferPtr);
        }

        private static readonly byte[] SpecularMap =
        {
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1,
            1, 1, 2, 2, 2, 2, 2, 2, 2, 3, 3, 3, 3, 3, 4, 4, 4, 4, 5, 5, 5, 6, 6, 6, 7, 7, 7, 8, 8, 9, 9, 10, 10, 11, 11, 12,
            13, 13, 14, 15, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 29, 30, 31, 33, 34, 36, 37, 39, 41, 43, 44,
            46, 48, 51, 53, 55, 57, 60, 62, 65, 68, 70, 73, 76, 80, 83, 86, 90, 93, 97, 101, 105, 109, 114, 118, 123, 127,
            132, 137, 143, 148, 154, 160, 166, 172, 178, 185, 192, 199, 206, 214, 222, 230, 238, 247, 255
        };

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        private static bool IsBackFacing(in Vertex a, in Vertex b, in Vertex c) =>
            (b.Position.X - a.Position.X) * (c.Position.Y - a.Position.Y) -
            (c.Position.X - a.Position.X) * (b.Position.Y - a.Position.Y) < 0.0F;

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        private static unsafe void Fill<T>(IntPtr array, int size, T value) where T : unmanaged
        {
            var mem = array.ToPointer();
            var ptr = (Vector256<int>*) mem;
            var val = Vector256.Create(*(int*) &value);
            for (int x = 0, length = size >> 5; x < length; ++x) ptr[x] = val;
        }
    }
}