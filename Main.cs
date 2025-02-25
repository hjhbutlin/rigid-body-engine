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
    internal class Game : GameWindow {

        public static float[] CircleVertices(float radius, int n)
        {
            float[] vertices = new float[n*2];
            float angleStep = MathF.PI * 2 / n;

            for (int i = 0; i < n; i++)
            {
                float angle = i * angleStep;
                vertices[i * 2] = radius * MathF.Cos(angle);
                vertices[i * 2 + 1] = radius * MathF.Sin(angle);
            }

            return vertices;
        }
        public static float[] SquareVertices(float sideLength)
        {
            float halfS = sideLength/2;
            float[] vertices =
            {
                halfS, halfS,
                halfS, -halfS,
                -halfS, -halfS,
                -halfS, halfS
            };

            return vertices;
        }

        const int ballCount = 16;
        const float ballRadius = 11.25f;
        const float ballDiameter = 2 * ballRadius;
        const float restitution = 0.7f;
        const float root3over2 = 0.86603f;
        const float friction = 0.1f;

        float[] vertices = CircleVertices(ballRadius, 20);
        //float[] vertices = SquareVertices(50f);
        float[] instancePositions = new float[2 * ballCount];
        float[] instanceRotations = new float[ballCount];

        int vao;
        int positionsVbo;
        int rotationsVbo;
        int shaderProgram;
        int width, height;

        // relative coords (coord * ballDiameter)
        float[] triangleXcoords = {
            0f,
            -0.5f, 0.5f,
            -1f, 0f, 1f,
            -1.5f, -0.5f, 0.5f, 1.5f,
            -2f, -1f, 0f, 1f, 2f
        };

        // relative coords (250 + coord * root3over2 * ballDiameter)
        float[] triangleYcoords = {
            0,
            1, 1,
            2, 2, 2,
            3, 3, 3, 3,
            4, 4, 4, 4, 4
        };

        List<RigidBody> balls;
        public Game(int width, int height) : base(GameWindowSettings.Default, NativeWindowSettings.Default)
        {
            this.width = width;
            this.height = height;

            CenterWindow(new Vector2i(width,height));

            // list of balls
            balls = new List<RigidBody>();
            for (int i = 0; i < ballCount - 1; i++)
            {
                float x = triangleXcoords[i] * (ballDiameter+1);
                float y = 250 + triangleYcoords[i] * root3over2 * (ballDiameter+1);
                balls.Add(new RigidBody(new Vector2(x, y), new Vector2(0f, 0f), 0f, 0f, 1f));
            }

            balls.Add(new RigidBody(new Vector2(0, -250), new Vector2(10f, 3000f), 0f, 0f, 1f));


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
                // check for collision at edge assuming rectangular boundaries and apply overlap correction
                if (balls[i].Position.X - ballRadius <= -width)
                {
                    balls[i].Velocity = new Vector2(balls[i].Velocity.X * -restitution, balls[i].Velocity.Y);
                    balls[i].Position = new Vector2(-width + ballRadius, balls[i].Position.Y);
                }
                else if (balls[i].Position.X + ballRadius >= width)
                {
                    balls[i].Velocity = new Vector2(balls[i].Velocity.X * -restitution, balls[i].Velocity.Y);
                    balls[i].Position = new Vector2(width - ballRadius, balls[i].Position.Y);
                }

                if (balls[i].Position.Y - ballRadius <= -height)
                {
                    balls[i].Velocity = new Vector2(balls[i].Velocity.X, balls[i].Velocity.Y * -restitution);
                    balls[i].Position = new Vector2(balls[i].Position.X, -height + ballRadius);
                }
                else if (balls[i].Position.Y + ballRadius >= height)
                {
                    balls[i].Velocity = new Vector2(balls[i].Velocity.X, balls[i].Velocity.Y * -restitution);
                    balls[i].Position = new Vector2(balls[i].Position.X, height - ballRadius);
                }

                for (int j = i + 1; j < ballCount; j++)
                {
                    Vector2 relativePosition = balls[i].Position - balls[j].Position;
                    float distance = relativePosition.Length;

                    if (distance <= ballDiameter)
                    {
                        Vector2 normal = Vector2.Normalize(relativePosition);
                        Vector2 relativeVelocity = balls[i].Velocity - balls[j].Velocity;
                        float speedProjection = Vector2.Dot(relativeVelocity, normal);

                        // no collision if balls are already moving apart
                        if (speedProjection > 0)
                        {
                            return;
                        }

                        // reduced mass and restitution
                        Vector2 impulse = normal * (1 + restitution) * speedProjection / ((1 / balls[i].Mass) + (1 / balls[j].Mass));
                        balls[i].Velocity -= impulse / balls[i].Mass;
                        balls[j].Velocity += impulse / balls[j].Mass;
                        
                        // in case of overlap, move balls apart to be just touching
                        float overlapCorrection = (ballDiameter - distance) / 2;
                        balls[i].Position -= normal * overlapCorrection;
                        balls[j].Position += normal * overlapCorrection;

                        System.Console.WriteLine(balls[i].Velocity);
                        System.Console.WriteLine(balls[j].Velocity);

                    }
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
            float dt = 0.0005f;

            foreach (RigidBody ball in balls)
            {
                if (ball.Velocity.Length > 0.001f)
                {
                    ball.EffectForce(-ball.Velocity.Normalized() * friction);
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
