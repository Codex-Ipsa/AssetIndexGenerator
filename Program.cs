using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace AssetIndexGenerator
{
    internal class Program
    {
        static string customUrl = "http://codex-ipsa.dejvoss.cz/launcher/assets/resources"; //Change this to the url you will upload your assets to
        static string manifestName = "af-2018-1.13"; //Change this to the json name

        static void Main(string[] args)
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

                if(!isOnServers)
                {
                    Directory.CreateDirectory($"out/resources/{firstTwo}");
                    if (!File.Exists($"out/resources/{firstTwo}/{chksum}"))
                        File.Copy(file, $"out/resources/{firstTwo}/{chksum}");
                }

                string fileNew = file.Replace("\\", "/").Replace("resources/", "");
                Console.WriteLine("Processing " + fileNew.ToString() + "...     " + i + " / " + files.Count());

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
            Console.WriteLine(manifest);
            Directory.CreateDirectory($"out/indexes");
            File.WriteAllText($"out/indexes/{manifestName}.json", manifest);

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
                return  false;
            }
        }
    }
}
