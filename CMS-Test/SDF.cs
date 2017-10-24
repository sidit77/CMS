using System;
using System.Numerics;

namespace CMS_Test {

    namespace SDF{

        //source: http://iquilezles.org/www/articles/distfunctions/distfunctions.htm

        static class Primitives {
            public static float Sphere(Vector3 c, float r, Vector3 p) {
                return (c - p).Length() - r;
            }
            public static float Box(Vector3 c, Vector3 b, Vector3 p) {
                Vector3 d = Vector3.Abs(c - p) - b;
                return Vector3.Max(d, Vector3.Zero).Length() + (float)Math.Min(Math.Max(d.X, Math.Max(d.Y, d.Z)), 0.0);
            }
            public static float Torus(Vector3 c, Vector2 t, Vector3 p){
                p = c - p;
                Vector2 q = new Vector2((float)Math.Sqrt(p.X * p.X + p.Z * p.Z) - t.X, p.Y);
                return q.Length() - t.Y;
            }

            public static float Cylinder(Vector3 c, Vector2 t, Vector3 p){
                return Math.Max(new Vector2(c.X - p.X, c.Z - p.Z).Length() - t.Y, Math.Abs(c.Y - p.Y) - t.X); 
            }
        }

        static class Operations {
            public static float Union(float a, float b) {
                return Math.Min(a, b);
            }

            public static float Substraction(float a, float b) {
                return -Math.Min(-a, b);
            }

            public static float Intersection(float a, float b) {
                return Math.Max(a, b);
            }
        }

    }

}
