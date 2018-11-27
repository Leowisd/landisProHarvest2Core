using System;
namespace LANDIS_II_Debug_Console
{
    class Program
    {
        static void Main(string[] args)
        {
            //string path = Environment.GetEnvironmentVariable("PATH");
            //string newPath = path + ";C:\\Program Files\\LANDIS-II\\GDAL\\1.9.1";
            //Environment.SetEnvironmentVariable("PATH", newPath);

            Landis.App.Main(args);

            Console.ReadLine();
        }
    }
}
