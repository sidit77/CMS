using CMS_Test.OpenGL;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Input;
using System;
using System.Numerics;

namespace CMS_Test{
    class Program{
        static void Main(string[] args){
            using (var game = new CMSTestApp(1280, 720, "Test")) {
                game.Run(120);
            }
        }
    }
    class CMSTestApp : OpenTK.GameWindow {

        public CMSTestApp(int width, int height, string title) : base(width, height, GraphicsMode.Default, title, OpenTK.GameWindowFlags.Default, OpenTK.DisplayDevice.Default, 4, 5, GraphicsContextFlags.ForwardCompatible) {

        }

        private VertexArrayObject vao;
        private BufferObject vbo;
        private int size;
        private ShaderProgram shader;
        private Camera camera;

        protected override void OnLoad(EventArgs e) {
            base.OnLoad(e);

            camera = new Camera(OpenTK.MathHelper.DegreesToRadians(75), (float)Width / (float)Height, 0.1f, 200.0f);

            GL.ClearColor(OpenTK.Color.DarkSlateGray);
            GL.Enable(EnableCap.DepthTest);

            float[] data = CMS.GetMesh();
            size = data.Length / 3;

            vao = new VertexArrayObject().Bind();
            vbo = BufferObject.CreateBuffer(data, BufferStorageFlags.DynamicStorageBit).Bind(BufferTarget.ArrayBuffer);
            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0 * sizeof(float));
            VertexArrayObject.Current = null;

            shader = ShaderProgram.FromFile("./Assets/Shader/Basic.xml");

            Console.WriteLine(GL.GetError());
        }

        private MouseState oldms;
        protected override void OnUpdateFrame(OpenTK.FrameEventArgs e) {
            base.OnUpdateFrame(e);

            if (Mouse[MouseButton.Left] && CursorVisible == true) {
                CursorVisible = false;
            }
            if (Keyboard[Key.Escape] && CursorVisible == false) {
                CursorVisible = true;
            }
            Vector2 mousepos = Vector2.Zero;
            if (CursorVisible == false) {
                MouseState ms = OpenTK.Input.Mouse.GetState();
                mousepos.X = ms.X - oldms.X;
                mousepos.Y = ms.Y - oldms.Y;
                mousepos /= 600;
                oldms = ms;
            }
            camera.Rotation *= Quaternion.CreateFromAxisAngle(new Vector3(0, 1, 0), mousepos.X);
            camera.Rotation *= Quaternion.CreateFromAxisAngle(camera.Right, mousepos.Y);

            if (Keyboard[Key.Left]) camera.Rotation *= Quaternion.CreateFromAxisAngle(new Vector3(0, 1, 0), -0.1f);
            if (Keyboard[Key.Right]) camera.Rotation *= Quaternion.CreateFromAxisAngle(new Vector3(0, 1, 0), 0.1f);
            if (Keyboard[Key.Up]) camera.Rotation *= Quaternion.CreateFromAxisAngle(camera.Left, 0.1f);
            if (Keyboard[Key.Down]) camera.Rotation *= Quaternion.CreateFromAxisAngle(camera.Left, -0.1f);

            if (Keyboard[Key.W]) camera.Position += camera.Forward * (Keyboard[Key.ShiftLeft] ? 60 : 10) * (float)e.Time;
            if (Keyboard[Key.S]) camera.Position += camera.Back * (Keyboard[Key.ShiftLeft] ? 60 : 10) * (float)e.Time;
            if (Keyboard[Key.A]) camera.Position += camera.Left * (Keyboard[Key.ShiftLeft] ? 60 : 10) * (float)e.Time;
            if (Keyboard[Key.D]) camera.Position += camera.Right * (Keyboard[Key.ShiftLeft] ? 60 : 10) * (float)e.Time;

        }

        protected override void OnRenderFrame(OpenTK.FrameEventArgs e) {
            base.OnRenderFrame(e);
            camera.Update();

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            if (Keyboard[Key.Space]) {
                GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);
                GL.Disable(EnableCap.CullFace);
            } else {
                GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
                GL.Enable(EnableCap.CullFace);
            }

            vao.Bind();
            shader.Bind();
            shader.SetUniform("mvp", false, ref camera.CameraMatrix);
            GL.UseProgram(shader.ID);
            GL.DrawArrays(PrimitiveType.Triangles, 0, size);

            this.SwapBuffers();
        }

        protected override void OnResize(EventArgs e) {
            GL.Viewport(0, 0, Width, Height);
            base.OnResize(e);
        }

        protected override void OnUnload(EventArgs e) {
            base.OnUnload(e);
            shader.Dispose();
            vbo.Dispose();
            vao.Dispose();
        }

    }
}