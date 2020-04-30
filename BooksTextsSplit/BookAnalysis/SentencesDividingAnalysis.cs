using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Collections;

namespace BooksTextsSplit
{
    public interface ISentencesDividingAnalysis
    {
        int DividePagagraphToSentencesAndEnumerate(int desiredTextLanguage);        
    }

    public class SentencesDividingAnalysis : ISentencesDividingAnalysis
    {
        private readonly ISharedDataAccess _bookData;
        private readonly IMessageService _msgService;
        private readonly ITextAnalysisLogicExtension _analysisLogic; 


        public SentencesDividingAnalysis(ISharedDataAccess bookData, IMessageService msgService, ITextAnalysisLogicExtension analysisLogic)
        {
            _bookData = bookData;
            _msgService = msgService;
            _analysisLogic = analysisLogic;//общая логика            
        }
        

        public int DividePagagraphToSentencesAndEnumerate(int desiredTextLanguage)
        {
            int totalSentencesCount = 0;

            List<List<char>> charsAllDelimiters = ConstanstListFillCharsDelimiters();//заполнили List разделителями из констант, вернули ненулевое количество групп разделителей (предложений, кавычки, скобки)
            int sGroupCount = DConst.charsGroupsSeparators.Length;

            //стейт-машина? - бесконечный цикл while по условию "все сделано, пора выходить"
            int currentParagraphIndex = 0;
            int paragraphTextLength = _bookData.GetParagraphTextLength(desiredTextLanguage);//нет, главу искать не будем, сразу ищем абзац - в его номере уже есть номер главы

            bool sentenceFSMwillWorkWithNExtParagraph = true;//для старта машины присваиваем true;
            while (sentenceFSMwillWorkWithNExtParagraph)//список условий и методов
            {
                int nextParagraphIndex = currentParagraphIndex + 1;
                string currentParagraph = _bookData.GetParagraphText(desiredTextLanguage, currentParagraphIndex);//string currentParagraph = GetParagraphText(currentParagraphIndex, desiredTextLanguage);
                
                bool foundParagraphMark = currentParagraph.StartsWith(DConst.beginParagraphMark);
                currentParagraphIndex++;//сразу прибавили счетчик абзаца для получения следующего абзаца в следующем цикле

                //нерешенные задачи анализа раздела на предложения (метода SentencesDividingAnalysis)
                //1. Неправильный раздел варианта One of Kratzi’s heads was looking in Chitiratte’s general direction. Now the mantis picked up the bowls and turned from the meal cart - ¶¶¶ “Hei, Johanna! How is it going?”
                //   Если в конце предложения нет точки, а есть кавычки, надо делить от предыдущей точки - то есть, проверять наличие точки перед кавычками, а не только внутри.
                //   Составить полную таблицу вариантов анализа кавычек и по ней программировать логику.
                //2. Еще проблема в предварительную подготовку - нет пробела после точки (или других знаков препинания)


                if (foundParagraphMark)
                {
                    int currentParagraphNumber = FindTextPartNumber(currentParagraph, DConst.beginParagraphMark, DConst.paragraptNumberTotalDigits);//тут уже знаем, что в начале абзаца есть нужный маркер и сразу ищем номер (FindTextPartNumber находится в AnalysisLogicCultivation)

                    //на всякий случай тут можно проверять, что индекс следующего абзаца (+1 к текущему) не уткнется в конец файла
                    string sentenceTextMarksWithOtherNumbers = FindParagrapNumberForSentenceNumber(desiredTextLanguage, currentParagraph, currentParagraphNumber);//получили строку типа -Paragraph-3-of-Chapter-3 - удалены марки, но сохранены номера главы и абзаца
                                        
                    string nextParagraph = _bookData.GetParagraphText(desiredTextLanguage, nextParagraphIndex);//достаем следующий абзац только при необходимости - когда точно знаем, что там текст, который надо делить                    

                    List<List<int>> allIndexResults = FoundAllDelimitersGroupsInParagraph(nextParagraph, charsAllDelimiters, sGroupCount);//собрали все разделители по группам в массив, каждая группа в своей ветке

                    int foundMAxDelimitersGroups = FoundMaxDelimitersGroupNumber(sGroupCount, allIndexResults);//создали массив, в котором указано, сколько найдено разделителей каждой группы - изменим, теперь отдаем значение старшей найденной группы (и добавить в тестовый текст скобок)                    

                    //bool quotesGroupFound = foundMAxDelimitersGroups > 0;//можно не проверять, что больше нуля - просто цикл не запустится
                    bool oddQuotesForever = false;
                    
                    //значит есть одна или две группы кавычек, кроме точек - ищем точки внутри кавычек (с допущениями) и помечаем их отрицательными индексами, потом удалим
                    for (int currentQuotesGroup = foundMAxDelimitersGroups; currentQuotesGroup > 0; currentQuotesGroup--)
                    {
                        bool evenQuotesCount = IsCurrentGroupDelimitersCountEven(desiredTextLanguage, nextParagraph, nextParagraphIndex, allIndexResults, currentQuotesGroup);//результат пока не используем - если кавычек нечетное количество, то при проверке сейчас остановит Assert, а потом - позовем пользователя сделать четное (nextParagraph используется только для аварийной печати)                        
                        evenQuotesCount = FindSentencesDelimitersBeetweenQuotes(allIndexResults, currentQuotesGroup, evenQuotesCount);//в этом месте foundMAxDelimitersGroups может быть 1 или 2, по очереди проверяем их, не вникая, какой именно был (если только группа 0, она прошла мимо)

                        if (!evenQuotesCount)
                        {
                            oddQuotesForever = true;//фиксируем, что встретились нечетные кавычки - чтобы на следующем цикле, если есть вторая группа кавычек, переменная не затерлась
                            
                            _msgService.ShowTrace(MethodBase.GetCurrentMethod().ToString(), "nextParagraph[" + nextParagraphIndex.ToString() + "] - " + nextParagraph + DConst.StrCRLF +
                               "currentQuotesGroup = " + currentQuotesGroup.ToString() + DConst.StrCRLF +
                               "oddQuotesForever = " + oddQuotesForever.ToString() + DConst.StrCRLF +
                               "evenQuotesCount = " + evenQuotesCount.ToString(), CurrentClassName, DConst.ShowMessagesLevel);
                        }
                    }

                    if (oddQuotesForever) continue;//если с кавычками беда, то с этим абзацем больше делать нечего, уходим на следующий цикл

                    int[] SentenceDelimitersIndexesArray = RemoveNegativeSentenceDelimitersIndexes(allIndexResults);//сжали ветку массива с точками - удалили отрицательный и сохранили в обычный временный массив

                    string[] paragraphSentences = DivideTextToSentencesByDelimiters(nextParagraph, SentenceDelimitersIndexesArray);//разделили текст на предложения согласно оставшимся разделителям
                    
                    paragraphSentences = EnumerateDividedSentences(desiredTextLanguage, sentenceTextMarksWithOtherNumbers, paragraphSentences);//пронумеровали разделенные предложения - еще в том же массиве

                    totalSentencesCount = WriteDividedSentencesInTheSameParagraph(desiredTextLanguage, nextParagraphIndex, paragraphSentences, totalSentencesCount);//здесь предложения уже поделенные и с номерами, теперь слить их опять вместе, чтобы записать на то же самое место                    
                }                
                sentenceFSMwillWorkWithNExtParagraph = currentParagraphIndex < (paragraphTextLength - 1);//-1 - чтобы можно было взять следующий абзац - или проверять в конце цикла, когда к текущему уже прибавили 1 для следующего прохода
            }
            _msgService.ShowTrace(MethodBase.GetCurrentMethod().ToString(), "totalSentencesCount = " + totalSentencesCount.ToString(), CurrentClassName, DConst.ShowMessagesLevel);
            return totalSentencesCount;
        }

        public string FindParagrapNumberForSentenceNumber(int desiredTextLanguage, string currentParagraph, int currentParagraphNumber)//когда заменять на FSM, надо позаботиться, чтобы desiredTextLanguage и currentParagraphIndex были доступны в самом низу
        {
            //§§§§§00003§§§-Paragraph-of-Chapter-3 - формат номера абзаца такой - взять currentParagraph, убрать позиции по длине ParagraphBegin + totalDigitsQuantity5 + ParagraphEnd, останется -Paragraph-of-Chapter-3
            int symbolsCountToRemove = DConst.beginParagraphMark.Length + DConst.paragraptNumberTotalDigits + DConst.endParagraphMark.Length + DConst.paragraphMarkNameLanguage[desiredTextLanguage].Length + 1; //(+1 - за тире) получим длину удаляемой группы символов, вместо нее добавить -of
            
            if (currentParagraphNumber > 0) //избегаем предисловия, его как-нибудь потом поделим добавив else
            {
                //тут сформируем всю маркировку для предложений, кроме собственно номера предложения - вместо 23 считать количество символов, как указано выше
                string sentenceTextMarksWithOtherNumbers = currentParagraph.Remove(0, symbolsCountToRemove);//Возвращает новую строку, в которой было удалено указанное число символов в указанной позиции
                string currentParagraphNumberToFind000 = _analysisLogic.AddSome00ToIntNumber(currentParagraphNumber.ToString(), DConst.paragraptNumberTotalDigits);
                sentenceTextMarksWithOtherNumbers = "-" + DConst.paragraphMarkNameLanguage[desiredTextLanguage] + "-" + currentParagraphNumberToFind000 + sentenceTextMarksWithOtherNumbers;//должно получиться -of-Paragraph-3-of-Chapter-3                    
                return sentenceTextMarksWithOtherNumbers;
            }

            _msgService.ShowTrace(MethodBase.GetCurrentMethod().ToString(), "Things come up! currentParagraphNumber -->" + currentParagraphNumber.ToString() + DConst.StrCRLF + currentParagraph, CurrentClassName, 3);

            return null;//сюда попадем, если currentParagraphNumber = -1 - это если не найден нужный номер, ну или вообще закончился общий текст - при правильной работе не должны попадать
        }

        public int FindTextPartNumber(string currentParagraph, string symbolsMarkBegin, int totalDigitsQuantity)
        {
            //найти и выделить номер главы
            int currentPartNumber = -1;//чтобы не спутать с нулевым индексом на выходе, -1 - ничего нет (совсем ничего)            
            int symbolsMarkBeginLength = symbolsMarkBegin.Length;
            bool partNumberFound = Int32.TryParse(currentParagraph.Substring(symbolsMarkBeginLength, totalDigitsQuantity), out currentPartNumber);//вместо 3 взять totalDigitsQuantity для главы
            if (partNumberFound)
            {
                return currentPartNumber;
            }
            else
            {
                //что-то пошло не так, остановиться - System.Diagnostics.Debug.Assert(partNumberFound, "Stop here - partNumberFound did not find!");
                _msgService.ShowTrace(MethodBase.GetCurrentMethod().ToString(), "STOP HERE - currentPartNumber did not find in currentParagraph!" + DConst.StrCRLF +
                    "currentParagraph = " + currentParagraph + " -->" + DConst.StrCRLF +
                    "symbolsMarkBegin --> " + symbolsMarkBegin + DConst.StrCRLF +
                    "totalDigitsQuantity = " + totalDigitsQuantity.ToString(), CurrentClassName, 3);
                return (int)MethodFindResult.NothingFound;
            }
        }

