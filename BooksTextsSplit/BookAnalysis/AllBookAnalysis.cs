using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace BooksTextsSplit
{
    public interface IAllBookAnalysis
    {
        string AnalyseTextBook();        

        event EventHandler AnalyseInvokeTheMain;
    }

    public class AllBookAnalysis : IAllBookAnalysis
    {
        
        private readonly ISharedDataAccess _bookData;


        private readonly IFileManager _manager;


        private readonly IMessageService _msgService;
        private readonly ITextAnalysisLogicExtension _analysisLogic;
        private readonly IChapterDividingAnalysis _chapterAnalysis;
        //private readonly IAnalysisLogicParagraph _paragraphAnalyser;
        private readonly ISentencesDividingAnalysis _sentenceAnalyser;          

        public event EventHandler AnalyseInvokeTheMain;


        public AllBookAnalysis(IFileManager manager, ISharedDataAccess bookData, IMessageService msgService, ITextAnalysisLogicExtension analysisLogic, IChapterDividingAnalysis chapterAnalysis, ISentencesDividingAnalysis sentenceAnalyser)
        {

            //IFileManager manager = new FileManager(bookData);

            _manager = manager;
            _bookData = bookData;
            _msgService = msgService;
            _analysisLogic = analysisLogic;//общая логика
            _chapterAnalysis = chapterAnalysis;//главы
            //_paragraphAnalyser = paragraphAnalysis;//абзацы
            _sentenceAnalyser = sentenceAnalyser;//предложения
        }

        public string AnalyseTextBook() // типа Main в логике анализа текста
        {
            int desiredTextLanguage = _analysisLogic.GetDesiredTextLanguage();//возвращает номер языка, если на нем есть AnalyseText или AnalyseChapterName
            if (desiredTextLanguage == (int)MethodFindResult.NothingFound)
            {
                return desiredTextLanguage.ToString();//типа, нечего анализировать
            }
            _msgService.ShowTrace(MethodBase.GetCurrentMethod().ToString(), DConst.StrCRLF + "Start desiredTextLanguage = " + desiredTextLanguage.ToString(), CurrentClassName, DConst.ShowMessagesLevel);

            if (_bookData.GetFileToDo(desiredTextLanguage) == (int)WhatNeedDoWithFiles.AnalyseText)//если первоначальный анализ текста, без подсказки пользователя о названии глав, ищем главы самостоятельно
            {
                //нерешенные задачи общего анализа текста (метода AnalyseTextBook)
                //1. убрать в абзаце лидирующие пробелы, проверить больше двух пробелов подряд, проверить пробелы перед и между знаками препинания
                //2. проверить метод при наличии в книге других ключевых слов названий глав - в нужных местах (близко к началу абзаца) и с подходящими по смыслу номерами (такие тестовые места в книге отметить $$$$$)
                //3. подумать над вторым проходом метода по желанию пользователя - если он отметил какие-то варианты старших разделов - сохранить главы и добавить разметку других разделов (как потом хранить в базе?)
                string textToAnalyse = _analysisLogic.NormalizeEllipsis(desiredTextLanguage);

                int portionBookTextResult = _analysisLogic.PortionBookTextOnParagraphs(desiredTextLanguage, textToAnalyse);//делит текст на абзацы по EOL, сохраняет в List в AllBookData (возвращает количество строк с текстом)

                int paragraphTextLength = _bookData.GetParagraphTextLength(desiredTextLanguage);

                int allEmptyParagraphsCount = paragraphTextLength - portionBookTextResult;

                _msgService.ShowTrace(MethodBase.GetCurrentMethod().ToString(), DConst.StrCRLF + "Total TEXTs Paragraphs count = " + portionBookTextResult.ToString() + DConst.StrCRLF +
                    "Total ANY paragraphs count = " + paragraphTextLength.ToString() + DConst.StrCRLF +
                    "Total EMPTY paragraphs count = " + allEmptyParagraphsCount.ToString(), CurrentClassName, DConst.ShowMessagesLevel);

                string lastFoundChapterNumberInMarkFormat = _chapterAnalysis.ChapterNameAnalysis(desiredTextLanguage);//находим название и номера, расставляем метки глав в тексте

                int enumerateParagraphsCount = _analysisLogic.MarkAndEnumerateParagraphs(desiredTextLanguage, lastFoundChapterNumberInMarkFormat);//тут раставляем метки и номера абзацев - lastFoundChapterNumberInMarkFormat - не особо нужен
                
                int countSentencesNumber = _sentenceAnalyser.DividePagagraphToSentencesAndEnumerate(desiredTextLanguage);

                StringBuilder appendFileContent = new StringBuilder();//тут сохраняем весь текст в файл для контрольной печати - убрать метод в дополнения
                //int paragraphTextLength = GetParagraphTextLength(desiredTextLanguage);

                string allNoticeNumbersInString = "";

                for (int cpi = 0; cpi < paragraphTextLength; cpi++)
                {
                    string currentParagraph = _bookData.GetParagraphText(desiredTextLanguage, cpi);
                    _msgService.ShowTrace(MethodBase.GetCurrentMethod().ToString(), "CurrentParagraph [" + cpi.ToString() + "] -- > " + currentParagraph, CurrentClassName, DConst.ShowMessagesLevel);
                    appendFileContent = appendFileContent.AppendLine(currentParagraph); // was + DConst.StrCRLF);
                    int currentChapterNumber = _bookData.GetNoticeNumber(desiredTextLanguage, cpi);
                    if(currentChapterNumber != 0) allNoticeNumbersInString += currentChapterNumber.ToString() + " | ";
                }


                _msgService.ShowTrace(MethodBase.GetCurrentMethod().ToString(), "allNoticeNumbersInString --> " + allNoticeNumbersInString, CurrentClassName, DConst.ShowMessagesLevel);


                string tracedFileContent = appendFileContent.ToString();
                //string tracedFileNameAddition = "D:\\OneDrive\\Gonchar\\C#2005\\testBooks\\testEndlishTexts_03R.txt";//путь только для тестов, для полного запуска надо брать путь, указанный пользователем

                //string hashSavedFile = _msgService.SaveTracedToFile(tracedFileNameAddition, tracedFileContent);

                string hashSavedFile = _manager.GetMd5Hash(tracedFileContent);

                //_msgService.ShowTrace(MethodBase.GetCurrentMethod().ToString(), "hash of the saved file - " + DConst.StrCRLF + hashSavedFile, CurrentClassName, DConst.ShowMessagesLevel);

                //и вообще, сделать запись в файл по строкам и там проверять результат - этот же файл потом можно сделать контрольным (посчитать хэш)
                //затем загрузить первую главу (предысловие?) в текстовое окно и показать пользователю

                //следующий блок - в отдельный метод - это потом что-то решить, что делать, если не удалось найти номера глав - сначала найти такой пример
                _bookData.SetFileToDo((int)WhatNeedDoWithFiles.SelectChapterName, desiredTextLanguage);//сообщаем Main, что надо поменять название на кнопке на Select
                    _msgService.ShowTrace(MethodBase.GetCurrentMethod().ToString(), DConst.StrCRLF +
                        "AnalyseInvokeTheMain with desiredTextLanguage = " + desiredTextLanguage.ToString(), CurrentClassName, DConst.ShowMessagesLevel);

                    if (AnalyseInvokeTheMain != null) AnalyseInvokeTheMain(this, EventArgs.Empty);//пошли узнавать у пользователя, как маркируются главы

                return hashSavedFile;
            }

            return "-100";//desiredTextLanguage.ToString();//типа, нечего анализировать
        }        

        public static string CurrentClassName
        {
            get { return MethodBase.GetCurrentMethod().DeclaringType.Name; }
        }
    }
}
