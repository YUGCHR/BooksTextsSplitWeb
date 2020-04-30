using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace BooksTextsSplit
{
    public interface ISharedDataAccess
    {        
        int GetFileToDo(int i);
        int SetFileToDo(int fileToDo, int i);
        int WhatFileNeedToDo(int whatNeedToDo);//возвращает индекс ToDo, в котором найдено значение whatNeedToDo, если не найдено, возвращает -1

        int GetFileToSave(int i);        
        int SetFileToSave(int fileToSave, int i);
        int WhatFileNeedToSave(int whatNeedToSave);//возвращает индекс FileToSave, в котором найдено значение WhatFileNeedToSave, если не найдено, возвращает -1

        string GetFilePath(int i);
        int SetFilePath(string filePath, int i);

        string GetSelectedText(int i);
        int SetSelectedText(string selectedText, int i);

        string GetFileContent(int i);
        int SetFileContent(string fileContent, int i);

        string GetSymbolsCount(int i);
        int SetSymbolsCount(int symbolsCount, int i);

        string GetParagraphText(int desiredTextLanguage, int paragraphCount);//возвращает строку из двумерного списка List
        int GetParagraphTextLength(int desiredTextLanguage);
        int SetParagraphText(int desiredTextLanguage, int paragraphCount, string paragraphText);
        int AddParagraphText(int desiredTextLanguage, string paragraphText);//тоже возвращает количество элементов
        int RemoveAtParagraphText(int desiredTextLanguage, int paragraphCount);//удаляет элемент списка с индексом paragraphCount

        int GetNoticeNumber(int desiredTextLanguage, int noticeIndex);
        string GetNoticeName(int desiredTextLanguage, int noticeIndex);
        int SetNotices(int desiredTextLanguage, int noticeIndex, int noticeNumber, string noticeName);        
    }

    public class SharedDataAccess : ISharedDataAccess
    {
        //private readonly IMessageService _messageService;

        readonly private int filesQuantity;

        private int[] filesToDo;
        private int[] filesToSave;
        private int[] symbolsCounts;        

        private string[] filesPath;
        private string[] selectedTexts;
        private string[] filesContents;
        
        private List<List<string>> paragraphsTexts = new List<List<string>>(); //инициализация динамического двумерного массива с абзацами (на двух языках + когда-то результат)        

        //два массива для замечаний - добавление и удаление ячеек синхронизировано с paragraphsTexts, так что, у них всегда одинаковая длина, а чтение-запись производится синхронно в оба эти массивы (потом можно заменить на общих класс для всех трех массивов)
        private List<List<string>> noticesNames = new List<List<string>>(); //массив названий замечаний
        private List<List<int>> noticesNumbers = new List<List<int>>(); //массив номеров замечаний

        public SharedDataAccess()//IMessageService service 
        {
            //_messageService = service;

            filesQuantity = DConst.FilesQuantity;

            filesToDo = new int[filesQuantity];
            filesToSave = new int[filesQuantity];
            symbolsCounts = new int[filesQuantity];

            filesPath = new string[filesQuantity];
            selectedTexts = new string[filesQuantity];
            filesContents = new string[filesQuantity];

            paragraphsTexts.Add(new List<string>());
            paragraphsTexts.Add(new List<string>()); //добавление второй строки для абзацев второго языка (пока нужно всего 2 строки)
            
            noticesNames.Add(new List<string>());
            noticesNames.Add(new List<string>());

            noticesNumbers.Add(new List<int>());
            noticesNumbers.Add(new List<int>()); 
        }


        #region ToDo
        //группа массива filesToDo - сделать синхронизированную группу filesToDo-filesToSave (или две строки List?)
        public int GetFileToDo(int i)
        {
            return filesToDo[i];
        }

        public int SetFileToDo(int fileToDo, int i)
        {
            filesToDo[i] = fileToDo;
            return (int)MethodFindResult.AllRight;
        }

        public int WhatFileNeedToDo(int whatNeedToDo)
        {
            for (int i = 0; i < filesQuantity; i++)
            {
                if (filesToDo[i] == whatNeedToDo) return i;
            }
            return (int)MethodFindResult.NothingFound;
        }
        //группа массива filesToSave
        public int GetFileToSave(int i)
        {
            return filesToSave[i];
        }

        public int SetFileToSave(int fileToSave, int i)
        {
            filesToSave[i] = fileToSave;
            return (int)MethodFindResult.AllRight;
        }

        public int WhatFileNeedToSave(int whatNeedToSave)
        {
            for (int i = 0; i < filesQuantity; i++)
            {
                if (filesToSave[i] == whatNeedToSave) return i;
            }
            return (int)MethodFindResult.NothingFound;
        }
        #endregion
        
        #region GetStringContent
        //группа выдачи string

        public string GetStringContent(string nameOfStringNeed, int indexCount)
        {
            int desiredTextLanguage = -1;
            string result = GetStringContent(desiredTextLanguage, nameOfStringNeed, indexCount);
            return result;
        }

        public string GetStringContent(int desiredTextLanguage, string nameOfStringNeed, int indexCount)
        {
            switch (nameOfStringNeed)
            {
                case
                "GetFilePath":
                    return filesPath[indexCount];
                case
                "GetSelectedText":
                    return selectedTexts[indexCount];
                case
                "GetFileContent":
                    return filesContents[indexCount];
                case
                "GetParagraphText":
                    if (desiredTextLanguage < 0) return null;//а еще лучше поставить Assert
                    return paragraphsTexts[desiredTextLanguage][indexCount];
                case
                "GetChapterName":
                    if (desiredTextLanguage < 0) return null;
                    if (noticesNames[desiredTextLanguage][indexCount] != null) return noticesNames[desiredTextLanguage][indexCount];//при замене метода обратить внимание, что desiredTextLanguage и indexCount в старом вызове стоят наоборот!
                    return null;
                case
                "GetSymbolsCount":
                    return symbolsCounts[indexCount].ToString();                
            }
            return null;
        }
        #endregion
        
        #region GetIntContent
        //группа выдачи int
        public int GetIntContent(int desiredTextLanguage, string needOperationName)//перегрузка для получения длины двуязычных динамических массивов
        {
            string stringToSet = null;
            int indexCount = -1;
            int result = GetIntContent(desiredTextLanguage, needOperationName, stringToSet, indexCount);
            return result;
        }

        public int GetIntContent(string needOperationName, string stringToSet, int indexCount)//перегрузка для записи обычных массивов
        {
            int desiredTextLanguage = -1;
            int result = GetIntContent(desiredTextLanguage, needOperationName, stringToSet, indexCount);
            return result;
        }

        public int GetIntContent(int desiredTextLanguage, string needOperationName, int indexCount)//перегрузка для удаления динамических массивов
        {
            string stringToSet = null;
            int result = GetIntContent(desiredTextLanguage, needOperationName, stringToSet, indexCount);
            return result;
        }

        public int GetIntContent(int desiredTextLanguage, string needOperationName, string stringToSet, int indexCount)
        {
            switch (needOperationName)
            {
                case
                "SetFilePath":
                    //для начала надо проверить, что индекс не выходит за границы массива
                    //еще можно проверить, что такой путь существует?


                    filesPath[indexCount] = stringToSet;                    
                    return (int)MethodFindResult.AllRight;
                case
                "SetSelectedText":
                    //проверить индекс
                    selectedTexts[indexCount] = stringToSet;
                    return (int)MethodFindResult.AllRight;
                case
                "SetFileContent":
                    //проверить индекс
                    filesContents[indexCount] = stringToSet;
                    return (int)MethodFindResult.AllRight;
                case
                "GetParagraphTextLength":
                    if (desiredTextLanguage < 0) return (int)MethodFindResult.NothingFound;//а еще лучше поставить Assert
                    return paragraphsTexts[desiredTextLanguage].Count; 
                case
                "SetParagraphText":
                    paragraphsTexts[desiredTextLanguage][indexCount] = stringToSet;
                    return (int)MethodFindResult.AllRight;
                case
                "AddParagraphText":
                    //тут можно требовать indexCount последнего существующего элемента для контроля, возвращать индекс добавленного (+1), но вычислять его, замерив длину (и -1)
                    //или - для большого цикла - чтобы не было больших расходов, просто какой-то ключевой индекс?
                    paragraphsTexts[desiredTextLanguage].Add(stringToSet);//добавление нового элемента в строку
                    return paragraphsTexts[desiredTextLanguage].Count;
                case
                "RemoveAtParagraphText":
                    bool indexCountInLimits = indexCount > 0 && indexCount < paragraphsTexts[desiredTextLanguage].Count;
                    System.Diagnostics.Debug.Assert(indexCountInLimits, "The index of paragraphsTexts is WRONG!");

                    paragraphsTexts[desiredTextLanguage].RemoveAt(indexCount);//удаление элемента по индексу
                    return paragraphsTexts[desiredTextLanguage].Count;//получение и возврат новой длины списка
                case
                "GetChapterNumberLength":
                    noticesNames[desiredTextLanguage][indexCount] = stringToSet;
                    return 0;
                case
                "AddChapterNumber":
                    noticesNames[desiredTextLanguage].Add(stringToSet);//добавление нового элемента в строку
                    return noticesNames[desiredTextLanguage].Count;
            }
            //а еще лучше поставить Assert - ваше ключевое слово не подошло к нашей замочной скважине
            return (int)MethodFindResult.NothingFound;
            
            
        }
        #endregion
        
        #region files
        //группа массива filesPath -  - сделать синхронизированную группу filesPath-filesContents (или две строки List?)
        public string GetFilePath(int i)
        {
            return filesPath[i];
        }

        public int SetFilePath(string filePath, int i)
        {
            filesPath[i] = filePath;
            return (int)MethodFindResult.AllRight;
        }
        //группа массива selectedTexts
        public string GetSelectedText(int i)
        {
            return selectedTexts[i];
        }

        public int SetSelectedText(string selectedText, int i)
        {
            selectedTexts[i] = selectedText;
            return (int)MethodFindResult.AllRight;
        }
        //группа массива filesContents
        public string GetFileContent(int i)
        {
            return filesContents[i];
        }

        public int SetFileContent(string fileContent, int i)
        {
            filesContents[i] = fileContent;
            return (int)MethodFindResult.AllRight;
        }
        //группа массива подсчета символов
        public string GetSymbolsCount(int i)
        {
            return symbolsCounts[i].ToString();
        }

        public int SetSymbolsCount(int symbolsCount, int i)
        {
            symbolsCounts[i] = symbolsCount;
            return (int)MethodFindResult.AllRight;
        }
        #endregion
        
        #region paragraphsTexts
        //группа массива Абзац текста - paragraphsTexts
        public string GetParagraphText(int desiredTextLanguage, int paragraphCount)
        {
            return paragraphsTexts[desiredTextLanguage][paragraphCount];            
        }

        public int GetParagraphTextLength(int desiredTextLanguage)
        {
            return paragraphsTexts[desiredTextLanguage].Count;
        }

        public int SetParagraphText(int desiredTextLanguage, int paragraphCount, string paragraphText)
        {
            paragraphsTexts[desiredTextLanguage][paragraphCount] = paragraphText;
            return 0;
        }

        //как сделать метод добавления Insert (int index, T item); - и, главное, зачем

        public int AddParagraphText(int desiredTextLanguage, string paragraphText)
        {
            paragraphsTexts[desiredTextLanguage].Add(paragraphText);//добавление нового элемента в строку
            noticesNames[desiredTextLanguage].Add("");
            noticesNumbers[desiredTextLanguage].Add(0);
            return paragraphsTexts[desiredTextLanguage].Count;
        }

        public int RemoveAtParagraphText(int desiredTextLanguage, int paragraphCount)
        {
            paragraphsTexts[desiredTextLanguage].RemoveAt(paragraphCount);//удаление элемента по индексу
            noticesNames[desiredTextLanguage].RemoveAt(paragraphCount);
            noticesNumbers[desiredTextLanguage].RemoveAt(paragraphCount);
            return paragraphsTexts[desiredTextLanguage].Count;//получение и возврат новой длины списка
        }
        #endregion

        #region Notices
        //группа массива Главы имя - chaptersNamesWithNumbers
        public int GetNoticeNumber(int desiredTextLanguage, int noticeIndex)
        {
            return noticesNumbers[desiredTextLanguage][noticeIndex];            
        }

        public string GetNoticeName(int desiredTextLanguage, int noticeIndex)
        {
            return noticesNames[desiredTextLanguage][noticeIndex];            
        }

        public int SetNotices(int desiredTextLanguage, int noticeIndex, int noticeNumber, string noticeName)
        {
            noticesNumbers[desiredTextLanguage][noticeIndex] = noticeNumber;
            noticesNames[desiredTextLanguage][noticeIndex] = noticeName;
            return 0;
        }        
        #endregion
        

        //public void NewVersion()//interface IAllBookData
        //{
        //    int GetFileToDo(int i);
        //    int SetFileToDo(int fileToDo, int i);
        //    int WhatFileNeedToDo(int whatNeedToDo);//возвращает индекс ToDo, в котором найдено значение whatNeedToDo, если не найдено, возвращает -1

        //    int GetFileToSave(int i);
        //    int SetFileToSave(int fileToSave, int i);
        //    int WhatFileNeedToSave(int whatNeedToSave);//возвращает индекс FileToSave, в котором найдено значение WhatFileNeedToSave, если не найдено, возвращает -1

        //    //объединенная группа OperationControlCommands
        //    int OperationControlCommands(string getToDoOrToSave, int i);//перегрузка для Get
        //    int OperationControlCommands(string setToDoOrToSave, int fileToDo, int i);//перегрузка для Set
        //    int OperationControlCommands(string setToDoOrToSave, int whatNeedToDo, int i);//перегрузка для whatNeed - может, тут не надо отдельной перегрузки - просто другое значение по другой строке выдавать



        //    string GetFilePath(int i);
        //    int SetFilePath(string filePath, int i);

        //    string GetSelectedText(int i);
        //    int SetSelectedText(string selectedText, int i);

        //    string GetFileContent(int i);
        //    int SetFileContent(string fileContent, int i);

        //    string GetParagraphText(int paragraphCount, int desiredTextLanguage);//возвращает строку из двумерного списка List
        //    int GetParagraphTextLength(int desiredTextLanguage);
        //    int SetParagraphText(string paragraphText, int paragraphCount, int desiredTextLanguage);
        //    int AddParagraphText(string paragraphText, int desiredTextLanguage);//тоже возвращает количество элементов
        //    int RemoveAtParagraphText(int paragraphCount, int desiredTextLanguage);//удаляет элемент списка с индексом paragraphCount

        //    string GetChapterName(int chapterCount, int desiredTextLanguage);
        //    int GetChapterNameLength(int desiredTextLanguage);
        //    int SetChapterName(string chapterNameWithNumber, int chapterCount, int desiredTextLanguage);
        //    int AddChapterName(string chapterNameWithNumber, int desiredTextLanguage);

        //    int GetChapterNumber(int chapterCount, int desiredTextLanguage);
        //    int GetChapterNumberLength(int desiredTextLanguage);
        //    int AddChapterNumber(int chapterNumberOnly, int desiredTextLanguage);

        //    string GetSymbolsCount(int i);
        //    int SetSymbolsCount(int symbolsCount, int i);
        //}
    }    
}