        public int[] RemoveNegativeSentenceDelimitersIndexes(List<List<int>> allIndexResults)
        {
            int SentenceDelimitersIndexesCount = allIndexResults[0].Count();
            for (int c = SentenceDelimitersIndexesCount - 1; c >= 0; c--)//сжимаем нулевую группу - удаляем отрицательные индексы, начиная сверху массива
            {
                bool currentIndexNegative = allIndexResults[0][c] < 0;
                if (currentIndexNegative)
                {
                    allIndexResults[0].RemoveAt(c);
                }
            }
            int[] SentenceDelimitersIndexesArray = allIndexResults[0].ToArray();
            return SentenceDelimitersIndexesArray;
        }        

        public List<List<char>> ConstanstListFillCharsDelimiters()//создавать временный массив каждый раз заново - только те, которые нужны в данный момент? нет, один раз хватит
        {//вариант многоточия из обычных точек надо обрабатывать отдельно - просто проверить, нет ли трех точек подряд - уже удалили такое
            List<List<char>> charsAllDelimiters = new List<List<char>> { new List<char>(), new List<char>(), new List<char>() };//временный массив для хранения всех групп разделителей в виде char[] для IndexOfAny
            int sGroupCount = DConst.charsGroupsSeparators.Length; //получили количество групп разделителей - длина массива слов для получения констант разделителей            
            int allDelimitersCount = 0;//только для контроля правильности сбора разделителей - поставим Assert? сейчас всего 20 разделителей - а константах, а не в предложениях

            for (int g = 0; g < sGroupCount; g++)
            {
                string stringCurrentDelimiters = DConst.charsGroupsSeparators[g];
                charsAllDelimiters[g].AddRange(stringCurrentDelimiters.ToCharArray());
                allDelimitersCount += charsAllDelimiters[g].Count;
            }
            _msgService.ShowTrace(MethodBase.GetCurrentMethod().ToString(), "allDelimitersCount = " + allDelimitersCount.ToString(), CurrentClassName, DConst.ShowMessagesLevel);
            System.Diagnostics.Debug.Assert(allDelimitersCount == DConst.AllDelimitersCount, "The total Delimiters count is WRONG!");
            return charsAllDelimiters;
        }        

        public List<List<int>> FoundAllDelimitersGroupsInParagraph(string textParagraph, List<List<char>> charsAllDelimiters, int sGroupCount)//выбираем, какой метод вызвать для обработки состояния
        {
            List<List<int>> allIndexResults = new List<List<int>> { new List<int>(), new List<int>(), new List<int>() };//временный массив для хранения индексов найденных в тексте разделителей
            //заполнили List индексами найденных разделителей            
            int startFindIndex = 0;
            int[] DelimitersQuantity = new int[sGroupCount];
            for (int sGroup = 0; sGroup < sGroupCount; sGroup++)
            {
                char[] charsCurrentGroupDelimiters = charsAllDelimiters[sGroup].ToArray();
                string printCharsCurrentGroupDelimiters = new string(charsCurrentGroupDelimiters);
                startFindIndex = 0;
                int indexResult = textParagraph.IndexOfAny(charsCurrentGroupDelimiters, startFindIndex);//ищем первый по порядку разделитель - для запуска while

                //если нулевая группа и не найден ни один разделитель (а мы тут знаем, что какой-то текст точно есть), надо поставить один в конце текста
                bool addImitationDelimiter = sGroup == 0 && indexResult < 0;

                if (addImitationDelimiter)
                {
                    indexResult = textParagraph.Length - 1;//делаем вид, как будто найден разделитель в конце текста - и тут сделать отметку, добавить в непонятки
                }

                while (indexResult != -1)//если одни нашлись (уже не -1), собираем все остальные разделительы, пока находятся
                {
                    allIndexResults[sGroup].Add(indexResult);//сохраняем индекс в массиве в нулевой строке
                    startFindIndex = indexResult + 1;//начинаем новый поиск с места найденных кавычек (наверное, тут надо добавить +1)
                    DelimitersQuantity[sGroup]++; //считаем разделители
                    indexResult = textParagraph.IndexOfAny(charsCurrentGroupDelimiters, startFindIndex);//Значение –1, если никакой символ не найдена. Индекс от нуля с начала строки, если любой символ найден
                }                
            }
            return allIndexResults;                
        }

        public bool IsCurrentGroupDelimitersCountEven(int desiredTextLanguage, string textParagraph, int cpi, List<List<int>> allIndexResults, int currentQuotesGroup)//результат пока не используем - если кавычек нечетное количество, то при проверке сейчас остановит Assert, а потом - позовем пользователя сделать четное, то есть в любом случае, считаем, что стало четное (хотя проверить все же стоит?))
        {
            int currentOFAllIndexResultsCount = allIndexResults[currentQuotesGroup].Count();//получили общее количество разделителей указанной в checkedDelimitersGroup группы            
            bool evenQuotesCount = (currentOFAllIndexResultsCount & 1) == 0;//true, если allIndexResults2Count - четное // if(a&1==0) Console.WriteLine("Четное")            
            if (!evenQuotesCount)
            {//правильно - удалить все кавычки битой группы, занести все номер группы и стертые индексы в массив с непонятками и туда же нужен индекс параграфа - и номер проблемы (хотя он будет виден по номеру группы)
                currentOFAllIndexResultsCount = currentOFAllIndexResultsCount * -1;//сделали для записи в массив замечаний количество кавычек отрицательным - признак, что этот абзац не обработан и на него надо обратить внимание
                int result = _bookData.SetNotices(desiredTextLanguage, cpi, currentOFAllIndexResultsCount, "ODD QUOTES");
                
                _msgService.ShowTrace(MethodBase.GetCurrentMethod().ToString(), "Current textParagraph with unpair quotes is - " + textParagraph + DConst.StrCRLF +
                    "current Group of Quotes = " + currentQuotesGroup.ToString() + DConst.StrCRLF +
                    "current Quotes Count = " + currentOFAllIndexResultsCount.ToString() + DConst.StrCRLF +                    
                    "The Quotes Quantity in NOT EVEN!" + evenQuotesCount.ToString(), CurrentClassName, DConst.ShowMessagesLevel); //включенная печать тут не нужна, можно потом на выходе метода включить контрольную печать всех замечаний
                return false;
            }
            System.Diagnostics.Debug.Assert(evenQuotesCount, "The Quotes Quantity in NOT EVEN");//тут проверили, что кавычек-скобок четное количество, если нечетное, то потом будем звать пользователя
            return evenQuotesCount;
        }

        //сделать простые примеры с точным расположением серараторов, написать все возможные ситуации обработки разделителей (FSM)
        public bool FindSentencesDelimitersBeetweenQuotes(List<List<int>> allIndexResults, int currentQuotesGroup, bool evenQuotesCount)//метод вызывется для проверки попадания точек (разделителей предложений) внутрь кавычек/скобок, тип кавычек - простые или откр-закр определяется checkedDelimitersGroup
        {
            if (!evenQuotesCount)//получаем и проверяем признак четности кавычек - если кавычки нечетные, сразу выходим
            {
                return false;
            }
            int SentenceDelimitersIndexesCount = allIndexResults[0].Count();
            int currentOFAllIndexResultsCount = allIndexResults[currentQuotesGroup].Count();//получили общее количество разделителей указанной в checkedDelimitersGroup группы
            //проверяем наличие разделителей нулевой группы (.?!;) между парами кавычек/скобок и при наличии таковых - удаляем (с осторожностью на правых краях)
            for (int currentNumberQuotesPair = 0; currentNumberQuotesPair < currentOFAllIndexResultsCount - 1; currentNumberQuotesPair += 2)//выбираем номер по порядку пары кавычек из общего количества кавычек, точнее выбираем номер открывающей кавычки и перекакиваем через одну в следюущем цикле
            {
                int startIndexQuotes = allIndexResults[currentQuotesGroup][currentNumberQuotesPair];//получаем индекс открывающей кавычки текущей по порядку пары
                int finishIndexQuotes = allIndexResults[currentQuotesGroup][currentNumberQuotesPair + 1];//получаем индекс закрывающей                
                //тут вообще-то можно было посчитать какой по счету индекс точки попадает в диапазон и не прогонять весь массив точек - но это на будущее
                for (int forCurrentDotPositionIndex = 0; forCurrentDotPositionIndex < SentenceDelimitersIndexesCount; forCurrentDotPositionIndex++)//достаем в цикле все индексы разделителей предложений и проверяем их на попадание в диапазон между кавычками
                {                    
                    int maxShiftLastDotBeforeRightQuote = 3;//параметр сдвига правой точки за закрывающую кавычку, здесь взять максимальный (и получить его из констант), но потом надо рассмотреть все случаи - 1. точка перед самой кавычкой, 2. пробел между ними 3. больше знаков - например многоточие 
                    int currentDotPosition = allIndexResults[0][forCurrentDotPositionIndex];
                    int rightQuoteZone = finishIndexQuotes - maxShiftLastDotBeforeRightQuote;//вычисляем границу критической зоны правой кавычки (отступ потом будет менять в маленьком цикле - или отдадим методу со switch)                    

                    bool dotAfterLeftQuote1 = currentDotPosition > startIndexQuotes;//сначала смотрим, находится ли точка после левой кавычки
                    bool dotBeforeRightQuote2 = currentDotPosition < finishIndexQuotes;//потом смотрим, находится ли точка до правой кавычки
                    bool dotInTheQuoteZone3 = currentDotPosition > rightQuoteZone;//и главный выбор - попадает ли точка в защитную зону правой кавычки - где ее надо спасать и переносить правее кавычки
                    bool dotBeforeQuoteZone3 = !dotInTheQuoteZone3;//соответственно - не попадает под защиту (но возможно попадает между кавычками)
                    bool dotNearRightQuoteNeedSave23 = dotBeforeRightQuote2 && dotInTheQuoteZone3;//проверка, что точка в зоне, но не правее правой кавычки (условие dotAfterLeftQuote1 лишнее)
                    bool dotBetweenQuotesNeedDelete1not3 = dotAfterLeftQuote1 && dotBeforeQuoteZone3;//не попадает под защиту, но попадает между кавычками - будет удалена (условие dotBeforeRightQuote2 лишнее)

                    if (dotBetweenQuotesNeedDelete1not3)
                    {//попадает до зоны кавычки - делаем индекс отрицательным для последующего удаления из массива
                        allIndexResults[0][forCurrentDotPositionIndex] = allIndexResults[0][forCurrentDotPositionIndex] * -1;//попадает до зоны кавычки - делаем индекс отрицательным для последующего удаления из массива                                                                                                                                 
                    }

                    if (dotNearRightQuoteNeedSave23)
                    {
                        allIndexResults[0][forCurrentDotPositionIndex] = finishIndexQuotes;// + 1; переносим точку на место кавычки (возможно +1, но не факт) - нет, никаких +1 не надо
                    }                    
                }
            }
            return true;
        }

