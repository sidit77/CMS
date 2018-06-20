using System;
using System.Collections.Generic;
using System.Numerics;

namespace CMS_Test{

    class CMS{

        private static FastNoise noise = new FastNoise();
        
        private static readonly Func<Vector3, float> Density =
            //return SDF.Operations.Union(
            //    SDF.Operations.Substraction(SDF.Primitives.Sphere(new Vector3(16, 16, 16), 10, p), SDF.Primitives.Sphere(new Vector3(18, 16, 8), 6, p)),
            //    SDF.Primitives.Box(new Vector3(14, 22, 22), new Vector3(5.5f), p));
            //return noise.GetNoise(p.X * 8, p.Y * 8, p.Z * 8);
            SDF.Operations.Union(
                SDF.Primitives.Torus(new Vector3(16, 16, 16), new Vector2(10, 4)),
                SDF.Primitives.Cylinder(new Vector3(16, 16, 16), new Vector2(8, 5.5f)));
            //return SDF.Primitives.Sphere(new Vector3(16, 16, 16), 10, p) - noise.GetNoise(p.X * 4, p.Y * 4, p.Z * 4) * 4;
            //return SDF.Primitives.Sphere(new Vector3(16), 10, p);
        

        private static Vector3 Normal(Vector3 p) {
            const float H = 0.001f;
            float dx = Density(p + new Vector3(H, 0, 0)) - Density(p - new Vector3(H, 0, 0));
            float dy = Density(p + new Vector3(0, H, 0)) - Density(p - new Vector3(0, H, 0));
            float dz = Density(p + new Vector3(0, 0, H)) - Density(p - new Vector3(0, 0, H));

            return Vector3.Normalize(new Vector3(dx, dy, dz));
        }

        public static float[] GetMesh() {

            List<float> mesh = new List<float>();

            List<Vector3> polygon = new List<Vector3>();
            float[] v = new float[8];
            int[] edgeconnection = { -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 };
            Vector3[,] e = new Vector3[12,2];
            
            for (int x = 0; x < 32; x++) {
                for (int y = 0; y < 32; y++) {
                    for (int z = 0; z < 32; z++) {

                        //calculate the density on each corner of the cube-cell
                        for (int i = 0; i < 8; i++) {
                            v[i] = -Density(new Vector3(x, y, z) + Vertices[i]);
                        }

                        //foreach(edge in cube-cell)
                        for(int i = 0; i < 12; i++) {
                            //if(edge intersects surface)
                            if(v[Edges[i, 0]] < 0 != v[Edges[i, 1]] < 0) {
                                //calculate intersection point and normal
                                e[i, 0] = Mix(Vertices[Edges[i, 0]], Vertices[Edges[i, 1]], -v[Edges[i, 0]] / (v[Edges[i, 1]] - v[Edges[i, 0]])) + new Vector3(x, y, z);
                                e[i, 1] = Normal(e[i, 0]);
                            }
                        }
                        
                        //foreach(face in cell-cube)
                        for (int f = 0; f < 6; f++) {

                            //calculate the marching square case of the face
                            int corners = 0;
                            for (int i = 0; i < 4; i++) {
                                if (v[Faces[f,0,i]] < 0)
                                    corners |= 1 << i;
                            }

                            //TODO solve case ambiguity to make the mesh topological consistent
                            if (Connections[corners, 0] == 2) {
                                Console.WriteLine("Special case");

                                int edge1 = Faces[f, 1, Connections[corners, 1]];
                                int edge2 = Faces[f, 1, Connections[corners, 2]];
                                int edge3 = Faces[f, 1, Connections[corners, 3]];
                                int edge4 = Faces[f, 1, Connections[corners, 4]];

                                Vector3 v1 = Vector3.Normalize(Vector3.Cross(Facenormals[f], e[edge1, 1]));
                            
                            }
                                
                            //foreach(line in case)
                            for (int i = 0; i < Connections[corners, 0]; i++) {

                                //get the two edges that the line connects
                                int edge1 = Faces[f,1,Connections[corners, 1 + 2 * i]];
                                int edge2 = Faces[f,1,Connections[corners, 2 + 2 * i]];

                                //switch edge1 and edge2
                                if (f != 0) {
                                    edge1 = edge1 ^ edge2;
                                    edge2 = edge1 ^ edge2;
                                    edge1 = edge1 ^ edge2;
                                }

                                //add the connection between the two edges to the per cell connection list
                                if (edgeconnection[edge1] == -1) {
                                    edgeconnection[edge1] = edge2;
                                } else {
                                    Console.WriteLine("Error [" + edge1 + "/" + edge2 + "][" + edge1 + "/" + edgeconnection[edge1] + "] at " + corners);
                                }

                            }

                        }

                        //foreach(edge in cell-cube)
                        for (int i = 0; i < 12; i++) {
                            //if(edge has connection)
                            if (edgeconnection[i] != -1) {
                                polygon.Clear();
                                int r = i;
                                //trace the polygon
                                while (edgeconnection[r] != -1) {
                                    //OLD CODE: Vector3 pos = Mix(vertices[edges[r,0]], vertices[edges[r,1]], -v[edges[r,0]] / (v[edges[r,1]] - v[edges[r,0]])) + new Vector3(x, y, z);
                                    
                                    polygon.Add(e[r, 0]);
                                    int newref = edgeconnection[r];
                                    edgeconnection[r] = -1;
                                    r = newref;
                                }

                                //triangulate polygon
                                for (int j = 0; j < polygon.Count - 2; j++) {

                                    mesh.Add(polygon[0].X);
                                    mesh.Add(polygon[0].Y);
                                    mesh.Add(polygon[0].Z);
                                    
                                    mesh.Add(polygon[1 + j].X);
                                    mesh.Add(polygon[1 + j].Y);
                                    mesh.Add(polygon[1 + j].Z);
                                    
                                    mesh.Add(polygon[2 + j].X);
                                    mesh.Add(polygon[2 + j].Y);
                                    mesh.Add(polygon[2 + j].Z);
                                }


                            }
                        }

                    }
                }
            }
            return mesh.ToArray();
        }

