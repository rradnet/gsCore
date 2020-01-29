using System;

namespace gsCore.FunctionalTests.Utility
{
    public interface ILogger
    {
        void WriteLine(string s);
    }

    public class ConsoleLogger : ILogger
    {
        public void WriteLine(string s)
        {
            Console.WriteLine(s);
        }
    }
}