        //все непонятки можно (нужно) записать в освободившийся массив ChapterNumber - кстати, их все надо чистить перед анализом следующего языка/текста - или можно завести аналогичный цифровой массив ParagraghNumber - чистить не надо, он на 2 языка
        public string[] DivideTextToSentencesByDelimiters(string textParagraph, int[] sentenceDelimitersIndexesArray)//разобраться с константами и почему не совпадает по сумме часть предложений
        {
            int foundSentenceCount = 0;
            int textParagraphLength = textParagraph.Length;
            int sentenceDelimitersIndexesCount = sentenceDelimitersIndexesArray.Length;
            
            string[] tempParagraphSentences = new string[sentenceDelimitersIndexesCount];//временный массив для хранения свежеподеленных предложений
            
            int startIndexSentence = 0;

            for (int i = 0; i < sentenceDelimitersIndexesCount; i++)
            {
                int currentSentenceDelimiterIndex = sentenceDelimitersIndexesArray[i];
                
                bool lastSentenceDelimiterFound = i == (sentenceDelimitersIndexesCount - 1);//текущее предложение - последнее                
                bool currentSymbolIsUpper = false;

                if (!lastSentenceDelimiterFound)//если предложение не последнее, то ищем после него прописную, если последнее предложение, то поиск следующего не нужен
                {
                    currentSymbolIsUpper = IsUpperSymbolExistAfterDelimiter(sentenceDelimitersIndexesCount, currentSentenceDelimiterIndex, textParagraphLength, textParagraph);
                }

                int lengthSentence = CalcLengthSentenceToDivide(currentSymbolIsUpper, lastSentenceDelimiterFound, sentenceDelimitersIndexesArray[i], startIndexSentence, textParagraphLength);

                //получается, что если не нашли большую букву и не последнее предложение, но все равно как-то пытаемся делить? этот вопрос надо осветить
                string dividedSentence = textParagraph.Substring(startIndexSentence, lengthSentence);//string Substring (int startIndex, int length)
                if(dividedSentence.Length > 0) foundSentenceCount++;
                tempParagraphSentences[i] = dividedSentence;
                startIndexSentence += lengthSentence;                
            }
            string[] paragraphSentences = new string[foundSentenceCount];
            foundSentenceCount = 0;
            for (int i = 0; i < sentenceDelimitersIndexesCount; i++)
            {
                if (tempParagraphSentences[i].Length > 0)
                {
                    paragraphSentences[foundSentenceCount] = tempParagraphSentences[i];
                    foundSentenceCount++;
                }
            }
                return paragraphSentences;//где-то тут можно сложить все символы предложений и сравнить их с исходным количеством символов, только их надо как-то сохранить, после разделения на абзацы
        }

        public int CalcLengthSentenceToDivide(bool currentSymbolIsUpper, bool lastSentenceDelimiterFound, int currertSentenceDelimitersIndexesArray, int startIndexSentence, int textParagraphLength)//считаем два варианта длины предложения - если не последнее и если последнее
        {
            int lengthSentence = 0;

            if (currentSymbolIsUpper)//если нашли новое предложение с большой буквы - делим (или если последнее предложение и уже не искали следующее) - по сути, если есть прописная, то это - не последнее - два if выполняются как if/else - но могут быть варианты
            {
                lengthSentence = currertSentenceDelimitersIndexesArray - startIndexSentence + 2;//надо +2, потому что иначе теряется пробел после точки                
            }

            if (lastSentenceDelimiterFound)//если последнее, то идем с другой стороны - отнимаем от длина абзаца начальный индекс предложения
            {
                lengthSentence = textParagraphLength - startIndexSentence;//так точнее делит последнее предложение абзаца - оказывается, есть вариант, когда последнее предложение без точки - он его захватывает - уже нет
            }

            return lengthSentence;
        }

        public bool IsUpperSymbolExistAfterDelimiter(int sentenceDelimitersIndexesCount, int currentSentenceDelimiterIndex, int textParagraphLength, string textParagraph)//ищем прописную букву - начало следующего предложения, если разделитель был не последний
        {
            bool currentSymbolIsLetterOrDigit = false;
            
            while (!currentSymbolIsLetterOrDigit)//пока не нашли букву или цифру после точки - ищем ее
            {
                currentSentenceDelimiterIndex++;
                bool keepToSearch = (currentSentenceDelimiterIndex + 1) < textParagraphLength;// + 1 было лишним?
                if (!keepToSearch)//продолжать поиск, пока индекс позиции не выходит за пределы длины абзаца
                {
                    return false;
                }
                    try
                    {
                        currentSymbolIsLetterOrDigit = Char.IsLetterOrDigit(textParagraph, currentSentenceDelimiterIndex);//Показывает, относится ли символ в указанной позиции в указанной строке к категории букв или десятичных цифр.
                    }
                    catch (ArgumentOutOfRangeException outOfRange)
                    {
                        _msgService.ShowTrace(MethodBase.GetCurrentMethod().ToString(), "textParagraph --> " + DConst.StrCRLF + textParagraph + DConst.StrCRLF +
                            "outOfRange --> " + outOfRange + DConst.StrCRLF +
                            "currentSentenceDelimiterIndex = " + currentSentenceDelimiterIndex.ToString() + DConst.StrCRLF +
                            "textParagraphLength = " + textParagraphLength.ToString(), CurrentClassName, 3);// DConst.ShowMessagesLevel);
                    }
                
            }//на самом деле, если не найдется буква или цифра после точки, то тут и останемся - надо хотя бы диагностику какую-то приделать по концу абзаца, что ли - вот он внезапно и закончился - приделали проверку
            bool currentSymbolIsUpper = Char.IsUpper(textParagraph, currentSentenceDelimiterIndex);//Показывает, относится ли указанный символ в указанной позиции в указанной строке к категории букв верхнего регистра.

            return currentSymbolIsUpper;
        }

        public string[] EnumerateDividedSentences(int desiredTextLanguage, string sentenceTextMarksWithOtherNumbers, string[] paragraphSentences) //в textParagraph получаем nextParagraph при вызове метода - следующий абзац с текстом после метки номера абзаца в пустой строке
        {            
            string needConstantnName = "Sentence" + desiredTextLanguage.ToString();//формирование ключевого слова запроса в зависимости от языка - можно языка прямо передавать параметром  
            int countSentencesNumbers = paragraphSentences.Length;
            //теперь к каждому предложению сгенерировать номер, добавить спереди метку и номер, добавить EOL и метку в конце и потом все сложить обратно во внешний массив ParagraphText                                                                               
            //генерация номера и метки очень похожа во всех трех случаях - потом можно было сделать единым методом
            for (int i = 0; i < countSentencesNumbers; i++)
            {
                int currentSentenceNumber = i + 1; //будем нумеровать с первого номера, а не с нулевого
                //создаем базовую маркировку и номер текущего предложения - ¶¶¶¶¶00001¶¶¶-Paragraph-3-of-Chapter-3

                string currentSentenceNumberToFind000 = _analysisLogic.AddSome00ToIntNumber(currentSentenceNumber.ToString(), DConst.sentenceNumberTotalDigits);//создать номер главы из индекса сохраненных индексов - как раз начинаются с нуля
                string sentenceTextMarks = DConst.beginSentenceMark + currentSentenceNumberToFind000 + DConst.endSentenceMark + "-" + DConst.SentenceMarkNameLanguage[desiredTextLanguage] + "-of" + sentenceTextMarksWithOtherNumbers;//¤¤¤¤¤001¤¤¤-Chapter- добавить номер главы от нулев
                
                paragraphSentences[i] = sentenceTextMarks + DConst.StrCRLF + paragraphSentences[i] + DConst.StrCRLF;                
            }
            return paragraphSentences;
        }

        private int WriteDividedSentencesInTheSameParagraph(int desiredTextLanguage, int nextParagraphIndex, string[] paragraphSentences, int totalSentencesCount)
        {            
            string currentParagraphToWrite = string.Join(DConst.StrCRLF, paragraphSentences);//добавить ли в конце еще один перевод строки?

            int countSentencesNumber = paragraphSentences.Length;
            int result = _bookData.SetParagraphText(desiredTextLanguage, nextParagraphIndex, currentParagraphToWrite);//ЗДЕСЬ запись SetParagraphText! - записываем абзац с пронумерованными предложениями на старое место! проверить, что попадаем на нужное место, а не в предыдущую ячейку
            
            totalSentencesCount += countSentencesNumber;

            return totalSentencesCount;
        }

        public int FoundMaxDelimitersGroupNumber(int sGroupCount, List<List<int>> allIndexResults)//проверяем, были ли кавычки, или только разделители предложений (группа 0)
        {
            int foundMAxDelimitersGroups = 0;

            for (int sGroup = 0; sGroup < sGroupCount; sGroup++)
            {
                if (allIndexResults[sGroup].Count != 0)
                {
                    foundMAxDelimitersGroups = sGroup;//максимальный номер группы разделителей, в которой есть найденные индексы
                }
            }
            return foundMAxDelimitersGroups;
        }

        public static string CurrentClassName
        {
            get { return MethodBase.GetCurrentMethod().DeclaringType.Name; }
        }



    }
}
//_msgService.ShowTrace(MethodBase.GetCurrentMethod().ToString(), "paragraphSentences[" + i.ToString() + "] --> " + DConst.StrCRLF + paragraphSentences[i] + DConst.StrCRLF +
//                    "countSentencesNumbers = " + countSentencesNumbers.ToString(), CurrentClassName, DConst.ShowMessagesLevel);
//_msgService.ShowTrace(MethodBase.GetCurrentMethod().ToString(), "textParagraph --> " + DConst.StrCRLF + textParagraph + DConst.StrCRLF +
//                            "sentenceDelimitersIndexesCount --> " + sentenceDelimitersIndexesCount.ToString() + DConst.StrCRLF +
//                            "currentSentenceDelimiterIndex = " + currentSentenceDelimiterIndex.ToString() + DConst.StrCRLF +
//                            "textParagraphLength = " + textParagraphLength.ToString(), CurrentClassName, DConst.ShowMessagesLevel);
//private bool IsCurrentSymbolDelimiter(char[] charsSentenceDelimiters, char currentSymbol)
//{
//    bool currentSymbolIsDelimiter = false;
//    foreach (char delimiter in charsSentenceDelimiters)
//    {
//        currentSymbolIsDelimiter = currentSymbol == delimiter;
//        if (currentSymbolIsDelimiter)
//        {
//            return currentSymbolIsDelimiter;
//        }
//    }
//    return currentSymbolIsDelimiter;
//}
//int textParagraphLengthFromSentences = 0;// было 1, но с 0 нет никакой разницы (она только для первого предложения, а там и так все сейчас неладно)
//string currentParagraph = GetParagraphText(i, desiredTextLanguage); //надо бы выводить или абзац, в котором беда или хотя бы его индекс (но можно в вызывающем методе, правда, там тоже всего этого нет) - когда дадут доступ, вызовем
//textParagraphLengthFromSentences += lengthSentence;//непонятно, зачем он нужен, нигде не используется
//int endParagraphMarkLength = DConst.endParagraphMark.Length;
//string paragraphKeyWord = DConst.paragraphMarkNameLanguage[desiredTextLanguage]; 
//int paragraphKeyWordLength = paragraphKeyWord.Length;
////новые методы из _bookData
//int GetIntContent(int desiredTextLanguage, string needOperationName) => _bookData.GetIntContent(desiredTextLanguage, needOperationName);//перегрузка для получения длины двуязычных динамических массивов
//int GetIntContent(string needOperationName, string stringToSet, int indexCount) => _bookData.GetIntContent(needOperationName, stringToSet, indexCount);//перегрузка для записи обычных массивов
//int GetIntContent(int desiredTextLanguage, string needOperationName, string stringToSet, int indexCount) => _bookData.GetIntContent(desiredTextLanguage, needOperationName, stringToSet, indexCount);

