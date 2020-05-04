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
        private readonly IAllBookData _bookData;        
        private readonly ITextAnalysisLogicExtension _analysisLogic; 


        public SentencesDividingAnalysis(IAllBookData bookData, ITextAnalysisLogicExtension analysisLogic)
        {
            _bookData = bookData;
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
