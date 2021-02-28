using System;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Reflections.Graphics
{
    public sealed class Visualizer
    {
        private readonly Vector3 _light = Vector3.Normalize(new Vector3(5.0F, 30.0F, -30.0F));
        private readonly int[] _pixels;
        private readonly Surface _surface;
        private bool _dragging;
        private float _angleX;
        private float _angleY;
        private int _oldX; 
        private int _oldY;

        public readonly int Width;
        public readonly int Height;
        public Mesh Mesh;

        public Texture Texture
        {
            set => _surface.Texture = value;
        }

        public Visualizer(int width, int height)
        {
            Width = width;
            Height = height;
            _pixels = new int[width * height];
            _surface = new Surface(width, height);
        }

        public void MouseDown(in double x, in double y)
        {
            _oldX = (int) x;
            _oldY = (int) y;
            _dragging = true;
        }

        public void MouseMove(in double x, in double y)
        {
            if (!_dragging) return;
            _angleY -= ((float)x - _oldX) / 100.0F;
            _angleX -= ((float)y - _oldY) / 100.0F;
            _angleX = Math.Min(1.57F, Math.Max(-1.57F, _angleX));
            _oldX = (int) x;
            _oldY = (int) y;
        }

        public void MouseUp()
        {
            _dragging = false;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public int[] RenderFrame(float timeStamp)
        {
            if (Mesh != null)
            {
                _surface.Clear();

                if (!_dragging) _angleY = timeStamp / 100.0F;
                var scale = _surface.Width;
                var normal = Matrix4x4.CreateRotationY(_angleY) * Matrix4x4.CreateRotationX(_angleX);
                var world = normal * Matrix4x4.CreateScale(scale, scale, 1.0F);
                unsafe
                {
                    fixed (Vertex* verts = Mesh.VertexBuffer)
                    {
                        for (int i = 0, len = Mesh.VertexBuffer.Length; i < len;)
                        {
                            var v0 = Transform(verts[i++], world, normal);
                            var v1 = Transform(verts[i++], world, normal);
                            var v2 = Transform(verts[i++], world, normal);
                            _surface.Render(v0, v1, v2);
                        }
                    }
                }
            }

            _surface.Transfer(_pixels);
            return _pixels;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        private Vertex Transform(in Vertex src, in Matrix4x4 world, in Matrix4x4 matrix)
        {
            var dst = new Vertex {Position = Vector3.Transform(src.Position, world)};
            var dz = dst.Position.Z + 2.0F;
            var dx = _surface.CenterX + dst.Position.X / dz;
            var dy = _surface.CenterY - dst.Position.Y / dz;
            dst.Position = new Vector3(dx, dy, dz);
            dst.Normal = Vector3.Transform(src.Normal, matrix);
            dst.Normal = new Vector3(dst.Normal.X, dst.Normal.Y, Scalar.UnitClamp(Vector3.Dot(dst.Normal, _light)));
            return dst;
        }
    }
}