//string GetStringContent(string nameOfStringNeed, int indexCount) => _bookData.GetStringContent(nameOfStringNeed, indexCount);        
//string GetStringContent(int desiredTextLanguage, string nameOfStringNeed, int indexCount) => _bookData.GetStringContent(desiredTextLanguage, nameOfStringNeed, indexCount);

//int FindTextPartNumber(string currentParagraph, string stringMarkBegin, int totalDigitsQuantity) => _analysisLogic.FindTextPartNumber(currentParagraph, stringMarkBegin, totalDigitsQuantity);
//string CreatePartTextMarks(string stringMarkBegin, string stringMarkEnd, int currentUpperNumber, int enumerateCurrentCount, string sentenceTextMarksWithOtherNumbers) => _analysisLogic.CreatePartTextMarks(stringMarkBegin, stringMarkEnd, currentUpperNumber, enumerateCurrentCount, sentenceTextMarksWithOtherNumbers);
//string AddSome00ToIntNumber(string currentChapterNumberToFind, int totalDigitsQuantity) => _analysisLogic.AddSome00ToIntNumber(currentChapterNumberToFind, totalDigitsQuantity);
//string sentenceTextMarks = CreatePartTextMarks(stringToPutMarkBegin, stringToPutMarkEnd, currentChapterNumber, currentSentenceNumber, sentenceTextMarksWithOtherNumbers);//метод CreatePartTextMarks находится в AnalysisLogicCultivation
//int totalSentencesCount = 0;
//SetParagraphText(currentParagraphToWrite, nextParagraphIndex, desiredTextLanguage);
//лучше достать-получить метку абзаца из предыдущего    
//bool tryToFindLetterOrDigitTillDeathDoUsPart = !currentSymbolIsLetterOrDigit || !stopToSearch;
//bool isSentenceWithoutDot = sentenceDelimitersIndexesCount == 0 && textParagraphLength != 0;
//if (isSentenceWithoutDot)
//{
//    _msgService.ShowTrace(MethodBase.GetCurrentMethod().ToString(), "textParagraph = " + textParagraph + DConst.StrCRLF +
//        "The Sentence is Without Dot!" + DConst.StrCRLF +
//        "textParagraphLength = " + textParagraphLength.ToString() + DConst.StrCRLF +
//        "sentenceDelimitersIndexesCount = " + sentenceDelimitersIndexesCount.ToString(), CurrentClassName, 3);
//    sentenceDelimitersIndexesCount = 1;
//    sentenceDelimitersIndexesArray = new int[1];
//    sentenceDelimitersIndexesArray[0] = textParagraphLength - 1;
//}
//int paragraphTotalDigits = 5;//для номера абзаца - перенести в AnalysisLogicDataArrays
//string[] allParagraphMarks = GetStringArrConstant("ParagraphMark");
//string beginParagraphMark = allParagraphMarks[0];
//int endParagraphMarkIndex = GetIntConstant("ParagraphMark") - 1;
//string endParagraphMark = allParagraphMarks[endParagraphMarkIndex];
//string[] numbersOfGroupsNames = GetConstantWhatNot("Groups"); //new string[sGroupCount];   
//старые методы из _bookData
//int GetParagraphTextLength(int desiredTextLanguage) => _bookData.GetParagraphTextLength(desiredTextLanguage);
//string GetParagraphText(int paragraphCount, int desiredTextLanguage) => _bookData.GetParagraphText(paragraphCount, desiredTextLanguage);
//int SetParagraphText(string paragraphText, int paragraphCount, int desiredTextLanguage) => _bookData.SetParagraphText(paragraphText, paragraphCount, desiredTextLanguage);
//event EventHandler AnalyseInvokeTheMain;
//новые методы из _arrayAnalysis
//int GetIntConstant(string needConstantnName) => _arrayAnalysis.GetIntConstant(needConstantnName);
//string[] GetStringArrConstant(string needConstantnName) => _arrayAnalysis.GetStringArrConstant(needConstantnName);
//public event EventHandler AnalyseInvokeTheMain;
//старые методы из _arrayAnalysis
//int GetConstantWhatNotLength(string ParagraphOrSentence) => _arrayAnalysis.GetIntConstant(ParagraphOrSentence);
//string[] GetConstantWhatNot(string ParagraphOrSentence) => _arrayAnalysis.GetStringArrConstant(ParagraphOrSentence);
//string needConstantnName = "Paragraph" + desiredTextLanguage.ToString();//формирование ключевого слова запроса в зависимости от языка - можно языка прямо передавать параметром
//GetStringArrConstant(needConstantnName)[0];//paragraphMarkNameLanguage
//int paragraphTotalDigits = GetIntConstant("ParagraphTotalDigits");//получим количество символов-цифр для номера абзаца в марке
//string[] allSentenceMarks = GetStringArrConstant("SentenceMark");
//string beginSentenceMark = allSentenceMarks[0];
//int endSentenceMarkIndex = GetIntConstant("SentenceMark") - 1;
//string endSentenceMark = allSentenceMarks[endSentenceMarkIndex];
//int sentenceTotalDigits = GetIntConstant("SentenceTotalDigits");//получим количество символов-цифр для номера предложения в маркировке
//string sentenceKeyWord = GetStringArrConstant(needConstantnName)[0];
//string stringToPutMarkBegin = "SentenceBegin";
//string stringToPutMarkEnd = "SentenceEnd";
//char[] charsSentenceDelimiters = charsAllDelimiters[0].ToArray();//создали массив точек List<List<char>> charsAllDelimiters,
//вставляет пустые предложения и вроде даже какие-то глотает, хэш текста - 9628dcb7e84a589eabb98590b96b4613, предложений - 528
//bool lengthSentenceIsPositive = lengthSentence < startIndexSentence;
//if (lengthSentenceIsPositive)
//{
//}
//else
//{
//    _msgService.ShowTrace(MethodBase.GetCurrentMethod().ToString(), "textParagraphLength = " + textParagraphLength.ToString() + DConst.StrCRLF +
//        "The Sentence length or start index is WRONG and CANNOT be used!" + DConst.StrCRLF +
//        "startIndexSentence = " + startIndexSentence.ToString() + DConst.StrCRLF +
//        "lengthSentence = " + lengthSentence.ToString(), CurrentClassName, 3);
//}
//bool upperCharFoundOrLastSentence = currentSymbolIsUpper;// || lastSentenceDelimiterFound;//новое предложение с большой буквы или если последнее предложение (не надо искать следующее)
//textParagraphLengthFromSentences -= 1; //вычитаем единицу, которую не удалось прибавить к последнему предложению
//int paragraphLengthNotEqualCount = 0;
//bool paragraphLengthNotEqual = textParagraphLengthFromSentences != textParagraphLength;
//if (paragraphLengthNotEqual)
//{
//    paragraphLengthNotEqualCount++;//так просто не сосчитать несовпадающие предложения, надо вынести переменную далеко наверх - или в AnalysisLogicDataArrays
//    int showMessagesTemp = 3;

//    if (paragraphLengthNotEqualCount > 5) showMessagesTemp = 0;//если несовпадающих предложений много, перестаем их показывать

//    _msgService.ShowTrace(MethodBase.GetCurrentMethod().ToString(), "textParagraphLength = " + textParagraphLength.ToString() + DConst.StrCRLF +
//            "The paragraph length is NOT EQUAL to sentences length sum!" + DConst.StrCRLF +
//            "textParagraphLengthFromSentences = " + textParagraphLengthFromSentences.ToString(), CurrentClassName, showMessagesTemp);//сюда поставить переменную или метод аварийного сообщения
//}
//_msgService.ShowTrace(MethodBase.GetCurrentMethod().ToString(), "paragraphLengthNotEqualCount = " + paragraphLengthNotEqualCount.ToString(), CurrentClassName, 3);
//int checkLengthOfLastSentence = startIndexSentence + lengthSentence;
//bool exceptionWillCome = checkLengthOfLastSentence >= textParagraphLength;
//if (exceptionWillCome)
//lengthSentence -= 1;//если последнее предложение абзаца, то уменьшаем длину раздела на 1 - вдруг там нет пробела после точки (можно впрямую проверить по длине абзаца)
//currentSymbolIsUpper = false;
//прежде всего рассмотрим положение текущей точки - что после нее (надо смотреть, что перед ней?)
//char currentSymbol = textParagraph[currentSentenceDelimiterIndex];
//bool currentSymbolIsDelimiter = IsCurrentSymbolDelimiter(charsSentenceDelimiters, currentSymbol);

