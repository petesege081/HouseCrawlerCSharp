using NLog;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Net;
using System.Threading;

namespace HouseCrawlerCSharp.Library
{
	class ImageDownloader
	{
        private readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private Bitmap Bitmap;

        public void Download(string imageUrl)
        {
            var tryCount = 0;

            // 如果是Error 503, 等待3秒後並重新下載, 最多嘗試3次
            while(tryCount < 3)
            {
                try
                {
                    using var client = new WebClient();
                    using var stream = client.OpenRead(imageUrl);
                    Bitmap = new Bitmap(stream);

                    return;
                }
                catch (WebException ex)
                {
                    //網路異常或伺服器暫時無法使用
                    if(ex.Response == null || ((HttpWebResponse)ex.Response).StatusCode == HttpStatusCode.ServiceUnavailable) {
                        Thread.Sleep(3000);
                        tryCount++;
                        continue;
                    }

                    throw ex;
                }
            }
        }
        public Bitmap GetImage()
        {
            return Bitmap;
        }
        public void SaveImage(string filename, ImageFormat format)
        {
            if (Bitmap != null)
            {
                Bitmap.Save(filename, format);
            }
        }
    }
}
