using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BooksTextsSplit
{
    public interface ILogFileMessages
    {
        //string[] LogFileMessagesTexts { get; set; }
        string GetLogFileMessages(int i);        
    }

    public class LogFileMessages : ILogFileMessages
    {
        public string[] LogFileMessagesTexts;

        public LogFileMessages()
        {
            int OpenFormTextBoxImplementationMessagesCount = Enum.GetNames(typeof(TextBoxImplementationMessages)).Length;
            LogFileMessagesTexts = new string[OpenFormTextBoxImplementationMessagesCount];

            LogFileMessagesTexts[(int)TextBoxImplementationMessages.EnglishFilePathSelected] = "Following path of English file has been selected - ";
            LogFileMessagesTexts[(int)TextBoxImplementationMessages.RussianFilePathSelected] = "Following path of Russian file has been selected - ";
            LogFileMessagesTexts[(int)TextBoxImplementationMessages.ResultFilePathSelected] = "Following path of result file has been selected - ";
            LogFileMessagesTexts[(int)TextBoxImplementationMessages.ResultFileCreated] = "Following result file has been created sucessfully - ";
        }

        public string GetLogFileMessages(int i)
        {
            return LogFileMessagesTexts[i];
        }
    }
}

