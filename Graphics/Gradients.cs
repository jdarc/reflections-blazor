using System.Runtime.CompilerServices;

namespace Reflections.Graphics
{
    public sealed class Gradients
    {
        public float Dxdu;
        public float Dydu;
        public float Dxdv;
        public float Dydv;
        public float Dxds;
        public float Dyds;
        public float Dxd1;
        public float Dyd1;
        public readonly float[] OverZ = new float[12];

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public void Configure(in Vertex v0, in Vertex v1, in Vertex v2)
        {
            OverZ[3] = 1.0F / v0.Position.Z;
            OverZ[0] = v0.Normal.X * OverZ[3];
            OverZ[1] = v0.Normal.Y * OverZ[3];
            OverZ[2] = v0.Normal.Z * OverZ[3];
            OverZ[7] = 1.0F / v1.Position.Z;
            OverZ[4] = v1.Normal.X * OverZ[7];
            OverZ[5] = v1.Normal.Y * OverZ[7];
            OverZ[6] = v1.Normal.Z * OverZ[7];
            OverZ[11] = 1.0F / v2.Position.Z;
            OverZ[8] = v2.Normal.X * OverZ[11];
            OverZ[9] = v2.Normal.Y * OverZ[11];
            OverZ[10] = v2.Normal.Z * OverZ[11];
            var v0X2X = v0.Position.X - v2.Position.X;
            var v1X2X = v1.Position.X - v2.Position.X;
            var v0Y2Y = v0.Position.Y - v2.Position.Y;
            var v1Y2Y = v1.Position.Y - v2.Position.Y;
            var oneOverDx = 1.0F / (v1X2X * v0Y2Y - v0X2X * v1Y2Y);
            var tx0 = oneOverDx * v0Y2Y;
            var tx1 = oneOverDx * v1Y2Y;
            var ty0 = oneOverDx * v0X2X;
            var ty1 = oneOverDx * v1X2X;
            Dxdu = (OverZ[4] - OverZ[8]) * tx0 - (OverZ[0] - OverZ[8]) * tx1;
            Dydu = (OverZ[8] - OverZ[4]) * ty0 - (OverZ[8] - OverZ[0]) * ty1;
            Dxdv = (OverZ[5] - OverZ[9]) * tx0 - (OverZ[1] - OverZ[9]) * tx1;
            Dydv = (OverZ[9] - OverZ[5]) * ty0 - (OverZ[9] - OverZ[1]) * ty1;
            Dxds = (OverZ[6] - OverZ[10]) * tx0 - (OverZ[2] - OverZ[10]) * tx1;
            Dyds = (OverZ[10] - OverZ[6]) * ty0 - (OverZ[10] - OverZ[2]) * ty1;
            Dxd1 = (OverZ[7] - OverZ[11]) * tx0 - (OverZ[3] - OverZ[11]) * tx1;
            Dyd1 = (OverZ[3] - OverZ[11]) * ty1 - (OverZ[7] - OverZ[11]) * ty0;
        }
    }
}