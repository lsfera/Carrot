using System.Diagnostics.Tracing;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Environments;
using BenchmarkDotNet.Jobs;
using Microsoft.Diagnostics.NETCore.Client;
using Microsoft.Diagnostics.Tracing.Parsers;

namespace Carrot.Benchmarks
{
    internal class CustomConfig : ManualConfig
    {
        public CustomConfig()
        {
            AddJob(Job.ShortRun.WithRuntime(CoreRuntime.Core30));

            var providers = new[] // <-- custom list of providers
            {
                new EventPipeProvider(ClrTraceEventParser.ProviderName, EventLevel.Verbose,
                    (long) (ClrTraceEventParser.Keywords.Exception
                            | ClrTraceEventParser.Keywords.GC
                            | ClrTraceEventParser.Keywords.Jit
                            | ClrTraceEventParser.Keywords.JitTracing // for the inlining events
                            | ClrTraceEventParser.Keywords.Loader
                            | ClrTraceEventParser.Keywords.NGen)),
                new EventPipeProvider("System.Buffers.ArrayPoolEventSource", EventLevel.Informational, long.MaxValue),
            };

            AddDiagnoser(new EventPipeProfiler(providers: providers));  //<-- Adds new profiler
        }
    }
}