using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace ThreadPoolDemo
{
    public class MyThreadPool
    {
        private BlockingCollection<int> _jobQueue;

        private int _minThread;

        private int _maxThread;

        private int _currentThreadCount;

        private ManualResetEvent _mre;

        private List<Thread> _threads;
        
        public MyThreadPool(int minThread,int maxThread)
        {
            this._minThread = minThread;
            this._maxThread = maxThread;
            // 最多只能放 100 個 Task
            this._jobQueue = new BlockingCollection<int>(100);
            this._currentThreadCount = 0;
            this._mre = new ManualResetEvent(false);
            this._threads = new List<Thread>();

            for (int i = 0; i < minThread; i++)
            {
                this.CreateThread();
            }
        }

        private void CreateThread()
        {
            int id = Interlocked.Increment(ref _currentThreadCount);
            if (id > _maxThread)
            {
                // 可開的 Thread 到達極限, 無法加開
                Interlocked.Decrement(ref _currentThreadCount);
                return;
            }

            Thread thread = new Thread(ThreadBody);
            thread.Name = $"Thread-{id}";
            thread.Start();

            this._threads.Add(thread);

            Console.WriteLine($"Thread count : {this._currentThreadCount}");
        }

        public void WaitFinished()
        {
            this._jobQueue.CompleteAdding();
            _mre.Set();

            foreach (var t in _threads)
            {
                if (t != null)
                {
                    t.Join();
                }
            }
        }

        public void Enqueue(int i)
        {
            _mre.Set();

            // 表示容量滿了
            while (_jobQueue.TryAdd(i) == false)
            {
                // Queue Length 過長, 需加開 Thread
                this.CreateThread();
            }

            _mre.Reset();
        }

        

        private void ThreadBody()
        {
            string name = Thread.CurrentThread.Name;
            Console.WriteLine(name + " starts");

            // 如果有 Task 還沒有塞完就一直搶來處理
            while (!this._jobQueue.IsCompleted)
            {
                int task = 0;
                while(_jobQueue.TryTake(out task, 100))
                {
                    Random rnd = new Random();
                    int excuteTime = 0;
                    excuteTime = rnd.Next(100, 500);
                    Console.WriteLine($"{name} do task_{task} spend {excuteTime} ms");
                    Thread.Sleep(excuteTime);
                }

                
                if (_mre.WaitOne(5000) == false)
                {
                    // 此條 Thread 5秒都沒有工作, 嘗試收掉
                    if (Interlocked.Decrement(ref _currentThreadCount) < _minThread)
                    {
                        Interlocked.Increment(ref _currentThreadCount);
                    }
                    else
                    {
                        Console.WriteLine($"Thread count : {this._currentThreadCount}");
                        break;
                    }
                }
            }

            Console.WriteLine(name + " are closed");
        }
    }
}
