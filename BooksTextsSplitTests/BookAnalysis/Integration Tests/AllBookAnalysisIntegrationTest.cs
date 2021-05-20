using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using BooksTextsSplit;
using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using BooksTextsSplit.Library.Models;
using FluentAssertions;
using BooksTextsSplit.Library.BookAnalysis;

namespace BooksTextsSplit.Integration_Tests
{
    [TestClass()]
    public class AllBookAnalysisIntegrationTest
    {
        private readonly ITextAnalysisLogicExtension _analysisLogic;

        [TestMethod()]
        [DataRow("hash here", 0, 1, "-Paragraph-00001-of-Chapter-000", "How to explain? How to describe? Even the omniscient viewpoint quails.", new int[] { 14, 31, 69 })]
        [DataRow("hash here", 0, 2, "-Paragraph-00002-of-Chapter-000", "A singleton star, reddish and dim. A ragtag of asteroids, and a single planet, more like a moon. In this era the star hung near the galactic plane, just beyond the Beyond. ", new int[] { 33, 95, 170 })]
        [DataRow("hash here", 0, 3, "-Paragraph-00003-of-Chapter-000", "The structures on the surface were gone from normal view, pulverized into regolith across a span of aeons. The treasure was far underground, beneath a network of passages, in a single room filled with black.", new int[] { 105, 206 })]
        [DataRow("hash here", 0, 4, "-Paragraph-00004-of-Chapter-000", "Information at the quantum density, undamaged. Maybe five billion years had passed since the archive was lost to the nets. The curse of the mummy’s tomb, a comic image from mankind’s own prehistory, lost before time.", new int[] { 45, 121, 215 })]
        [DataRow("hash here", 0, 5, "-Paragraph-00005-of-Chapter-000", "They had laughed when they said it, laughed with joy at the treasure… and determined to be cautious just the same. They would live here a year or five, the little company from Straum, the archaeologist programmers, their families and schools. ", new int[] { 68, 113, 231 })]
        [DataRow("hash here", 0, 6, "-Paragraph-00006-of-Chapter-000", "A year or five would be enough to handmake the protocols, to skim the top and identify the treasure’s origin in time and space, to learn a secret or two that would make Straumli Realm rich. And when they were done, they would sell the location; perhaps build a network link (but chancier that  —  this was beyond the Beyond; who knew what Power might grab what they’d found).", new int[] { 188, 243, 374 })]
        [DataRow("hash here", 0, 1, "-Paragraph-00001-of-Chapter-001", "So now there was a tiny settlement on the surface, and they called it the High Lab. It was really just humans playing with an old library. It should be safe, using their own automation, clean and benign. ", new int[] { 82, 137, 202 })]
        [DataRow("hash here", 0, 2, "-Paragraph-00002-of-Chapter-001", "This library wasn’t a living creature, or even possessed of automation (which here might mean something more, far more, than human). They would look and pick and choose, and be careful not to be burned… ", new int[] { 131, 201 })]
        [DataRow("hash here", 0, 3, "-Paragraph-00003-of-Chapter-001", "Humans starting fires and playing with the flames.", new int[] { 49 })]
        [DataRow("hash here", 0, 1, "-Paragraph-00001-of-Chapter-002", "The archive informed the automation. Data structures were built, recipes followed. A local network was built, faster than anything on Straum, but surely safe. Nodes were added, modified by other recipes. ", new int[] { 35, 81, 157, 202 })]
        [DataRow("hash here", 0, 2, "-Paragraph-00002-of-Chapter-002", "The archive was a friendly place, with hierarchies of translation keys that led them along. Straum itself would be famous for this.", new int[] { 90, 130 })]

        //string[] DivideTextToSentencesByDelimiters(int languageId, int paragraphId, string sentenceTextMarksWithOtherNumbers, string textParagraph, int[] sentenceDelimitersIndexesArray)

        public void Test01_DivideTextToSentencesByDelimitersTest(string expTextSentenceHash, int languageId, int paragraphId, string sentenceTextMarksWithOtherNumbers, string textParagraph, int[] sentenceDelimitersIndexesArray)
        {
            IAllBookData bookData = new AllBookData();
            ITextAnalysisLogicExtension analysisLogic = new TextAnalysisLogicExtension(bookData);
            var target = new SentencesDividingAnalysis(bookData, analysisLogic);
            int textSentenceLengthBefore = bookData.GetTextSentenceLength();

            string[] paragraphSentences = target.DivideTextToSentencesByDelimiters(textParagraph, sentenceDelimitersIndexesArray);//разделили текст на предложения согласно оставшимся разделителям
            int textSentenceLengthAfter = bookData.GetTextSentenceLength();
            int addedTextSentencesCount = textSentenceLengthAfter - textSentenceLengthBefore;
            if (addedTextSentencesCount > 0)
            {

            }
            //TextSentence previousTextSentence = bookData.GetTextSentence(textSentenceLength - 1);
            string resultTextSentenceHash = analysisLogic.GetMd5Hash(expTextSentenceHash);

            Trace.WriteLine("resultTextSentenceHash: " + resultTextSentenceHash);
            Assert.AreEqual(expTextSentenceHash, resultTextSentenceHash);
        }
    }
}