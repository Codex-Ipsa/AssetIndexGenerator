using System;
using System.IO;
using System.Linq;
using System.Net;

namespace AssetIndexGenerator
{
    internal class Program
    {
        static string customUrl = string.Empty; //Change this to the url you will upload your assets to
        static string manifestName = string.Empty; //Change this to the json name

        static void Main(string[] args)
        {
            Console.WriteLine("AssetIndexGenerator v1.0; by DEJVOSS Productions");

            if (!Directory.Exists("resources") || !Directory.EnumerateFileSystemEntries("resources").Any())
            {
                Directory.CreateDirectory("resources");
                Logger("Please put your resources to the .\\resources directory. Press any key to exit.");
                Console.ReadLine();
                Environment.Exit(0);
            }

            foreach(string arg in args)
            {
                if (arg == "-help")
                {
                    Console.WriteLine("Arguments:");
                    Console.WriteLine("  -help         Shows this menu");
                    Console.WriteLine("  -name=        Name the manifest will be saved as [REQUIRED]");
                    Console.WriteLine("  -url=         URL to where you upload your assets [REQUIRED]");
                    Console.WriteLine("Example usage:");
                    Console.WriteLine("  AssetIndexGenerator.exe -name=my_cool_manifest -url=http://example.com/assets/resources");
                    Console.WriteLine("  Then upload the contents of .\\out\\ to the folder on your webserver you specified above");
                    Console.ReadLine();
                    break;
                }
                else if (arg.StartsWith("-url="))
                    customUrl = arg.Replace("-url=", "");
                else if(arg.StartsWith("-name="))
                    manifestName = arg.Replace("-name=", "");
            }

            if(manifestName == string.Empty)
            {
                Console.Write("Enter the name of your manifest: ");
                manifestName = Console.ReadLine();
            }
            if(customUrl == string.Empty)
            {
                Console.Write("Enter the URL where you'll upload your assets to: ");
                customUrl = Console.ReadLine();
            }

            Console.WriteLine($"URL: {customUrl}");
            Console.WriteLine($"Name: {manifestName}");
            Work();
        }

        static void Logger(string message)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("[ERROR] " + message);
            Console.ForegroundColor = ConsoleColor.Gray;
        }

        static void Work()
        {
            string manifest = "";
            manifest += "{";
            manifest += "\"objects\": {";

            Directory.CreateDirectory("resources");
            string[] files = Directory.GetFiles("resources", "*.*", SearchOption.AllDirectories);

            int i = 0;
            foreach (string file in files)
            {
                FileInfo fi = new FileInfo(file);
                FileStream fop = File.OpenRead(file);
                string chksum = BitConverter.ToString(System.Security.Cryptography.SHA1.Create().ComputeHash(fop)).Replace("-", "").ToLower();
                string firstTwo = chksum.Substring(0, 2);

                bool isOnServers = DoesExist($"https://resources.download.minecraft.net/{firstTwo}/{chksum}");

                if (!isOnServers)
                {
                    Directory.CreateDirectory($"out/resources/{firstTwo}");
                    if (!File.Exists($"out/resources/{firstTwo}/{chksum}"))
                        File.Copy(file, $"out/resources/{firstTwo}/{chksum}");
                }

                string fileNew = file.Replace("\\", "/").Replace("resources/", "");
                Console.WriteLine("Processing " + fileNew.ToString() + "...     " + (i + 1) + " / " + files.Count());

                manifest += $"\"{fileNew}\": {{";
                manifest += $"\"hash\": \"{chksum}\",";
                manifest += $"\"size\": {fi.Length}";
                if (!isOnServers)
                    manifest += $",\"custom_url\": \"{customUrl}/{firstTwo}/{chksum}\"";
                manifest += $"}},";

                i++;
            }

            manifest = manifest.Remove(manifest.Length - 1);
            manifest += "}";
            manifest += "}";
            Directory.CreateDirectory($"out/indexes");
            File.WriteAllText($"out/indexes/{manifestName}.json", manifest);

            Console.WriteLine("Finished!");
            Console.WriteLine("Upload the contents of .\\out\\ to the folder on your webserver you specified.\nPress any key to exit.");
            Console.ReadLine();
        }

        static bool DoesExist(string url)
        {
            try
            {
                WebClient wc = new WebClient();
                string resp = wc.DownloadString(url);
                return true;
            }
            catch (WebException)
            {
                return false;
            }
        }
    }
}
