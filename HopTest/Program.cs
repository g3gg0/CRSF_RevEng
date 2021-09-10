using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HopTest
{
    class Program
    {
        public static int[] hopSequence = new int[50] { 0, 6, 28, 11, 39, 23, 17, 9, 37, 1, 48, 38, 31, 3, 49, 46, 25, 15, 44, 8, 27, 47, 24, 2, 29, 10, 43, 40, 26, 42, 7, 4, 19, 33, 12, 5, 36, 22, 45, 14, 20, 35, 32, 13, 16, 30, 21, 0, 18, 28 /*, 11, 41, 23, 17, 34, 37, 1, 6, 38, 31, 39, 49, 46, 9, 15, 44, 48, 27, 47, 3, 2, 29, 25, 43, 40, 8, 42, 7, 24, 19, 33, 10, 5, 36, 26, 45, 14, 4, 35, 32, 12, 16, 30, 22, 0, 18, 20, 11, 41, 13, 17, 34, 21, 1, 6, 28, 31, 39, 23, 46, 9, 37, 44, 48, 38, 47, 3, 49, 29, 25, 15, 40, 8, 27, 7, 24, 2, 33, 10, 43, 36, 26, 42, 14, 4, 19, 32, 12, 5, 30, 22, 45, 18, 20, 35, 41, 13, 16, 34, 21 */};

        static void Main(string[] args)
        {
            int[] hopTest = new int[hopSequence.Length];

            for (int mod = hopSequence.Length; mod < 10000000; mod++)
            {
                for (int pos = 0; pos < hopSequence.Length; pos++)
                {
                    hopTest[pos] = (pos * mod) % hopSequence.Length;
                }

                if (hopTest.SequenceEqual(hopSequence))
                {
                    Console.WriteLine("Found: " + mod);
                    return;
                }
            }
        }
    }
}
