﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Security.Cryptography;

namespace BooksTextsSplit
{
    public interface IAllBookAnalysis
    {
        string AnalyseTextBook();        

        event EventHandler AnalyseInvokeTheMain;
    }

    public class AllBookAnalysis : IAllBookAnalysis
    {
        
        private readonly IAllBookData _bookData;
        private readonly ITextAnalysisLogicExtension _analysisLogic;
        private readonly IChapterDividingAnalysis _chapterAnalysis;
        //private readonly IAnalysisLogicParagraph _paragraphAnalyser;
        private readonly ISentencesDividingAnalysis _sentenceAnalyser;          

        public event EventHandler AnalyseInvokeTheMain;


        public AllBookAnalysis(IAllBookData bookData, ITextAnalysisLogicExtension analysisLogic, IChapterDividingAnalysis chapterAnalysis, ISentencesDividingAnalysis sentenceAnalyser)
        {
            _bookData = bookData;            
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

                string lastFoundChapterNumberInMarkFormat = _chapterAnalysis.ChapterNameAnalysis(desiredTextLanguage);//находим название и номера, расставляем метки глав в тексте

                int enumerateParagraphsCount = _analysisLogic.MarkAndEnumerateParagraphs(desiredTextLanguage, lastFoundChapterNumberInMarkFormat);//тут раставляем метки и номера абзацев - lastFoundChapterNumberInMarkFormat - не особо нужен
                
                int countSentencesNumber = _sentenceAnalyser.DividePagagraphToSentencesAndEnumerate(desiredTextLanguage);

                StringBuilder appendFileContent = new StringBuilder();//тут сохраняем весь текст в файл для контрольной печати - убрать метод в дополнения
                //int paragraphTextLength = GetParagraphTextLength(desiredTextLanguage);

                string allNoticeNumbersInString = "";

                for (int cpi = 0; cpi < paragraphTextLength; cpi++)
                {
                    string currentParagraph = _bookData.GetParagraphText(desiredTextLanguage, cpi);                    
                    appendFileContent = appendFileContent.AppendLine(currentParagraph); // was + DConst.StrCRLF);
                    int currentChapterNumber = _bookData.GetNoticeNumber(desiredTextLanguage, cpi);
                    if(currentChapterNumber != 0) allNoticeNumbersInString += currentChapterNumber.ToString() + " | ";
                }

                string tracedFileContent = appendFileContent.ToString();
                
                string hashSavedFile = GetMd5Hash(tracedFileContent);

                //следующий блок - в отдельный метод - это потом что-то решить, что делать, если не удалось найти номера глав - сначала найти такой пример
                _bookData.SetFileToDo((int)WhatNeedDoWithFiles.SelectChapterName, desiredTextLanguage);//сообщаем Main, что надо поменять название на кнопке на Select
                    
                    if (AnalyseInvokeTheMain != null) AnalyseInvokeTheMain(this, EventArgs.Empty);//пошли узнавать у пользователя, как маркируются главы

                return hashSavedFile;
            }

            return "-100";//desiredTextLanguage.ToString();//типа, нечего анализировать
        }        

        public static string CurrentClassName
        {
            get { return MethodBase.GetCurrentMethod().DeclaringType.Name; }
        }

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
