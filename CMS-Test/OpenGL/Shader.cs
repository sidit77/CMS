#define DETACH_AFTER_LINKING
#define UNIFORM_CACHING

using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Xml;

namespace CMS_Test.OpenGL {

    class ShaderProgram : IDisposable {
        private static ShaderProgram current = null;
        public static ShaderProgram Current {
            get {
                return current;
            }
            set {
                current = value;
                GL.UseProgram(current == null ? 0 : current.ID);
            }
        }

        private readonly int id;
        #if UNIFORM_CACHING
        private Dictionary<string, int> uniforms;
        private Dictionary<string, int> uniformblocks;
        #endif

        public ShaderProgram() {
            id = GL.CreateProgram();
            #if UNIFORM_CACHING
            uniforms = new Dictionary<string, int>();
            uniformblocks = new Dictionary<string, int>();
            #endif
        }

        public int ID {
            get {
                return id;
            }
        }

        public ShaderProgram Link() {
            GL.LinkProgram(id);

            int status;
            GL.GetProgram(id, GetProgramParameterName.LinkStatus, out status);

            #if DETACH_AFTER_LINKING
            int count;
            int[] shader = new int[10];
            GL.GetAttachedShaders(id, shader.Length, out count, shader);
            while(count > 0) {
                count--;
                GL.DetachShader(id, shader[count]);
            }
            #endif

            if(status != 1) {
                string info;
                GL.GetProgramInfoLog(id, out info);
                Console.WriteLine(info);
                Dispose();
            }

            return this;
        }

        public ShaderProgram AttachShader(Shader shader, bool delete = true) {
            GL.AttachShader(id, shader.ID);
            if(delete)
                shader.Dispose();
            return this;
        }

        public ShaderProgram Bind() {
            if(Current == null || Current.ID != ID)
                Current = this;
            return this;
        }

        public int GetUniform(string name) {
            #if UNIFORM_CACHING
            if(!uniforms.ContainsKey(name)) {
                uniforms.Add(name, GL.GetUniformLocation(id, name));
            }
            return uniforms[name];
            #else
            return GL.GetUniformLocation(id, name);
            #endif
        }

        public int GetUniformBlock(string name) {
            #if UNIFORM_CACHING
            if(!uniformblocks.ContainsKey(name)) {
                uniformblocks.Add(name, GL.GetUniformBlockIndex(id, name));
            }
            return uniformblocks[name];
            #else
            return GL.GetUniformBlockIndex(id, name);
            #endif
        }

        public void BindUniformBlock(string name, int slot) {
            BindUniformBlock(GetUniformBlock(name), slot);
        }

        public void BindUniformBlock(int location, int slot) {
            GL.UniformBlockBinding(id, location, slot);
        }

        public void Dispose() {
            GL.DeleteProgram(id);
            #if UNIFORM_CACHING
            uniforms = null;
            uniformblocks = null;
            #endif
        }
        
        #region UNIFORMS

        public void SetUniform(string name, int value) {
            GL.Uniform1(GetUniform(name), value);
        }

        public void SetUniform(string name, float value) {
            GL.Uniform1(GetUniform(name), value);
        }

        public void SetUniform(string name, int xValue, int yValue) {
            GL.Uniform2(GetUniform(name), xValue, yValue);
        }

        public void SetUniform(string name, float xValue, float yValue) {
            GL.Uniform2(GetUniform(name), xValue, yValue);
        }

        public void SetUniform(string name, Vector2 vec) {
            GL.Uniform2(GetUniform(name), vec.X, vec.Y);
        }

        public void SetUniform(string name, ref Vector2 vec) {
            unsafe {
                fixed (float* vec_ptr = &vec.X) {
                    GL.Uniform2(GetUniform(name), 2, vec_ptr);
                }
            }
        }

        public void SetUniform(string name, int xValue, int yValue, int zValue) {
            GL.Uniform3(GetUniform(name), xValue, yValue, zValue);
        }

        public void SetUniform(string name, float xValue, float yValue, float zValue) {
            GL.Uniform3(GetUniform(name), xValue, yValue, zValue);
        }

