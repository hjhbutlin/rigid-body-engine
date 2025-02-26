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

                    if (e.Key == Keys.Space)
                    {
                        sim.balls[^1].Velocity = new Vector2(0f,Sim.cueBallVy);
                    }


                };
                sim.Run();

            }
        }
    }


}