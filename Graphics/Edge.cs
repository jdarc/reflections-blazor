using System;
using System.Runtime.CompilerServices;

namespace Reflections.Graphics
{
    public sealed class Edge
    {
        public float X;
        public int Y;
        public int Height;
        public readonly float[] OverZ = new float[4];

        private float _xStep;
        private readonly float[] _overZStep = new float[4];

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public void Configure(Vertex v0, Vertex v1, Gradients gradients, int offset)
        {
            Y = (int) Math.Ceiling(v0.Position.Y);
            Height = (int) Math.Ceiling(v1.Position.Y) - Y;
            if (Height <= 0) return;
            
            var yPreStep = Y - v0.Position.Y;
            _xStep = (v1.Position.X - v0.Position.X) / (v1.Position.Y - v0.Position.Y);
            X = yPreStep * _xStep + v0.Position.X;

            _overZStep[0] = _xStep * gradients.Dxdu + gradients.Dydu;
            _overZStep[1] = _xStep * gradients.Dxdv + gradients.Dydv;
            _overZStep[2] = _xStep * gradients.Dxds + gradients.Dyds;
            _overZStep[3] = _xStep * gradients.Dxd1 + gradients.Dyd1;

            var xPreStep = X - v0.Position.X;
            OverZ[0] = yPreStep * gradients.Dydu + xPreStep * gradients.Dxdu + gradients.OverZ[offset + 0];
            OverZ[1] = yPreStep * gradients.Dydv + xPreStep * gradients.Dxdv + gradients.OverZ[offset + 1];
            OverZ[2] = yPreStep * gradients.Dyds + xPreStep * gradients.Dxds + gradients.OverZ[offset + 2];
            OverZ[3] = yPreStep * gradients.Dyd1 + xPreStep * gradients.Dxd1 + gradients.OverZ[offset + 3];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public void Step()
        {
            ++Y;
            X += _xStep;
            OverZ[0] += _overZStep[0];
            OverZ[1] += _overZStep[1];
            OverZ[2] += _overZStep[2];
            OverZ[3] += _overZStep[3];
        }
    }
}