using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;
using OpenTK.Mathematics;

namespace spherical_pool_in_a_vacuum
{
    public class Program
    {
        static void Main(string[] args)
        {
            using(Sim sim = new Sim(500,1000)) // 250, 500
            {                
                sim.KeyDown += (KeyboardKeyEventArgs e) =>
                {
                    if (e.Key == Keys.Equal)
                    {
                        sim.timeStep += 0.0002f;
                    }

                    if (e.Key == Keys.Minus && sim.timeStep > 0.0002f)
                    {
                        sim.timeStep -= 0.0002f;
                    }

                    if (e.Key == Keys.Space)
                    {
                        sim.balls[^1].Velocity = new Vector2(0f,Sim.cueBallVy);
                    }

                    Console.WriteLine($"Time Step: {sim.timeStep}");

                };
                sim.Run();

            }
        }
    }


}