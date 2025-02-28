using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;
using OpenTK.Mathematics;

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
                        sim.balls[^1].Velocity = new Vector2(0f,Sim.cueBallVy);
                    }
                    if (e.Key == Keys.W)
                    {
                        sim.balls[^1].overridePosition(0,5f);
                    }
                    if (e.Key == Keys.S)
                    {
                        sim.balls[^1].overridePosition(0,-5f);
                    }
                    if (e.Key == Keys.D)
                    {
                        sim.balls[^1].overridePosition(5f,0);
                    }
                    if (e.Key == Keys.A)
                    {
                        sim.balls[^1].overridePosition(-5f,0f);
                    }


                };
                sim.Run();

            }
        }
    }


}