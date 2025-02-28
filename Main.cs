using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;
using OpenTK.Mathematics;
using OpenTK.Graphics.OpenGL;

namespace spherical_pool_in_a_vacuum
{
    public class Program
    {
        static void Main(string[] args)
        {
            using(Sim sim = new Sim(500,850)) // 500, 850
            {                
                sim.KeyDown += (KeyboardKeyEventArgs e) =>
                {

                    if (e.Key == Keys.Space)
                    {
                        sim.balls[^1].Velocity = new Vector2(MathF.Sin(sim.direction),MathF.Cos(sim.direction));
                        sim.balls[^1].Velocity *= Sim.cueBallV;
                    }
                    if (e.Key == Keys.E)
                    {
                        sim.direction += 0.05f;
                    }

                    if (e.Key == Keys.Q)
                    {
                        sim.direction -= 0.05f;
                    }
                    if (e.Key == Keys.W && Sim.cueBallV < Sim.maxV)
                    {
                        Sim.cueBallV += 500f;
                    }
                    if (e.Key == Keys.S && Sim.cueBallV > Sim.minV)
                    {
                        Sim.cueBallV -= 500f;
                    }

                    if (e.Key == Keys.Up)
                    {
                        sim.balls[^1].overridePosition(0,5f);
                    }
                    if (e.Key == Keys.Down)
                    {
                        sim.balls[^1].overridePosition(0,-5f);
                    }
                    if (e.Key == Keys.Right)
                    {
                        sim.balls[^1].overridePosition(5f,0);
                    }
                    if (e.Key == Keys.Left)
                    {
                        sim.balls[^1].overridePosition(-5f,0f);
                    }

                    Sim.directionLength = Sim.cueBallV * Sim.directionLengthFactor;


                };
                sim.Run();

            }
        }
    }


}