        public void SetUniform(string name, Vector3 vec) {
            GL.Uniform3(GetUniform(name), vec.X, vec.Y, vec.Z);
        }

        public void SetUniform(string name, ref Vector3 vec) {
            unsafe{
                fixed (float* vec_ptr = &vec.X) {
                    GL.Uniform3(GetUniform(name), 3, vec_ptr);
                }
            }
        }

        public void SetUniform(string name, float xValue, float yValue, float zValue, float wValue) {
            GL.Uniform4(GetUniform(name), xValue, yValue, zValue, wValue);
        }

        public void SetUniform(string name, int xValue, int yValue, int zValue, int wValue) {
            GL.Uniform4(GetUniform(name), xValue, yValue, zValue, wValue);
        }

        public void SetUniform(string name, Vector4 vec) {
            GL.Uniform4(GetUniform(name), vec.X, vec.Y, vec.Z, vec.W);
        }

        public void SetUniform(string name, ref Vector4 vec) {
            unsafe{
                fixed (float* vec_ptr = &vec.X) {
                    GL.Uniform4(GetUniform(name), 4, vec_ptr);
                }
            }
        }

        public void SetUniform(string name, bool transpose, ref Matrix4x4 matrix) {
            unsafe{
                fixed (float* matrix_ptr = &matrix.M11) {
                    GL.UniformMatrix4(GetUniform(name), 1, transpose, matrix_ptr);
                }
            }
        }

        public void SetUniform(string name, Color4 color) {
            GL.Uniform4(GetUniform(name), color);
        }

        #endregion


        public static ShaderProgram FromFile(string path) {
            ShaderProgram program = new ShaderProgram();

            using(XmlReader reader = XmlReader.Create(Path.GetFullPath(path), new XmlReaderSettings() { IgnoreComments = true, IgnoreWhitespace = true})) {
                while(reader.Read()) {
                    if(reader.NodeType == XmlNodeType.Element) {
                        int type = -1;
                        switch(reader.Name) {
                            case "Vertex":
                            case "vertex":          type = (int)ShaderType.VertexShader;         break;
                            case "Fragment":
                            case "fragment":        type = (int)ShaderType.FragmentShader;       break;
                            case "Compute":
                            case "compute":         type = (int)ShaderType.ComputeShader;        break;
                            case "Geometry":
                            case "geometry":        type = (int)ShaderType.GeometryShader;       break;
                            case "TessEvaluation":
                            case "Tessevaluation":
                            case "tessevaluation":  type = (int)ShaderType.TessEvaluationShader; break;
                            case "TessControl":
                            case "Tesscontrol":
                            case "tesscontrol":     type = (int)ShaderType.TessControlShader;    break;
                            default: break;
                        }
                        if(type >= 0) {
                            if(reader["file"] == null) {
                                reader.Read();
                                program.AttachShader(new Shader(reader.ReadContentAsString(), (ShaderType)type));
                            } else {
                                program.AttachShader(Shader.FromFile(Path.GetDirectoryName(path) + Path.DirectorySeparatorChar + reader["file"], (ShaderType)type));
                            }
                        }
                    }
                }
            }

            program.Link();
            return program;
        }

    }

    class Shader : IDisposable {

        private readonly int id;

        public Shader(string source, ShaderType type) {
            id = GL.CreateShader(type);

            GL.ShaderSource(id, source);
            GL.CompileShader(id);

            int status;
            GL.GetShader(id, ShaderParameter.CompileStatus, out status);

            if(status != 1) {
                string info;
                GL.GetShaderInfoLog(id, out info);
                Console.WriteLine(info);
                Dispose();
            }
        }

        public int ID {
            get {
                return id;
            }
        }

        public void Dispose() {
            GL.DeleteShader(id);
        }

        public static Shader FromFile(string file, ShaderType type) {
            string line = "";
            using(StreamReader sr = File.OpenText(file)) {
                line = sr.ReadToEnd();
            }
            return new Shader(line, type);
        }


    }

}
