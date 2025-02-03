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

namespace particle_simulation
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

        const int particleCount = 5;
        const float particleRadius = 15f;
        const float particleDiameter = 2 * particleRadius;
        const float restitution = 0.5f;
        const float friction = 0.1f;
        const float tickRate = 1f/60f;
        const float G = 1000f;

        float[] vertices = CircleVertices(particleRadius, 20);
        //float[] vertices = SquareVertices(50f);
        float[] instancePositions = new float[2 * particleCount];
        float[] instanceRotations = new float[particleCount];

        int vao;
        int positionsVbo;
        int rotationsVbo;
        int shaderProgram;
        int width, height;

        List<RigidBody> particles;
        public Game(int width, int height) : base(GameWindowSettings.Default, NativeWindowSettings.Default)
        {
            this.width = width;
            this.height = height;

            CenterWindow(new Vector2i(width,height));

            // list of particles
            particles = new List<RigidBody>();
            for (int i = 0; i < particleCount; i++)
            {
                float x = -width + 50 + 200*i;
                float y = 10*i*i;
                particles.Add(new RigidBody(new Vector2(x, y), new Vector2(0f, 0f), 0f, 0f, (1+i*i)));
            }

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
            for (int i=0; i < particleCount; i++)
            {
                instancePositions[2*i] = particles[i].Position.X;
                instancePositions[2*i + 1] = particles[i].Position.Y;
                instanceRotations[i] = particles[i].Theta;
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

            foreach (RigidBody particle in particles)
            {
            System.Console.WriteLine(particle.Position.X);
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
            for (int i = 0; i < particleCount; i++)
            {
                // check for collision at edge assuming rectangular boundaries and apply overlap correction
                if (particles[i].Position.X - particleRadius <= -width)
                {
                    particles[i].Velocity = new Vector2(particles[i].Velocity.X * -restitution, particles[i].Velocity.Y);
                    particles[i].Position = new Vector2(-width + particleRadius, particles[i].Position.Y);
                }
                else if (particles[i].Position.X + particleRadius >= width)
                {
                    particles[i].Velocity = new Vector2(particles[i].Velocity.X * -restitution, particles[i].Velocity.Y);
                    particles[i].Position = new Vector2(width - particleRadius, particles[i].Position.Y);
                }

                if (particles[i].Position.Y - particleRadius <= -height)
                {
                    particles[i].Velocity = new Vector2(particles[i].Velocity.X, particles[i].Velocity.Y * -restitution);
                    particles[i].Position = new Vector2(particles[i].Position.X, -height + particleRadius);
                }
                else if (particles[i].Position.Y + particleRadius >= height)
                {
                    particles[i].Velocity = new Vector2(particles[i].Velocity.X, particles[i].Velocity.Y * -restitution);
                    particles[i].Position = new Vector2(particles[i].Position.X, height - particleRadius);
                }

                for (int j = i + 1; j < particleCount; j++)
                {
                    Vector2 relativePosition = particles[i].Position - particles[j].Position;
                    float distance = relativePosition.Length;

                    if (distance <= particleDiameter)
                    {
                        Vector2 normal = Vector2.Normalize(relativePosition);
                        Vector2 relativeVelocity = particles[i].Velocity - particles[j].Velocity;
                        float speedProjection = Vector2.Dot(relativeVelocity, normal);

                        // no collision if particles are already moving apart
                        if (speedProjection > 0)
                        {
                            return;
                        }

                        // reduced mass and restitution
                        Vector2 impulse = normal * (1 + restitution) * speedProjection / ((1 / particles[i].Mass) + (1 / particles[j].Mass));
                        particles[i].Velocity -= impulse / particles[i].Mass;
                        particles[j].Velocity += impulse / particles[j].Mass;
                        
                        // in case of overlap, move particles apart to be just touching
                        float overlapCorrection = (particleDiameter - distance) / 2;
                        particles[i].Position -= normal * overlapCorrection;
                        particles[j].Position += normal * overlapCorrection;

                        System.Console.WriteLine(particles[i].Velocity);
                        System.Console.WriteLine(particles[j].Velocity);

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
            GL.DrawArraysInstanced(PrimitiveType.TriangleFan, 0, vertices.Length / 2, particleCount);

            Context.SwapBuffers();

            base.OnRenderFrame(args);
        }


        // separate here
        protected override void OnUpdateFrame(FrameEventArgs args)
        {
            float dt = (float)args.Time;

            for (int i=0; i < particleCount; i++)
            {
                particles[i].Update(dt);
                instancePositions[2*i] = particles[i].Position.X;
                instancePositions[2*i + 1] = particles[i].Position.Y;
                instanceRotations[i] = particles[i].Theta;
            }
            
            for (int i=0; i<particleCount; i++)
            {
                for (int j=0; j<particleCount; j++)
                {
                    if (i!=j)
                    {
                        Vector2 relativePos = particles[j].Position - particles[i].Position;
                        float d = relativePos.Length;
                        Vector2 rHat = relativePos.Normalized();
                        particles[i].EffectForce(rHat * particles[i].Mass * particles[j].Mass * G/(d*d));
                    }
                }
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