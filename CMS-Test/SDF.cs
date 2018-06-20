using System;
using System.Numerics;

namespace CMS_Test {

    namespace SDF{

        //source: http://iquilezles.org/www/articles/distfunctions/distfunctions.htm

        static class Primitives {

            public static Func<Vector3, float> Sphere(float r) {
                return p => p.Length() - r;
            }
            
            public static Func<Vector3, float> Box(Vector3 b) {
                return p => {
                    Vector3 d = Vector3.Abs(p) - b;
                    return Vector3.Max(d, Vector3.Zero).Length() +
                           (float)Math.Min(Math.Max(d.X, Math.Max(d.Y, d.Z)), 0.0);
                };
            }
            public static Func<Vector3, float> Torus(Vector2 t) {
                return p => {
                    Vector2 q = new Vector2((float)Math.Sqrt(p.X * p.X + p.Z * p.Z) - t.X, p.Y);
                    return q.Length() - t.Y;
                };
            }

            public static Func<Vector3, float> Cylinder(Vector2 t) {
                return p => Math.Max(new Vector2(-p.X, -p.Z).Length() - t.Y, Math.Abs(p.Y) - t.X);
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
            
            public static Func<Vector3, float> Transform(Matrix4x4 m, Func<Vector3, float> f) {
                Matrix4x4.Invert(m, out Matrix4x4 im);
                return p => f(Vector3.Transform(p, im));
            }
            
        }

    }

}
