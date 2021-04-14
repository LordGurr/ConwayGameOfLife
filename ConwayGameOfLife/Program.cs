using System;

namespace ConwayGameOfLife
{
    public static class Program
    {
        [STAThread]
        private static void Main()
        {
            Console.WriteLine();
            using (var game = new Game1())//added this
                game.Run();
        }
    }
}