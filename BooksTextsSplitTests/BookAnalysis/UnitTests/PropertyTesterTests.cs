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
        [DataRow("LanguageId", true)]
        [DataRow("language_Id", false)]
        public void Test(string propName, bool expected)
        {
            Assert.AreEqual(expected, BookTextsController.IsPropertyOf<TextSentence>(propName));
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
