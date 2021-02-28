using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace Reflections.Graphics
{
    public sealed class Mesh
    {
        public readonly Vertex[] VertexBuffer;

        private Mesh(Vertex[] buffer) => VertexBuffer = buffer;

        public static Mesh Load(string directives)
        {
            var vertices = new List<Vector3>();
            var normals = new List<Vector3>();
            var buffer = new List<Vertex>();
            foreach (var line in directives.Split('\n').Where(it => !string.IsNullOrEmpty(it)))
            {
                var fragments = line.Split(' ');
                switch (fragments[0])
                {
                    case "v":
                    {
                        var x = float.Parse(fragments[1].Trim());
                        var y = float.Parse(fragments[2].Trim());
                        var z = float.Parse(fragments[3].Trim());
                        vertices.Add(new Vector3(x, y, z));
                        break;
                    }
                    case "vn":
                    {
                        var x = float.Parse(fragments[1].Trim());
                        var y = float.Parse(fragments[2].Trim());
                        var z = float.Parse(fragments[3].Trim());
                        normals.Add(new Vector3(x, y, z));
                        break;
                    }
                    case "f":
                    {
                        var f0 = fragments[1].Split("//").Select(v => int.Parse(v.Trim()) - 1).ToArray();
                        var f1 = fragments[2].Split("//").Select(v => int.Parse(v.Trim()) - 1).ToArray();
                        var f2 = fragments[3].Split("//").Select(v => int.Parse(v.Trim()) - 1).ToArray();
                        buffer.Add(new Vertex(vertices[f0.First()], normals[f0.Last()]));
                        buffer.Add(new Vertex(vertices[f1.First()], normals[f1.Last()]));
                        buffer.Add(new Vertex(vertices[f2.First()], normals[f2.Last()]));
                        break;
                    }
                }
            }

            return new Mesh(buffer.ToArray());
        }
    }
}