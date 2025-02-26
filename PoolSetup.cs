namespace spherical_pool_in_a_vacuum
{
    internal class PoolSetup {
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
        

        // public float[] colours() {
        //     return;
        // }
    }

}