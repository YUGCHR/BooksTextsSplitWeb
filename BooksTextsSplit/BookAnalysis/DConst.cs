using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BooksTextsSplit
{
    public static class DConst
    {
        #region GlobalConstant
        public const int FilesQuantity = 3; //the length of the all Files___ arrays (except FilesToDo)
        public const int LanguagesQuantity = 2; //the length of the all Files___ arrays (except FilesToDo)
        public const int ToDoQuantity = FilesQuantity + 1; //the length of the FilesToDo array (+1 for BreakpointManager)
        public const int FilesQuantityPlus = FilesQuantity + 1; //the length of the FilesToDo array (+1 for BreakpointManager)
        public const int ResultFileNumber = FilesQuantity - 1;
        public const int TextFieldsQuantity = 2;//количество текстовых окон в форме OpenForm (используется в Main - в isFilesExistCheckAndOpen)
        public const int ButtonNamesCountInLanguageGroup = 2;//количество названий для одной кнопки - перенести на прямое измерение массива или enum
        public const int IBreakpointManager = FilesQuantityPlus - 1; //index of BreakpointManager in the FilesToDo array
        public const string StrCRLF = "\r\n";
        public const string ResultFileName = "sampleResultTextDoc";
        public const int ShowMessagesLevel = 0;//0 - no messages, 1 - Trace messages, 2 - Value messages, -1 - Print mesaages...
        #endregion

        #region AnalysisConstantArrays
        public const int FindChapterNameSpace = 21;

        public const int BaseKeyWordFormsCount = 3;
        public const int chapterNumberTotalDigits = 3;
        public const int paragraptNumberTotalDigits = 5;
        public const int sentenceNumberTotalDigits = 5;       

        public static string[] charsEllipsisToChange1 = new string[] { "“”", "...", "!!!", "!!", "?!!", "?!", "!!?", "!?", "???", "??" };//“” - обе вместе не будет работать - так не ищутся, их надо ставить по одной
        public static string[] charsEllipsisToChange2 = new string[] { "«»", "…", "\u203C", "\u203C", "\u2048", "\u2048", "\u2049", "\u2049", "\u2047", "\u2047" };//но все не просто, такие «» используются по одной - как указатели
        public static char[] charsParagraphSeparator = new char[] { '\r', '\n' };
        public static string[] charsGroupsSeparators = new string[] { ".…!?;", "\u0022\u02BA\u02EE\u02DD", "()[]{}<>“”" };
        //…\u2026 (Horizontal Ellipsis) 
        //(непарные кавычки  - "\u0022 ʺ\u02BA ˮ\u02EE ˝\u02DD) 
        //(парные кавычки  - “\u201C ”\u201D «\u00AB »\u00BB) 
        //(\u002F - / - убрали, встречатеся в датах и вообще, а также временно убрали «») ⁇\u2047 ⁈\u2048 ⁉\u2049 ‼\u203C

        public static int AllDelimitersCount = charsGroupsSeparators[0].Length + charsGroupsSeparators[1].Length + charsGroupsSeparators[2].Length;
        public static string[] numbersOfGroupsNames = new string[] { "Sentence", "Quotes", "Brackets" }; //номера групп сепараторов для получения их значений в цикле
        
        public static char[] AllDigits = new char[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' };

        public static string beginChapterMark = "\u00A4\u00A4\u00A4\u00A4\u00A4";//¤¤¤¤¤ - метка строки перед началом названия главы, ¤¤¤ - метка строки после названия главы, еще \u007E - ~
        public static string endChapterMark = "\u00A4\u00A4\u00A4";
        public static string beginParagraphMark = "\u00A7\u00A7\u00A7\u00A7\u00A7";//§§§§§ - метка строки перед началом абзаца, §§§ - метка строки после абзаца
        public static string endParagraphMark = "\u00A7\u00A7\u00A7";
        public static string beginSentenceMark = "\u00B6\u00B6\u00B6\u00B6\u00B6";//¶¶¶¶¶ - метка строки перед началом предложния, ¶¶¶ - метка строки после конца предложения
        public static string endSentenceMark = "\u00B6\u00B6\u00B6";

        public static string[,] chapterNamesSamples = new string[,]//а номера глав бывают буквами!
            { { "chapter", "paragraph", "section", "subhead", "part" },
                { "глава", "параграф" , "раздел", "подраздел", "часть" }, };//можно разделить на два отдельных массива и тоже передавать через метод сепараторов - выбирая потом язык (прибавляя цифру языка к строке выбора)
        
        public static string[] paragraphMarkNameLanguage = new string[] { "Paragraph", "Абзац" };//название абзаца в маркировке абзаца
        public static string[] SentenceMarkNameLanguage = new string[] { "Sentence", "Предложение" };//название предложения в маркировке предложения
        #endregion

        //Database structure and constants
        //0 - Languages - cannot insert records (ID, ID_Language, nvchar10 Language_name)
        //1 - Chapters - Columns - ID, ID_Language, int Chapter, nvchar10 Chapter_name 
        //2 - Paragraphs - Columns - ID, ID_Language, ID_Chapter, int Paragraph, nvchar10 Paragraph_name
        //3 - Sentences - Columns - ID, ID_Language, ID_Chapter, ID_Paragraph, int Sentence, ntext Sentence_name
        public static readonly string[] DataBaseTableNames = new string[] { "Languages", "Chapters", "Paragraphs", "Sentences" };
    }

    public enum DesiredTextLanguage : int
    {
        English = 0,
        Russian = 1,        
        NothingFound = -1
    };

    public enum PiecesEdgesMarks : int
    {
        Begin = 0,
        End = 1,
        NothingFound = -1
    };

    public enum MethodFindResult : int {
        English = 0,
        Russian = 1,
        Result = 2,
        AllRight = 3,
        Error = 4,
        NothingFound = -1 }; // for work with (not only) AllBookData (int)MethodFindResult.AllRight

    public enum ButtonName : int {
        OpenFile = 0,
        SaveFile = 1,
        AnalyseText = 2,
        SelectChapterName = 3,
        Reserved = 4 };

    public enum TextBoxImplementationMessages : int {
        EnglishFilePathSelected = 0,
        RussianFilePathSelected = 1,
        ResultFilePathSelected = 2,
        ResultFileCreated = 3 };

    public enum TablesNamesNumbers : int {
        Languages = 0,
        Chapters = 1,
        Paragraphs = 2,
        Sentences = 3 }; 

    public enum TableLanguagesContent : int {
        English = 0,
        Russian = 1,
        Result = 2 };

    public enum WhatNeedDoWithFiles : int {
        PassThrough = 0,
        ReadFileFirst = 1,
        ContentChanged = 2,
        AnalyseText = 3,
        CountSymbols = 4,
        CountSentences = 5,
        WittingIncomplete = 7,
        ContinueProcessing = 8,
        StopProcessing = 9,
        Reserved10 = 10,
        SelectChapterName = 11,
        Reserved12 = 12,
        AnalyseChapterName = 13 };

    public enum WhatNeedSaveFiles : int {
        PassThrough = 0,
        SaveFileFirst = 1,        
        SaveFile = 2,
        FileSavedSuccessfully = 3,
        CannotSaveFile = 4,
        Reserved5 = 5,
        Reserved6 = 6,
        WittingIncomplete = 7,
        ContinueProcessing = 8,
        StopProcessing = 9 };    

    public enum WhatNeedDoWithTables : int {
        PassThrough = 0,
        ReadRecord = 1,
        ReadAllRecord = 2,
        Reserved3 = 3,
        InsertRecord = 4,
        DeleteRecord = 5,
        ClearTable = 7,
        ContinueProcessing = 8,
        StopProcessing = 9 };

    public enum TablesProcessingResult : int {
        Successfully = 0,
        CannotRead = 1,
        Reserved2 = 2,
        CannotInsert = 3,
        UsingWrongTableName = 4 };
}
//public enum SavingResults : int
//{
//    PassThrough = 0,
//    FileSavedSuccessfully = 1,
//    CannotRead = 2,
//    Reserved3 = 3,
//    CannotWrite = 4,
//    Reserved4 = 5,
//    WittingIncomplete = 6,
//    ContinueProcessing = 6,
//    StopProcessing = 8,
//};

