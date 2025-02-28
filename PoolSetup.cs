using OpenTK.Mathematics;

namespace spherical_pool_in_a_vacuum
{
    public class PoolSetup
    {
        public static float[] RackX() {
            float[] output = {
                0f,
                -0.5f, 0.5f,
                -1f, 0f, 1f,
                -1.5f, -0.5f, 0.5f, 1.5f,
                -2f, -1f, 0f, 1f, 2f
            };

            return output;
        }
        
        public static float[] RackY() {
            float[] output = {
                0,
                1, 1,
                2, 2, 2,
                3, 3, 3, 3,
                4, 4, 4, 4, 4
            };

            return output;
        }

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
        
        public class Colours
        {
            public static readonly Vector4 Red = new Vector4(1.0f, 0.0f, 0.0f, 1.0f);
            public static readonly Vector4 Yellow = new Vector4(1.0f, 1.0f, 0.0f, 1.0f);
            public static readonly Vector4 Black = new Vector4(0.0f, 0.0f, 0.0f, 1.0f);
            public static readonly Vector4 White = new Vector4(0.9f, 0.9f, 0.9f, 1.0f);
        }

        public static Vector4[] InitialColours()
        {
            //     r
            //    y r
            //   r b y
            //  y r y r
            // r y y r y

            Vector4[] output = {
                Colours.Red,
                Colours.Yellow, Colours.Red,
                Colours.Red, Colours.Black, Colours.Yellow,
                Colours.Yellow, Colours.Red, Colours.Yellow, Colours.Red,
                Colours.Red, Colours.Yellow, Colours.Yellow, Colours.Red, Colours.Yellow,
                Colours.White
            };

            return output;
        }
    }

}