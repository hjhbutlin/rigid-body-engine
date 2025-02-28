// Need to fix checks for lots of collisions in quick succession

using System;
using System.IO;
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
using StbImageSharp;

namespace spherical_pool_in_a_vacuum
{
    public class Sim : GameWindow {
        public const float maxV = 25000f;
        public const float minV = 500f;

        public static float cueBallV = 10000f; // base
        public const float directionLengthFactor = 0.01f;
        public static float directionLength = cueBallV * directionLengthFactor;
        public const float timeStep = 0.0005f;
        public float friction = 1.0f;
        public const float ballRadius = 22.5f; // 22.5 for a window size 500x 850y
        public const float ballDiameter = 2 * ballRadius;
        const float collisionRestitution = 0.8f;
        const float railRestitution = 0.7f;
        const float root3over2 = 0.86603f;
        protected static int ballCount = 16;
        public const float boundaryLeftFrac = 2 * 66f/500f;
        public const float boundaryRightFrac = 2 * 72f/500f;
        public const float boundaryTopFrac = 2 * 67f/850f;
        public const float boundaryBottomFrac = 2 * 69f/850f;
        public static float boundaryLeft, boundaryRight, boundaryTop, boundaryBottom;

        float[] vertices = PoolSetup.CircleVertices(ballRadius, 20);
        float[] instancePositions = new float[2 * ballCount];
        float[] instanceRotations = new float[ballCount];
        Vector4[] instanceColours = PoolSetup.InitialColours();

        float[] bgVertices =
        {
            // positions     // texture cords
            -1.0f, 1.0f,  0.0f, 0.0f, // bottom left
            1.0f, 1.0f,  1.0f, 0.0f, // bottom right
            -1.0f,  -1.0f,  0.0f, 1.0f, // top left
            1.0f,  -1.0f,  1.0f, 1.0f  // top right
        };

        float[] directionVertices =
        {
            0.0f, 0.0f,
            0.0f, directionLength
        };
        public float direction = 0f;

        int bgShaderProgram, directionShaderProgram, bgVao, directionVao, bgTexture, vao, positionsVbo, rotationsVbo, coloursVbo, directionVbo,shaderProgram, width, height;

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
                float y = 300 + rackYcoords[i] * root3over2 * (ballDiameter+0.1f);
                balls.Add(new RigidBody(new Vector2(x, y), new Vector2(0f, 0f), 0f, 0f, 1f, false));
            }

