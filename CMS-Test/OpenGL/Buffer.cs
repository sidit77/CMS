using System;
using OpenTK.Graphics.OpenGL4;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace CMS_Test.OpenGL{

    class BufferObject : IDisposable {

        private const int IMMUTABLE = 1;
        private const int NOT_DYNAMIC = 2;

        [Conditional("DEBUG")]
        private void CheckAccess(int i) {
            if((immutable & i) != 0)
                throw new InvalidOperationException();
        }

        private readonly int id;
        private int immutable = 0;

        public BufferObject() {
            id = GL.GenBuffer();
        }

        public int ID {
            get {
                return id;
            }
        }

        public BufferObject Bind(BufferTarget bt) {
            GL.BindBuffer(bt, id);
            return this;
        }

        public BufferObject SubData(IntPtr offset, int size, IntPtr data) {
            CheckAccess(NOT_DYNAMIC);
            GL.NamedBufferSubData(id, offset, size, data);
            return this;
        }

        public BufferObject SubData<T>(IntPtr offset, int size, T[] data)  where T : struct {
            CheckAccess(NOT_DYNAMIC);
            GL.NamedBufferSubData(id, offset, size, data);
            return this;
        }

        public BufferObject SubData<T>(IntPtr offset, T[] data) where T : struct {
            CheckAccess(NOT_DYNAMIC);
            GL.NamedBufferSubData(id, offset, data.Length * Marshal.SizeOf(typeof(T)), data);
            return this;
        }

        public BufferObject Data(int size, IntPtr data, BufferUsageHint hint) {
            CheckAccess(IMMUTABLE);
            GL.NamedBufferData(id, size, data, hint);
            return this;
        }

        public BufferObject Data<T>(int size, T[] data, BufferUsageHint hint) where T : struct {
            CheckAccess(IMMUTABLE);
            GL.NamedBufferData(id, size, data, hint);
            return this;
        }

        public BufferObject Data<T>(T[] data, BufferUsageHint hint) where T : struct {
            CheckAccess(IMMUTABLE);
            GL.NamedBufferData(id, data.Length * Marshal.SizeOf(typeof(T)), data, hint);
            return this;
        }

        public void Dispose() {
            GL.DeleteBuffer(id);
        }

        public static BufferObject CreateBuffer(int size, IntPtr data, BufferStorageFlags flags) {
            BufferObject b = new BufferObject();
            b.Bind(BufferTarget.CopyWriteBuffer);
            b.immutable |= IMMUTABLE;
            if((flags & BufferStorageFlags.DynamicStorageBit) == 0)
                b.immutable |= NOT_DYNAMIC;
            GL.NamedBufferStorage(b.ID, size, data, flags);
            return b;
        }

        public static BufferObject CreateBuffer(int size, BufferStorageFlags flags) {
            BufferObject b = new BufferObject();
            b.Bind(BufferTarget.CopyWriteBuffer);
            b.immutable |= IMMUTABLE;
            if((flags & BufferStorageFlags.DynamicStorageBit) == 0)
                b.immutable |= NOT_DYNAMIC;
            GL.NamedBufferStorage(b.ID, size, IntPtr.Zero, flags);
            return b;
        }

        public static BufferObject CreateBuffer<T>(T[] data, BufferStorageFlags flags) where T : struct{
            BufferObject b = new BufferObject();
            b.Bind(BufferTarget.CopyWriteBuffer);
            b.immutable |= IMMUTABLE;
            if((flags & BufferStorageFlags.DynamicStorageBit) == 0)
                b.immutable |= NOT_DYNAMIC;
            GL.NamedBufferStorage(b.ID, data.Length * Marshal.SizeOf(typeof(T)), data, flags);
            return b;
        }

        public static BufferObject CreateBuffer<T>(int size, T[] data, BufferStorageFlags flags) where T : struct {
            BufferObject b = new BufferObject();
            b.Bind(BufferTarget.CopyWriteBuffer);
            b.immutable |= IMMUTABLE;
            if((flags & BufferStorageFlags.DynamicStorageBit) == 0)
                b.immutable |= NOT_DYNAMIC;
            GL.NamedBufferStorage(b.ID, size, data, flags);
            return b;
        }
    }

}
