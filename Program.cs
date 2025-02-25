namespace spherical_pool_in_a_vacuum
{
    public class Program
    {
        static void Main(string[] args)
        {
            using(Sim sim = new Sim(250,500))
            {
                sim.Run();
            }
        }
    }


}