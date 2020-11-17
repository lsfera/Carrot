using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Running;

namespace Carrot.Benchmarks
{
    public class Program
    {
        public static void Main(string[] args)
        {
            BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly)
                .Run(args
                    //, new DebugInProcessConfig() //enable debug
                );
        }
    }
}