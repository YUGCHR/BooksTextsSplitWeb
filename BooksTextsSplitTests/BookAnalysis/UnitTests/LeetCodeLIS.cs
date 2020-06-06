using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace BooksTextsSplit.UnitTests
{
    [TestClass()]
    public class Solution
    {
        [TestMethod()]
        [DataRow(new int[] { 10, 9, 2, 5, 3, 7, 101, 18 }, 4)]
        [DataRow(new int[] { 2, 5, 3, 101 }, 3)]
        //    0  1  2  3  4  5    6   7
        // { 10, 9, 2, 5, 3, 7, 101, 18 }
        // {  1, 1, 1, 2, 1, 1, 1, 1 }
        // { -1, 1, 2, 2, 2, 4, 5, 5 }
        public void LengthOfLIS(int[] nums, int expCountOfMaxLIS)
        {
            int numsLength = nums.Length;
            int[] indexI = new int[numsLength];
            int[] countsOfLIS = new int[numsLength];
            int[] parentNumberOfCount = new int[numsLength];

            countsOfLIS[0] = 1;
            parentNumberOfCount[0] = -1;
            for (int i = 0; i < numsLength; i++)
            {
                indexI[i] = i;
            }
            Console.WriteLine("Index i:         {0}", string.Join(", ", indexI.Select(n => string.Format("{0,5:###}", n))));

            for (int i = 1; i < numsLength; i++)
            {
                indexI[i] = i;
                countsOfLIS[i] = 1;                

                for (int j = 0; j < i; j++)
                {
                    if (nums[i] > nums[j])
                    {
                        countsOfLIS[i] = countsOfLIS[j] + 1;
                        parentNumberOfCount[i] = j;
                        
                        var s = string.Join(", ", nums.Select(n => string.Format("{0,5:###}",n)));
                        Console.WriteLine("Nums:    {0}: {1}", i,s);
                        Console.WriteLine("Counts:  {0}: {1}", j, string.Join(", ", countsOfLIS.Select(n => string.Format("{0,5:###}", n))));
                        Console.WriteLine("Parents: {0}: {1}", j, string.Join(", ", parentNumberOfCount.Select(n => string.Format("{0,5:###}", n))));
                        Console.WriteLine("------");                        
                    }
                }
            }
            int countOfMaxLIS = countsOfLIS[0];
            for (int n = 1; n < numsLength; n++)
            {
                if (countsOfLIS[n] > countsOfLIS[n - 1])
                {
                    countOfMaxLIS = countsOfLIS[n];
                }
            }
            Assert.AreEqual(expCountOfMaxLIS, countOfMaxLIS);
        }
    }
}
