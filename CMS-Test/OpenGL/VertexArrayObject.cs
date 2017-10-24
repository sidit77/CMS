using System;
using OpenTK.Graphics.OpenGL4;

namespace CMS_Test.OpenGL {

    class VertexArrayObject : IDisposable{

        private static VertexArrayObject current = null;
        public static VertexArrayObject Current {
            get {
                return current;
            }
            set {
                current = value;
                GL.BindVertexArray(current == null ? 0 : current.ID);
            }
        }

        private readonly int id;

        public VertexArrayObject() {
            id = GL.GenVertexArray();
        }

        public VertexArrayObject Bind() {
            if(Current == null || Current.ID != ID)
                Current = this;
            return this;
        }

        public int ID {
            get {
                return id;
            }
        }

        public void Dispose() {
            GL.DeleteVertexArray(id);
        }
    }
}
