﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace BooksTextsSplit
{
    public interface ITextAnalysisLogicExtension
    {
        int GetDesiredTextLanguage();
        //int FindTextPartNumber(string currentParagraph, string stringMarkBegin, int totalDigitsQuantity);
        string AddSome00ToIntNumber(string currentNumberToFind, int totalDigitsQuantity);

        string NormalizeEllipsis(int desiredTextLanguage);//plogic
        int PortionBookTextOnParagraphs(int desiredTextLanguage, string textToAnalyse);//plogic
        int MarkAndEnumerateParagraphs(int desiredTextLanguage, string lastFoundChapterNumberInMarkFormat);//plogic        
    }

    public class TextAnalysisLogicExtension : ITextAnalysisLogicExtension
    {
        private readonly ISharedDataAccess _bookData;
        private readonly IMessageService _msgService;
        
        readonly private int filesQuantity;
        readonly private int showMessagesLevel;
        readonly private string strCRLF;
        

        public TextAnalysisLogicExtension(ISharedDataAccess bookData, IMessageService msgService)
        {
            _bookData = bookData;
            _msgService = msgService;            

            filesQuantity = DConst.FilesQuantity;
            showMessagesLevel = DConst.ShowMessagesLevel;
            strCRLF = DConst.StrCRLF;            
        }
        
        public int GetDesiredTextLanguage()
        {
            int desiredTextLanguage = (int)MethodFindResult.NothingFound;

            for (int i = 0; i < filesQuantity; i++)//пока остается цикл - все же один вместо двух, если вызывать специальный метод 2 раза
            {
                int iDesiredTextLanguage = _bookData.GetFileToDo(i);
                if (iDesiredTextLanguage == (int)WhatNeedDoWithFiles.AnalyseText) desiredTextLanguage = i;
                if (iDesiredTextLanguage == (int)WhatNeedDoWithFiles.AnalyseChapterName) desiredTextLanguage = i;
            }
            return desiredTextLanguage;
        }

        public string AddSome00ToIntNumber(string currentNumberToFind, int totalDigitsQuantity)
        {
            int currentChapterNumberLength = currentNumberToFind.Length;
            int add00Digits = totalDigitsQuantity - currentChapterNumberLength;
            if(add00Digits <= 0)
            {
                return null;
            }
            for(int i = 0; i < add00Digits; i++)
            {
                currentNumberToFind = "0" + currentNumberToFind;
            }            
            return currentNumberToFind;
        }

        public int PortionBookTextOnParagraphs(int desiredTextLanguage, string textToAnalyse)//делит текст на абзацы по EOL, сохраняет в List в AllBookData
        {
            string[] TextOnParagraphsPortioned = textToAnalyse.Split(DConst.charsParagraphSeparator);//portioned all book content in the ParagraphsArray via EOL
            //потом тут можно написать свой метод деления на абзацы (или этот пусть делит по одному сепаратору) - но не нужно
            int textOnParagraphsPortionedLength = TextOnParagraphsPortioned.Length;
            int allParagraphsWithTextCount = 0;

            for (int i = 0; i < textOnParagraphsPortionedLength; i++)//загружаем получившиеся абзацы в динамический массив, потом сравниваем длину массивов
            {
                string currentTextParagraph = TextOnParagraphsPortioned[i];
                bool currentTextParagraphIsEmpty = String.IsNullOrEmpty(currentTextParagraph);

                if (!currentTextParagraphIsEmpty)
                {
                    _bookData.AddParagraphText(desiredTextLanguage, "");//записываем пустую строку 
                    int addParagraphTextCount = _bookData.AddParagraphText(desiredTextLanguage, currentTextParagraph);//также возвращает количество уже существующих элементов
                    //нельзя, он позже понадобится чистым (подумать над этим) - здесь выгрузить данные из временного массива проблем в массив Notice, весь абзац (или найденные кавычки?) в Name, а в Number - что-то отрицательное, например количество кавычек (тут еще нет кавычек, но можно приспособить проверку)
                    allParagraphsWithTextCount++;
                    _msgService.ShowTrace(MethodBase.GetCurrentMethod().ToString(), "Current Paragraph[" + i.ToString() + "] --> " + TextOnParagraphsPortioned[i] + "---control<CR>---" + DConst.StrCRLF +
                        "currentTextParagraphIsEmpty = " + currentTextParagraphIsEmpty.ToString(), CurrentClassName, DConst.ShowMessagesLevel);
                }
            }
            return allParagraphsWithTextCount;
        }
        
        public string NormalizeEllipsis(int desiredTextLanguage)//тут заменим все многоточия фирменным символом, а также другие составные знаки
        {
            string textToAnalyse = _bookData.GetFileContent(desiredTextLanguage);
            
            int changedEllipsisVariationCount = 0;
            int charsEllipsisToChangeLength = DConst.charsEllipsisToChange1.Length;

            for (int evtc = 0; evtc < charsEllipsisToChangeLength; evtc++)//evtc - ellipsis variation to change
            {
                int ellipsisVariationIndexFound = textToAnalyse.IndexOf(DConst.charsEllipsisToChange1[evtc]);//предварительно проверяем, есть ли вообще в тексте хоть один такой знак препинания, чтобы зря не искать по всему тексту
                //интересно, что проверка занимает сильно больше времени, чем глобальная замена по тексту - может, что-то делаем не так? 
                if (ellipsisVariationIndexFound > 0)
                {
                    string textToAnalyseEllipsisChanged = textToAnalyse.Replace(DConst.charsEllipsisToChange1[evtc], DConst.charsEllipsisToChange2[evtc]);//Возвращает новую строку, в которой все вхождения заданной строки(1) в текущем экземпляре заменены другой строкой(2)
                    textToAnalyse = textToAnalyseEllipsisChanged;//освобождаем переменную для следующего прохода и сохраняем результат в переменной для return
                    changedEllipsisVariationCount++;
                }
            }
            _msgService.ShowTrace(MethodBase.GetCurrentMethod().ToString(), "Found and changed Ellipsis Variation Count = " + changedEllipsisVariationCount.ToString(), CurrentClassName, DConst.ShowMessagesLevel);

            return textToAnalyse;
        }

        public int MarkAndEnumerateParagraphs(int desiredTextLanguage, string lastFoundChapterNumberInMarkFormat)//ChapterNumberParagraphsIndexes - вычесть 1
        {
            //цикл запускаем по длине массива всех абзацев, номера глав достаем из массива Notice, ключевое слово маркировки получаем из ChapterNameAnalysis
            int paragraphTextLength = _bookData.GetParagraphTextLength(desiredTextLanguage);            

            int totalParagraphsNumbersCount = 0;//всего пронумеровано абзацев - без названий глав (вернем из метода - вдруг пригодится)
            int currentChapterParagraphsNumbersCount = 1;//счетчик абзацев в текущей главе - нумерация начинается с 1
            int currentChapterNumber = 0;
            bool cpiWithChapterName = false;

            //маркировка глав выглядит так - ¤¤¤¤¤001¤¤¤-Chapter (слово в конце может меняться - ставится такое, как в книжке) - кстати, если в книге слова нет, надо поставить из константы?
            int symbolsCountToRemove = DConst.beginChapterMark.Length + DConst.chapterNumberTotalDigits + DConst.endChapterMark.Length + 1; //(+1 - за тире) получим длину удаляемой группы символов, вместо нее добавить -of
            string chapterKeyWordForParagraphMarks = lastFoundChapterNumberInMarkFormat.Remove(0, symbolsCountToRemove);//Возвращает новую строку, в которой было удалено указанное число символов в указанной позиции

            //тут достанем номера глав и ключевое слово главы из служебного массива, а пример нумерации главы получили в параметрах - из примера сделаем заготовку нумерации абзаца
            for (int cpi = 0; cpi < paragraphTextLength; cpi++)//cpi - current paragraph index
            {
                string currentParagraph = _bookData.GetParagraphText(desiredTextLanguage, cpi);
                int savedChapterNumber = _bookData.GetNoticeNumber(desiredTextLanguage, cpi);//ищем сохраненный номер главы - в соотвествующем номере абзаца
                if (savedChapterNumber > 0)
                {
                    currentChapterNumber = savedChapterNumber;//перенесли номер главы в переменную для создания полной маркировки абзаца (§§§§§00001§§§-Paragraph-of-CHAPTER-001)
                    currentChapterParagraphsNumbersCount = 1;//сбросили счетчик номеров абзаца - в каждой главе абзацы нумеруем заново с 1 (ну, так подумалось, что лучше)
                    cpiWithChapterName = true;//флаг для пропуска абзаца с именем главы - чтобы не нумеровать его еще и как абзац

                    _msgService.ShowTrace(MethodBase.GetCurrentMethod().ToString(), "textParagraph[" + cpi.ToString() + "] = " + DConst.StrCRLF + currentParagraph + DConst.StrCRLF +
                        "currentChapterNumber - " + currentChapterNumber.ToString() + DConst.StrCRLF + "cpiWithChapterName - " + cpiWithChapterName.ToString() + DConst.StrCRLF +
                        "currentChapterParagraphsNumbersCount - " + currentChapterParagraphsNumbersCount.ToString(), CurrentClassName, DConst.ShowMessagesLevel);
                }
                bool textParagraphIsEmpty = String.IsNullOrEmpty(currentParagraph);
                if (!textParagraphIsEmpty && !cpiWithChapterName)//если абзац НЕ пустой и НЕ является именем главы, то будем его нумеровать
                {
                    //формировать номера абзацев, помня про нулевую главу (теперь нулевая формируется автоматически по первоначальному отсутствию номера главы)
                    string currentParagraphNumberToFind000 = AddSome00ToIntNumber(currentChapterParagraphsNumbersCount.ToString(), DConst.paragraptNumberTotalDigits);
                    if (currentParagraphNumberToFind000 == null)//это можно убрать
                    {
                        //return null;//лучше поставить Assert - и можно прямо в AddSome00ToIntNumber?
                    }                    
                    string currentChapterNumberToFind000 = AddSome00ToIntNumber(currentChapterNumber.ToString(), DConst.chapterNumberTotalDigits);//создать номер главы из индекса сохраненных индексов - как раз начинаются с нуля
                    string paragraphTextMarks = DConst.beginParagraphMark + currentParagraphNumberToFind000 + DConst.endParagraphMark + "-" + DConst.paragraphMarkNameLanguage[desiredTextLanguage] + "-of-" + chapterKeyWordForParagraphMarks + "-" + currentChapterNumberToFind000;//¤¤¤¤¤001¤¤¤-Chapter- добавить номер главы от нулевой

                    int emptyParagraphIndexBeforeText = cpi - 1;//пишем номер абзаца в предыдущую ячейку перед текстом - она всегда пустая
                    int setParagraphResult = _bookData.SetParagraphText(desiredTextLanguage, emptyParagraphIndexBeforeText, paragraphTextMarks);
                    string newCurrentParagraph = _bookData.GetParagraphText(desiredTextLanguage, emptyParagraphIndexBeforeText);

                    currentChapterParagraphsNumbersCount++;//начинаем нумерацию с 1 (не забыть потом сбросить счетчик) - и потом удалить коммент
                    totalParagraphsNumbersCount++;

                    _msgService.ShowTrace(MethodBase.GetCurrentMethod().ToString(), "textParagraph[" + cpi.ToString() + "] = " + DConst.StrCRLF + currentParagraph + DConst.StrCRLF +
                        "ParagraphTextMarks - " + paragraphTextMarks + DConst.StrCRLF +
                        "newCurrentParagraph - " + newCurrentParagraph, CurrentClassName, DConst.ShowMessagesLevel);
                }
                cpiWithChapterName = false;
            }
            return totalParagraphsNumbersCount;//пока не очень понятно, зачем он нужен
        }

        public string SaveTextToFile(int desiredTextLanguage)
        {
            string hashFile = "";

            return hashFile;
        }

        public static string CurrentClassName
        {
            get { return MethodBase.GetCurrentMethod().DeclaringType.Name; }
        }
    }
}
