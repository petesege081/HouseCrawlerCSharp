using HouseCrawlerCSharp.Model;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Text;
using System.Threading;

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
        private const string NewProcessFileName = "process_new.json";
        private static string TempProcessFileName = null;

        public static bool SaveProcessData(string dir, CrawlerProcessData data)
        {
            var tmpName = $"{Guid.NewGuid()}_{ProcessFileName}";
            var tmpFile = Path.Combine(dir, tmpName);

            try
            {
                var oldFile = Path.Combine(dir, ProcessFileName);
                var newFile = Path.Combine(dir, NewProcessFileName);

                //建立新的進度檔
                using var sw = new StreamWriter(newFile, false, Encoding.GetEncoding("utf-8"));
                sw.WriteLine(JsonConvert.SerializeObject(data));
                sw.Close();

                //與舊的進度檔變更為暫存
                if (File.Exists(oldFile)){
                    File.Move(oldFile, tmpFile, true);
                }
                TempProcessFileName = tmpName;

                //將新的進度檔改為當前進度檔
                File.Move(newFile, oldFile);

                //刪除暫存進度檔
                File.Delete(tmpFile);

                return true;
            }
            catch (Exception)
            {
                RecoverProcessData(dir);
                return false;
            }
            finally
            {
                TempProcessFileName = null;
            }
        }

        public static void RecoverProcessData(string dir){
            if(TempProcessFileName != null)
            {
                var tmpFile = Path.Combine(dir, TempProcessFileName);
                if (File.Exists(tmpFile))
                {
                    var originFile = Path.Combine(dir, ProcessFileName);
                    File.Move(tmpFile, originFile, true);
                }
            }

            var newFile = Path.Combine(dir, NewProcessFileName);
            if (File.Exists(newFile))
            {
                File.Delete(newFile);
            }
            

            TempProcessFileName = null;
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
