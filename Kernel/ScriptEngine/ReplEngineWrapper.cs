using System;
using System.IO;
using Common.Logging;
using ScriptCs;
using ScriptCs.Contracts;

using ILog = Common.Logging.ILog;

namespace iCSharp.Kernel.ScriptEngine
{
    internal class ReplEngineWrapper : IReplEngine
    {
        private readonly ILog logger;
        private readonly Repl repl;
        private readonly MemoryBufferConsole console;

        private readonly TextWriter _orgConsoleOut;
        private readonly TextWriter _orgConsoleError;
        private readonly StringWriter _tempConsoleOut;
        private readonly StringWriter _tempConsolerError;

        public ReplEngineWrapper(ILog logger, Repl repl, MemoryBufferConsole console)
        {
            this.logger = logger;
            this.repl = repl;
            this.console = console;

            _orgConsoleOut = Console.Out;
            _orgConsoleError = Console.Error;
            _tempConsoleOut = new StringWriter();
            _tempConsolerError = new StringWriter();
        }

        private ScriptResult ExecuteAndCatchConsoleOutputs(Func<String, string[], ScriptResult> replExecute, String script,
            out String consoleOut, out String consoleError)
        {
            Console.SetOut(_tempConsoleOut);
            Console.SetError(_tempConsolerError);

            ScriptResult scriptResult = replExecute(script, new[]  {""});

            consoleOut = _tempConsoleOut.ToString();
            _tempConsoleOut.GetStringBuilder().Clear();

            consoleError = _tempConsolerError.ToString();
            _tempConsolerError.GetStringBuilder().Clear();

            Console.SetOut(_orgConsoleOut);
            Console.SetError(_orgConsoleError);

            return scriptResult;
        }

        public ExecutionResult Execute(string script)
        {
            String consoleOut;
            String consoleError;

            this.logger.Debug(string.Format("Executing: {0}", script));
            this.console.ClearAllInBuffer();

            ScriptResult scriptResult = ExecuteAndCatchConsoleOutputs(repl.Execute, script, out consoleOut, out consoleError);

            if (consoleOut != "")
            {
                console.WriteLine(consoleOut);
            }

            if (consoleError != "")
            {
                console.WriteLine(consoleError, ConsoleColor.Red);
            }

            ExecutionResult executionResult = new ExecutionResult()
            {
                OutputResultWithColorInformation = this.console.GetAllInBuffer()
            };

            return executionResult;
        }

        private bool IsCompleteResult(ScriptResult scriptResult)
        {
            return scriptResult.ReturnValue != null && !string.IsNullOrEmpty(scriptResult.ReturnValue.ToString());
        }

        

        
    }
}
