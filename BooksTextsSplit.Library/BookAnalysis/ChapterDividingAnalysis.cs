using System;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace BooksTextsSplit.Library.BookAnalysis
{
    public interface IChapterDividingAnalysis
    {
        string ChapterNameAnalysis(int desiredTextLanguage);        
    }

    public class ChapterDividingAnalysis : IChapterDividingAnalysis
    {
        private readonly IAllBookData _bookData;        
        private readonly ITextAnalysisLogicExtension _analysisLogic;        

        delegate bool IsOrNotEqual(char x);
        delegate bool DoElseConditions(string x, int i, int j);

        delegate string TransduceWord(string baseKeyWord);

        
        public ChapterDividingAnalysis(IAllBookData bookData, ITextAnalysisLogicExtension analysisLogic)
        {
            _bookData = bookData;            
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
                }
            }
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
                }
            }
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
                        }
                    }
                    while (digitFoundForSure);

                        //преобразуем кучку найденных цифр в число int и сохраняем в allParagraphsWithDigitsFound - (или сначала делаем выборку из строки по найденным индексам и проверяем bool Int32.TryParse(string, out number)?)                            
                        bool digitFoundDefinitely = Int32.TryParse(digitsParcel, out int chapterNumberBidder);

                    if (digitFoundDefinitely)//кучка цифр успешно преобразовалась в число и можно сложить его в массив найденных в началах абзацев чисел
                    {
                        allParagraphsWithDigitsFound[cpi] = chapterNumberBidder;//тут не надо прибавлять 1 - портим все номера (было +1 чтобы убрать вариант нулевого номера главы - оказывается, существуют и такие (при сжатии массива номеров вычтем 1)
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
