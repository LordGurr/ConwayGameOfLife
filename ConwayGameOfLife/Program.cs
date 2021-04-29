using System;
using System.Diagnostics;

namespace ConwayGameOfLife
{
    public static class Program
    {
        [STAThread]
        private static void Main()
        {
            Debug.WriteLine("Hjälp mig");
            using (var game = new Game1())
                game.Run();
        }
    }
}