        private static Vector3 Mix(Vector3 a, Vector3 b, float s) {
            return a * (1 - s) + b * s;
        }

        private static readonly Vector3[] Vertices = {
            new Vector3(0, 0, 0),
            new Vector3(0, 1, 0),
            new Vector3(1, 1, 0),
            new Vector3(1, 0, 0),
            new Vector3(0, 0, 1),
            new Vector3(0, 1, 1),
            new Vector3(1, 1, 1),
            new Vector3(1, 0, 1),
        };

        private static readonly int[,] Connections = {
	        {0,-1,-1,-1,-1},
	        {1, 0, 3,-1,-1},
	        {1, 1, 0,-1,-1},
	        {1, 1, 3,-1,-1},
	        {1, 2, 1,-1,-1},
	        {2, 0, 1, 2, 3},
	        {1, 2, 0,-1,-1},
	        {1, 2, 3,-1,-1},
	        {1, 3, 2,-1,-1},
	        {1, 0, 2,-1,-1},
	        {2, 3, 0, 1, 2},
	        {1, 1, 2,-1,-1},
	        {1, 3, 1,-1,-1},
	        {1, 0, 1,-1,-1},
	        {1, 3, 0,-1,-1},
	        {0,-1,-1,-1,-1},
        };

        private static readonly int[,,] Faces = {
	        {{ 0, 1, 2, 3 }, { 0,  1,  2,  3}},
	        {{ 0, 1, 5, 4 }, { 0,  9,  4,  8}},
	        {{ 5, 1, 2, 6 }, { 9,  1, 10,  5}},
	        {{ 7, 6, 2, 3 }, { 6, 10,  2, 11}},
	        {{ 0, 4, 7, 3 }, { 8,  7, 11,  3}},
	        {{ 4, 5, 6, 7 }, { 4,  5,  6,  7}}
        };

        private static readonly Vector3[] Facenormals = {
            new Vector3( 0, 0,-1),
            new Vector3(-1, 0, 0),
            new Vector3( 0, 1, 0),
            new Vector3( 1, 0, 0),
            new Vector3( 0,-1, 0),
            new Vector3( 0, 0, 1),
        };

        private static readonly int[,] Edges = {
	        {0,1},
	        {1,2},
	        {2,3},
	        {3,0},
	        {4,5},
	        {5,6},
	        {6,7},
	        {7,4},
	        {0,4},
	        {1,5},
	        {2,6},
	        {3,7},
        };
    }
}