//if (!currentSymbolIsDelimiter)
//{
//    _msgService.ShowTrace(MethodBase.GetCurrentMethod().ToString(), "textParagraph = " + textParagraph + DConst.StrCRLF +
//        "currentSymbol = " + currentSymbol.ToString() + DConst.StrCRLF +
//        "currentSentenceDelimiterIndex = " + currentSentenceDelimiterIndex.ToString(), CurrentClassName, DConst.ShowMessagesLevel);
//}
//вышли с указателем на букву/цифру currentSentenceDelimiterIndex
//currentSymbol = textParagraph[currentSentenceDelimiterIndex];
//_msgService.ShowTrace(MethodBase.GetCurrentMethod().ToString(), "currentSymbol = " + currentSymbol.ToString() + DConst.StrCRLF +
//    "currentSentenceDelimiterIndex = " + currentSentenceDelimiterIndex.ToString() + DConst.StrCRLF +
//    "currentSymbolIsLetterOrDigit = " + currentSymbolIsLetterOrDigit.ToString(), CurrentClassName, DConst.ShowMessagesLevel);
//_msgService.ShowTrace(MethodBase.GetCurrentMethod().ToString(), "startIndexSentence = " + startIndexSentence.ToString() + DConst.StrCRLF +
//    "lengthSentence = " + lengthSentence.ToString() + DConst.StrCRLF +
//    "chechLengthLastSentence = " + checkLengthOfLastSentence.ToString() + DConst.StrCRLF +
//    "textParagraphLength = " + textParagraphLength.ToString() + DConst.StrCRLF +
//    "exceptionWillCome = " + exceptionWillCome.ToString(), CurrentClassName, DConst.ShowMessagesLevel);
//_msgService.ShowTrace(MethodBase.GetCurrentMethod().ToString(), "textParagraph - " + textParagraph + DConst.StrCRLF +
//    "textParagraphLength = " + textParagraphLength.ToString() + DConst.StrCRLF +
//    "paragraphSentences[" + i.ToString() + "] - " + paragraphSentences[i] + DConst.StrCRLF +
//    "textParagraphLengthFromSentences = " + textParagraphLengthFromSentences.ToString(), CurrentClassName, DConst.ShowMessagesLevel);
//попадает в зону кавычки, тут потом рассмотрим разные варианты расстояний до кавычки (когда пройдет старый тест)
//варианты для switch - расстояние от точки до кавычки - 1 символ, 2, 3, 4, 5 или больше (5 - это многоточие из точек и пробел между ним и кавычками)
//начинаем поиск точки со значения maxShiftLastDotBeforeRightQuote = 5 - вот, куда двигаться, влево от кавычки или наоборот?
//сразу же можно доставать реальные символы во временный массив - отталкиваясь от индекса правой кавычки
//
//сначала проверяем, есть ли в следующей позиции (после текущей позиции, с которой сюда попали) еще точки - сюда мы попали с самой левой точкой, так что двигаемся вправо, пока очередной индекс не превысит кавычку
//someDotsInQuoteZone[0] = 10;//в первой ячейке есть точка по умолчанию - с ней мы попали сюда, нет, это не так - эта точка в произвольном месте
//заполнили массив (индексы - позиции в тексте в защитной зоне правой кавычки слева-направо) метками найденных точек (10 - заменить на enum), но нет

//int[] someDotsIndexInQuoteZone = new int[maxShiftLastDotBeforeRightQuote];
//int totalDotsCountInZone = 0;
//for (int shiftFromQuote = 1; shiftFromQuote < maxShiftLastDotBeforeRightQuote; shiftFromQuote++)//проверили следующие maxShiftLastDotBeforeRightQuote индексов в allIndexResults[0] - которые попадают в зону кавычки, сохранили в массиве
//{
//    int shiftDotPositionIndex = forCurrentDotPositionIndex + shiftFromQuote;//достали следующий индекс массива точки, проверим, где она
//    bool nextIndexIsExist = shiftDotPositionIndex < SentenceDelimitersIndexesCount;
//    if (nextIndexIsExist)
//    {
//        int shiftDotPosition = allIndexResults[0][shiftDotPositionIndex];//достали значение индекса положения точки
//        bool currentDotIsBeforeRightQuote = shiftDotPosition < finishIndexQuotes;//сравнили положение точки с правой кавычкой - что она тоже не выходит за правую кавычку
//        if (currentDotIsBeforeRightQuote)
//        {
//            someDotsIndexInQuoteZone[shiftFromQuote] = shiftDotPosition;
//            allIndexResults[0][shiftDotPositionIndex] = allIndexResults[0][shiftDotPositionIndex] * -1;
//            totalDotsCountInZone++;
//        }
//    }
//}
//if (totalDotsCountInZone > 1)//одна точка там точно есть, а если больше, то это или многоточие или сдвоенные знаки (типа ?!)
//{
//    int totalDotsCountInZone1 = totalDotsCountInZone - 1;
//    forCurrentDotPositionIndex += totalDotsCountInZone1;
//    _msgService.ShowTrace(MethodBase.GetCurrentMethod().ToString(), "Current textParagraph has ELLIPSIS - " + DConst.StrCRLF +
//        textParagraph + DConst.StrCRLF +
//        "First Dot with the Index = " + forCurrentDotPositionIndex.ToString() + DConst.StrCRLF +
//        "First Dot is in the Position = " + currentDotPosition.ToString() + DConst.StrCRLF +
//        "Total Dots Quantity = " + totalDotsCountInZone.ToString(), CurrentClassName, DConst.ShowMessagesLevel);
//}
//for (int shiftFromQuote = 1; shiftFromQuote < maxShiftLastDotBeforeRightQuote; shiftFromQuote++)//проверяем остальные позиции - что там есть
//{

//    //если попадает, то смотрим на значение индекса - положение точки
//    //достанем реальный символ
//    int currentSymbolIndex = finishIndexQuotes - shiftFromQuote;
//    char currentSymbol = textParagraph[currentSymbolIndex];
//}
//positiveCurrentIndexOfDotPosition = 0;
//int currentRestDelimitersIndex = 0;
//currentRestDelimitersIndex = -1;
//currentRestDelimitersIndex = currentIndexOfDotPosition; //индекс точки не попадает между кавычек никаким образом - ничего не делаем, возвращаем исходный индекс
//сдвинем все разделители на 1 вправо, чтобы не терялись точки - но последний сдвигать нельзя, проверяем длину и отрезаем
// - по дороге похоже потерялся пробел на границе предложений, но это не выход
//(где-то здесь теряются точки в конце предложений, попробуем поставить +1 (но нет, наверное, надо не здесь, а сдвинуть индекс разделителя на 1 вверх))
//bool dotAfterLeftButBeforeRightQuote12 = dotAfterLeftQuote1 && dotBeforeRightQuote2;//суммарное условие - точка между кавычками (но не нужное, потом удалить)
//int nextParagraphIndex = 0;
//string currentParagraph = null;
//string nextParagraph = null;            
//bool foundParagraphMark = false;
//string sentenceTextMarksWithOtherNumbers = null;            
//int foundMAxDelimitersGroups = 0;
//noNeedCheckParagraphIsEndNow = currentParagraphIndex >= (paragraphTextLength-1);//-1 - чтобы можно было взять следующий абзац - или проверять в конце цикла
//if (noNeedCheckParagraphIsEndNow)//если больше не нужен новый абзац, стейт-машина заканчивает работу
//{
//    sentenceFSMstillWork = false;
//}
//bool needFillConstantList = sGroupCount == 0;//проверяем наполнение константами групп разделителей, должно быть не 0
//if (needFillConstantList)
//{
//_msgService.ShowTrace(MethodBase.GetCurrentMethod().ToString(), "sGroupCount == 0 is " + needFillConstantList.ToString() + " - (" + sGroupCount.ToString() + ")", CurrentClassName, DConst.ShowMessagesLevel);
//_msgService.ShowTrace(MethodBase.GetCurrentMethod().ToString(), "sGroupCount = " + sGroupCount.ToString(), CurrentClassName, DConst.ShowMessagesLevel);
//}
//int positiveCurrentIndexOfDotPosition = 0;
//int[] tempAllIndexResultsCheckedDelimitersGroup = allIndexResults[foundMAxDelimitersGroups].ToArray();//временный массив индесов текущей группы кавычек
//int[] temp0IndexResultsCheckedDelimitersGroup = allIndexResults[0].ToArray();//временный сохраненный (исходный) массив индесов точек
//надо избавиться от foundDelimitersGroups? где-то тут надо определить число проходов соотвественно количеству найденных групп - можно возращать инт самой старшей найденной группы и бегать по кругу, пока оно не станет отрицательным - добавив условие к вызову нужных методов
//if (foundMAxDelimitersGroups == 0)
//{
//    return allIndexResults; //allIndexResults[0].ToArray();
//}
//int[] tempSentenceDelimitersIndexesArray = allIndexResults[0].ToArray();//временно рабочий массив индесов разделителей предложений - для сохранения результатов - вроде бы один и тот же массив, а объединить не получается
//if (currentRestDelimitersIndex > 0)
//{
//    tempSentenceDelimitersIndexesArray[positiveCurrentIndexOfDotPosition] = currentRestDelimitersIndex;//заполнение массива - если бы знать заранее его длину, то можно было бы обойтись без временного - подумать над этим
//    positiveCurrentIndexOfDotPosition++;
//}
//int[] SentenceDelimitersIndexesArray = tempSentenceDelimitersIndexesArray.Take(positiveCurrentIndexOfDotPosition).ToArray();//настоящий (выходной) временный массив (упакованный без нулей и прочего) - теперь известна его длина, а раньше был временно-временный - вместо отрицательных значений нули, хотя и все в конце            
//return SentenceDelimitersIndexesArray;//возвращать полезнее число оставшихся разделителей - кому нужны уже удаленные? а возвращать новый маленький массив индексов - еще полезнее, еще его надо сразу сжать - выкинуть отрицательные - уже успешно сжали
//int checkedDelimitersGroup = 0;
//int sGroupCount = foundDelimitersGroups.Length;//вместо того, что смотреть во всякие foundDelimitersGroups, надо посмотреть, сколько строк есть у allIndexResults - и сразу все понятно из первых рук
//for (int sGroup = 0; sGroup < sGroupCount; sGroup++)
//{
//    if (foundDelimitersGroups[sGroup] > 0)
//    {
//        checkedDelimitersGroup = foundDelimitersGroups[sGroup];//получаем количество разделителей самой старшей группы, которая нашлась 
//    }
//}
//checkedDelimitersGroup = foundMAxDelimitersGroups;
//public int RemoveSentencesDelimitersBeetweenQuotes(int[] temp0IndexResultsCheckedDelimitersGroup, int forCurrentIndexOfDotPosition, int startIndexQuotes, int finishIndexQuotes)
//{
//    int currentRestDelimitersIndex = 0;
//    int maxShiftLastDotBeforeRightQuote = 3;//параметр сдвига правой точки за закрывающую кавычку, здесь взять максимальный (и получить его из констант), но потом надо рассмотреть все случаи - 1. точка перед самой кавычкой, 2. пробел между ними 3. больше знаков - например многоточие 
//    int currentIndexOfDotPosition = temp0IndexResultsCheckedDelimitersGroup[forCurrentIndexOfDotPosition];

//    //варианты для switch - расстояние от точки до кавычки - 1 символ, 2, 3, 4, 5 или больше (5 - это многоточие из точек и пробел между ним и кавычками)             

