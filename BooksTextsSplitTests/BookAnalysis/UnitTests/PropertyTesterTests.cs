using BooksTextsSplit.Controllers;
using BooksTextsSplit.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;

namespace BooksTextsSplitTests.BookAnalysis.UnitTests
{
    [TestClass]
    public class PropertyTesterTests
    {
        [TestMethod]
        [DataRow("LanguageId", true)]
        [DataRow("languageId", true)]
        [DataRow("language_Id", false)]
        public void Test(string propName, bool expected)
        {
            Assert.AreEqual(expected, BookTextsController.IsPropertyOf<TextSentence>(propName));
        }

        [TestMethod]
        [DataRow("LanguageId", 1)] // property set
        [DataRow("UploadVersion", 0)] // not set, default value for int
        [DataRow("language_Id", null)] // missing property, so null
        public void TestGetProperty(string propName, object expected)
        {
            var s = new TextSentence { LanguageId = 1 };
            object actual = BookTextsController.GetProperty(s, propName);
            Assert.AreEqual(expected, actual);
        }
        
        [TestMethod]
        [DataRow("LanguageId", 1)] // property set
        [DataRow("UploadVersion", 0)] // not set, default value for int
        [DataRow("language_Id", null)] // missing property, so null
        public void TestGetPropertyG(string propName, object expected)
        {
            var s = new TextSentence { LanguageId = 1 };
            object actual = BookTextsController.GetPropertyGeneric<TextSentence>(s, propName);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        [DataRow(new string[] { "LanguageId", "BookId", "SentenceId" }, true)]
        [DataRow(new string[] { "LanguageId", "BookId", "SentenceId", "UploadVersion", "BookName", "AuthorNameId" }, true)]
        [DataRow(new string[] { "LanguageId", "BookId", "EvilSQLinjection" }, false)]
        public void TestArray(string[] propName, bool expected)
        {
            Assert.AreEqual(expected, BookTextsController.AreParamsRealPropertiesOf<TextSentence>(propName));
        }
    }
}
