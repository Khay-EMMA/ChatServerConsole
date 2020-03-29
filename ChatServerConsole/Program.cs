using System;
using System.Threading;
using SystemUtility;

namespace ClientServerConsole
{
    class Program
    {
        static Thread _mainThread = new Thread(MainThread);
        static void Main(string[] args)
        {


            _mainThread.Name = "Main thread";
            Console.Title = "Chat Server";

            Text.WriteLine($"Initializing {_mainThread.Name}",TextType.DEBUG);
            _mainThread.Start();
        }

        static void MainThread()
        {
            General.InitServer();
            while (true)
            {
                
            }
        }
    }
}
