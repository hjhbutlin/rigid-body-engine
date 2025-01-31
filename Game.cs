using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace physics_engine
{
    internal class Game : GameWindow {


        float[] vertices = 
        {
            -50f, 50f,
            -50f, -50f,
            50f, -50f,
            50f, 50f
        };

        // Render Pipeline variables
        int vao;
        int shaderProgram;
        int width, height;
        public Game(int width, int height) : base(GameWindowSettings.Default, NativeWindowSettings.Default)
        {
            this.width = width;
            this.height = height;

            CenterWindow(new Vector2i(width,height));

        }

        protected override void OnResize(ResizeEventArgs e)
        {
            base.OnResize(e);
            GL.Viewport(0,0,e.Width,e.Height);
            this.width = e.Width;
            this.height = e.Height;

            Matrix4 projection = Matrix4.CreateOrthographic(width, height, -1.0f, 1.0f);
            GL.UseProgram(shaderProgram);
            GL.UniformMatrix4(GL.GetUniformLocation(shaderProgram, "projection"), false, ref projection);

        }
        protected override void OnLoad()
        {
            base.OnLoad();

            vao = GL.GenVertexArray();

            int vbo = GL.GenBuffer();
            // bind vbo
            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
            // data into vbo
            GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length*sizeof(float), vertices, BufferUsageHint.StaticDraw);
            
            // bind vao
            GL.BindVertexArray(vao);
            // slot 0, floats per vertex, type, normalised?, stride, offset
            GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, 0, 0);
            // enable slot 0
            GL.EnableVertexAttribArray(0);

            // unbind both
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindVertexArray(0);

            // create shader program
            shaderProgram = GL.CreateProgram();

            int vertexShader = GL.CreateShader(ShaderType.VertexShader);
            GL.ShaderSource(vertexShader,LoadShaderSource("Default.vert"));
            GL.CompileShader(vertexShader);

            int fragmentShader = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(fragmentShader,LoadShaderSource("Default.frag"));
            GL.CompileShader(fragmentShader);

            GL.AttachShader(shaderProgram, vertexShader);
            GL.AttachShader(shaderProgram, fragmentShader);

            GL.LinkProgram(shaderProgram);

            // delete all shaders

            GL.DeleteShader(vertexShader);
            GL.DeleteShader(fragmentShader);

            Matrix4 projection = Matrix4.CreateOrthographic(width, height, -1.0f, 1.0f);
            GL.UseProgram(shaderProgram);
            GL.UniformMatrix4(GL.GetUniformLocation(shaderProgram, "projection"), false, ref projection);

        }

        protected override void OnUnload()
        {
            base.OnUnload();

            GL.DeleteVertexArray(vao);
            GL.DeleteProgram(shaderProgram);
        }

        protected override void OnRenderFrame(FrameEventArgs args)
        {
            GL.ClearColor(0.2f,0.2f,0.2f,1.0f);
            GL.Clear(ClearBufferMask.ColorBufferBit);

            // draw object

            GL.UseProgram(shaderProgram);
            GL.BindVertexArray(vao);
            GL.DrawArrays(PrimitiveType.TriangleFan, 0, 4);


            Context.SwapBuffers();

            base.OnRenderFrame(args);
        }

        protected override void OnUpdateFrame(FrameEventArgs args)
        {
            base.OnUpdateFrame(args);
        }

        // function that loads a .txt and returns a str

        public static string LoadShaderSource(String filePath)
        {
            string shaderSource = "";

            try
            {
                using (StreamReader reader = new StreamReader("./Shaders/" + filePath))
                {
                    
                    shaderSource = reader.ReadToEnd();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Failed to read shader source file: " + e.Message);
            }

            return shaderSource;
        }

    }


}