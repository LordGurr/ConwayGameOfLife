using System;

namespace ConwayGameOfLife
{
    public static class Program
    {
        [STAThread]
        private static void Main()
        {
            Console.WriteLine("Hjälp mig");
            using (var game = new Game1())
                game.Run();
        }
    }
}