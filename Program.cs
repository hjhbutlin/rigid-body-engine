﻿namespace physics_engine
{
    public class Program
    {
        static void Main(string[] args)
        {
            using(Game game = new Game(500,500))
            {
                game.Run();
            }
        }
    }


}