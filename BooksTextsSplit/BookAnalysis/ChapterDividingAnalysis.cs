using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace BooksTextsSplit
{
    public interface IChapterDividingAnalysis
    {
        string ChapterNameAnalysis(int desiredTextLanguage);        
    }

    public class ChapterDividingAnalysis : IChapterDividingAnalysis
    {
        private readonly ISharedDataAccess _bookData;
        private readonly IMessageService _msgService;
        private readonly ITextAnalysisLogicExtension _analysisLogic;        

        delegate bool IsOrNotEqual(char x);
        delegate bool DoElseConditions(string x, int i, int j);

        delegate string TransduceWord(string baseKeyWord);

        
        public ChapterDividingAnalysis(ISharedDataAccess bookData, IMessageService msgService, ITextAnalysisLogicExtension analysisLogic)
        {
            _bookData = bookData;
            _msgService = msgService;
            _analysisLogic = analysisLogic;//общая логика               
        }

        public string ChapterNameAnalysis(int desiredTextLanguage)//Main здесь - ищем все названия и номера глав, создаем метку названия главы в фиксированном формате и записываем ее обратно в строку перед оригинальным называнием главы
        {
            int[] allParagraphsWithDigitsFound = CollectAllParagraphsWithDigitsFound(desiredTextLanguage);//в массив такой же длины, как все абзацы текста собираем найденные цифры в начале строки с сохранением индекса абзаца, где найдена цифра

            int[] chapterNameIsDigitsOnly = IsChaptersNumbersIncreased(allParagraphsWithDigitsFound);//делаем новый сжатый массив в котором все найденные цифры идут подряд (первый массив сохраняем и не портим)            

            string[] totalBaseFormsOfNamesSamples = CreateBaseFormsOfNamesSamples(desiredTextLanguage);//создали массив всех форм (первая прописная, все прописные, все строчные) ключевых слов для поиска в тексте названий глав

            int[] kMultiple = FindAllParagraphsWithKeyForms(desiredTextLanguage, allParagraphsWithDigitsFound, totalBaseFormsOfNamesSamples);//проверили строки с найденными цифрами на наличие ключевых слов, составили статистику, сколько раз встречается каждое слово

            int kMultipleMaxKeyWordIndex = KeyWordFormIndexFound(totalBaseFormsOfNamesSamples, kMultiple);//возвращаем индекс формы найденного ключевого слова из массива образцов GetChapterNamesSamples (а слово следующий метод сам достанет из массива)

            string keyWordFoundForm = CompareChapterNumbersAndManes(desiredTextLanguage, totalBaseFormsOfNamesSamples, allParagraphsWithDigitsFound, chapterNameIsDigitsOnly, kMultipleMaxKeyWordIndex);//проверяем строки с номерами и названиями глав на совпадение и формируем окончательный массив индексов глав
            
            string lastFoundChapterNumberInMarkFormat = MarkChaptersInTextBook(desiredTextLanguage, keyWordFoundForm);//делим текст на главы, ставим метки с собственными номерами глав (совпадающими по значению с исходными)

            return lastFoundChapterNumberInMarkFormat;//вернем образец маркировки, все остальное есть в служебном массиве заметок
        }

        //нерешенные задачи анализа имен глав (метода ChapterDividingAnalysis)
        //1. наличие в книге более старших разделов - Книга, Раздел и т.д. если нумерация глав идет непрерывно по книге, то они не помешают, а если заново, то надо что-то думать
        //2. проверить метод при наличии в книге других ключевых слов названий глав - в нужных местах (близко к началу абзаца) и с подходящими по смыслу номерами (такие тестовые места в книге отметить $$$$$)
        //3. подумать над вторым проходом метода по желанию пользователя - если он отметил какие-то варианты старших разделов - сохранить главы и добавить разметку других разделов (как потом хранить в базе?)

        public string MarkChaptersInTextBook(int desiredTextLanguage, string keyWordFoundForm)//разделили текст на главы - перед каждой главой поставили специальную метку вида ¤¤¤¤¤KeyWord-NNN-¤¤¤, где KeyWord - в той же форме, в какой в тексте, а NNN - три цифры номера главы с лидирующими нулями
        {
            string lastFoundChapterNumberInMarkFormat = ""; //можно убрать (вернем из метода маркировку последней главы - лучше массив с индексами глав в общем тексте, хотя, он не меняется, чего его возвращать)
            int allParagraphsWithDigitsFoundLength = _bookData.GetParagraphTextLength(desiredTextLanguage);
            for (int cpi = 0; cpi < allParagraphsWithDigitsFoundLength; cpi++)//cpi - current paragraph index
            {
                int currentChapterNumber = _bookData.GetNoticeNumber(desiredTextLanguage, cpi);
                bool isChapterNumberFound = currentChapterNumber > 0;//allParagraphsWithDigitsFound[cpi] > 0;
                if (isChapterNumberFound)
                {
                    string currentParagraph = _bookData.GetParagraphText(desiredTextLanguage, cpi);

                    //можно было бы проверить строку, что там есть ключевое слово и номер
                    string currentChapterNumberToFind000 = _analysisLogic.AddSome00ToIntNumber(currentChapterNumber.ToString(), DConst.chapterNumberTotalDigits);
                    string chapterTextMarks = DConst.beginChapterMark + currentChapterNumberToFind000 + DConst.endChapterMark + "-" + keyWordFoundForm;//¤¤¤¤¤001¤¤¤-Chapter-
                    lastFoundChapterNumberInMarkFormat = chapterTextMarks;
                    chapterTextMarks += DConst.StrCRLF + currentParagraph + DConst.StrCRLF;//дописываем маркировку главы к названию главы, добавив строку для нее

                    int setParagraphResult = _bookData.SetParagraphText(desiredTextLanguage, cpi, chapterTextMarks);//int GetIntContent(desiredTextLanguage, SetParagraphText, stringToSet, indexCount) надо писать в индекс[i] из массива индексов, а не в сам i - currentParagraphChapterNumberIndex
                    //проверять setParagraphResult - но сначала сделать его осмысленным в самом методе записи

                    string newCurrentParagraph = _bookData.GetParagraphText(desiredTextLanguage, cpi);//только для печати, убрать вместе с ней, если надо
                    _msgService.ShowTrace(MethodBase.GetCurrentMethod().ToString(), "CurrentParagraph[" + cpi.ToString() + "] -- > " + currentParagraph + DConst.StrCRLF +
                            "chapterTextMarks - " + chapterTextMarks + DConst.StrCRLF +
                            "newCurrentParagraph - " + newCurrentParagraph + DConst.StrCRLF +
                            "GetNoticeNumber(desiredTextLanguage, [cpi = " + cpi.ToString() + "] = " + currentChapterNumber.ToString(), CurrentClassName, DConst.ShowMessagesLevel);
                }
            }
            _msgService.ShowTrace(MethodBase.GetCurrentMethod().ToString(), "lastFoundChapterNumberInMarkFormat -- > " + lastFoundChapterNumberInMarkFormat, CurrentClassName, DConst.ShowMessagesLevel);

            return lastFoundChapterNumberInMarkFormat;//непонятно, что вернуть - дальше класса уже не уйдет, только что-то для проверки в классе напоследок (ранее - вернуть уточненное количество глав) - возвращаем формат метки для последующего разбора, хотя логичнее было бы вернуть ключевое слово - потом меньше хлопот
        }

        public string CompareChapterNumbersAndManes(int desiredTextLanguage, string[] totalBaseFormsOfNamesSamples, int[] allParagraphsWithDigitsFound, int[] chapterNameIsDigitsOnly, int kMultipleMaxKeyWordIndex)
        {
            int chapterNamesParagraphIndexesCount = 0;//счетчик найденных глав
            int allParagraphsWithDigitsFoundLength = allParagraphsWithDigitsFound.Length;
            int chapterNameIsDigitsOnlyLength = chapterNameIsDigitsOnly.Length;

            string keyWordFoundForm = totalBaseFormsOfNamesSamples[kMultipleMaxKeyWordIndex];

            int[] chapterNamesParagraphIndexesTemp = new int[chapterNameIsDigitsOnlyLength];//временный сжатый массив индексов абзацев, в которых названия глав (длина массива с запасом, потом обрежем - когда найдем настоящие названия глав)

            //теперь надо сравнить ключевые слова с массивом возрастающих номеров и оставить только те пересечения, где есть все 3 значения - возрастающий номер по порядку (с него начинаем искать), затем на одинаковом индексе ключевое слово и такой же номер, если какой-то номер не найден - беда (Assert)
            for (int ccni = 0; ccni < chapterNameIsDigitsOnlyLength; ccni++) //ccni - current chapter number index
            {//тут второй цикл до макс.значения chapterNameIsDigitsOnly - и логичнее бы выглядел do-while

                int currentChapterNumber = chapterNameIsDigitsOnly[ccni];//достаем первый упорядоченный номер из последовательности номеров                                

                for (int cpi = 0; cpi < allParagraphsWithDigitsFoundLength; cpi++)//cpi - current paragraph index
                {
                    int currentPossibleChapterNumber = allParagraphsWithDigitsFound[cpi];//достаем припрятанные номера возможно глав                    

                    //тут, наверное, надо рассмотреть случай, когда номер совпал, а слово - нет (сделать пример в тексте с другими ключевыми словами и разными номерами - такими же как ближайшие главы
                    if (currentPossibleChapterNumber == currentChapterNumber)//дальше проверяем, было ли на этом индексе ключевое слово главы и на соответствие индекса ключевого слова найденному слову по статистике
                    {
                        int currentPossibleChapterNameIndex = _bookData.GetNoticeNumber(desiredTextLanguage, cpi);//достаем сохраненные названия глав - int GetNoticeNumber(int desiredTextLanguage, int noticeIndex);
                        if (currentPossibleChapterNameIndex == kMultipleMaxKeyWordIndex)
                        {
                            int result = _bookData.SetNotices(desiredTextLanguage, cpi, currentPossibleChapterNumber, keyWordFoundForm);//заодно сохранили все номера глав и ключевое слово в служебный массив заметок - для передачи дальше
                            chapterNamesParagraphIndexesTemp[ccni] = cpi;
                            chapterNamesParagraphIndexesCount++;//считаем найденные главы
                        }
                    }
                }
            }
            _msgService.ShowTrace(MethodBase.GetCurrentMethod().ToString(), "chapter names Count = " + chapterNamesParagraphIndexesCount.ToString(), CurrentClassName, DConst.ShowMessagesLevel);
            //не забыть рассмотреть случай, когда вообще нет ключевого слова, только номера
            return keyWordFoundForm;//надо вернуть чего-то хорошее и полезное... - пока возвращаем найденную форму ключевого слова называния главы
        }

        public int KeyWordFormIndexFound(string[] totalBaseFormsOfNamesSamples, int[] kMultiple)//возвращаем готовую форму найденного ключевого слова 
        {
            //формируем полный перечень вариантов ключевых слов названий глав (обязательно с делегатами?) - сложим их в общий временный массив для IndexOfAny
            int totalBaseFormsOfNamesSamplesLength = totalBaseFormsOfNamesSamples.Length;
            //тут получили второй разреженный массив найденных ключевых слов, используя статистику в kMultiple, надо выкинуть лишние значения из массива
            int kMultipleMax = 0;
            int kMultipleMaxKeyWordIndex = 0;//тут похоже узнаем, какое же слово используется для обозначения глав
            for (int k = 0; k < totalBaseFormsOfNamesSamplesLength; k++)//находим максимальное значение в kMultiple
            {
                if (kMultiple[k] > kMultipleMax)
                {
                    kMultipleMax = kMultiple[k];
                    kMultipleMaxKeyWordIndex = k;

                    _msgService.ShowTrace(MethodBase.GetCurrentMethod().ToString(), "Total chapters names count kMultipleMax = " + kMultipleMax.ToString() + "] = " + DConst.StrCRLF +
                                "kMultipleMaxKeyWordIndex = " + kMultipleMaxKeyWordIndex, CurrentClassName, DConst.ShowMessagesLevel);
                }
            }
            _msgService.ShowTrace(MethodBase.GetCurrentMethod().ToString(), "Total chapters names count kMultipleMax = " + kMultipleMax.ToString() + "] = " + DConst.StrCRLF +
                                "keyWordForm was found --> " + totalBaseFormsOfNamesSamples[kMultipleMaxKeyWordIndex], CurrentClassName, DConst.ShowMessagesLevel);

            return kMultipleMaxKeyWordIndex;//возвращаем найденный индекс ключевого слова
        }

        public int[] FindAllParagraphsWithKeyForms(int desiredTextLanguage, int[] allParagraphsWithDigitsFound, string[] totalBaseFormsOfNamesSamples)
        {
            int allParagraphsWithDigitsFoundLength = allParagraphsWithDigitsFound.Length;
            int totalBaseFormsOfNamesSamplesLength = totalBaseFormsOfNamesSamples.Length;

            int[] kMultiple = new int[totalBaseFormsOfNamesSamplesLength];//массив, в котором временно сохраним количество найденных вариантов названий глав - чтобы отбросить редко встречающиеся

            int startIndexOf = 0;
            int findChapterNameSpace = DConst.FindChapterNameSpace;

            for (int cpi = 0; cpi < allParagraphsWithDigitsFoundLength; cpi++)//cpi - current paragraph index
            {
                int currentPossibleChapterNumber = allParagraphsWithDigitsFound[cpi];//достаем припрятанные номера возможно глав                

                if (currentPossibleChapterNumber > 0)//если на этом индексе был найден какой-то номер, достаем полный текст абзаца и проверяем первые FindChapterNameSpace символов на наличие ключевых слов - всех по очереди
                {
                    string currentParagraph = _bookData.GetParagraphText(desiredTextLanguage, cpi);
                    int currentParagraphLength = currentParagraph.Length;

                    if (findChapterNameSpace > currentParagraphLength)//если абзац короче длины абзаца, берем его длину - могут быть названия глав просто из двух цифр
                    {
                        findChapterNameSpace = currentParagraphLength - startIndexOf;//стартовый индекс для длины вычитаем всегда автоматически (тут он всегда нулевой, но мало ли)
                    }

                    for (int k = 0; k < totalBaseFormsOfNamesSamplesLength; k++)//k = i + m*iMax
                    {
                        string keyWordForm = totalBaseFormsOfNamesSamples[k];
                        int keyWordFormIndex = currentParagraph.IndexOf(keyWordForm, startIndexOf, findChapterNameSpace);

                        if (keyWordFormIndex >= 0)
                        {
                            int result = _bookData.SetNotices(desiredTextLanguage, cpi, k, keyWordForm);//сохранили все номера глав и ключевые слова в служебный массив заметок - для передачи дальше
                            kMultiple[k]++;                            
                        }
                    }
                }
            }
            return kMultiple;//тут пора задуматься о том, что главы могут быть и без ключевых слов - найти или сделать подходящую книгу и проверить такой вариант
        }

        public string[] CreateBaseFormsOfNamesSamples(int desiredTextLanguage)
        {
            int namesSamplesLength = DConst.chapterNamesSamples.Length / 2;//получили длину массива ключевых слов названий глав в зависимости от требуемого языка
            int baseKeyWordFormsCount = DConst.BaseKeyWordFormsCount;
            int totalBaseFormsOfNamesSamplesLength = namesSamplesLength * baseKeyWordFormsCount;
            string[] totalBaseFormsOfNamesSamples = new string[totalBaseFormsOfNamesSamplesLength];//массив, в котором временно сохраним все варианты ключевых слов во всех формах - чтобы не возиться с двумерностями            

            TransduceWord[] TransduceKeyWord = new TransduceWord[baseKeyWordFormsCount];
            TransduceKeyWord[0] = TransduceKeyWordToTitleCase;
            TransduceKeyWord[1] = TransduceKeyWordToUpper;
            TransduceKeyWord[2] = TransduceKeyWordToLower;

            for (int i = 0; i < namesSamplesLength; i++)//выборка базовых ключевых слов (разные слова из строчных букв)
            {
                string keyWordBaseForm = DConst.chapterNamesSamples[desiredTextLanguage, i];

                for (int m = 0; m < baseKeyWordFormsCount; m++)//выборка различных форм написания ключевых слов (первая прописная, все прописные, все строчные)
                {
                    string keyWordForm = TransduceKeyWord[m](keyWordBaseForm);
                    int k = m + i * baseKeyWordFormsCount;
                    totalBaseFormsOfNamesSamples[k] = keyWordForm;                    
                }
            }
            return totalBaseFormsOfNamesSamples;
        }

        public int[] IsChaptersNumbersIncreased(int[] allParagraphsWithDigitsFound)//за начальный номер жестко принимали нулевой номер главы, из-за этого получается, что зависит от того, какой будет реальный первый номер главы - а это неправильно
        {
            //выкинуть нули и лишние цифры из номеров глав, проверить, что остальные цифры идут по порядку, посчитать количество номеров глав
            int allParagraphsWithDigitsFoundLength = allParagraphsWithDigitsFound.Length;// = paragraphTextLength            
            int[] workChapterNameIsDigitsOnly = new int[allParagraphsWithDigitsFoundLength];
            int increasedChapterNumbers = 1;//нет, не пофиг - нули нам не нужны, а нулевую превратили в 1, если что (пофиг на нулевую главу (сознательно пропускаем нулевую главу - попросим пользователя проверить, что ее нет)
            int workIndex = 0;
            bool increasedNumderFound;//все равно чему равно, просто объявление

            do
            {
                increasedNumderFound = false;
                for (int i = 0; i < allParagraphsWithDigitsFoundLength; i++)
                {
                    if (allParagraphsWithDigitsFound[i] == increasedChapterNumbers)//найдем в массиве возрастающий ряд чисел (+1 от предыдущей)
                    {
                        workChapterNameIsDigitsOnly[workIndex] = increasedChapterNumbers;
                        increasedNumderFound = true;
                    }
                }
                increasedChapterNumbers++;
                workIndex++;
            }
            while (increasedNumderFound);

            workIndex -= 1;//на выходе к workIndex еще прибавили единицу, поэтому это была бы длина массива, а не последний индекс
            int[] chapterNameIsDigitsOnly = workChapterNameIsDigitsOnly.Take(workIndex).ToArray();//метод Take(workIndex) извлекает workIndex элементов из массива - выбрали в новый массив только монотонно возрастающие числа
            int chapterNameIsDigitsOnlyLength = chapterNameIsDigitsOnly.Length;
            int lastValueIndex = chapterNameIsDigitsOnlyLength - 1;

            return chapterNameIsDigitsOnly;//-1 еще не делали - чтобы получить реальные номера глав
        }

        public int[] CollectAllParagraphsWithDigitsFound(int desiredTextLanguage)//метод 1 - фактически вместо FirstTenGroupsChecked - собираем индексы строк, в начале которых нашлись цифры
        {            
            int paragraphTextLength = _bookData.GetParagraphTextLength(desiredTextLanguage); // это вместо _bookData.GetParagraphTextLength(desiredTextLanguage);
            int[] allParagraphsWithDigitsFound = new int[paragraphTextLength];

            for (int cpi = 0; cpi < paragraphTextLength; cpi++)//cpi - current paragraph index
            {
                string currentParagraph = _bookData.GetParagraphText(desiredTextLanguage, cpi);//_bookData.GetParagraphText(i, desiredTextLanguage);                
                int startIndexOfAny = 0;//сбросили начало поиска на начало строки для следующего абзаца
                int findChapterNameSpace = DConst.FindChapterNameSpace;//чтобы не рыться в большом абзаце, проверяем только первые сколько-то символов (количество установить в переменной в AnalysisLogicDataArrays)
                int currentParagraphLength = currentParagraph.Length;
                
                if (currentParagraphLength > 0)//пустые абзацы, понятное дело, пропускаем
                {
                    string digitsParcel = "";
                    
                    if (findChapterNameSpace > currentParagraphLength)//если абзац короче длины абзаца, берем его длину - могут быть названия глав просто из двух цифр
                    {
                        findChapterNameSpace = currentParagraphLength - startIndexOfAny;//стартовый индекс для длины вычитаем всегда автоматически (тут он всегда нулевой, но мало ли)
                    }                  
                    bool digitFoundForSure = false;//digitFoundIndex >= 0;
                    
                    do //если нашли цифру, проверяем следующие за ней символы в строке, пока не кончатся цифры
                    {
                        int digitFoundIndex = currentParagraph.IndexOfAny(DConst.AllDigits, startIndexOfAny, findChapterNameSpace);//подходяще будет IndexOfAny(Char[], Int32, Int32) - Возвращает индекс с отсчетом от нуля первого обнаруженного в данном экземпляре символа из указанного массива символов Юникода. Поиск начинается с указанной позиции знака; проверяется заданное количество позиций

                        digitFoundForSure = digitFoundIndex >= 0;//таки нашли цифру, если не отрицательная - переменная digitFoundIndex несет полезный индекс найденной цифры в строке
                        if (digitFoundForSure)
                        {
                            digitsParcel += currentParagraph[digitFoundIndex].ToString();//достаем символы по индексу первой найденной цифры в строке и собираем все найденные цифры в кучку, прибавляя индекс, пока не станет отрицательным
                            startIndexOfAny = digitFoundIndex + 1;//если нашли первую цифру (вообще попали сюда), то установили начало поиска на следующий символ после найденной цифры, а длину поиска - в один символ
                            findChapterNameSpace = 1;//сейчас ищем только на один символ вглубь - собираем группу цифр, идущих подряд
                            
                            if ((startIndexOfAny + findChapterNameSpace) > currentParagraphLength)//после начала движения вглубь текста опять проверить, что не выходим за границу абзаца
                            {
                                digitFoundForSure = false;//если абзац кончился, то прекрацаем цикл поиска
                            }
                            _msgService.ShowTrace(MethodBase.GetCurrentMethod().ToString(), "currentParagraph[" + cpi.ToString() + "] = " + currentParagraph + DConst.StrCRLF +
                                "currentParagraph[found symbol] = " + currentParagraph[digitFoundIndex] + DConst.StrCRLF +
                                "digitFoundForSure = " + digitFoundForSure.ToString() + DConst.StrCRLF +
                                "digitFoundIndex = " + digitFoundIndex.ToString() + DConst.StrCRLF +
                                "startIndexOfAny = " + startIndexOfAny.ToString() + DConst.StrCRLF +
                                "findChapterNameSpace = " + findChapterNameSpace.ToString() + DConst.StrCRLF +
                                "currentParagraphLength = " + currentParagraphLength.ToString(), CurrentClassName, DConst.ShowMessagesLevel);
                        }
                    }
                    while (digitFoundForSure);

                        //преобразуем кучку найденных цифр в число int и сохраняем в allParagraphsWithDigitsFound - (или сначала делаем выборку из строки по найденным индексам и проверяем bool Int32.TryParse(string, out number)?)                            
                        bool digitFoundDefinitely = Int32.TryParse(digitsParcel, out int chapterNumberBidder);

                    if (digitFoundDefinitely)//кучка цифр успешно преобразовалась в число и можно сложить его в массив найденных в началах абзацев чисел
                    {
                        allParagraphsWithDigitsFound[cpi] = chapterNumberBidder;//тут не надо прибавлять 1 - портим все номера (было +1 чтобы убрать вариант нулевого номера главы - оказывается, существуют и такие (при сжатии массива номеров вычтем 1)

                        _msgService.ShowTrace(MethodBase.GetCurrentMethod().ToString(), "currentParagraph[" + cpi.ToString() + "] = " + currentParagraph + DConst.StrCRLF +
                                "chapterNumberBidder = " + chapterNumberBidder.ToString(), CurrentClassName, DConst.ShowMessagesLevel);
                    }                    
                }
            }
            return allParagraphsWithDigitsFound;//таким образом на выходе мы получим полный перечень встречающихся в тексте цифр в начале абзаца
        }

        public void QuickSortOfAllDigits(int[] allParagraphsWithDigitsFound, int startAllDigits, int endAllDigits)//прикольный метод, но так ине понадобился, сохраним на память
        {
            //MessageBox.Show("start = " + start.ToString() + "\r\n" + "end = " + end.ToString(), "ChapterNameDigitQuickSort started", MessageBoxButtons.OK, MessageBoxIcon.Information);

            if (startAllDigits >= endAllDigits)
            {
                return;
            }
            int pivot = PivotOfAllDigits(allParagraphsWithDigitsFound, startAllDigits, endAllDigits);
            QuickSortOfAllDigits(allParagraphsWithDigitsFound, startAllDigits, pivot - 1);
            QuickSortOfAllDigits(allParagraphsWithDigitsFound, pivot + 1, endAllDigits);
        }

        public int PivotOfAllDigits(int[] allParagraphsWithDigitsFound, int startAllDigits, int endAllDigits)//сначала находим наименьшее число в массиве чисел
        {
            int temp;//swap helper
            int marker = startAllDigits;
            //MessageBox.Show("start = " + start.ToString() + "\r\n" + "end = " + end.ToString(), "ChapterNameDigitQuickSort started", MessageBoxButtons.OK, MessageBoxIcon.Information);
            for (int i = startAllDigits; i <= endAllDigits; i++)
            {
                if (allParagraphsWithDigitsFound[i] < allParagraphsWithDigitsFound[endAllDigits])//chapterNameIsDigitsOnly[end] is pivot
                {
                    temp = allParagraphsWithDigitsFound[marker];//swap
                    allParagraphsWithDigitsFound[marker] = allParagraphsWithDigitsFound[i];
                    allParagraphsWithDigitsFound[i] = temp;
                    marker += 1;
                }
            }
            //put pivot (allParagraphsWithDigitsFound[endIndex]) between left anf right subarrays
            temp = allParagraphsWithDigitsFound[marker];
            allParagraphsWithDigitsFound[marker] = allParagraphsWithDigitsFound[endAllDigits];
            allParagraphsWithDigitsFound[endAllDigits] = temp;
            return marker;
        }

        private string TransduceKeyWordToTitleCase(string baseKeyWord)//преобразуем ключевое слово в вариант с первой прописной буквой
        {
            //проверять baseKeyWord чтобы не null и останавливать с диагностикой
            TextInfo ti = CultureInfo.CurrentCulture.TextInfo;
            string transduceWord = ti.ToTitleCase(baseKeyWord);
            return transduceWord;
        }

        private string TransduceKeyWordToUpper(string baseKeyWord)//преобразуем ключевое слово в вариант из всех прописных букв
        {
            string transduceWord = baseKeyWord.ToUpper();
            return transduceWord;
        }

        private string TransduceKeyWordToLower(string baseKeyWord)//ничего не делаем, отдаем ключевое слово из строчных (сохраним на случай изменения базовых ключевых слов)
        {
            return baseKeyWord;
        }        
        
        public static string CurrentClassName
        {
            get { return MethodBase.GetCurrentMethod().DeclaringType.Name; }
        }


        
    }
}

