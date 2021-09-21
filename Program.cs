using System;
using craw;

namespace ConsoleEngine
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            var app = new CrawController();
            app.ShouldRun = true;
            app.Run();
        }
    }
}