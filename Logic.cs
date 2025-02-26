// Need to fix checks for lots of collisions in quick succession

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

namespace spherical_pool_in_a_vacuum
{
    internal class Sim : GameWindow {
        const float baseTimeStep = 0.0005f;
        public const float cueBallVy = 5000f;
        public float timeStep = baseTimeStep;
        public float friction = 1.0f;
        const float ballRadius = 40f; //11.25 for correct pool scale
        const float ballDiameter = 2 * ballRadius;
        const float restitution = 0.8f;
        const float root3over2 = 0.86603f;
        const int ballCount = 16;

        float[] vertices = PoolSetup.CircleVertices(ballRadius, 20);
        float[] instancePositions = new float[2 * ballCount];
        float[] instanceRotations = new float[ballCount];
        float[] instanceColours = PoolSetup.Colours();

        int vao;
        int positionsVbo;
        int rotationsVbo;
        int coloursVbo;
        int shaderProgram;
        int width, height;


        // relative coords (coord * ballDiameter)
        float[] rackXcoords = PoolSetup.RackX();

        // relative coords (250 + coord * root3over2 * ballDiameter)
        float[] rackYcoords = PoolSetup.RackY();

        public List<RigidBody> balls;

        
        public Sim(int width, int height) : base(GameWindowSettings.Default, NativeWindowSettings.Default)
        {
            this.width = width;
            this.height = height;

            CenterWindow(new Vector2i(width,height));

            // list of balls
            balls = new List<RigidBody>();
            for (int i = 0; i < ballCount - 1; i++)
            {
                float x = rackXcoords[i] * (ballDiameter+0.1f);
                float y = 250 + rackYcoords[i] * root3over2 * (ballDiameter+0.1f);
                balls.Add(new RigidBody(new Vector2(x, y), new Vector2(0f, 0f), 0f, 0f, 1f));
            }

            balls.Add(new RigidBody(new Vector2(0, -250), new Vector2(0f, 0f), 0f, 0f, 1f));
        }

        protected override void OnResize(ResizeEventArgs e)
        {
            base.OnResize(e);
            GL.Viewport(0,0,e.Width,e.Height);
            this.width = e.Width;
            this.height = e.Height;
            
            // only projection on resize
            Matrix4 projection = Matrix4.CreateOrthographic(width, height, -1.0f, 1.0f);

            GL.UseProgram(shaderProgram);
            GL.UniformMatrix4(GL.GetUniformLocation(shaderProgram, "projection"), false, ref projection);

        }
        protected override void OnLoad()
        {
            base.OnLoad();

            // set up VAO
            vao = GL.GenVertexArray();
            GL.BindVertexArray(vao);

            // set up static vertex data in vertex VBO
            int vertexVbo = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, vertexVbo);
            GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw);
            