//int GetIntContent(int desiredTextLanguage, string needOperationName) => _bookData.GetIntContent(desiredTextLanguage, needOperationName);//перегрузка для получения длины двуязычных динамических массивов
//int GetIntContent(string needOperationName, string stringToSet, int indexCount) => _bookData.GetIntContent(needOperationName, stringToSet, indexCount);//перегрузка для записи обычных массивов
//int GetIntContent(int desiredTextLanguage, string needOperationName, string stringToSet, int indexCount) => _bookData.GetIntContent(desiredTextLanguage, needOperationName, stringToSet, indexCount);
//string GetStringContent(string nameOfStringNeed, int indexCount) => _bookData.GetStringContent(nameOfStringNeed, indexCount);
//string GetStringContent(int desiredTextLanguage, string nameOfStringNeed, int indexCount) => _bookData.GetStringContent(desiredTextLanguage, nameOfStringNeed, indexCount);
//string AddSome00ToIntNumber(string currentChapterNumberToFind, int chapterTotalDigits) =>_analysisLogic.AddSome00ToIntNumber(currentChapterNumberToFind, chapterTotalDigits);        
//достанем метки начала и конца номера главы
//string[] allChapterMarks = GetStringArrConstant("ChapterMark");
//string beginChapterMark = allChapterMarks[0];
//int endChapterMarkIndex = GetIntConstant("ChapterMark") - 1;
//string endChapterMark = allChapterMarks[endChapterMarkIndex];
//int chapterTotalDigits = GetIntConstant("ChapterTotalDigits");//получим количество символов-цифр для номера главы в марке
//кстати, надо брать предисловие и ставить туда нулевую главу
//int baseKeyWordFormsQuantity = GetBaseKeyWordFormsQuantity();
//для этого все найденные цифры заносим во временный массив длиной в количество абзацев - с индексом, совпадающим с индексом абзаца в главном массиве (пусть будет все тот же allParagraphsWithDigitsFound)
//получить строку всех цифр из AnalysisLogicDataArrays
//для поиска цифр сначала применяем String.IndexOfAny с массивом char из всех символов цифр - получаем строку "AllDigits" и разбираем ее в char[]
//string[] allDigitsString = GetStringArrConstant("AllDigitsIn0");
//char[] allDigits = allDigitsString[0].ToCharArray();
//private string[] GetStringArrConstant0()
//{
//    string[] NamesSamples0 = GetStringArrConstant("NamesSamples0");
//    return NamesSamples0;
//}
//private string[] GetStringArrConstant1()
//{
//    string[] NamesSamples1 = GetStringArrConstant("NamesSamples1");
//    return NamesSamples1;
//}
//тут вызвать метод string CreatePartTextMarks(string stringMarkBegin, string stringMarkEnd, int currentUpperNumber, int enumerateCurrentCount, string sentenceTextMarksWithOtherNumbers)
//писать маркировку с номерами прямо в абзац названия главы - добавить строку перед названием
//public void FirstTenGroupsChecked(string currentParagraph, int[] allDigitsInParagraphs, int iParagraphNumber, int desiredTextLanguage)//придумать внятное название
//{//начало анализа строки (абзаца)            
//    int firstNumberOfParagraph = 0;
//    int keyWordChapterName = 0;
//    int foundNumberInParagraph = 0;
//    int foundWordsOfParagraphCount = WordsOfParagraphSearch(currentParagraph);//выделяем в текущем параграфе первые 10 групп (сохраняются в массиве-методе), получаем количество слов-чисел + лидирующая группа символов
//    for (int j = 0; j < foundWordsOfParagraphCount; j++)//перебираем все полученные слова - ищем сначала числа и затем ключевые слова (на самом деле одновременно)
//    {//если есть число заносим в его массив int номеров глав, потом разберемся, что там, а всю строку - в массив названий глав, если нашлось ключевое слово - прибавляем счетчик ключевых слов
//        string currentWordOfParagraph = GetFoundWordsOfParagraph(j); //_arrayChapter.GetFoundWordsOfParagraph(j);
//        _msgService.ShowTrace(MethodBase.GetCurrentMethod().ToString(), "currentWordOfParagraph = " + currentWordOfParagraph, CurrentClassName, DConst.ShowMessagesLevel);
//        if (currentWordOfParagraph != null)
//        {
//            bool successSearchNumber = Int32.TryParse(currentWordOfParagraph, out firstNumberOfParagraph);//проверяем найденное слово, что оно число - ищем номера глав
//            _msgService.ShowTrace(MethodBase.GetCurrentMethod().ToString(), "currentWordOfParagraph = " + currentWordOfParagraph + DConst.StrCRLF + "is digit = " + successSearchNumber.ToString(), CurrentClassName, DConst.ShowMessagesLevel);
//            if (successSearchNumber)//складываем найденные номера глав, потом проверим на монотонное возрастание и выкинем лишние
//            {
//                allDigitsInParagraphs[iParagraphNumber] = firstNumberOfParagraph + 1; //(+1 - чтобы избавиться от нулевой главы, бывают и такие, потом не забыть отнять)
//                foundNumberInParagraph = iParagraphNumber;//найден номер в абзаце, надо записать индекс абзаца в массив 
//                int chaptersNamesWithNumbersLength = AddChapterName(currentParagraph, desiredTextLanguage);//заносим всю строку в массив названий глав
//                _msgService.ShowTrace(MethodBase.GetCurrentMethod().ToString(),
//                "currentParagraph = " + currentParagraph + DConst.StrCRLF +
//                "Names Length = " + chaptersNamesWithNumbersLength.ToString() + DConst.StrCRLF +
//                "iParagraphNumber = " + iParagraphNumber.ToString() + "    and j = " + j.ToString() + DConst.StrCRLF +
//                "firstNumberOfParagraph = " + firstNumberOfParagraph.ToString(), CurrentClassName, DConst.ShowMessagesLevel);
//            }
//            else
//            {//количество найденных ключевых слов сохраняем в массиве _arrayChapter.IncrementOfChapterNamesVersionsCount(n)
//                keyWordChapterName = CheckWordOfParagraphCompare(currentWordOfParagraph, j, desiredTextLanguage);//проверяем текущее слово на совпадение с ключевыми                        
//            }//индекс найденного слова совпадает с индексом наибольшего значения в массиве и должен совпадать по значению с количеством найденных глав - проверка
//        }
//    }            
//    int chaptersNamesNumbersOnlysLength = AddChapterNumber(foundNumberInParagraph, desiredTextLanguage);//сохраняем индексы массива абзацев c цифрами, остальные значения - нули            
//}
//public int CheckWordOfParagraphCompare(string currentWordOfParagraph, int jWordNumber, int desiredTextLanguage)//проверяем полученное слово на совпадение с ключевыми из массива
//{
//    int baseKeyWordFormsQuantity = GetBaseKeyWordFormsQuantity();
//    TransduceWord[] TransduceKeyWord = new TransduceWord[baseKeyWordFormsQuantity];
//    TransduceKeyWord[0] = TransduceKeyWordToTitleCase;
//    TransduceKeyWord[1] = TransduceKeyWordToUpper;
//    TransduceKeyWord[2] = TransduceKeyWordToLower;
//    int chapterNamesSamplesLength = GetChapterNamesSamplesLength(desiredTextLanguage);
//    int[,] tempArrChapterNamesSamples = new int[baseKeyWordFormsQuantity, chapterNamesSamplesLength];//перебрать 3 возможные формы заглавия глав - все строчные, все прописные, первая прописная - такое же количество методов у делегата TransduceKeyWord
//    for (int n = 0; n < chapterNamesSamplesLength; n++)//тут перебираем базовые формы ключевых слов
//    {
//        for (int m = 0; m < baseKeyWordFormsQuantity; m++)//тут перебираем варианты написания ключевых слов
//        {
//            string baseKeyWord = GetChapterNamesSamples(desiredTextLanguage, n);
//            string currentKeyWordForm = TransduceKeyWord[m](baseKeyWord);
//            bool currentWordOfParagraphCheck = currentWordOfParagraph.Contains(currentKeyWordForm);//проверяем, содержатся ли стандартные называния глав в строке - если в другое слово входит ключевое, то тоже находит - исправить
//            if (currentWordOfParagraphCheck)
//            {//надо еще проверять вариант, когда все буквы прописные (или часть?) - проще всего текущее слово перевести в строчные и ключевые сделать все из строчных
//                int incrementFactor = GetChapterNamesVersionsCount(m, n); //сюда прибавляем количество встреченных ключевых слов, в ряд, в зависимости от найденной формы ключевого слова - чтобы потом выбрать похожее по количеству с номерами (не менее количества номеров, больше можно)
//                incrementFactor++;
//                SetChapterNamesVersionsCount(m, n, incrementFactor);
//                //нашли название главы
//                _msgService.ShowTrace(MethodBase.GetCurrentMethod().ToString(), "ChapterNamesVersionsCount(n) = " + GetChapterNamesVersionsCount(m, n).ToString() + DConst.StrCRLF + 
//                    "n = " + n.ToString() + "   /m = " + n.ToString(), CurrentClassName, DConst.ShowMessagesLevel);
//                if (jWordNumber > 0)
//                {
//                    SymbolsFoundBeforeKeyWord(jWordNumber); //найденное ключевое слово не первое в строке, перед ним были какие-то символы, ищем их и сохраняем, чтобы потом было проще искать названия глав
//                }
//                return n;//возвращаем индекс найденного ключевого слова из массива
//            }
//        }
//    }
//    return (int)MethodFindResult.NothingFound;//не нашли...
//}
////старые методы из _bookData
//int GetParagraphTextLength(int desiredTextLanguage) => _bookData.GetParagraphTextLength(desiredTextLanguage);
//string GetParagraphText(int paragraphCount, int desiredTextLanguage) => _bookData.GetParagraphText(paragraphCount, desiredTextLanguage);
//int SetParagraphText(string paragraphText, int paragraphCount, int desiredTextLanguage) => _bookData.SetParagraphText(paragraphText, paragraphCount, desiredTextLanguage);
////string GetChapterName(int chapterCount, int desiredTextLanguage) => _bookData.GetChapterName(chapterCount, desiredTextLanguage);
////int GetChapterNameLength(int desiredTextLanguage) => _bookData.GetChapterNameLength(desiredTextLanguage);
//int AddChapterName(string chapterNameWithNumber, int desiredTextLanguage) => _bookData.AddChapterName(chapterNameWithNumber, desiredTextLanguage);
//int GetChapterNumber(int chapterCount, int desiredTextLanguage) => _bookData.GetChapterNumber(chapterCount, desiredTextLanguage);
//int GetChapterNumberLength(int desiredTextLanguage) => _bookData.GetChapterNumberLength(desiredTextLanguage);
//int AddChapterNumber(int chapterNumberOnly, int desiredTextLanguage) => _bookData.AddChapterNumber(chapterNumberOnly, desiredTextLanguage);
////новые методы из _arrayAnalysis
//int GetIntConstant(string needConstantnName) => _arrayAnalysis.GetIntConstant(needConstantnName);
//string[] GetStringArrConstant(string needConstantnName) => _arrayAnalysis.GetStringArrConstant(needConstantnName);
////старые методы из _arrayAnalysis
//int GetChapterNamesSamplesLength(int desiredTextLanguage) => _arrayAnalysis.GetChapterNamesSamplesLength(desiredTextLanguage);
//string GetChapterNamesSamples(int desiredTextLanguage, int i) => _arrayAnalysis.GetChapterNamesSamples(desiredTextLanguage, i);
//int GetFoundWordsOfParagraphLength() => GetFoundWordsOfParagraphLength();
//string GetFoundWordsOfParagraph(int i) => _arrayAnalysis.GetFoundWordsOfParagraph(i);
//int SetFoundWordsOfParagraph(string wordOfParagraph, int i) => _arrayAnalysis.SetFoundWordsOfParagraph(wordOfParagraph, i);
////int ClearFoundWordsOfParagraph();
//int GetFoundSymbolsOfParagraphLength() => _arrayAnalysis.GetFoundSymbolsOfParagraphLength();
//int SetFoundSymbolsOfParagraph(string symbolsOfParagraph, int i) => _arrayAnalysis.SetFoundSymbolsOfParagraph(symbolsOfParagraph, i);
//string GetFoundSymbolsOfParagraph(int i) => _arrayAnalysis.GetFoundSymbolsOfParagraph(i);
////int ClearFoundSymbolsOfParagraph();               
//int GetBaseKeyWordFormsQuantity() => _arrayAnalysis.GetBaseKeyWordFormsQuantity();        
//int GetChapterNamesVersionsCount(int m, int i) => _arrayAnalysis.GetChapterNamesVersionsCount(m, i);
//int SetChapterNamesVersionsCount(int m, int i, int countValue) => _arrayAnalysis.SetChapterNamesVersionsCount(m, i, countValue);
//int GetChapterSymbolsVersionsCount(int i) => _arrayAnalysis.GetChapterSymbolsVersionsCount(i);
//int SetChapterSymbolsVersionsCount(int i, int countValue) => _arrayAnalysis.SetChapterSymbolsVersionsCount(i, countValue);
//public void SymbolsFoundBeforeKeyWord(int j)//передать и получить все параметры
//{//найденное ключевое слово не первое, перед ним были какие-то символы - ищем их и складываем количество встретившихся групп в массив chapterSymbolsVersionsCount
// //потом сравнить с числом найденных глав и, если совпадает, то найден постоянная группа в названии главы - но зачем она?
//    for (int m = 0; m < j; m++)//записываем группы символов до встреченного ключевого слова (текущее j)
//    {
//        if (GetFoundWordsOfParagraph(m) == GetFoundSymbolsOfParagraph(m))
//        {
//            int incrementFactor = GetChapterSymbolsVersionsCount(m); //сюда прибавляем количество встреченных ключевых слов, в ряд, в зависимости от найденной формы ключевого слова - чтобы потом выбрать похожее по количеству с номерами (не менее количества номеров, больше можно)
//            incrementFactor++;
//            SetChapterSymbolsVersionsCount(m, incrementFactor);
//            //_arrayChapter.IncrementOfChapterSymbolsVersionsCount(m);//если новое значение равно предыдущему, то увеличиваем счетчик
//        }
//        SetFoundSymbolsOfParagraph(GetFoundWordsOfParagraph(m), m);// - это что такое тут делаем?
//    //подумать - если левая группа испортит переменную, что потом будет? по идее через пару правильных групп счет восстановится                                                                      
//    //заносить все равно надо, потому что вдруг уже первая группа будет неправильная - потом она постепенно заменится правильной
//    //но вообще не очень правильно - надо все же все значения занести в массив и потом анализировать - исключения выкинуть и найти такое же количество групп, как и глав
//    //здесь надо занести в массив (из 10-ти элементов) номер слова названия главы - по порядку в строке, т.е. есть ли перед названием лишние спецсимволы и другое
//    }
//}
//public int WordsOfParagraphSearch(string currentParagraph)
//{//метод выделяет из строки (абзаца текста) первые десять (или больше - по размерности передаваемого массива) слов или чисел (и сохраняет лидирующую группу спецсимволов)
//    if (String.IsNullOrWhiteSpace(currentParagraph))
//    {
//        return (int)MethodFindResult.NothingFound;//пустая строка без слов, вернули -1 для ясности
//    }            
//    string currentParagraphWithSingleBlanks = RemoveMoreThenOneBlank(currentParagraph);//убрать сдвоенные пробелы из строки
//    currentParagraph = currentParagraphWithSingleBlanks;
//    _arrayAnalysis.ClearFoundWordsOfParagraph();//можно передать контрольное число, подтверждающее, что массив можно очистить
//    int foundWordsCount = 0;
//    IsOrNotEqual isEqual = IsEqual;//check condition is current charArrayOfChapterNumber[] LetterOrDigit
//    IsOrNotEqual isNotEqual = IsNotEqual;//check condition is NOT LetterOrDigit
//    DoElseConditions stringIsNotNull = StringIsNotNull;//check condition is current word not null
//    DoElseConditions jSubIisAboveOne = JsubIisAboveOne;//check condition is j-i > 1
//    ////разделяем абзац на слова или числа и на скопления спецсимволов (если больше одного подряд)
//    char[] charArrayOfChapterNumber = currentParagraph.ToCharArray();
//    int charArrayOfChapterNumberLength = charArrayOfChapterNumber.Length;
//    for (int i = 0; i < charArrayOfChapterNumberLength; i++)
//    {
//        if (foundWordsCount < _arrayAnalysis.GetFoundWordsOfParagraphLength())
//        {
//            if (Char.IsLetterOrDigit(charArrayOfChapterNumber[i]))
//            {//проверка, есть ли цепочка букв-цифр
//                int j = SymbolGroupSaving(charArrayOfChapterNumber, foundWordsCount, i, isEqual, stringIsNotNull, 0);//0 - не надо вычитать единицу
//                foundWordsCount++;
//                i = j;
//            }
//            else
//            {//проверка, есть ли цепочка спецсимволов
//                int j = SymbolGroupSaving(charArrayOfChapterNumber, foundWordsCount, i, isNotEqual, jSubIisAboveOne, 1);//1 - надо вычитать единицу в одном месте
//                if (j - i > 1)
//                {
//                    foundWordsCount++;//кстати - в методе тоже такое же сравнение, можно ли совместить?
//                }
//                i = j;
//            }
//        }
//    }
//    _msgService.ShowTrace(MethodBase.GetCurrentMethod().ToString(), "foundWordsCount = " + foundWordsCount.ToString() + DConst.StrCRLF +
//    "currentParagraph --> " + currentParagraph + DConst.StrCRLF +
//    "foundWordsOfParagraph[0] --> " + _arrayAnalysis.GetFoundWordsOfParagraph(0) + DConst.StrCRLF +
//    "foundWordsOfParagraph[1] --> " + _arrayAnalysis.GetFoundWordsOfParagraph(1) + DConst.StrCRLF +
//    "foundWordsOfParagraph[2] --> " + _arrayAnalysis.GetFoundWordsOfParagraph(2) + DConst.StrCRLF +
//    "foundWordsOfParagraph[3] --> " + _arrayAnalysis.GetFoundWordsOfParagraph(3) + DConst.StrCRLF +
//    "foundWordsOfParagraph[4] --> " + _arrayAnalysis.GetFoundWordsOfParagraph(4), CurrentClassName, DConst.ShowMessagesLevel);
//    return foundWordsCount;
//}
//private int SymbolGroupSaving(char[] charArrayOfChapterNumber, int foundWordsCount, int i, IsOrNotEqual currentIf, DoElseConditions currentDoElse, int doMinusOne)//убрать все массивы из параметров всех методов
//{//метод вызывается для проверки, есть ли цепочка букв-цифр или спецсимволов (используется для WordsOfParagraphSearch, тестируется вместе с ним)
//    string wordOfParagraph = "";
//    int currentCharIndex = 0;
//    int charArrayOfChapterNumberLength = charArrayOfChapterNumber.Length;
//    for (int j = i; j < charArrayOfChapterNumberLength; j++)
//    {
//        currentCharIndex = j;
//        bool resultIf = currentIf(charArrayOfChapterNumber[j]);//выбираем сравнение - символ буква-цифра и НЕ буква-цифра
//        if (resultIf)
//        {
//            wordOfParagraph = wordOfParagraph + charArrayOfChapterNumber[j];
//        }
//        else
//        {
//            bool resultElse = currentDoElse(wordOfParagraph, i, j);//выбираем сравнение - непустое слово или j-i>1 - это означает, что найдена лидирующая группа спецсимволов, которую надо сохранить, как слово
//            if (resultElse)                    
//            {
//                _arrayAnalysis.SetFoundWordsOfParagraph(wordOfParagraph, foundWordsCount);
//            }
//            return currentCharIndex - doMinusOne;//вычитаем 1 или нет - по ситуации (вычитать надо когда?)
//        }
//    }
//    bool resultIsNullOrEmpty = StringIsNotNull(wordOfParagraph, 0, 0);//в методе используется только первый аргумент, два других для совпадения вызова в делегате - ой, криво!
//    if(resultIsNullOrEmpty)            
//    {
//        _arrayAnalysis.SetFoundWordsOfParagraph(wordOfParagraph, foundWordsCount);
//    }
//    return currentCharIndex;
//}
//private bool IsEqual(char x)
//{
//    bool result = Char.IsLetterOrDigit(x);
//    return result;                
//}
//private bool IsNotEqual(char x)
//{
//    bool result = !Char.IsLetterOrDigit(x);
//    return result;
//}
//private bool StringIsNotNull(string wordOfParagraph, int j, int i)
//{
//    bool result = !string.IsNullOrEmpty(wordOfParagraph);
//    return result;
//}
//private bool JsubIisAboveOne(string wordOfParagraph, int j, int i)
//{
//    bool result = (j - i > 1);
//    return result;
//}
//public string RemoveMoreThenOneBlank(string currentParagraph)
//{
//    string currentParagraphWithSingleBlanks = "";
//    char previousCharOfCurrentParagraph = '¶';
//    int resultSymbols = 0;
//    int sourceSymbols = 0;
//    foreach (char charOfcurrentParagraph in currentParagraph)
//    {
//        if (charOfcurrentParagraph == ' ' && previousCharOfCurrentParagraph == ' ')
//        { }
//        else
//        {
//            currentParagraphWithSingleBlanks = currentParagraphWithSingleBlanks + charOfcurrentParagraph;
//            resultSymbols++;
//        }
//        previousCharOfCurrentParagraph = charOfcurrentParagraph;
//        //singleBlankSpaceDetected = true;
//        sourceSymbols++;
//    }
//    _msgService.ShowTrace(MethodBase.GetCurrentMethod().ToString(), 
//        "foreach sourceSymbols = " + sourceSymbols.ToString() + DConst.StrCRLF +
//        "resultSymbols = " + resultSymbols.ToString() + DConst.StrCRLF +
//        "Source string --> " + currentParagraph + DConst.StrCRLF +
//        "Result string --> " + currentParagraphWithSingleBlanks, CurrentClassName, DConst.ShowMessagesLevel);
//    return currentParagraphWithSingleBlanks;
//}
//public string FindDigitsInChapterName(string paragraphWihtChapterName)
//{
//    //в цикле проверяем символ на цифру, если да, то записываем в строку и ставим флаг, что цифра
//    //если следующий символ тоже цифра, то поддерживаем флаг и записываем в строку, если нет - ?
//    //string digitsOfChapterNumber = FindDigitsInChapterName(currentParagraph);//получили строку с цифрами в названии главы
//    string digitsOfChapterNumber = "";
//    char[] isStringChapterNumber = paragraphWihtChapterName.ToCharArray();
//    int digitAtChapterNameCount = 0;
//    bool isDigitAtChapterName = false;
//    foreach (char charOfChapterNumber in isStringChapterNumber)
//    {
//        isDigitAtChapterName = Char.IsDigit(charOfChapterNumber);
//        if (isDigitAtChapterName)
//        {
//            digitsOfChapterNumber = digitsOfChapterNumber + charOfChapterNumber.ToString();
//            digitAtChapterNameCount++;
//        }
//        else
//        {
//            if (digitAtChapterNameCount != 0) return digitsOfChapterNumber;
//        }
//    }
//    if (digitAtChapterNameCount != 0) return digitsOfChapterNumber;
//    else return null;
//}
//public int ChapterKeyNamesAnalysis(int desiredTextLanguage)
//{
//    int maxKeyNameLength = 0;
//    int previousKeyNameLength = 0;
//    for (int n = 0; n < _arrayAnalysis.GetChapterNamesSamplesLength(desiredTextLanguage); n++)
//    {
//        string currentKeyName = _arrayAnalysis.GetChapterNamesSamples(desiredTextLanguage, n);
//        int currentKeyNameLength = currentKeyName.Length;
//        if (n > 0 && currentKeyNameLength > previousKeyNameLength)
//        {
//            maxKeyNameLength = currentKeyNameLength;
//        }
//        previousKeyNameLength = currentKeyNameLength;
//    }
//    return maxKeyNameLength;
//}
//public int UserSelectedChapterNameAnalysis(int desiredTextLanguage)//метод, когда не получился анализ глав со стандартными названиями, тогда просим подсказки у пользователя
//{//пока не используется, потом придется переписать 
//    string userSelectedText = _bookData.GetSelectedText(desiredTextLanguage);//получили припрятанный фрагмент, выбранный пользователем - предположительно вид нумерации глав
//    _msgService.ShowTrace(MethodBase.GetCurrentMethod().ToString(), "In the TextBox " + desiredTextLanguage.ToString() + DConst.StrCRLF +
//       "User Selected the following Text ==> " + userSelectedText, CurrentClassName, DConst.ShowMessagesLevel);
//    int lengthUserSelectedText = userSelectedText.Length;//считаем, что длина больше нуля, иначе не попали бы сюда
//    //сначала ищем полученный фрагмент в тексте, чтобы добыть и проверить предыдущие (и последующие) строки - т.е. абзацы
//    int findChapterResult = 0; //FindParagraphTextNumber(userSelectedText, desiredTextLanguage, 0);//в результате получим номер элемента, в котором нашелся фрагмент
//    if (findChapterResult > 0)//в то, что название главы в первой строке книги, мы не верим
//    {
//        string previousParagraphChapterName = _bookData.GetParagraphText(findChapterResult - 1, desiredTextLanguage);
//        string paragraphWihtChapterName = _bookData.GetParagraphText(findChapterResult, desiredTextLanguage);
//        //string nextParagraphChapterName = _book.GetParagraphText(findChapterResult + 1, desiredTextLanguage);
//        _msgService.ShowTrace(MethodBase.GetCurrentMethod().ToString(), "findChapterResult = " + findChapterResult.ToString() + DConst.StrCRLF +
//            "previousParagraphChapterName - " + previousParagraphChapterName + DConst.StrCRLF +
//            "paragraphWihtChapterName - " + paragraphWihtChapterName + DConst.StrCRLF +
//            "nextParagraphChapterName - " + "nextParagraphChapterName", CurrentClassName, DConst.ShowMessagesLevel);
//        //тут надо убедиться, что предыдущая (и желательно последующая) строки - пустые, если последующая не пустая - запросить доп.подтверждение у пользователя
//        //а пока проверим, совпадает ли фрагмент с найденной строкой и есть ли в нем цифры
//        if (userSelectedText == paragraphWihtChapterName)
//        {
//            string digitsOfChapterNumber = FindDigitsInChapterName(paragraphWihtChapterName);//получили строку с цифрами в названии главы
//            if (digitsOfChapterNumber.Length > 0)//если хоть одна цифра нашлась, выделяем названия глав без цифр (типа - Глава )
//            {
//                char[] DigitsOfChapterNumber = digitsOfChapterNumber.ToCharArray();
//                string chapterNameWithoutDigits = paragraphWihtChapterName.Trim(DigitsOfChapterNumber);
//                //проверить все строки (абзацы) текста - есть ли там еще chapterNameWithoutDigits с увеличивающимися номерами
//                int paragraphTextLength = _bookData.GetParagraphTextLength(desiredTextLanguage);
//                int startParagraphTextNumber = 0;
//                int chaptersMaxNumber = 0;
//                int AddChapterNumberLength = 0;
//                for (int i = 0; i < paragraphTextLength; i++)
//                {
//                    int findChapterNameWithoutDigitsResult = 0;//FindParagraphTextNumber(chapterNameWithoutDigits, desiredTextLanguage, startParagraphTextNumber);//в результате получим номера элементов, в которых нашлась "Глава"
//                    if (findChapterNameWithoutDigitsResult > 0)
//                    {
//                        //тут массив названий из book, заполняемый полными названиями глав (с номерами)                            
//                        string foundChapterName = _bookData.GetParagraphText(findChapterNameWithoutDigitsResult, desiredTextLanguage);//достаем текст абзаца, номер которого получили от FindParagraphTextNumber - там имя главы
//                        int AddChapterNameLength = _bookData.AddChapterName(foundChapterName, desiredTextLanguage);//полное имя главы в массив имен глав
//                        int chapterNumber = Convert.ToInt32(FindDigitsInChapterName(foundChapterName));//получили строку с цифрами в названии главы
//                        AddChapterNumberLength = _bookData.AddChapterNumber(chapterNumber, desiredTextLanguage);
//                        if (findChapterNameWithoutDigitsResult < paragraphTextLength) startParagraphTextNumber = findChapterNameWithoutDigitsResult + 1;//на следующем круге начинаем искать после предыдущего нахождения
//                                                                                                                                                        //после выхода взять массив названий и проанализировать его на возрастание номеров глав
//                        _msgService.ShowTrace(MethodBase.GetCurrentMethod().ToString(), "i = " + i.ToString() + "and paragraphTextLength = " + paragraphTextLength.ToString() + DConst.StrCRLF +
//                            "startParagraphTextNumber = " + startParagraphTextNumber.ToString() + DConst.StrCRLF +
//                            "foundChapterName - " + foundChapterName + DConst.StrCRLF +
//                            "chapterNumber - " + chapterNumber.ToString(), CurrentClassName, 3);
//                        chaptersMaxNumber++; // счетчик найденных глав, чтобы проверить идут ли номера глав с последовательным возрастанием - сделать по другому - если главы с 0 или с 1
//                    }
//                }
//                if ((chaptersMaxNumber - AddChapterNumberLength) >= 0)//нашлось хоть одно имя главы
//                {
//                    return chaptersMaxNumber;
//                }
//            }
//        }
//    }
//    return 0;
//}
//int paragraphTextLength = GetParagraphTextLength(desiredTextLanguage); // это вместо _bookData.GetParagraphTextLength(desiredTextLanguage);
//int baseKeyWordFormsQuantity = GetBaseKeyWordFormsQuantity();
//int paragraphTextLength = GetParagraphTextLength(desiredTextLanguage);
//int chapterNameIsDigitsOnlyLength = chapterNameIsDigitsOnly.Length;
//int previousChapterNameIsDigitsOnly = chapterNameIsDigitsOnly[0] - 1;//получить первый (предположительно? - проверить на разных текстах) первый номер главы и отнять 1 для запуска цикла
//string[] allTextWithChapterNames = new string[paragraphTextLength];
//int totalDigitsQuantity = 3; //для номера главы используем 3 цифры (до 99, должно хватить) - перенести в AnalysisLogicDataArrays
//List<string> workParagraphText = new List<string>();
//_msgService.ShowTrace(MethodBase.GetCurrentMethod().ToString(), "allParagraphsWithDigitsFound[" + currentIndex.ToString() + "] = " + allParagraphsWithDigitsFound[currentIndex].ToString(), CurrentClassName, 3);
//_msgService.ShowTrace(MethodBase.GetCurrentMethod().ToString(), "WHILE digitsParcel - " + digitsParcel, CurrentClassName, 3);
//if (digitFoundForSure)
//{
//digitsParcel = currentParagraph[digitFoundIndex].ToString();
//if(startIndexOfAny > (currentParagraphLength - 3))
//{
//    digitFoundForSure = false;
//}
//int chapterNumberLength = GetChapterNumberLength(desiredTextLanguage);
//_msgService.ShowTrace(MethodBase.GetCurrentMethod().ToString(), "CurrentParagraph with i = " + i.ToString() + "  -- > " + currentParagraph + DConst.StrCRLF +
//    "paragraphTextLength = " + paragraphTextLength.ToString() + DConst.StrCRLF +
//    "chapterNumberLength = " + chapterNumberLength.ToString(), CurrentClassName, DConst.ShowMessagesLevel);
//int foundNumberInChapter = GetChapterNumber(i, desiredTextLanguage);
//if (foundNumberInChapter > 0)
//{
//    if (currentParagraph.Contains(keyWordFoundForm))
//    {
//        int correctedChapterNameIsDigitsOnly = allParagraphsWithDigitsFound[i] - 1;
//        int savedChapterNameIsDigitsOnly = chapterNameIsDigitsOnly[currentNumberIndex] - 1;
//        if (correctedChapterNameIsDigitsOnly != savedChapterNameIsDigitsOnly)
//        {
//            _msgService.ShowTrace(MethodBase.GetCurrentMethod().ToString(), "CurrentParagraph with i = " + i.ToString() + "  -- > " + currentParagraph + DConst.StrCRLF +
//                "correctedChapterNameIsDigitsOnly = " + correctedChapterNameIsDigitsOnly.ToString() + DConst.StrCRLF +
//                " savedChapterNameIsDigitsOnly = " + savedChapterNameIsDigitsOnly.ToString() + DConst.StrCRLF +
//                " (currentNumberIndex = " + currentNumberIndex + ")" + DConst.StrCRLF +
//                "foundNumberInParagraph [" + i.ToString() + "] --> " + foundNumberInChapter.ToString(), CurrentClassName, DConst.ShowMessagesLevel);
//        }
//        string currentChapterNumberToFind = correctedChapterNameIsDigitsOnly.ToString();
//        if (correctedChapterNameIsDigitsOnly == savedChapterNameIsDigitsOnly) currentNumberIndex++;
//        _msgService.ShowTrace(MethodBase.GetCurrentMethod().ToString(), "CurrentParagraph with i = " + i.ToString() + "  -- > " + currentParagraph + DConst.StrCRLF +
//                "correctedChapterNameIsDigitsOnly = " + correctedChapterNameIsDigitsOnly.ToString() + DConst.StrCRLF +
//                " == previousChapterNameIsDigitsOnly = " + previousChapterNameIsDigitsOnly.ToString() + DConst.StrCRLF +
//                " (" + currentChapterNumberToFind + ")" + DConst.StrCRLF +
//                "foundNumberInParagraph [" + i.ToString() + "] --> " + foundNumberInChapter.ToString(), CurrentClassName, DConst.ShowMessagesLevel);
//        if (currentParagraph.Contains(currentChapterNumberToFind))
//        for (int m = 0; m < baseKeyWordFormsCount; m++)
//        {
//            int currentChapterNamesVersionsCount = GetChapterNamesVersionsCount(m, i);
//            if (currentChapterNamesVersionsCount > keyWordCount)//keyWordCount - уже найденное самое большое, currentChapterNamesVersionsCount - текущий претендент на самое большое
//            {
//                keyWordCount = currentChapterNamesVersionsCount;
//                keyWordIndex = i;
//                keyWordForm = m;
//                _msgService.ShowTrace(MethodBase.GetCurrentMethod().ToString(), "keyWordCount = " + keyWordCount.ToString() + DConst.StrCRLF +
//                    "currentChapterNamesVersionsCount = " + currentChapterNamesVersionsCount.ToString() + DConst.StrCRLF +
//                    "keyWordIndex = " + keyWordIndex.ToString() + DConst.StrCRLF +
//                    "keyWordForm = " + keyWordForm.ToString(), CurrentClassName, DConst.ShowMessagesLevel);
//            }
//        }
//    }
//    //string keyWordBaseForm = GetChapterNamesSamples(desiredTextLanguage, keyWordIndex);//получаем базовое ключевое слово - строчное
//    _msgService.ShowTrace(MethodBase.GetCurrentMethod().ToString(), "keyWordIndex = " + keyWordIndex.ToString() + DConst.StrCRLF +
//        "keyWordForm = " + keyWordForm.ToString() + DConst.StrCRLF +
//        "keyWordBaseForm = " + keyWordBaseForm, CurrentClassName, DConst.ShowMessagesLevel);
//    //string keyWordFoundForm = TransduceKeyWord[keyWordForm](keyWordBaseForm);
//int keyWordCount = GetChapterNamesVersionsCount(keyWordForm, keyWordIndex);
//int getChapterNamesSamplesLength = GetChapterNamesSamplesLength(desiredTextLanguage);
//allParagraphsWithDigitsFound[i] -= 1;//cделали -1 - чтобы получить реальные номера глав (в приницпе этот массив вряд ли еще понадобится)
//int[] allParagraphsWithDigitsFound = new int[paragraphTextLength];
//int maxKeyNameLength = ChapterKeyNamesAnalysis(desiredTextLanguage);//ищем слово с макс.символов из ключевых (непонятно, зачем это нужно, но пока пусть будет)
//for (int i = 0; i < paragraphTextLength; i++)//перебираем все абзацы текста
//{
//    string currentParagraph = GetParagraphText(i, desiredTextLanguage);//_bookData.GetParagraphText(i, desiredTextLanguage);
//    FirstTenGroupsChecked(currentParagraph, allParagraphsWithDigitsFound, i, desiredTextLanguage);//объявить allParagraphsWithDigitsFound внутри и его вернуть!!!
//}            
//int[] chapterNameIsDigitsOnly = IsChapterNumbersIncreased(allParagraphsWithDigitsFound, desiredTextLanguage);//выделяем монотонно возрастающие номера глав и считаем количество
//int end = chapterNameIsDigitsOnlyLength - 1;
//ChapterNameDigitQuickSort(chapterNameIsDigitsOnly, start, end);
//for (int i = start; i <= end; i++)
//{
//
//_msgService.ShowTrace(MethodBase.GetCurrentMethod().ToString(), "chapterNameIsDigitsOnly[" + i.ToString() + "] = " + chapterNameIsDigitsOnly[i].ToString(), CurrentClassName, 3);
//}
//записать значения во временный массив, а потом занести их в начало исходного массива, его остальной обнулить (есть же отличный List в Data)
//или вариант - вернуть заполненный массив нужной длины - еще лучше
//if (chapterNameIsDigitsOnly[i] != 0)
//{
//    workChapterNameIsDigitsOnly[increasedChapterNumbers] = chapterNameIsDigitsOnly[i] - 1;//1
//    if ((minChapterNameDigit) == (workChapterNameIsDigitsOnly[increasedChapterNumbers] - 1))
//    {
//        minChapterNameDigit = workChapterNameIsDigitsOnly[increasedChapterNumbers];
//        increasedChapterNumbers++;
//    }
//}
//
//_msgService.ShowTrace(MethodBase.GetCurrentMethod().ToString(), "BEFORE IF currentParagraph = " + currentParagraph + "   i = " + i.ToString(), CurrentClassName, 3);
//if (!String.IsNullOrWhiteSpace(currentParagraph))//если текущая строка не пустая, получаем и смотрим предыдущий абзац
//{
//    string previousParagraph = GetParagraphText((i - 1), desiredTextLanguage);
//    //если предыдущая пустая, то начинаем анализ строки (абзаца), выбираем первые 10 групп символов (по пробелу) и ищем в них название (ключевые слова) и номера глав
//    if (String.IsNullOrWhiteSpace(previousParagraph))//метод вызывается не на каждый абзац, а в нем массив для каждого абзаца!
//    {
//        //_msgService.ShowTrace(MethodBase.GetCurrentMethod().ToString(), "IN IF currentParagraph = " + currentParagraph + "   i = " + i.ToString(), CurrentClassName, 3);
//
//int SymbolGroupSaving1(char[] charArrayOfChapterNumber, string[] foundWordsOfParagraph, int foundWordsCount, int i, IsOrNotEqual currentIf, DoElseCircumstance currentDoElse, int doMinusOne)
//{
//    string wordOfParagraph = "";
//    int currentCharIndex = 0;
//    int charArrayOfChapterNumberLength = charArrayOfChapterNumber.Length;
//    for (int j = i; j < charArrayOfChapterNumberLength; j++)
//    {
//        currentCharIndex = j;
//        bool result = currentIf(charArrayOfChapterNumber[j]);                
//        if (result)
//        {
//            wordOfParagraph = wordOfParagraph + charArrayOfChapterNumber[j];
//        }
//        else
//        {
//            bool result1 = currentDoElse(wordOfParagraph, i, j);
//            if (result1)
//                //if (j - i > 1)
//            {
//                foundWordsOfParagraph[foundWordsCount] = wordOfParagraph;
//            }
//            return currentCharIndex - doMinusOne;
//        }
//    }
//    return currentCharIndex;
//}
//
//foreach (char charOfChapterNumber in currentParagraph)
//{               
//    if (i < findWordsCount)//если массив еще не заполнен, заполняем (если будет переполняться, вычесть 1)
//    {
//        if (Char.IsLetterOrDigit(charOfChapterNumber))//слабое место, что может быть комбинация букв и цифр - протестировать этот вариант
//        {
//            if (flagSymbolsStarted > 1)
//            {//найдена цепочка спецсимволов больше одного подряд (она только что завершилась)
//                foundWordsOfParagraph[i] = symbolsOfParagraph;                            
//                symbolsOfParagraph = "";
//                i++;
//            }
//            flagSymbolsStarted = 0; //цепочка символов прервалась, сбрасываем счетчик                    
//            wordOfParagraph = wordOfParagraph + charOfChapterNumber;//нашли начало слова (после возможных спецсимволов в начале строки) и собираем слово, пока идут буквы (или цифры)
//            flagWordStarted++;
//        }
//        else
//        {//слово кончилось (или еще не началось)
//            if (flagWordStarted > 0)
//            {
//                if (charOfChapterNumber == ' ')
//                {//нашли пробел после него, прибавляем его к слову для совпадения с ключевыми словами - сомнительное действие, надо предусмотреть дефис и прочие варианты
//                    foundWordsOfParagraph[i] = wordOfParagraph + charOfChapterNumber;                                
//                    wordOfParagraph = "";
//                    i++;
//                }                            
//            }
//            flagWordStarted = 0; //цепочка букв или цифр прервалась, сбрасываем счетчик                        
//            symbolsOfParagraph = symbolsOfParagraph + charOfChapterNumber;
//            flagSymbolsStarted++;                        
//        }
//    }
//    else
//    {
//        _messageService.ShowTrace(MethodBase.GetCurrentMethod().ToString(), "else i = " + i.ToString() + DConst.StrCRLF +
//            "foundWordsOfParagraph[0] --> " + foundWordsOfParagraph[0] + DConst.StrCRLF +
//            "foundWordsOfParagraph[1] --> " + foundWordsOfParagraph[1] + DConst.StrCRLF +
//            "foundWordsOfParagraph[2] --> " + foundWordsOfParagraph[2] + DConst.StrCRLF +
//            "foundWordsOfParagraph[3] --> " + foundWordsOfParagraph[3] + DConst.StrCRLF +
//            "foundWordsOfParagraph[4] --> " + foundWordsOfParagraph[4] + DConst.StrCRLF +
//            "flagWordStarted - " + flagWordStarted.ToString(), CurrentClassName, 3); 
//        return i;
//    }
//}
//if (flagWordStarted > 0)
//{
//    foundWordsOfParagraph[i] = wordOfParagraph;
//    i++;
//}
//if (flagSymbolsStarted > 1)
//{
//    foundWordsOfParagraph[i] = symbolsOfParagraph;
//    i++;
//}
//_messageService.ShowTrace(MethodBase.GetCurrentMethod().ToString(), "foreach i = " + i.ToString() + DConst.StrCRLF +
//            "foundWordsOfParagraph[0] --> " + foundWordsOfParagraph[0] + DConst.StrCRLF +
//            "foundWordsOfParagraph[1] --> " + foundWordsOfParagraph[1] + DConst.StrCRLF +
//            "foundWordsOfParagraph[2] --> " + foundWordsOfParagraph[2] + DConst.StrCRLF +
//            "foundWordsOfParagraph[3] --> " + foundWordsOfParagraph[3] + DConst.StrCRLF +
//            "foundWordsOfParagraph[4] --> " + foundWordsOfParagraph[4] + DConst.StrCRLF +
//            "flagWordStarted - " + flagWordStarted.ToString(), CurrentClassName, 3);
//
//пусть он работает в обратном порядке (удалите с самого высокого индекса на самый низкий)
//List<T>.RemoveAll(Predicate<T>) или LINQ для замены исходного списка новым списком путем фильтрации элементов 
//Когда вы вызываете RemoveAt для удаления элемента, остальные элементы в списке перенумеровываются, чтобы заменить удаленный элемент. 
//Например, если вы удаляете элемент по индексу 3, элемент в индексе 4 перемещается в позицию 3. Поэтому удаляйте элементы из хвоста массива!
//
//private int CurrentParagraphsPortioned(int desiredTextLanguage)
//{
//    int ID_Chapter = -1;// - gimp stick?
//    int ID_Paragraph = 0;
//    int ID_Sentenses = 0;
//    int iSentStep = 0;
//    int previousParagraphLength = -1;//start flag value - before Paragraph exists
//    foreach (string currentParagraph in currentTextParagraphsPortioned)//place in currentParagraph each Paragraph
//    {
//        int lengthOfCurrentParagraph = currentParagraph.Length;//count symbols quantity in the current Paragraph - придумать более осмысленый параметр
//        string paragraph_name = lengthOfCurrentParagraph.ToString();//(for test only)
//        #region Chapters 
//        //ID_Chapter = -1; on the first pass of the foreach
//        //find Chapters and set Chapters ID and name
//        bool isPreviousLineBlank = previousParagraphLength == 0;//check if previous line was blank
//        if (iSentStep > 0)//if not first step
//        {
//            ID_Chapter = FindChapterNumber(currentParagraph, desiredTextLanguage, ID_Chapter, isPreviousLineBlank);
//        }
//        #endregion
//        #region Paragraphs & Sentences
//        if (ID_Chapter >= 0)//if the first Chapter has not inserted in dB yet, we cannot insert the Paragraph
//        {
//            Array.Clear(dataBaseTableToDo, 0, dataBaseTableToDo.Length);
//            dataBaseTableToDo[(int)TablesNamesNumbers.Paragraphs] = (int)WhatNeedDoWithTables.InsertRecord;// - массив объявлен вне метода и не передан в метод
//            //dataBaseTableNames[(int)TablesNamesNumbers.Paragraphs] - искать имя таблицы по индексу dataBaseTableToDo который равен InsertRecord
//            int insertResultParagraphs = _data.InsertRecordInTable(dataBaseTableNames, dataBaseTableToDo, ID_Paragraph, desiredTextLanguage, ID_Chapter, lengthOfCurrentParagraph, paragraph_name);//Insert Record in Table Paragraphs
//            _messageService.ShowTrace(MethodBase.GetCurrentMethod().ToString() + " insertResult after Paragraphs ==> ", insertResultParagraphs.ToString(), CurrentClassName, DConst.ShowMessagesLevel);
//            string[] currentParagraphSentences = currentParagraph.Split(charsSentenceSeparator);//portioned current paragraph on sentences - съел все точки и прочие окончания предложений!
//                                                                                                // - написать свое разделение на предложния с учетом кавычек и всяких p.m. 
//                                                                                                // - показать пользователю проблемные места в поле редактора формы - точки без последующих пробелов и т.д. 
//                                                                                                // - чтобы пользователь мог написать маски для обработки
//            ID_Sentenses = PortionParagraphOnSentences(currentParagraphSentences, desiredTextLanguage, ID_Chapter, ID_Paragraph, ID_Sentenses);
//            ID_Paragraph++;
//        }
//        #endregion
//        iSentStep++;//count of paragraphs
//        previousParagraphLength = currentParagraph.Length;//count symbols quantity in the current Paragraph - will be in previous Paragraph on the next pass
//        // to print in file
//        //int textParagraphesCount = textParagraphs.Count;
//        //string[] textParagraphesArray = new string[textParagraphesCount];
//        //textParagraphesArray = textParagraphs.ToArray();
//        //_manager.WriteToFilePathPlus(textParagraphesArray, filesPath[(int)TableLanguagesContent.English], "001");
//    }
//    return 0;
//}
//for (int m = 0; m < baseKeyWordFormsQuantity; m++)
//{
//    //в этом массиве значения показывают, сколько раз какое ключевое слово встретилось в тексте
//    _msgService.ShowTrace(MethodBase.GetCurrentMethod().ToString(), "Must be found 52 the same chapters name" + DConst.StrCRLF +            
//        "ChapterNamesVersionsCount[0] --> " + GetChapterNamesVersionsCount(m, 0) + DConst.StrCRLF +            
//        "ChapterNamesVersionsCount[1] --> " + GetChapterNamesVersionsCount(m, 1) + DConst.StrCRLF +            
//        "ChapterNamesVersionsCount[2] --> " + GetChapterNamesVersionsCount(m, 2) + DConst.StrCRLF +            
//        "ChapterNamesVersionsCount[3] --> " + GetChapterNamesVersionsCount(m, 3) + DConst.StrCRLF +            
//        "ChapterNamesVersionsCount[4] --> " + GetChapterNamesVersionsCount(m, 4), CurrentClassName, DConst.ShowMessagesLevel);
//}         
//int currentNumberIndex = 0;//счетчик найденных настоящих номеров глав
//int baseKeyWordFormsCount = GetConstantWhatNotLength("baseKeyWordFormsCount");//получим количество базовых форм ключевых слов (первая прописная, все прописные, все строчные)
//string[] NamesSamples = DConst.chapterNamesSamples[desiredTextLanguage].ToArray;//и сами массивы
//int keyWordIndex = 0;//i
//SetParagraphText(chapterTextMarks, currentParagraphIndex - 1, desiredTextLanguage);//проверить, что i больше 0, иначе некуда заносить - ЗДЕСЬ запись SetParagraphText! - записываем номер главы в строку перед именем главы! проверить, что будет, если добавить перевод строки? ничего хорошего - потом не найти маркировку, так как строка теперь начинается не маркой
//int increasedChapterNumberStart = -1;
//do
//{                
//    increasedChapterNumberStart = Array.IndexOf(allParagraphsWithDigitsFound, increasedChapterNumbers);
//    increasedChapterNumbers++;//если и была нулевая глава, то сделали +1 для всех, чтобы отличить от пустых нулей, поэтому мин.номер главы не меньше 1, обычно - 2
//    _msgService.ShowTrace(MethodBase.GetCurrentMethod().ToString(), "Min realistic chapter number found = " + (increasedChapterNumbers - 1).ToString(), CurrentClassName, 3);//DConst.ShowMessagesLevel);
//}
//while (increasedChapterNumberStart < 0);
//все методы - только отсюда (кроме методов в вспомогательном классе) - похоже, в реальной жизни идея не покатит - цифр просто бездна, надо искать ключевые слова, так надежнее
//первый метод вернет сжатый временный массив со всеми числами подряд (и куда его использовать? скорее всего, нужны мин и макс значения, но это уже больше одного) - но нет, нужен разреженный массив в общем доступе
//int baseKeyWordFormsCount = GetIntConstant("baseKeyWordFormsCount");//получим количество базовых форм ключевых слов (первая прописная, все прописные, все строчные)
//if (keyWordFormIndex >= 0)
//{
//    int chapterNumberIndex = currentParagraph.IndexOf(currentChapterNumberString, startIndexOf, findChapterNameSpace);
//    if (chapterNumberIndex >= 0)
//    {
//        if (c > 0 && keyWordForm != keyWordFoundForm)
//        {
//            _msgService.ShowTrace(MethodBase.GetCurrentMethod().ToString(), "keyWordFoundForm WAS - " + keyWordFoundForm + DConst.StrCRLF +
//                "keyWordForm FOUND NEW - " + keyWordFoundForm + DConst.StrCRLF +
//                "in currentParagraph[" + c.ToString() + "] = " + currentParagraph, CurrentClassName, 3);
//        }
//    }
//}
//string needConstantnName = "NamesSamples" + desiredTextLanguage.ToString();
//allParagraphsWithDigitsFound = RemoveNonChaptersNumbersFromArray(allParagraphsWithDigitsFound, chapterNameIsDigitsOnly);//идея в том, чтобы удалить из массива allParagraphsWithDigitsFound все индексы, которые не принадлежат именам глав - все остальные нули
//int[] ChapterNumberParagraphsIndexes = CreateArrayParagraphIndexesWhereChaptersNumbersFound(allParagraphsWithDigitsFound, chapterNameIsDigitsOnly);//если не делаем -1 из chapterNumberIndex, то можно сюда передавать только его длину, а не целиком
//string lastFoundChapterNumberInMarkFormat = ""; //можно убрать (вернем из метода маркировку последней главы - лучше массив с индексами глав в общем тексте, хотя, он не меняется, чего его возвращать)
//все, что до первого номера главы, маркировать нулевой главой! но сначала проверить, что нумерация глав не с нуля!
//int firstChapterNumber = chapterNameIsDigitsOnly[0];
//bool isFirstChapterNumberNot0 = firstChapterNumber > 0;
//if(isFirstChapterNumberNot0)
//{
//    string currentChapterNumberToFind000 = _analysisLogic.AddSome00ToIntNumber("0", DConst.chapterNumberTotalDigits);//получим строку номера для нулевой главы
//    //запишем маркировку нулевой главы прямо в первый абзац, добавив строку в начале текста
//    string chapterTextMarks = DConst.beginChapterMark + currentChapterNumberToFind000 + DConst.endChapterMark + "-" + keyWordFoundForm + "-";//¤¤¤¤¤000¤¤¤-Chapter-                                                                                                                                           
//    string currentParagraph = _bookData.GetStringContent(desiredTextLanguage, "GetParagraphText", 0);//достаем самый первый абзац
//    chapterTextMarks += DConst.StrCRLF + currentParagraph;//дописываем маркировку главы к названию главы, добавив строку для нее 
//    int setParagraphResult = _bookData.GetIntContent(desiredTextLanguage, "SetParagraphText", chapterTextMarks, 0);//записываем обратно в самый первый абзац
//    _msgService.ShowTrace(MethodBase.GetCurrentMethod().ToString(), "CurrentParagraph -- > " + currentParagraph + DConst.StrCRLF +
//            "chapterTextMarks " + chapterTextMarks, CurrentClassName, DConst.ShowMessagesLevel);
//}
//int ChapterNumberParagraphsIndexesLength = ChapterNumberParagraphsIndexes.Length;
//for (int chapterNumberIndex = 0; chapterNumberIndex < ChapterNumberParagraphsIndexesLength; chapterNumberIndex++)
//{//из массива всего текста доставать абзац, проверять на название главы, если названия нет, присвоить метку и номер абзаца - но лучше в отдельный метод
//    int currentParagraphChapterNumberIndex = ChapterNumberParagraphsIndexes[chapterNumberIndex];
//    int currentChapterNumber = chapterNameIsDigitsOnly[chapterNumberIndex] - 1;
//    string currentChapterNumberString = currentChapterNumber.ToString();
//    string currentParagraph = _bookData.GetStringContent(desiredTextLanguage, "GetParagraphText", currentParagraphChapterNumberIndex);
//    //контрольная проверка номера главы
//    int startIndexOf = 0;
//    int currentParagraphLength = currentParagraph.Length;
//    int findChapterNameSpace = currentParagraphLength - startIndexOf;//стереотип - всегда отнять точку старта
//    int chapterNumberCheck = currentParagraph.IndexOf(currentChapterNumberString, startIndexOf, findChapterNameSpace);
//    bool isChapterNumberNotExist = chapterNumberCheck < 0;
//    if (isChapterNumberNotExist)
//    {
//        //можно сюда добавить Assert
//        _msgService.ShowTrace(MethodBase.GetCurrentMethod().ToString(), "ambush in the currentParagraph[" + currentParagraphChapterNumberIndex.ToString() + "] = " + DConst.StrCRLF + currentParagraph + DConst.StrCRLF +
//            "currentChapterNumberString was not found - " + currentChapterNumberString + DConst.StrCRLF +
//            "the result of IndexOf is - " + chapterNumberCheck.ToString() + DConst.StrCRLF +
//            "chapterNumberIndex = " + chapterNumberIndex.ToString(), CurrentClassName, 3);
//    }
//    string currentChapterNumberToFind000 = _analysisLogic.AddSome00ToIntNumber(currentChapterNumberString, DConst.chapterNumberTotalDigits);
//    if (currentChapterNumberToFind000 == null)
//    {
//        return null;//лучше поставить Assert - и можно прямо в AddSome00ToIntNumber?
//    }
//    string chapterTextMarks = DConst.beginChapterMark + currentChapterNumberToFind000 + DConst.endChapterMark + "-" + keyWordFoundForm;//¤¤¤¤¤001¤¤¤-Chapter-
//    chapterTextMarks += DConst.StrCRLF + currentParagraph + DConst.StrCRLF;//дописываем маркировку главы к названию главы, добавив строку для нее 
//    int setParagraphResult = _bookData.GetIntContent(desiredTextLanguage, "SetParagraphText", chapterTextMarks, currentParagraphChapterNumberIndex);//int GetIntContent(desiredTextLanguage, SetParagraphText, stringToSet, indexCount) надо писать в индекс[i] из массива индексов, а не в сам i - currentParagraphChapterNumberIndex
//    //проверять setParagraphResult - но сначала сделать его осмысленным в самом методе записи
//    string newCurrentParagraph = _bookData.GetStringContent(desiredTextLanguage, "GetParagraphText", currentParagraphChapterNumberIndex);
//    _msgService.ShowTrace(MethodBase.GetCurrentMethod().ToString(), "CurrentParagraph with i = " + currentParagraphChapterNumberIndex.ToString() + "  -- > " + currentParagraph + DConst.StrCRLF +
//            "chapterTextMarks - " + chapterTextMarks + DConst.StrCRLF +
//            "newCurrentParagraph - " + newCurrentParagraph + DConst.StrCRLF +
//            "currentChapterNumber [" + chapterNumberIndex.ToString() + "] --> " + currentChapterNumber.ToString(), CurrentClassName, DConst.ShowMessagesLevel);
//    lastFoundChapterNumberInMarkFormat = chapterTextMarks;                
//}    
//int chapterNameIsDigitsOnlyLength = chapterNameIsDigitsOnly.Length;
//int ChapterNumberParagraphsIndexesLength = ChapterNumberParagraphsIndexes.Length;
//int paragraphTextLength = _bookData.GetIntContent(desiredTextLanguage, "GetParagraphTextLength");
//_msgService.ShowTrace(MethodBase.GetCurrentMethod().ToString(), "ChapterNumberParagraphsIndexesLength = " + ChapterNumberParagraphsIndexesLength.ToString() + DConst.StrCRLF +
//    "ChapterNumberParagraphsIndexes[0] = " + ChapterNumberParagraphsIndexes[0].ToString() + DConst.StrCRLF +
//    "ChapterNumberParagraphsIndexes[ChapterNumberParagraphsIndexesLength - 1] = " + ChapterNumberParagraphsIndexes[ChapterNumberParagraphsIndexesLength - 1].ToString() + DConst.StrCRLF +
//    "chapterNameIsDigitsOnlyLength = " + chapterNameIsDigitsOnlyLength.ToString() + DConst.StrCRLF +
//    "chapterNameIsDigitsOnly[0] = " + chapterNameIsDigitsOnly[0].ToString() + DConst.StrCRLF +
//    "chapterNameIsDigitsOnly[ChapterNumberParagraphsIndexesLength - 1] = " + chapterNameIsDigitsOnly[chapterNameIsDigitsOnlyLength - 1].ToString() + DConst.StrCRLF +
//    "paragraphTextLength = " + paragraphTextLength.ToString(), CurrentClassName, DConst.ShowMessagesLevel);
//_msgService.ShowTrace(MethodBase.GetCurrentMethod().ToString(), "keyWordFoundForm = " + keyWordFoundForm, CurrentClassName, DConst.ShowMessagesLevel);
//сортировка все же не нужна
//int startAllDigits = 0;
//int endAllDigits = allParagraphsWithDigitsFoundLength - 1;
//_msgService.ShowTrace(MethodBase.GetCurrentMethod().ToString(), "Array allParagraphsWithDigitsFound BEFORE sorting - start-end values" + DConst.StrCRLF +
//    "allParagraphsWithDigitsFound[" + startAllDigits.ToString() + "] = " + allParagraphsWithDigitsFound[startAllDigits].ToString() + DConst.StrCRLF +
//    "allParagraphsWithDigitsFound[" + endAllDigits.ToString() + "] = " + allParagraphsWithDigitsFound[endAllDigits].ToString(), CurrentClassName, DConst.ShowMessagesLevel);
////тут придется отсортировать массив allParagraphsWithDigitsFound, а потом уже искать монотонно возрастающие цифры от нуля (или нуль выкинуть?)
//QuickSortOfAllDigits(allParagraphsWithDigitsFound, startAllDigits, endAllDigits);
//_msgService.ShowTrace(MethodBase.GetCurrentMethod().ToString(), "Array allParagraphsWithDigitsFound AFTER sorting - start-end values" + DConst.StrCRLF +
//    "allParagraphsWithDigitsFound[" + startAllDigits.ToString() + "] = " + allParagraphsWithDigitsFound[startAllDigits].ToString() + DConst.StrCRLF +
//    "allParagraphsWithDigitsFound[" + endAllDigits.ToString() + "] = " + allParagraphsWithDigitsFound[endAllDigits].ToString(), CurrentClassName, DConst.ShowMessagesLevel);    
//}
//while (currentNumderFound);
//int[] allParagraphsWithKeyFormsFound = new int[allParagraphsWithDigitsFoundLength];//заводим временный массив индексов ключевых слов - параллельно с номерами сделаем такой же массив с найденными словами (делать одновременно? наверное, не очень - там искали IndexOfAny, а тут вложенный цикл)
//allParagraphsWithDigitsFound[chapterNamesParagraphIndexesTemp[f]] = chapterNameIsDigitsOnly[f];//по индексу, сохраненному в ячейке сжатого массива сохраняем номер главы - в таком массиве унесем больше - и индекс, где находится глава и сам номер главы
//string currentParagraph = _bookData.GetParagraphText(desiredTextLanguage, chapterNamesParagraphIndexesTemp[f]);//достаем только для печати, потом убрать
//массив allParagraphsWithDigitsFound неявно возвращаем из метода, остальные не нужны
//chapterNamesParagraphIndexes[f] = chapterNamesParagraphIndexesTemp[f];//метод Take(workIndex) извлекает workIndex элементов из массива
//_msgService.ShowTrace(MethodBase.GetCurrentMethod().ToString(), "currentParagraph[" + chapterNamesParagraphIndexesTemp[f].ToString() + "] = " + currentParagraph + DConst.StrCRLF +
//    "allParagraphsWithDigitsFound[" + chapterNamesParagraphIndexesTemp[f].ToString() + "] = " + chapterNameIsDigitsOnly[f].ToString() + DConst.StrCRLF +
//    "f = " + f.ToString() + " - from - chapterNamesParagraphIndexesCount = " + chapterNamesParagraphIndexesCount.ToString(), CurrentClassName, DConst.ShowMessagesLevel);
//проверяем, что номер из текста не нулевой и равен текущему упорядоченному номеру - нет, просто равен, а там уже нет нулей - и таки есть, добавить, что не нулевой
//тут надо что-то сделать - найдено все, надо куда-то это сохранить и перейти к следующему номеру последовательности (так и тянет сохранить в тот же массив с последовательностью - но некрасиво)
//chapterNameIsDigitsOnly[ccni] = 0;//вряд ли этот массив еще понадобится (тут обнулим считанную ячейку массива, чтобы записать только номера найденных глав - очистить массив от лишних номеров)
//тут обнулим массивs, чтобы затем записать только индексы найденных глав - очистить массивы от посторонних индексов
//Array.Clear(allParagraphsWithDigitsFound, 0, allParagraphsWithDigitsFoundLength);
//Array.Clear(allParagraphsWithKeyFormsFound, 0, allParagraphsWithDigitsFoundLength); //этот массив уже вряд ли понадобится
//int[] chapterNamesParagraphIndexes = new int[chapterNamesParagraphIndexesCount];
//        keyWordFoundForm = keyWordForm;//если найдена и какая-то форма какого-то слова и нужный номер главы, сохраняем найденную форму
//        if (chapterNumberIndex > keyWordFormIndex)
//        {
//            int stringBetweenKeyWordAndChapterNumberLlength = chapterNumberIndex - (keyWordFormIndex + keyWordForm.Length);
//            string stringBetweenKeyWordAndChapterNumber = currentParagraph.Substring(keyWordFormIndex + keyWordForm.Length, stringBetweenKeyWordAndChapterNumberLlength);//String substring = value.Substring(startIndex, length);
//            _msgService.ShowTrace(MethodBase.GetCurrentMethod().ToString(), "currentParagraph[" + c.ToString() + "] = " + currentParagraph + DConst.StrCRLF +
//                                                "keyWordFormIndex = " + keyWordFormIndex.ToString() + DConst.StrCRLF +
//                                                "chapterNumberIndex = " + chapterNumberIndex.ToString() + DConst.StrCRLF +
//                                                "stringBetweenKeyWordAndChapterNumber - " + stringBetweenKeyWordAndChapterNumber + DConst.StrCRLF +
//                                                "m (keyWordForm) = " + m.ToString(), CurrentClassName, DConst.ShowMessagesLevel);
//        }
//allParagraphsWithKeyFormsFound[cpi] = k;//индекс, а не слово сохранять и потом проверять проще, а в конце достанем настоящую форму ключевого слова из массива (а вообще-то можно и базовую форму на выбор)
//for (int f = 0; f < chapterNamesParagraphIndexesCount; f++)
//{
//    int result = _bookData.SetNotices(desiredTextLanguage, chapterNamesParagraphIndexesTemp[f], chapterNameIsDigitsOnly[f], keyWordFoundForm);//сохранили все номера глав и ключевое слово в служебный массив заметок - для передачи дальше                
//}
//int[] chapterNamesParagraphIndexesTemp = new int[chapterNameIsDigitsOnlyLength];//временный сжатый массив индексов абзацев, в которых названия глав (длина массива с запасом, потом обрежем - когда найдем настоящие названия глав)
//string keyWordFoundForm = totalBaseFormsOfNamesSamples[kMultipleMaxKeyWordIndex];
//int totalBaseFormsOfNamesSamplesLength = totalBaseFormsOfNamesSamples.Length;
//int allParagraphsWithDigitsFoundLength = allParagraphsWithDigitsFound.Length;
//int chapterNameIsDigitsOnlyLength = chapterNameIsDigitsOnly.Length;
//тут можно заодно заполнять сжатый массив индексов абзацев, где расположены названия глав - для передачи дальше (в идеале 3 массива - индексы, номера глав и строка полного имени клавы уже с ее нумерацией)
//int currentChapterNumber = allParagraphsWithDigitsFound[cpi];//скорее всего, логичнее так и дальше передавать этот массив - индексы и номера в одном флаконе - можно сделать словарь и потом перебирать его по foreach
//public int[] RemoveNonChaptersNumbersFromArray(int[] allParagraphsWithDigitsFound, int[] chapterNameIsDigitsOnly)
//{//из временного массива allParagraphsWithDigitsFound выкинем все числа, кроме найденного возрастающего ряда - сохранив привязку к индексам абзацев (вообще, он может пригодиться и в дальнейшем - пока не изменится структура массива абзацев - можно сохранить его в AllBookDataArrays)
//    int allParagraphsWithDigitsFoundLength = allParagraphsWithDigitsFound.Length;
//    int chapterNameIsDigitsOnlyLength = chapterNameIsDigitsOnly.Length;
//    int chapterNumberIndex = 0;
//    for (int i = 0; i < allParagraphsWithDigitsFoundLength; i++)
//    {
//        bool numberIsNotChapterNumber = allParagraphsWithDigitsFound[i] != chapterNameIsDigitsOnly[chapterNumberIndex];
//        bool numberIsChapterNumber = allParagraphsWithDigitsFound[i] == chapterNameIsDigitsOnly[chapterNumberIndex];
//        if (numberIsNotChapterNumber)
//        {
//            allParagraphsWithDigitsFound[i] = 0;
//        }
//        if (numberIsChapterNumber)
//        {
//            _msgService.ShowTrace(MethodBase.GetCurrentMethod().ToString(), "allParagraphsWithDigitsFound[" + i.ToString() + "] = " + allParagraphsWithDigitsFound[i].ToString() + DConst.StrCRLF +
//              "chapterNameIsDigitsOnly[" + chapterNumberIndex.ToString() + "] = " + chapterNameIsDigitsOnly[chapterNumberIndex].ToString(), CurrentClassName, DConst.ShowMessagesLevel);
//            if (chapterNumberIndex < chapterNameIsDigitsOnlyLength - 1) chapterNumberIndex++;                    
//        }
//    }
//        return allParagraphsWithDigitsFound;//-1 еще не делали - чтобы получить реальные номера глав, потому что куча пустых нулей (вообще нельзя этого делать)
//}        
//public int[] CreateArrayParagraphIndexesWhereChaptersNumbersFound(int[] allParagraphsWithDigitsFound, int[] chapterNameIsDigitsOnly)//если не делаем -1 из chapterNumberIndex, то можно сюда передавать только его длину, а не целиком
//{//для удобства можно сделать массив с индексами номеров абзацев, где найдены правильно возрастающие числа, но мин и макс значения все равно будут нужны - посмотреть по месту
//    int chapterNameIsDigitsOnlyLength = chapterNameIsDigitsOnly.Length;
//    int[] ChapterNumberParagraphsIndexes = new int[chapterNameIsDigitsOnlyLength];
//    int allParagraphsWithDigitsFoundLength = allParagraphsWithDigitsFound.Length;
//    int chapterNumberIndex = 0;
//    for (int i = 0; i < allParagraphsWithDigitsFoundLength; i++)
//    {
//        if(allParagraphsWithDigitsFound[i] > 0)
//        {
//            ChapterNumberParagraphsIndexes[chapterNumberIndex] = i;//сохранили индекс абзаца с номером главы в массив индексов
//            _msgService.ShowTrace(MethodBase.GetCurrentMethod().ToString(), "ChapterNumberParagraphsIndexes[" + chapterNumberIndex.ToString() + "] = " + ChapterNumberParagraphsIndexes[chapterNumberIndex].ToString() + DConst.StrCRLF +
//              "chapterNameIsDigitsOnly[" + chapterNumberIndex.ToString() + "] = " + chapterNameIsDigitsOnly[chapterNumberIndex].ToString(), CurrentClassName, DConst.ShowMessagesLevel);
//            if (chapterNumberIndex < chapterNameIsDigitsOnlyLength - 1) chapterNumberIndex++; //chapterNumberIndex++;
//        }//вычитать -1 из chapterNameIsDigitsOnly тут пока не будем, хотя и хочется - а как его вернуть? только неявно - так себе идея
//    }
//        return ChapterNumberParagraphsIndexes;
//}
//нашли непрерывный возрастающий ряд chapterNameIsDigitsOnly, но в нем есть лишние цифры сверху - надо отсечь их при поиске ключевых слов в RemoveNonChaptersNumbersFromArray (например)
//номера глав - это прежде всего цифры, поэтому сначала собираем все встречающиеся цифры в тексте (только в начале строки)
//тут не работает - проскакивает главы и увеличивает номера - что странно (вовсе не странно)
//проверяем все абзацы с числами на наличие ключевых слов - если какое-то найдено во всех случаях, берем его как найденное (если никакое не найдено, могут быть просто числа - без слов, тогда можно проверить, что в абзаце особо ничего, кроме чисел и нет - имеется в виду букв)
//если что-то где-то не совпадает, либо помечать такие абзацы, но лучше завести глобальный массив с индексами абзацев с непонятнками - будем потом показывать их пользователю (а пока печать и/или Assert - смотря по уровню критичности)
//надо вернуть чего-то хорошее и полезное... - пока возвращаем найденную форму ключевого слова называния главы - уже нужный индекс