            // CUE BALL
            balls.Add(new RigidBody(new Vector2(0, -375), new Vector2(0f, 0f),0f, 0f, 1f, false));
        }

        protected override void OnResize(ResizeEventArgs e)
        {
            base.OnResize(e);
            GL.Viewport(0,0,e.Width,e.Height);
            width = e.Width;
            height = e.Height;

            boundaryLeft = boundaryLeftFrac * width;
            boundaryRight = boundaryRightFrac * width;
            boundaryTop = boundaryTopFrac * height;
            boundaryBottom = boundaryBottomFrac * height;
            
            // only projection on resize
            Matrix4 projection = Matrix4.CreateOrthographic(width, height, -1.0f, 1.0f);

            GL.UseProgram(shaderProgram);
            GL.UniformMatrix4(GL.GetUniformLocation(shaderProgram, "projection"), false, ref projection);

        }

        public static int LoadTexture()
        {
            int texture;
            GL.GenTextures(1, out texture);
            GL.BindTexture(TextureTarget.Texture2D, texture);

            using (Stream stream = File.OpenRead("./pool_bg.jpg"))
            {
                ImageResult image = ImageResult.FromStream(stream, ColorComponents.RedGreenBlueAlpha);
                GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba,
                    image.Width, image.Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, image.Data);
            }

            // texture settings
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);

            GL.BindTexture(TextureTarget.Texture2D, 0);
            return texture;
        }
        protected override void OnLoad()
        {
            base.OnLoad();

            boundaryLeft = boundaryLeftFrac * width;
            boundaryRight = boundaryRightFrac * width;
            boundaryTop = boundaryTopFrac * height;
            boundaryBottom = boundaryBottomFrac * height;

            bgTexture = LoadTexture();

            // set up background VAO & VBO
            bgVao = GL.GenVertexArray();
            int bgVbo = GL.GenBuffer();

            GL.BindVertexArray(bgVao);
            GL.BindBuffer(BufferTarget.ArrayBuffer, bgVbo);
            GL.BufferData(BufferTarget.ArrayBuffer, bgVertices.Length * sizeof(float), bgVertices, BufferUsageHint.StaticDraw);

            GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, 4 * sizeof(float), 0);
            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, 4 * sizeof(float), 2 * sizeof(float));
            GL.EnableVertexAttribArray(1);

            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);

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
            directionVertices[0] = balls[^1].Position.X;
            directionVertices[1] = balls[^1].Position.Y;
            directionVertices[2] = balls[^1].Position.X + directionLength * MathF.Sin(direction);
            directionVertices[3] = balls[^1].Position.Y + directionLength * MathF.Cos(direction);

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

            // direction indicator vao
            directionVao = GL.GenVertexArray();
            GL.BindVertexArray(directionVao);

            directionVbo = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, directionVbo);
            GL.BufferData(BufferTarget.ArrayBuffer, directionVertices.Length * sizeof(float), directionVertices, BufferUsageHint.DynamicDraw);

            GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, 0, 0);
            GL.EnableVertexAttribArray(0);

            // unbind vbo
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);

            // unbind vao
            GL.BindVertexArray(0);

            // create shader programs
            // BACKGROUND
            bgShaderProgram = GL.CreateProgram();
            int bgVertexShader = GL.CreateShader(ShaderType.VertexShader);
            GL.ShaderSource(bgVertexShader, LoadShaderSource("Background.vert"));
            GL.CompileShader(bgVertexShader);

            int bgFragmentShader = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(bgFragmentShader, LoadShaderSource("Background.frag"));
            GL.CompileShader(bgFragmentShader);

            GL.AttachShader(bgShaderProgram, bgVertexShader);
            GL.AttachShader(bgShaderProgram, bgFragmentShader);
            GL.LinkProgram(bgShaderProgram);

            GL.DeleteShader(bgVertexShader);
            GL.DeleteShader(bgFragmentShader);

            // BALLS
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

            GL.DeleteShader(vertexShader);
            GL.DeleteShader(fragmentShader);

            // DIRECTION INDICATOR
            directionShaderProgram = GL.CreateProgram();
            int directionVertexShader = GL.CreateShader(ShaderType.VertexShader);
            GL.ShaderSource(directionVertexShader, LoadShaderSource("Direction.vert"));
            GL.CompileShader(directionVertexShader);

            int directionFragmentShader = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(directionFragmentShader, LoadShaderSource("Direction.frag"));
            GL.CompileShader(directionFragmentShader);

            GL.AttachShader(directionShaderProgram, directionVertexShader);
            GL.AttachShader(directionShaderProgram, directionFragmentShader);
            GL.LinkProgram(directionShaderProgram);

            GL.DeleteShader(directionVertexShader);
            GL.DeleteShader(directionFragmentShader);

            // 
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
                if (!balls[i].Potted)
                {
                    balls[i].EdgeCheckAndResolve(width, height, railRestitution);
                

                    for (int j = i + 1; j < ballCount; j++)
                    {
                        RigidBody.CollisionCheckAndResolve(balls[i], balls[j],collisionRestitution);
                    }
                }
            }
        }

        protected override void OnRenderFrame(FrameEventArgs args)
        {
            // draw background
            GL.ClearColor(0f,0.2f,0f,1.0f);
            GL.Clear(ClearBufferMask.ColorBufferBit);

            GL.Clear(ClearBufferMask.ColorBufferBit);

            GL.UseProgram(bgShaderProgram);
            GL.BindVertexArray(bgVao);
            GL.BindTexture(TextureTarget.Texture2D, bgTexture);
            GL.DrawArrays(PrimitiveType.TriangleStrip, 0, 4);

            // draw balls            
            GL.BindBuffer(BufferTarget.ArrayBuffer,positionsVbo);
            GL.BufferData(BufferTarget.ArrayBuffer,instancePositions.Length * 2 * sizeof(float), instancePositions, BufferUsageHint.DynamicDraw);

            GL.BindBuffer(BufferTarget.ArrayBuffer,rotationsVbo);
            GL.BufferData(BufferTarget.ArrayBuffer,instanceRotations.Length * sizeof(float), instanceRotations, BufferUsageHint.DynamicDraw);
            
            Matrix4 projection = Matrix4.CreateOrthographic(width, height, -1f, 1f);   
            GL.UniformMatrix4(GL.GetUniformLocation(shaderProgram, "Projection"), false, ref projection);

            GL.UseProgram(shaderProgram);
            GL.BindVertexArray(vao);
            GL.DrawArraysInstanced(PrimitiveType.TriangleFan, 0, vertices.Length / 2, ballCount);

            // draw direction indicator
            GL.BindBuffer(BufferTarget.ArrayBuffer,directionVbo);
            GL.BufferData(BufferTarget.ArrayBuffer,directionVertices.Length * 2 * sizeof(float), directionVertices, BufferUsageHint.DynamicDraw);

            GL.UseProgram(directionShaderProgram);

            GL.BindVertexArray(directionVao);
            GL.LineWidth(5f);

            GL.Enable(EnableCap.LineSmooth);
            GL.DrawArrays(PrimitiveType.Lines, 0, 2);
            GL.LineWidth(5f);


            GL.BindVertexArray(0);

            Context.SwapBuffers();

            base.OnRenderFrame(args);
        }

        protected override void OnUpdateFrame(FrameEventArgs args)
        {
            float dt = timeStep;

            foreach (RigidBody ball in balls)
            {
                if (ball.Velocity.Length > 1f)
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
            directionVertices[0] = balls[^1].Position.X;
            directionVertices[1] = balls[^1].Position.Y;
            directionVertices[2] = balls[^1].Position.X + directionLength * MathF.Sin(direction);
            directionVertices[3] = balls[^1].Position.Y + directionLength * MathF.Cos(direction);


            for (int i=ballCount - 1; i > -1; i--)
            {
                if (balls[i].IsPotted(width,height))
                {
                    balls[i].Potted = true;
                    balls[i].Position = new Vector2(-4000f, i*ballDiameter*2);
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
