using Stride.Engine;

namespace MyFPSTest
{
    class MyFPSTestApp
    {
        static void Main(string[] args)
        {
            using (var game = new Game())
            {
                game.Run();
            }
        }
    }
}
