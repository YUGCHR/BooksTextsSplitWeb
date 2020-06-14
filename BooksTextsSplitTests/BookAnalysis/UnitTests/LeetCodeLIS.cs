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
        [DataRow(0, 0)]
        [DataRow(new int[] { 10, 9, 2, 5, 3, 7, 101, 18 }, 4)]
        [DataRow(new int[] { 6, 7, 9, 4, 10, 5, 6 }, 4)]
        [DataRow(new int[] { 2, 5, 3, 101 }, 3)]
        [DataRow(new int[] { 2 }, 1)]
        [DataRow(new int[] { 1, 3, 6, 7, 9, 4, 10, 5, 6 }, 6)]

        //    0  1  2  3  4  5    6   7
        // { 10, 9, 2, 5, 3, 7, 101, 18 }
        // {  1, 1, 1, 2, 1, 1, 1, 1 }
        // { -1, 1, 2, 2, 2, 4, 5, 5 }
        public void LengthOfLIS(int[] nums, int expCountOfMaxLIS)
        {
            int numsLength = nums.Length;
            if (numsLength == 0)
            {
                Assert.AreEqual(expCountOfMaxLIS, 0); ;
            }
            int[] indexI = new int[numsLength];
            int[] countsOfLIS = new int[numsLength];
            int[] parentNumberOfCount = new int[numsLength];

            countsOfLIS[0] = 1;
            for (int i = 0; i < numsLength; i++)
            {
                indexI[i] = i;
                parentNumberOfCount[i] = -1;
            }
            Console.WriteLine("Index i:         {0}", string.Join(", ", indexI.Select(n => string.Format("{0,5:##0}", n))));

            for (int i = 1; i < numsLength; i++)
            {
                indexI[i] = i;
                countsOfLIS[i] = 1;

                for (int j = 0; j < i; j++)
                {
                    if (nums[i] > nums[j])
                    {
                        if (countsOfLIS[i] < (countsOfLIS[j] + 1))
                        {
                            countsOfLIS[i] = countsOfLIS[j] + 1;
                            parentNumberOfCount[i] = j;
                        }

                        var s = string.Join(", ", nums.Select(n => string.Format("{0,5:##0}", n)));
                        Console.WriteLine("Nums:    {0}: {1}", i, s);
                        Console.WriteLine("Counts:  {0}: {1}", j, string.Join(", ", countsOfLIS.Select(n => string.Format("{0,5:##0}", n))));
                        //Console.WriteLine("Parents: {0}: {1}", j, string.Join(", ", parentNumberOfCount.Select(n => string.Format("{0,5:##0}", n))));
                        Console.WriteLine("------");
                    }
                }
            }
            int countOfMaxLIS = countsOfLIS[0];
            for (int n = 1; n < numsLength; n++)
            {
                if (countsOfLIS[n] > countOfMaxLIS)
                {
                    countOfMaxLIS = countsOfLIS[n];
                }
            }
            Assert.AreEqual(expCountOfMaxLIS, countOfMaxLIS);
        }

        //Fibonacci

        [TestMethod()]
        [DataRow(0, 0)]
        [DataRow(1, 1)]
        [DataRow(2, 1)]
        [DataRow(3, 2)]
        [DataRow(4, 3)]
        [DataRow(5, 5)]
        [DataRow(6, 8)]
        [DataRow(7, 13)]
        [DataRow(8, 21)]
        [DataRow(9, 34)]
        [DataRow(1000, 1556111435)]
        [DataRow(-1, 1)]
        [DataRow(-2, -1)]
        [DataRow(-3, 2)]
        [DataRow(-4, -3)]
        [DataRow(-5, 5)]
        [DataRow(-6, -8)]
        [DataRow(-7, 13)]
        [DataRow(-8, -21)]
        [DataRow(-9, 34)]

        // -9  -8 -7 -6 -5 -4 -3 -2 -1 0 1 2 3 4 5 6  7  8  9
        // 34 -21 13 -8  5 -3  2 -1  1 0 1 1 2 3 5 8 13 21 34

        public void Fibonacci(int index, int expFibonacciNumber)
        {
            int iEnd = 0;
            int fibonacciNumbers2 = 0;

            if (index == 0)
            {
                Assert.AreEqual(expFibonacciNumber, 0);
            }
            else
            {
                if (index < 0)
                {
                    iEnd = index * (-1) + 1;
                    fibonacciNumbers2 = -1;
                }

                if (index > 0)
                {
                    iEnd = index + 1;
                    fibonacciNumbers2 = 1;
                }

                int[] fibonacciNumbers = new int[iEnd+1];
                fibonacciNumbers[0] = 0;
                fibonacciNumbers[1] = 1;
                fibonacciNumbers[2] = fibonacciNumbers2;

                for (int i = 3; i < iEnd; i++)  //4
                {
                    fibonacciNumbers[i] = fibonacciNumbers[i - 1] + fibonacciNumbers[i - 2];
                    Console.WriteLine("Fibonacci:  {0}: {1}", i, string.Join(", ", fibonacciNumbers.Select(n => string.Format("{0,5:##0}", n))));
                    Console.WriteLine("------");
                }

                Assert.AreEqual(expFibonacciNumber, fibonacciNumbers[iEnd - 1]);
            }
        }

    }
}