//    bool action1 = currentIndexOfDotPosition > startIndexQuotes;//сначала смотрим, больше ли индекс точки первой кавычки - если нет, то сразу выходим
//    if(action1)
//    {
//        bool action2 = currentIndexOfDotPosition < finishIndexQuotes;//потом смотрим, меньше ли индекс точки второй кавычки - если нет, то сразу выходим
//        if (action2)
//        {
//            int rightQuoteZone = finishIndexQuotes - maxShiftLastDotBeforeRightQuote;//вычисляем границу критической зоны правой кавычки
//            bool action3 = currentIndexOfDotPosition > rightQuoteZone;//и главный выбор - попадает ли индекс точки до зоны второй кавычки или в зону
//            if(action3)
//            {//попадает в зону, тут потом рассмотрим разные варианты расстояний до кавычки (когда пройдет старый тест)
//                temp0IndexResultsCheckedDelimitersGroup[forCurrentIndexOfDotPosition] = finishIndexQuotes;// + 1; переносим точку на место кавычки (возможно +1, но не факт)
//                currentRestDelimitersIndex = finishIndexQuotes;                        
//                return currentRestDelimitersIndex;
//            }
//            else
//            {//попадает до зоны - делаем индекс отрицательным для последующего удаления из массива
//                temp0IndexResultsCheckedDelimitersGroup[forCurrentIndexOfDotPosition] = temp0IndexResultsCheckedDelimitersGroup[forCurrentIndexOfDotPosition] * -1;
//                currentRestDelimitersIndex = -1;
//                return currentRestDelimitersIndex;
//            }
//        }
//    }
//    currentRestDelimitersIndex = currentIndexOfDotPosition; //тут просто выходим из условий - ничего не делаем, возвращаем исходный индекс
//    return currentRestDelimitersIndex;
//}
//int currentRestDelimitersIndex = RemoveSentencesDelimitersBeetweenQuotes(temp0IndexResultsCheckedDelimitersGroup, forCurrentIndexOfDotPosition, startIndexQuotes, finishIndexQuotes);//временный метод для сохранения старой логики                    
//не рассмотрен вариант нахождения и группы 2 и группы 1 - после обработки группы 2 надо вернуться опять сюда
//    if (foundMAxDelimitersGroups < 0)
//{
//    return null;//была пустая строка - тоже не надо возвращать - будет просто пустой allIndexResults, может, даже не созданный? кстати, тогда FindSentencesDelimitersBeetweenQuotes вообще вызывать и не надо - проходим мимо
//}            
//int[] SentenceDelimitersIndexesArray = FindSentencesDelimitersBeetweenQuotes(textParagraph, allIndexResults, foundDelimitersGroups);//foundDelimitersGroups может быть 0, 1 или 2 - если два, то потом надо как-то рассмотреть вариант и с 1
//string[] paragraphSentences = DivideTextToSentencesByDelimiters(textParagraph, SentenceDelimitersIndexesArray);//разделили текст на предложения согласно оставшимся разделителям
//return paragraphSentences;            
//string[] paragraphSentences = SelectActionByDelimitersGroupState(nextParagraph, charsAllDelimiters, sGroupCount);////здесь делим текст на предложения! и вызываем селектор методов обработки состояний - согласно найденному состоянию
//int foundMAxDelimitersGroups = ResultListFillDelimitersIndexes(textParagraph, charsAllDelimiters, allIndexResults, sGroupCount);//поиск старшей группы разделителей (кавычек) в тексте
//выбрать по очереди все абзацы, найти номер главы (а так же предисловие), потом в главе найти номер абзаца, достать абзац, получить и сравнить его номер (со счетчиком) и начать работать с предложениями
//for (int currentParagraphIndex = 0; currentParagraphIndex < paragraphTextLength; currentParagraphIndex++)//перебираем все абзацы текста - тут надо запустить большой FSM и вызывать все методы отсюда
//string currentParagraph = GetParagraphText(currentParagraphIndex, desiredTextLanguage);
//состояние - есть маркер абзаца или нет, действие если есть - вызов PrepareToDividePagagraphToSentences, если нет - перейти к следующему абзацу
//public int ResultListFillDelimitersIndexes(string textParagraph, List<List<char>> charsAllDelimiters, List<List<int>> allIndexResults, int sGroupCount)
//{             
//    int startFindIndex = 0;
//    int[] DelimitersQuantity = new int[sGroupCount];
//    int delimitersGroupsState = 0;

//    for (int sGroup = 0; sGroup < sGroupCount; sGroup++)
//    {
//        char[] charsCurrentGroupDelimiters = charsAllDelimiters[sGroup].ToArray();
//        string printCharsCurrentGroupDelimiters = new string(charsCurrentGroupDelimiters);
//        startFindIndex = 0;
//        int indexResult = textParagraph.IndexOfAny(charsCurrentGroupDelimiters, startFindIndex);//ищем первый по порядку разделитель - для запуска while
//        while (indexResult != -1)//если одни нашлись (уже не -1), собираем все остальные разделительы, пока находятся
//        {
//            allIndexResults[sGroup].Add(indexResult);//сохраняем индекс в массиве в нулевой строке
//            startFindIndex = indexResult + 1;//начинаем новый поиск с места найденных кавычек (наверное, тут надо добавить +1)
//            DelimitersQuantity[sGroup]++; //считаем разделители
//            indexResult = textParagraph.IndexOfAny(charsCurrentGroupDelimiters, startFindIndex);//Значение –1, если никакой символ не найдена. Индекс от нуля с начала строки, если любой символ найден
//            _msgService.ShowTrace(MethodBase.GetCurrentMethod().ToString(), "sGroup = " + sGroup.ToString() + DConst.StrCRLF +
//                "DelimitersQuantity = " + DelimitersQuantity[sGroup].ToString() + DConst.StrCRLF +
//                "printCharsCurrentGroupDelimiters --> " + printCharsCurrentGroupDelimiters + DConst.StrCRLF +
//                "NEW startFindIndex (+ indexResultQuotes) = " + startFindIndex.ToString(), CurrentClassName, DConst.ShowMessagesLevel);
//        }                
//        if (DelimitersQuantity[sGroup] > 0)
//        {
//            delimitersGroupsState = sGroup;
//        }                
//    }                
//    return delimitersGroupsState;
//}
//string stringDelimitersGroupsState = null;
//int allDelimitersQuantity = 0;//только для тестов - общее количество найденных разделителей в примерах
//allDelimitersQuantity = allDelimitersQuantity + DelimitersQuantity[sGroup];
//if (DelimitersQuantity[0] > 0)
//{
//    bool successStringToInt = Int32.TryParse(stringDelimitersGroupsState, out delimitersGroupsState);
//    if (!successStringToInt)
//    {
//        _msgService.ShowTrace(MethodBase.GetCurrentMethod().ToString(), "String with delimitersGroupsState --> " + stringDelimitersGroupsState, CurrentClassName, 3);
//    }
//    System.Diagnostics.Debug.Assert(successStringToInt, "что за фигня?, в строке не числовые символы");
//}      
//switch (delimitersGroupsState)
//{
//    case
//    0:// - найдена только нулевая группа                    
//int[] SentenceDelimitersIndexesArray = allIndexResults[0].ToArray();
//case
//10:// - найдена нулевая и первая
//    SentenceDelimitersIndexesArray = FindSentencesDelimitersBeetweenQuotes(textParagraph, allIndexResults, foundDelimitersGroups);//foundDelimitersGroups = 1
//    paragraphSentences = DivideTextToSentencesByDelimiters(textParagraph, SentenceDelimitersIndexesArray);//разделили текст на предложения согласно оставшимся разделителям
//    return paragraphSentences;
//case
//20:// - найдена нулевая и вторая - обе группы кавычек(1 и 2) обрабатываются одинаково - скорее всего, одним и тем же методом
//    SentenceDelimitersIndexesArray = FindSentencesDelimitersBeetweenQuotes(textParagraph, allIndexResults, foundDelimitersGroups);//foundDelimitersGroups = 2
//    paragraphSentences = DivideTextToSentencesByDelimiters(textParagraph, SentenceDelimitersIndexesArray);//разделили текст на предложения согласно оставшимся разделителям
//    return paragraphSentences;
//case
//21:// - найдена первая и вторая - не имеет смысла, но допустим
//    actionsResults = -12;//или еще чему-то приравнять
//    return actionsResults;
//case
//210:// - найдены все три группы (тяжелый случай, скорее всего скобки и кавычки)
//    actionsResults = CommandProcess120();
//    return actionsResults;
//}
//return null;
//рассмотреть для начала два варианта - точка находится до "зоны правой кавычки" и в этой зоне - в отдельный метод
//int removeDelimitersCount = RemoveSentencesDelimitersBeetweenQuotes(allIndexResults, checkedDelimitersGroup);//удалили все точки(?!) между кавычками-скобками
//bool currentDotPositionInQuotesZone = (currentIndexOfDotPosition > (finishIndexQuotes - maxShiftLastDotBeforeRightQuote)) && (currentIndexOfDotPosition < finishIndexQuotes);//точка попадает в зону кавычек - разделить на 2 случая? точка вплотную к кавычке и точка на символ от кавычки
//if (currentDotPositionInQuotesZone)//если точка(?!) находится близко к закрывающим кавычкам (между ними еще может быть пробел), то принимаем меры - переносим точку на место кавычки (возможно +1, но не факт)
//{                
//    temp0IndexResultsCheckedDelimitersGroup[forCurrentIndexOfDotPosition] = finishIndexQuotes;// + 1;
//    currentRestDelimitersIndex = finishIndexQuotes;
//    return currentRestDelimitersIndex;
//}

//bool currentDotPositionBetweenQuotes = (currentIndexOfDotPosition > startIndexQuotes) && (currentIndexOfDotPosition < finishIndexQuotes);//точка попадает между кавычек - до зоны правой кавычки ( - maxShiftLastDotBeforeRightQuote)
//if (currentDotPositionBetweenQuotes)//точка попадает между кавычек, удаляем ее
//{
//    temp0IndexResultsCheckedDelimitersGroup[forCurrentIndexOfDotPosition] = temp0IndexResultsCheckedDelimitersGroup[forCurrentIndexOfDotPosition] * -1;
//    currentRestDelimitersIndex = -1;
//    return currentRestDelimitersIndex;
//}
//public string[] FSM1(string textParagraph, List<List<char>> charsAllDelimiters, int sGroupCount) //запускаем машину состояний - поиск разделителей, их анализ, потом деление текста на предложения
//{
//    List<List<int>> allIndexResults = new List<List<int>> { new List<int>(), new List<int>(), new List<int>() };//временный массив для хранения индексов найденных в тексте разделителей
//    //заполнили List индексами найденных разделителей
//    int delimitersGroupsState = ResultListFillDelimitersIndexes(textParagraph, charsAllDelimiters, allIndexResults, sGroupCount);//FSM - запуск вычисления состояния по массиву разделителей

//    //string[] paragraphSentences = SelectActionByDelimitersGroupState(textParagraph, delimitersGroupsState, charsAllDelimiters, allIndexResults, sGroupCount);//вызываем селектор методов обработки состояний - согласно найденному состоянию

