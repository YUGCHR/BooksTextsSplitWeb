using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using System.Security.Cryptography;

namespace BooksTextsSplit
{
    public interface IFileManager
    {        
        string GetContent(int i);
        string GetContent(int i, Encoding encoding);        
        int SaveContent(int i);
        int SaveContent(int i, Encoding encoding);
        int SaveContent(string tracedFilePathAndName, string tracedFileContent, int i);//i - no need yet
        int GetSymbolsCount(int i);        
        bool IsFileExist(string FilesPath);
        string GetMd5Hash(string fileContent);


        void WriteToFilePathPlus(string[] textParagraphes, string filesPath, string filesPathPlus);
        void AppendContent(string tracePointName, string tracePointValue, string tracePointPlace);
        void AppendContent(string tracePointName, string tracePointValue, string tracePointPlace, Encoding encoding);
        void AppendContent(string tracePointName, string[] tracePointValue, string tracePointPlace);
        void AppendContent(string tracePointName, string[] tracePointValue, string tracePointPlace, Encoding encoding);
        string CreateFile(string filesPath, string resultFileName);
    }
    public class FileManager : IFileManager
    {
        private readonly ISharedDataAccess _book;
        //private readonly IMessageService _messageService;
        private readonly Encoding _defaultEncoding = Encoding.GetEncoding(1251);

        private string logFilePathName;
        private bool isLogFileExist;        
        //string logFilePath = Directory.GetCurrentDirectory();//Will check log-file existing        

        public FileManager(ISharedDataAccess book) //IMessageService service
        {
            _book = book;            
                        
            //string[] logFilePathandName = { logFilePath, "log.txt" };
            //logFilePathName = String.Join("\\", logFilePathandName);
            //isLogFileExist = File.Exists(logFilePathName);

            //if (!isLogFileExist)
            //{                
            //    File.AppendAllText(logFilePathName, "LogFile Created \r\n", _defaultEncoding);
            //}            
        }

        
        public bool IsFileExist(string filePath)
        {
            return File.Exists(filePath);
        }        
       
        public string GetContent(int i)
        {            
                return GetContent(i, _defaultEncoding); 
        }

        public string GetContent(int i, Encoding encoding)//This method cannot be access outside so we do not need to check isFileExist
        {
            return File.ReadAllText(_book.GetFilePath(i), encoding);            
        }
        
        public int SaveContent(int i)
        {
            return SaveContent(i, _defaultEncoding);
        }

        public int SaveContent(int i, Encoding encoding)
        {
            try
            {
                File.WriteAllText(_book.GetFilePath(i), _book.GetFileContent(i), encoding);
                return (int)WhatNeedSaveFiles.FileSavedSuccessfully;
            }
            catch
            {                
                return (int)WhatNeedSaveFiles.CannotSaveFile;
            }
        }

        public int SaveContent(string tracedFilePathAndName, string tracedFileContent, int i)//i - no need yet
        {
            try
            {
                File.WriteAllText(tracedFilePathAndName, tracedFileContent, _defaultEncoding);
                return (int)WhatNeedSaveFiles.FileSavedSuccessfully;
            }
            catch
            {
                return (int)WhatNeedSaveFiles.CannotSaveFile;
            }
        }        

        public int GetSymbolsCount(int i)
        {
            string fileContent = _book.GetFileContent(i);
            if (fileContent != null) return fileContent.Length;
            else return 0;
        }
        
        #region WriteToFile
        public void WriteToFilePathPlus(string[] textParagraphes, string filesPath, string filesPathPlus)
        {
            string[] pathNameExt = filesPath.Split(new char[] { '.' });
            string filesPathAddPlus = pathNameExt[0] + filesPathPlus + "." + pathNameExt[1];
            //MessageBox.Show(filesPathAddPlus, "WriteToFilePathPlus", MessageBoxButtons.OK, MessageBoxIcon.Information);
            File.WriteAllLines(filesPathAddPlus, textParagraphes, _defaultEncoding);
        }
        #endregion
        #region AppendContent

        public void AppendContent(string tracePointName, string tracePointValue, string tracePointPlace)
        {
            AppendContent(tracePointName, tracePointValue, tracePointPlace, _defaultEncoding);
        }

        public void AppendContent(string tracePointName, string[] tracePointValue, string tracePointPlace)
        {
            AppendContent(tracePointName, tracePointValue, tracePointPlace, _defaultEncoding);
        }

        public void AppendContent(string tracePointName, string tracePointValue, string tracePointPlace, Encoding encoding)
        {
            string[] traceMessage = { "\r\n tracePointPlace - ", tracePointPlace, "\r\n ----------------- tracePointName - ", tracePointName, tracePointValue };//to remove all outsider symbols
            string fileLineAppend = String.Join(" ", traceMessage);            
            File.AppendAllText(logFilePathName, fileLineAppend, encoding);
        }        

        public void AppendContent(string tracePointName, string[] tracePointValue, string tracePointPlace, Encoding encoding)
        {
            int ii = tracePointValue.Length;            

            for (int i = 0; i < ii; i++)
            {
                string tracePointValueCut = tracePointValue[i].Remove(32);
                tracePointValue[i] = tracePointValueCut;
            }
            
            string tracePointValues = String.Join(" *** ", tracePointValue);

            string[] traceMessage = { "\r\n tracePointPlace - ", tracePointPlace, "\r\n --- tracePointName - ", tracePointName, "\r\n" };
            string fileLineAppend = String.Join("", traceMessage);
            File.AppendAllText(logFilePathName, fileLineAppend, encoding);

            traceMessage = new string[] { "", tracePointValues };
            fileLineAppend = String.Join(" ", traceMessage);            
            File.AppendAllText(logFilePathName, fileLineAppend, encoding);
        }
        #endregion
        #region CreateFile
        public string CreateFile(string filesPathSample, string resultFileName)
        {
            //check is path exist
            string logFilePathName = Path.GetDirectoryName(filesPathSample) + "\\" + resultFileName + ".txt";
            if (IsFileExist(logFilePathName)) return null;//ordered file is exist, cannot create it
            
            string fileLineAppend = "New result file created \r\n";
            File.AppendAllText(logFilePathName, fileLineAppend, _defaultEncoding);
            return logFilePathName;
        }
        #endregion

        public string GetMd5Hash(string fileContent)
        {
            MD5 md5Hasher = MD5.Create(); //создаем объект класса MD5 - он создается не через new, а вызовом метода Create            
            byte[] data = md5Hasher.ComputeHash(Encoding.Default.GetBytes(fileContent));//преобразуем входную строку в массив байт и вычисляем хэш
            StringBuilder sBuilder = new StringBuilder();//создаем новый Stringbuilder (изменяемую строку) для набора байт
            for (int i = 0; i < data.Length; i++)// Преобразуем каждый байт хэша в шестнадцатеричную строку
            {
                sBuilder.Append(data[i].ToString("x2"));//указывает, что нужно преобразовать элемент в шестнадцатиричную строку длиной в два символа
            }
            string pasHash = sBuilder.ToString();

            return pasHash;
        }
    }
}


