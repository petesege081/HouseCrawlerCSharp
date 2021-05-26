using HouseCrawlerCSharp.Model;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Text;

namespace HouseCrawlerCSharp.Library
{
    class FileHelper
    {
        public static bool IsLocked(string path)
        {
            try
            {
                using var stream = new FileInfo(path).Open(FileMode.Open, FileAccess.Read, FileShare.None);
                stream.Close();
            }
            catch (IOException)
            {
                return true;
            }

            return false;
        }

        private const string ProcessFileName = "process.json";

        public static bool SaveProcessData(string dir, CrawlerProcessData data)
        {
            try
            {
                using var sw = new StreamWriter(Path.Combine(dir, ProcessFileName), false, Encoding.GetEncoding("utf-8"));
                sw.WriteLine(JsonConvert.SerializeObject(data));
                sw.Close();

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static CrawlerProcessData ReadProcessData(string dir)
        {
            try
            {
                var path = Path.Combine(dir, ProcessFileName);

                if (!File.Exists(path)) {
                    return null;
                }

                using var r = new StreamReader(path);
                return JsonConvert.DeserializeObject<CrawlerProcessData>(r.ReadToEnd());
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