//    return paragraphSentences;
//}        
//тут-то и вызываем метод дележки - передавая ему текст и собранные номера главы и абзаца
//allIndexResults0Count = allIndexResults[0].Count();//заново посчитали длину массива индексов точек(?!) после децимации
//countSentencesNumber = allIndexResults0Count;//фактически количество предложений должно быть равно окончательному количеству разделителей
//allIndexResults[0][forCurrentIndexOfDotPosition] = allIndexResults[0][forCurrentIndexOfDotPosition] * -1;//чтобы не менять количество разделителей на ходу, делаем его отрицательным (или надо идти с конца массива, если Remove ненужные)
//theRestDelimitersCount++;//изменить способ подсчета (или добавить?)
//тут избавиться от списка allIndexResults - его время вышло и перейти к простому временному массиву
//int allIndexResults0Count = allIndexResults[0].Count();//потом еще надо будет проверить следующий за разделителем символ - там может быть кавычка (и вообще, проверить есть ли там пробел и заглавная буква, если последний - это не проверять)            
//for (int i = allIndexResults0Count - 1; i >= 0; i--)
//{
//    if (allIndexResults[0][i] < 0)
//    {
//        allIndexResults[0].RemoveAt(i);//удаляем ставшие отрицательные индексы - для удобства
//    }
//}
//allIndexResults0Count = allIndexResults[0].Count();//заново посчитали длину массива индексов точек(?!) после децимации
//int countSentencesNumber = allIndexResults0Count;//фактически количество предложений должно быть равно окончательному количеству разделителей
//string[] paragraphSentences = DivideTextToSentencesByDelimiters(textParagraph, allIndexResults, allIndexResults0Count);//разделили текст на предложения согласно оставшимся разделителям
//проверяем наличие разделителей каждой группы, начиная с последней (кавычки окр-закр) - по сути, проверяем сначала 2-ю группу, потом 1-ю
//for (int checkedDelimitersGroup = sGroupCount - 1; checkedDelimitersGroup > 0; checkedDelimitersGroup--)
//{
//    if (allIndexResults[checkedDelimitersGroup] != null)
//    {
//        int removeDelimitersCount = RemoveSentencesDelimitersBeetweenQuotes(allIndexResults, checkedDelimitersGroup);//удалили все точки(?!) между кавычками-скобками
//    }
//}
//краевые эффекты - не находит разделители в конце абзаца, кроме этого, возможно, надо нормировать текст - убрать пробелы между ?! " . - и другие варианты
//теряем точку в конце обычного предложения без кавычек
//тут надо рассмотреть случаи p.m. и аналогичную фигню с левыми точками - проверять пробелы после точки и смотреть на заглавную букву (или EOL)
//if (finishIndexQuotes > startIndexQuotes)//проверили, что закрывающая правее, чем открывающая - надо?
//int newCurrentIndexOfDotPosition = Method4(result, finishIndexQuotes, currentIndexOfDotPosition);//метод-переключатель состояния FSM, только вот выдача нуля явно лишняя - наверное, надо присваивать allIndexResults[0][forCurrentIndexOfDotPosition] внутри него
//if (newCurrentIndexOfDotPosition != 0) allIndexResults[0][forCurrentIndexOfDotPosition] = newCurrentIndexOfDotPosition;
//переносим точку разделения предложений с разделителя предложений на (за!) кавычки, чтобы ничего не терялось
//-1 - прошли возможный пробел, -2 - попали на кавычки, -3 - вышли за кавычки (но надо проверять пробелы и другие знаки между точками и кавычками - если многоточие например)
//
//
//результат не используем - возвращать что-то другое? например, последняя группа найденных разделителей
//что вернуть-то? количество предложений = метод дележки(текст, номер главы, номер абзаца)
//проверяем, что текущий абзац - это номер абзаца, можно его достать еще раз через currentParagraphIndex и сравнить с полученным - что находимся в нужном месте
//string controlParagraph = GetParagraphText(currentParagraphIndex, desiredTextLanguage);
//bool theFollowingNeedToDivide = String.Equals(currentParagraph, controlParagraph);//тут правильно было бы добавить CultureInfo.CreateSpecificCulture(cultureName), но наверное не надо, строки должны совпадать в точности
//if (theFollowingNeedToDivide)
//тут надо решить, что дальше делать с полученными предложениями - вообще, надо бы их вернуть в DividePagagraphToSentences и пусть он там с ними разбирается, но с временным массивом сложности - его размерность становится известна только тут
//public int SelectAllQuotesOrDelimitersInText(string textParagraph, Dictionary<int, string> searchResultQuotes, string[] charsQuotesOrDelimiters)
//{
//    int textParagraphLength = textParagraph.Length;
//    int startFindIndex = 0;
//    int finishFindIndex = textParagraphLength;
//    int indexResultQuotes = 0;            
//    int quotesTypesQuantity = 0;//общее количество найденных типов
//    int quotesQuantity = 0;//общее количество найденных чего искали всех типов
//    int charsQuotesOrSeparatorLength = charsQuotesOrDelimiters.Length;
//    for (int i = 0; i < charsQuotesOrSeparatorLength; i++)
//    {
//        startFindIndex = 0;
//        finishFindIndex = textParagraphLength;//сброс точки старта и длины текста перед поиском нового варианта кавычек
//        indexResultQuotes = textParagraph.IndexOf(charsQuotesOrDelimiters[i], startFindIndex, finishFindIndex);//ищем, есть ли кавычки (первые по по порядку) - для запуска while

//        if (indexResultQuotes > -1)
//        {
//            quotesTypesQuantity++;
//        }
//        while (indexResultQuotes != -1)//если одни нашлись (уже не -1), собираем все остальные кавычки, пока находятся
//        {
//            searchResultQuotes.Add(indexResultQuotes, charsQuotesOrDelimiters[i]);//сохраняем индекс в тексте и тип найденных кавычек

//            _msgService.ShowTrace(MethodBase.GetCurrentMethod().ToString(), "textParagraph [" + i.ToString() + "] -- > " + textParagraph + DConst.StrCRLF + DConst.StrCRLF +
//                "finishFindIndex (textLengh) = " + finishFindIndex.ToString() + DConst.StrCRLF +
//                "startFindIndex (start of the search) = " + startFindIndex.ToString() + DConst.StrCRLF +
//                "indexResultQuotes (quotes found on position No.) = " + indexResultQuotes.ToString() + DConst.StrCRLF +
//                "charsQuotesSeparator[i] --> " + charsQuotesOrDelimiters[i] + DConst.StrCRLF +
//                "searchResultQuotes value on [" + indexResultQuotes.ToString() + "] = " + searchResultQuotes[indexResultQuotes], CurrentClassName, DConst.ShowMessagesLevel);

//            startFindIndex = indexResultQuotes + 1;//начинаем новый поиск с места найденных кавычек (наверное, тут надо добавить +1)
//            finishFindIndex = textParagraphLength - startFindIndex;//надо каждый раз вычитать последнюю найденную позицию из полной длины текста, а не остатка
//            quotesQuantity++; //считаем кавычки

//            indexResultQuotes = textParagraph.IndexOf(charsQuotesOrDelimiters[i], startFindIndex, finishFindIndex);//Значение –1, если строка не найдена. Индекс от нуля с начала строки, если строка найдена
//            _msgService.ShowTrace(MethodBase.GetCurrentMethod().ToString(), "NEW finishFindIndex (- indexResultQuotes) = " + finishFindIndex.ToString() + DConst.StrCRLF +
//                "NEW startFindIndex (+ indexResultQuotes) = " + startFindIndex.ToString(), CurrentClassName, DConst.ShowMessagesLevel);
//        }
//    }
//    return quotesTypesQuantity;
//}

//public int CollectTextInQuotes(string textParagraph, Dictionary<int, string> searchResultQuotes, int quotesTypesQuantity, string[] charsSentencesDelimiters, int charsSentenceSeparatorLength)
//{
//    //проверить символы-разделительы до и после закрывающих кавычек, а также между закрывающей и следующей открывающей, все как-то занести в общий массив
//    //или собрать все символы-разделительы в отдельный массив, а потом уже анализировать все вместе

//    //добавить сюда константы формирования номера предложения

//    int quotesQuantity = searchResultQuotes.Count;
//    int[,] allPairsQuotes = new int[2, quotesQuantity/2];//в нулевой строке массива индекс (по тексту абзаца) открывающей кавычки, в первой - закрывающей

//    string[] allSentencesInParagraph = new string[quotesQuantity];//спорная размерность массива, скорее, надо динамический
//    int sentenceNumber = 0;//индекс массива allSentencesInParagraph

//    int endIndexQuotes = (int)MethodFindResult.NothingFound; ;//если -1 - текст в кавычках не найден, придумать более подходящее название переменной

//    switch (quotesTypesQuantity)
//    {
//        case
//        0:
//            return (int)MethodFindResult.NothingFound;//не нашли никаких кавычек - чего тогда звали? не может быть такого варианта - можно убрать
//        case
//        1://нашли один тип кавычек
//            int forwardOrBacksparkQuote = 0;
//            int i = 0;
//            int j = 0;
//            foreach (int indexQuotes in searchResultQuotes.Keys)
//            {
//                forwardOrBacksparkQuote = i & 1;
//                allPairsQuotes[forwardOrBacksparkQuote, j] = indexQuotes;//записали открывающие и закрывающие кавычки в разные линейки массива
//                _msgService.ShowTrace(MethodBase.GetCurrentMethod().ToString(), "indexQuotes = " + indexQuotes.ToString() + DConst.StrCRLF +
//                    "i = " + i.ToString() + DConst.StrCRLF +
//                    "j = " + j.ToString() + DConst.StrCRLF +
//                    "forwardOrBacksparkQuote = " + forwardOrBacksparkQuote.ToString(), CurrentClassName, 3);
//                if ((i & 1) == 1) j++;
//                i++;
//            }
//            int forwardQuoteIndex = 0;//индекс открывающей кавычки
//            int backsparkQuoteIndex = 0;//индекс закрывающей кавычки
//            int indexResultDelimitersBeforeQuoute = -1;//инициализация отрицанием
//            int indexResultDelimitersAfterQuoute = -1;
//            //int indexResultDelimitersNearQuoute = -1;
//            for (int n = 0; n < (quotesQuantity / 2); n++)
//            {                        
//                forwardQuoteIndex = allPairsQuotes[0, n];//достаем пару кавычек из массива - открывающую и закрывающую
//                backsparkQuoteIndex = allPairsQuotes[1, n];
//                //int startFindIndexNearQuoute = backsparkQuoteIndex - 1;//старт поиска за один символ до закрывающей кавычки
//                //int charCountForCheking = 3; //длина поиска - символ до кавычки, кавычка и символ после кавычки
//                for (int t = 0; t < charsSentenceSeparatorLength; t++)
//                {
//                    indexResultDelimitersBeforeQuoute = textParagraph.IndexOf(charsSentencesDelimiters[t], forwardQuoteIndex, 1);//ищем разделительы перед кавычкой
//                    indexResultDelimitersAfterQuoute = textParagraph.IndexOf(charsSentencesDelimiters[t], forwardQuoteIndex+1, 1);//ищем разделительы после кавычки
//                }
//                //проверяем сеператор сразу после кавычки - если есть, то переписываем предложение в окончательный массив по кавычке + символ-разделитель после нее
//                if (indexResultDelimitersAfterQuoute > 0)//нуль быть не может, потому что закрывающая кавычка и была найдена пара
//                {                            
//                    allSentencesInParagraph[sentenceNumber] = textParagraph.Substring(forwardQuoteIndex, backsparkQuoteIndex + 1); //string substring = value.Substring(startIndex, length);
//                }
//                else//сеператор сразу после кавычки не найден - тогда проверяем, был ли разделитель перед кавычкой
//                {
//                    if (indexResultDelimitersBeforeQuoute > 0)//если был разделитель перед кавычкой, то переписываем найденное предложение, включая кавычку
//                    {
//                        allSentencesInParagraph[sentenceNumber] = textParagraph.Substring(forwardQuoteIndex, backsparkQuoteIndex); //string substring = value.Substring(startIndex, length);
//                    }
//                }


