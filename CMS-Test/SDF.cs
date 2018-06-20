using System;
using System.Numerics;
using OpenTK.Platform.Windows;

namespace CMS_Test {

    namespace SDF{

        //source: http://iquilezles.org/www/articles/distfunctions/distfunctions.htm

        static class Primitives {

            public static Func<Vector3, float> Sphere(Vector3 c, float r) {
                return p => (c - p).Length() - r;
            }
            
            public static Func<Vector3, float> Box(Vector3 c, Vector3 b) {
                return p => {
                    Vector3 d = Vector3.Abs(c - p) - b;
                    return Vector3.Max(d, Vector3.Zero).Length() +
                           (float)Math.Min(Math.Max(d.X, Math.Max(d.Y, d.Z)), 0.0);
                };
            }
            public static Func<Vector3, float> Torus(Vector3 c, Vector2 t) {
                return p => {
                    p = c - p;
                    Vector2 q = new Vector2((float)Math.Sqrt(p.X * p.X + p.Z * p.Z) - t.X, p.Y);
                    return q.Length() - t.Y;
                };
            }

            public static Func<Vector3, float> Cylinder(Vector3 c, Vector2 t) {
                return p => Math.Max(new Vector2(c.X - p.X, c.Z - p.Z).Length() - t.Y, Math.Abs(c.Y - p.Y) - t.X);
            }
        }

        static class Operations {
            public static Func<Vector3, float> Union(Func<Vector3, float> a, Func<Vector3, float> b) {
                return p => Math.Min(a(p), b(p));
            }

            public static Func<Vector3, float> Substraction(Func<Vector3, float> a, Func<Vector3, float> b) {
                return p => -Math.Min(-a(p), b(p));
            }

            public static Func<Vector3, float> Intersection(Func<Vector3, float> a, Func<Vector3, float> b) {
                return p => Math.Max(a(p), b(p));
            }
        }

    }

}
