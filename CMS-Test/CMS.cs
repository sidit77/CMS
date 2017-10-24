using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace CMS_Test{

    class CMS{

        private static FastNoise noise = new FastNoise();

        private static float Density(Vector3 p) {
            return SDF.Operations.Union(
                SDF.Operations.Substraction(SDF.Primitives.Sphere(new Vector3(16, 16, 16), 10, p), SDF.Primitives.Sphere(new Vector3(18, 16, 8), 6, p)),
                SDF.Primitives.Box(new Vector3(14, 22, 22), new Vector3(5.5f), p));
            //return noise.GetNoise(p.X * 3, p.Y * 3, p.Z * 3);
        }

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
                        
                        for (int i = 0; i < 8; i++) {
                            v[i] = -Density(new Vector3(x, y, z) + vertices[i]);
                        }

                        for(int i = 0; i < 12; i++) {
                            if(v[edges[i, 0]] < 0 != v[edges[i, 1]] < 0) {
                                e[i, 0] = Mix(vertices[edges[i, 0]], vertices[edges[i, 1]], -v[edges[i, 0]] / (v[edges[i, 1]] - v[edges[i, 0]])) + new Vector3(x, y, z);
                                e[i, 1] = Normal(e[i, 0]);
                            }
                        }
                        
                        for (int f = 0; f < 6; f++) {

                            int corners = 0;
                            for (int i = 0; i < 4; i++) {
                                if (v[faces[f,0,i]] < 0)
                                    corners |= 1 << i;
                            }

                            if(connections[corners, 0] == 2) {
                                Console.WriteLine("Special case");

                                int edge1 = faces[f, 1, connections[corners, 1]];
                                int edge2 = faces[f, 1, connections[corners, 2]];
                                int edge3 = faces[f, 1, connections[corners, 3]];
                                int edge4 = faces[f, 1, connections[corners, 4]];

                                Vector3 v1 = Vector3.Normalize(Vector3.Cross(facenormals[f], e[edge1, 1]));
                            
                            }
                                

                            for (int i = 0; i < connections[corners, 0]; i++) {

                                int edge1 = faces[f,1,connections[corners, 1 + 2 * i]];
                                int edge2 = faces[f,1,connections[corners, 2 + 2 * i]];

                                if (f != 0) {
                                    int temp = edge1;
                                    edge1 = edge2;
                                    edge2 = temp;
                                }

                                if (edgeconnection[edge1] == -1) {
                                    edgeconnection[edge1] = edge2;
                                } else {
                                    Console.WriteLine("Error [" + edge1 + "/" + edge2 + "][" + edge1 + "/" + edgeconnection[edge1] + "] at " + corners);
                                }

                            }

                        }

                        for (int i = 0; i < 12; i++) {
                            if (edgeconnection[i] != -1) {
                                polygon.Clear();
                                int r = i;
                                while (edgeconnection[r] != -1) {
                                    //Vector3 pos = Mix(vertices[edges[r,0]], vertices[edges[r,1]], -v[edges[r,0]] / (v[edges[r,1]] - v[edges[r,0]])) + new Vector3(x, y, z);
                                    
                                    polygon.Add(e[r, 0]);
                                    int newref = edgeconnection[r];
                                    edgeconnection[r] = -1;
                                    r = newref;
                                }

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

        private static readonly Vector3[] vertices = {
            new Vector3(0, 0, 0),
            new Vector3(0, 1, 0),
            new Vector3(1, 1, 0),
            new Vector3(1, 0, 0),
            new Vector3(0, 0, 1),
            new Vector3(0, 1, 1),
            new Vector3(1, 1, 1),
            new Vector3(1, 0, 1),
        };

        private static readonly int[,] connections = {
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

        private static readonly int[,,] faces = {
	        {{ 0, 1, 2, 3 }, { 0,  1,  2,  3}},
	        {{ 0, 1, 5, 4 }, { 0,  9,  4,  8}},
	        {{ 5, 1, 2, 6 }, { 9,  1, 10,  5}},
	        {{ 7, 6, 2, 3 }, { 6, 10,  2, 11}},
	        {{ 0, 4, 7, 3 }, { 8,  7, 11,  3}},
	        {{ 4, 5, 6, 7 }, { 4,  5,  6,  7}}
        };

        private static readonly Vector3[] facenormals = {
            new Vector3( 0, 0,-1),
            new Vector3(-1, 0, 0),
            new Vector3( 0, 1, 0),
            new Vector3( 1, 0, 0),
            new Vector3( 0,-1, 0),
            new Vector3( 0, 0, 1),
        };

        private static readonly int[,] edges = {
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
