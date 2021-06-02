using FileHelpers;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace HouseCrawlerCSharp.Library
{
	class CsvFileHandler<T> where T : class
	{
		private readonly FileHelperEngine<T> CsvEngine;
		private readonly string CsvFilePath;
		private string[] HeaderColumns;

		public CsvFileHandler(string csvFilePath){
			CsvFilePath = csvFilePath;
			CsvEngine = new FileHelperEngine<T>();
			Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
		}

		public void SetHeader(string[] headerColumns)
		{
			HeaderColumns = headerColumns;
		}

		public bool IsExist(){
			return File.Exists(CsvFilePath);
		}

		public bool IsLocked()
		{
			try
			{
				using var stream = new FileInfo(CsvFilePath).Open(FileMode.Open, FileAccess.Read, FileShare.None);
				stream.Close();
			}
			catch (IOException)
			{
				return true;
			}

			return false;
		}

		public void CreateFile()
		{
			using var sw = new StreamWriter(CsvFilePath, false, Encoding.GetEncoding("utf-8"));
			sw.WriteLine(string.Join(",", HeaderColumns));
			sw.Close();
		}

		public void CreateFileWithHeader(string[] headerColumns)
		{
			using var sw = new StreamWriter(CsvFilePath, false, Encoding.GetEncoding("utf-8"));
			sw.WriteLine(string.Join(",", headerColumns));
			sw.Close();
		}

		public void AppendToFile(List<T> datas)
		{
			if(!IsExist()) {
				CreateFile();
			}

			using var sw = new StreamWriter(CsvFilePath, true, Encoding.GetEncoding("utf-8"));
			sw.Write(CsvEngine.WriteString(datas));
			sw.Close();
		}
	}
}
