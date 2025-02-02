namespace spherical_pool_in_a_vacuum
{
    public class Program
    {
        static void Main(string[] args)
        {
            using(Game game = new Game(250,500))
            {
                game.Run();
            }
        }
    }


}