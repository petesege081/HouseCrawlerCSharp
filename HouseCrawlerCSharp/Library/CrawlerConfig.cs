using Microsoft.Extensions.Configuration;

namespace HouseCrawlerCSharp.Library
{
    public class AppConfig
    {
        public static CrawlerOpts CrawlerOpts { get; set; }
        public static WebDriverOpts WebDriverOpts { get; set; }
        public static ConsoleOpts ConsoleOpts { get; set; }

        public static void Init()
        {
            var config = new ConfigurationBuilder()
               .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
               .Build();

            CrawlerOpts = config.GetSection("CrawlerOpts").Get<CrawlerOpts>();
            WebDriverOpts = config.GetSection("WebDriverOpts").Get<WebDriverOpts>();
            ConsoleOpts = config.GetSection("ConsoleOpts").Get<ConsoleOpts>();
        }
    }

    public enum WebDriverType : int
    {
        Chrome = 1,
        Edge = 2,
        FireFox = 3
    }

    public enum CrawlerModuleType : int
    {
        _591 = 1,
        Sinyi = 2,
        YungChing = 3
    }

    public class CrawlerOpts
    {
        public CrawlerModuleType ModuleType { get; set; }
        public string WorkFolder { get; set; }
        public int AutoSaveData { get; set; }
    }

    public class WebDriverOpts
    {
        public WebDriverType DriverType { get; set; }
        public int MaxThreads { get; set; }
        public int BrowserWidth { get; set; }
        public int BrowserHight { get; set; }
        public string ChromeDriverPath { get; set; }
        public string EdgeDriverPath { get; set; }
        public string FirefoxDriverPath { get; set; }
    }

    public class ConsoleOpts
    {
        public bool OnTop { get; set; }
        public int WindowWidth { get; set; }
        public int WindowHight { get; set; }
    }
}
