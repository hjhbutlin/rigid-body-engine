using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace spherical_pool_in_a_vacuum
{
    public class Program
    {
        static void Main(string[] args)
        {
            using(Sim sim = new Sim(250,500))
            {                
                sim.KeyDown += (KeyboardKeyEventArgs e) =>
                {
                    if (e.Key == Keys.Equal) sim.timeStep += 0.0002f;
                    if (e.Key == Keys.Minus && sim.timeStep > 0.0002f) sim.timeStep -= 0.0002f;
                    Console.WriteLine($"Time Step: {sim.timeStep}");
                };
                sim.Run();

            }
        }
    }


}