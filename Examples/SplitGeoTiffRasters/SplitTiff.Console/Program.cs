using System.IO;

namespace SplitTiff.Console
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var inputStream = new FileStream(args[0], FileMode.Open);
            var tiffArray = SplitTiff.Split(inputStream, 1000, 1000);

            for (var i = 0; i < tiffArray.Count; i++)
                File.WriteAllBytes(string.Format(args[1], i), tiffArray[i]);

            System.Console.ReadKey();
        }
    }
}
