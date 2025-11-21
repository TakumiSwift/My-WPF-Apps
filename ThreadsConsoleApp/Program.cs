using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ThreadsConsoleApp
{
    class Program
    {

        public static List<int[,]> GetRandomMatrix ()
        {
            Random r = new Random();
            int[,] matrixA = new int[1000, 2000];
            int[,] matrixB = new int[2000, 3000];
            for(int i = 0; i < 1000; i++)
            {
                for(int j = 0; j < 2000; j++)
                {
                    matrixA[i, j] = r.Next(0, 2);
                }
            }
            for (int i = 0; i < 1000; i++)
            {
                for (int j = 0; j < 2000; j++)
                {
                    matrixB[i, j] = r.Next(0, 2);
                }
            }
            List<int[,]> result = new List<int[,]>() { matrixA, matrixB };
            return result;
        }

        static void Main(string[] args)
        {
            var matrix = GetRandomMatrix();
            int[,] result = new int[1000, 3000];
            Parallel.For(0, 1000, i =>
            {
                for(int j = 0; j < 3000; j++)
                {
                    int sum = 0;
                    for(int k = 0; k < 2000; k++)
                    {
                        sum += matrix[0][i, k] * matrix[1][k, j];
                    }
                    result[i, j] = sum;
                }
            });
            Console.WriteLine("Выполнено");
            Console.ReadLine();
        }
    }
}
