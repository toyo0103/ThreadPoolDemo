using System;
using System.Collections;
using System.Threading;

namespace ThreadPoolDemo
{
    class Program
    {
        static void Main(string[] args)
        {
            // 建立自己的 ThreadPool ，設定 Thread 至少保持 3 條，上限不超過 10 條 
            MyThreadPool myTreadPool = new MyThreadPool(3,10);

            // 一值塞任務進去,不管任務執行完了沒
            // 所以 ThreadPool 應該要能接住 Task 讓它們排隊消耗 
            for (int i = 0; i < 200; i++)
            {
                myTreadPool.Enqueue(i);
            }

            Console.ReadKey();
            // 等待執行結束
            myTreadPool.WaitFinished();
        }
    }
}