//            }
//            //pair.Key, pair.Value
//            _msgService.ShowTrace(MethodBase.GetCurrentMethod().ToString(), 
//                "allPairsQuotes[0,0] = " + allPairsQuotes[0, 0].ToString() + DConst.StrCRLF +
//                "allPairsQuotes[1,0] = " + allPairsQuotes[1, 0].ToString() + DConst.StrCRLF + 
//                "allPairsQuotes[0,1] = " + allPairsQuotes[0, 1].ToString() + DConst.StrCRLF +
//                "allPairsQuotes[1,1] = " + allPairsQuotes[1, 1].ToString() + DConst.StrCRLF +
//                "allPairsQuotes[0,2] = " + allPairsQuotes[0, 2].ToString() + DConst.StrCRLF +
//                "allPairsQuotes[1,2] = " + allPairsQuotes[1, 2].ToString() + DConst.StrCRLF +                        
//                "quotesQuantity = " + quotesQuantity.ToString(), CurrentClassName, 3);
//            i++;
//            return quotesTypesQuantity; //endIndexQuotes;
//        case
//        2://нашли (наверное) сдвоенный тип кавычек - проверяем, так ли это (или раньше надо было проверить?)

//            return endIndexQuotes;
//    }


//    //кусок абзаца от кавычки до кавычки проверить на окончания предложения, кроме последнего символа перед кавычкой
//    //если нет окончаний, проверить последний символ перед кавычкой и первый после кавычки
//    //если там есть, то закончить предложение, если нет - искать разделительы до первого найденного, там закончить предложение - каши не дошли до следующей кавычки
//    //если началась следующая - опять этот же метод - рекурсия, что ле?
//    //если в кавычках несколько разделителей, дня начала проверить, что на второй кавычке есть разделитель, сразу до или после
//    //если есть, то проверить, что во всех предложениях внутри кавычек меньше 5-ти слов
//    //если меньше, собрать предложение и перейти к следующему
//    //если больше - думать (может, не пяти слов, а 10-ти?)

//    return quotesTypesQuantity;// endIndexQuotes; //-1 - текст в кавычках не найден - или вторые кавычки не найдены? в общем, засада и что-то надо делать
//}
//три динамических массива - по группам разделителей и три раза пройти IndexOfAny - и все это в отдельный метод
//проверять, разделительы из одного символа или из двух, если из двух - то разделять на отдельные строки List - например - все еще проще, слили и разделили в массив char
//тогда все доставания констант и передачу их в массив char можно сделать в цикле - определения для метода GetConstantWhatNot тоже в массив и через него же доставать - уст.
//разделить разделительы на 3 группы по номеру и сделать временный массив на 3 значения, где хранить количество найденных разделителей
//достать и сформировать временный массив разделителей (разделителей предложений) - нулевой
//int charsSentenceSeparatorLength = GetConstantWhatNotLength("Sentence");
//string[] stringArraySentencesDelimiters = new string[charsSentenceSeparatorLength];
//stringArraySentencesDelimiters = GetConstantWhatNot("Sentence");//вариант многоточия из обычных точек надо обрабатывать отдельно - просто проверить, нет ли трех точек подряд
//string stringSentencesDelimiters = String.Join("", stringArraySentencesDelimiters);
//charsAllDelimiters[0].AddRange(stringSentencesDelimiters.ToCharArray());
//достать и сформировать временный массив кавычек-скобок - первый
//int charsQuotesTypesLength = GetConstantWhatNotLength("Quotes");
//string[] stringArrayQuotesTypes = new string[charsQuotesTypesLength];
//stringArrayQuotesTypes = GetConstantWhatNot("Quotes");
//string stringQuotesTypes = String.Join("", stringArrayQuotesTypes);
//charsAllDelimiters[1].AddRange(stringQuotesTypes.ToCharArray());
//достать и сформировать временный массив откр-закр кавычек-скобок - второй и третий
//int charsBraketsTypesLength = GetConstantWhatNotLength("Brackets");
//string[] stringArrayBraketsTypes = new string[charsBraketsTypesLength];
//string[] stringArrayBraketsClosingTypes = new string[charsBraketsTypesLength];
//stringArrayBraketsTypes = GetConstantWhatNot("Brackets");
//for (int t = 0; t < charsBraketsTypesLength; t++)//разделяем строковые константы с двумя символами на открывающий и закрывающий 
//{
//    string currentChar = stringArrayBraketsOpeningTypes[t];
//    stringArrayBraketsOpeningTypes[t] = currentChar[0].ToString();
//    stringArrayBraketsClosingTypes[t] = currentChar[1].ToString();
//}
//string stringBraketsTypes = String.Join("", stringArrayBraketsTypes);
//string stringBraketsClosingTypes = String.Join("", stringArrayBraketsClosingTypes);
//charsAllDelimiters[2].AddRange(stringBraketsTypes.ToCharArray());            
//charsAllDelimiters[3].AddRange(stringBraketsClosingTypes.ToCharArray());
//int charCount = 0;
//foreach(char ch in textParagraph)
//{
//    _msgService.ShowTrace(MethodBase.GetCurrentMethod().ToString(), "ch[" + charCount.ToString() + "] -->" + ch.ToString(), CurrentClassName, 3);
//    charCount++;
//}
//искать будем методом IndexOf(String, Int32, Int32) - Возвращает индекс с отсчетом от нуля первого вхождения значения указанной строки в данном экземпляре. Поиск начинается с указанной позиции знака; проверяется заданное количество позиций
//еще лучше IndexOf(String, Int32) - Возвращает индекс с отсчетом от нуля первого вхождения значения указанной строки в данном экземпляре. Поиск начинается с указанной позиции знака
//или можно поменять на IndexOf(String, Int32, StringComparison) - Возвращает индекс с отсчетом от нуля первого вхождения указанной строки в текущем объекте String. Параметры задают начальную позицию поиска в текущей строке и тип поиска
//правила раздела текста на предложения -
//начинаем нумерацию предложений - в этом абзаце и общую в тексте (может быть во всем тексте или метод вызвали для отдельной главы?)
//тогда не надо перегонять в char - сразу ищем вхождения символов (сохраняем строку абзаца в массив char, вычисляем его длину и проверяем текст на разделители (включая варианты кавычек? или они отдельным массивом - хлопотнее, но выгоднее)
//смотрим первый символ - str[0] - если это строчная буква - IsUpper(String, Int32) - , вызываем метод, собирающий все предложение, но пока делаем на месте - сохраняем посимвольно в строку
//можно в начале сделать быструю проверку строки на все варианты кавычек, если кавычек нет, то и хорошо, а если есть, то какие-то одни конкретные (и сразу посчитать количество) и потом только их и искать (найденные кавычки = кавычки[index])
//проверка содержания всех кавычек - 
//bool contains = nextParagraph.Any(s => s.Contains(pattern)); - непонятно, как проверять сразу весь список кавычек
//if (nextParagraph.Any(s => s.Contains(stringToCheck)))
//{
//    //do your stuff here
//}
//если первый символ - кавычки (во всех вариантах) вызываем метод определения строки в кавычках (как совместить кавычки с остальными разделителями в едином цикле перебора? или сделать два разных?)

//если первый символ не прописная буква и не кавычка, то это может быть тире (то же самое, что и с прописной), цифра (непонятно, надо смотреть варианты) или что еще?
//потом ищем символ-разделитель, который заканчивает предложение, после него смотрим, чтобы был пробел и следующая буква была строчной (варианты - тире, цифра?)
//если встретили опять кавычки, но в середине строки, то тоже вызываем метод определения строки в кавычках, пусть сам разбирается, он умный (должен быть)
//сначала ищем все варианты кавычек, текст в кавычках сразу сохраняем, как предложение, потом проверяем, какая буква после закрывающей кавычки, если строчная, то продолжаем искать разделитель
//метод FindTextPartMarker находится в AnalysisLogicCultivation
//bool foundParagraphMark = FindTextPartMarker(currentParagraph, "ParagraphMark");//добавить в метод 0 для начальной марки / проверяем начало абзаца на маркер абзаца, если есть, то надо вызвать подготовку деления абзаца (она возьмет следующий абзац для деления на предложения)
//bool FindTextPartMarker(string currentParagraph, string stringMarkBegin) => _analysisLogic.FindTextPartMarker(currentParagraph, stringMarkBegin);
//если встретился разделитель, опять же проверяем, что за ним строчная буква
//sentenceTextMarksWithOtherNumbers = null;//сбрасываем (не)нужный флаг для следующего прохода - должен автоматически измениться на следующем проходе
//string sentenceTextMarks = CreatePartTextMarks(stringToPutMarkBegin, stringToPutMarkEnd, currentChapterNumber, currentSentenceNumber, sentenceTextMarksWithOtherNumbers);//создали базовую маркировку и номер текущего предложения - ¶¶¶¶¶00001¶¶¶-Paragraph-3-of-Chapter-3
////foundParagraphMark = false;//сбрасываем (не)нужный флаг для следующего прохода - должен автоматически измениться на следующем проходе
//_msgService.ShowTrace(MethodBase.GetCurrentMethod().ToString(), "textParagraph -->" + textParagraph + DConst.StrCRLF +
//                        "sGroup = " + sGroup.ToString() + DConst.StrCRLF +
//                        "DelimitersQuantity = " + DelimitersQuantity[sGroup].ToString() + DConst.StrCRLF +
//                        "printCharsCurrentGroupDelimiters --> " + printCharsCurrentGroupDelimiters + DConst.StrCRLF +
//                        "NEW startFindIndex (+ indexResultQuotes) = " + startFindIndex.ToString(), CurrentClassName, DConst.ShowMessagesLevel);
//_msgService.ShowTrace(MethodBase.GetCurrentMethod().ToString(), "currentSymbolIsUpper = " + currentSymbolIsUpper.ToString() + DConst.StrCRLF +
//    "currertSentenceDelimitersIndexesArray = " + currertSentenceDelimitersIndexesArray.ToString() + DConst.StrCRLF +
//    "startIndexSentence = " + startIndexSentence.ToString() + DConst.StrCRLF +
//    "lengthSentence = " + lengthSentence.ToString(), CurrentClassName, DConst.ShowMessagesLevel);
//_msgService.ShowTrace(MethodBase.GetCurrentMethod().ToString(), "lastSentenceDelimiterFound = " + lastSentenceDelimiterFound.ToString() + DConst.StrCRLF +
//    "textParagraphLength = " + textParagraphLength.ToString() + DConst.StrCRLF +
//    "startIndexSentence = " + startIndexSentence.ToString() + DConst.StrCRLF +
//    "lengthSentence = " + lengthSentence.ToString(), CurrentClassName, DConst.ShowMessagesLevel);
//тут сначала проверить, есть ли нечетное количество других кавычек в абзаце и, если других нет - игнорировать (или еще проще - если кавычка в группе всего одна, удалить ее индекс и всего делов) - нет, звать пользователя - показать ему абзац и перевод
