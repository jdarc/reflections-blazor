using System.Numerics;

namespace Reflections.Graphics
{
    public struct Vertex
    {
        public Vector3 Position;
        public Vector3 Normal;

        public Vertex(in Vector3 position = default, in Vector3 normal = default)
        {
            Position = position;
            Normal = normal;
        }
    }
}