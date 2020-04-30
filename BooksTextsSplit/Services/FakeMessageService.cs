using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;


namespace BooksTextsSplit
{ 

    public class FakeMessageService : IMessageService
    {
        public string SaveTracedToFile(string tracedFilePathAndName, string tracedFileContent)
        {
            return "";
        }

        public void ShowError(string error)
        {
            
        }

        public void ShowExclamation(string exclamation)
        {
            
        }

        public void ShowMessage(string message)
        {
            
        }

        public void ShowTrace(string tracePointName, string tracePointValue, string tracePointPlace, int showLevel)
        {
            
        }

        public void ShowTrace(string tracePointName, string[] tracePointValue, string tracePointPlace, int showLevel)
        {
            
        }
    }
}