            // slot 0, floats per vertex, type, normalised?, stride, offset
            GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, 0, 0);
            GL.EnableVertexAttribArray(0);

            // refresh instances arrays
            for (int i=0; i < ballCount; i++)
            {
                instancePositions[2*i] = balls[i].Position.X;
                instancePositions[2*i + 1] = balls[i].Position.Y;
                instanceRotations[i] = balls[i].Theta;
            }
            
            // POSITIONS vbo setup
            positionsVbo = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer,positionsVbo);
            GL.BufferData(BufferTarget.ArrayBuffer,instancePositions.Length * 2 * sizeof(float), instancePositions, BufferUsageHint.DynamicDraw);
            
            // slot 1, floats per vertex, type, normalised?, stride, offset
            GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, 0, 0);
            GL.EnableVertexAttribArray(1);
            GL.VertexAttribDivisor(1, 1);

            // ROTATIONS vbo setup
            rotationsVbo = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer,rotationsVbo);
            GL.BufferData(BufferTarget.ArrayBuffer,instanceRotations.Length * sizeof(float), instanceRotations, BufferUsageHint.DynamicDraw);
            
            // slot 2, floats per vertex, type, normalised?, stride, offset
            GL.VertexAttribPointer(2, 1, VertexAttribPointerType.Float, false, 0, 0);
            GL.EnableVertexAttribArray(2);
            GL.VertexAttribDivisor(2, 1);

            // COLOURS vbo setup
            coloursVbo = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer,coloursVbo);
            GL.BufferData(BufferTarget.ArrayBuffer,instanceColours.Length * 4 * sizeof(float), instanceColours, BufferUsageHint.DynamicDraw);
            
            // slot 3, floats per vertex, type, normalised?, stride, offset
            GL.VertexAttribPointer(3, 4, VertexAttribPointerType.Float, false, 0, 0);
            GL.EnableVertexAttribArray(3);
            GL.VertexAttribDivisor(3, 1);


            // unbind vbo
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);            

            // unbind vao
            GL.BindVertexArray(0);

            // create shader program
            shaderProgram = GL.CreateProgram();

            int vertexShader = GL.CreateShader(ShaderType.VertexShader);
            GL.ShaderSource(vertexShader, LoadShaderSource("Default.vert"));
            GL.CompileShader(vertexShader);

            int fragmentShader = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(fragmentShader, LoadShaderSource("Default.frag"));
            GL.CompileShader(fragmentShader);

            GL.AttachShader(shaderProgram, vertexShader);
            GL.AttachShader(shaderProgram, fragmentShader);

            GL.LinkProgram(shaderProgram);

            // delete all shaders
            GL.DeleteShader(vertexShader);
            GL.DeleteShader(fragmentShader);

            foreach (RigidBody ball in balls)
            {
                System.Console.WriteLine(ball.Position.X);
            }

            Matrix4 projection = Matrix4.CreateOrthographic(width, height, -1f, 1f);   
            GL.UseProgram(shaderProgram);
            GL.UniformMatrix4(GL.GetUniformLocation(shaderProgram, "Projection"), false, ref projection);
            
            
        }

        protected override void OnUnload()
        {
            base.OnUnload();

            GL.DeleteVertexArray(vao);
            GL.DeleteProgram(shaderProgram);
        }

        public void CheckAndResolveCollisions()
        {
            for (int i = 0; i < ballCount; i++)
            {
                balls[i].EdgeCheckAndResolve(ballRadius, width, height, restitution);

                for (int j = i + 1; j < ballCount; j++)
                {
                    RigidBody.CollisionCheckAndResolve(balls[i], balls[j],ballRadius,restitution);
                }
            }
        }

        protected override void OnRenderFrame(FrameEventArgs args)
        {
            GL.ClearColor(0.2f,0.2f,0.2f,1.0f);
            GL.Clear(ClearBufferMask.ColorBufferBit);

            // draw objects            
            GL.BindBuffer(BufferTarget.ArrayBuffer,positionsVbo);
            GL.BufferData(BufferTarget.ArrayBuffer,instancePositions.Length * 2 * sizeof(float), instancePositions, BufferUsageHint.DynamicDraw);

            GL.BindBuffer(BufferTarget.ArrayBuffer,rotationsVbo);
            GL.BufferData(BufferTarget.ArrayBuffer,instanceRotations.Length * sizeof(float), instanceRotations, BufferUsageHint.DynamicDraw);
            
            Matrix4 projection = Matrix4.CreateOrthographic(width, height, -1f, 1f);   
            GL.UniformMatrix4(GL.GetUniformLocation(shaderProgram, "Projection"), false, ref projection);

            GL.UseProgram(shaderProgram);
            GL.BindVertexArray(vao);
            GL.DrawArraysInstanced(PrimitiveType.TriangleFan, 0, vertices.Length / 2, ballCount);

            Context.SwapBuffers();

            base.OnRenderFrame(args);
        }

        protected override void OnUpdateFrame(FrameEventArgs args)
        {
            float dt = timeStep;

            foreach (RigidBody ball in balls)
            {
                if (ball.Velocity.Length > 0.001f)
                {
                    ball.EffectForce(-ball.Velocity.Normalized() * friction * timeStep / baseTimeStep);
                }
                else
                {
                    ball.Velocity = new Vector2(0,0);
                }
            }

            for (int i=0; i < ballCount; i++)
            {
                balls[i].Update(dt);
                instancePositions[2*i] = balls[i].Position.X;
                instancePositions[2*i + 1] = balls[i].Position.Y;
                instanceRotations[i] = balls[i].Theta;
            }

            CheckAndResolveCollisions();

            base.OnUpdateFrame(args);
        }

        // function that loads a text file and returns a str

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
