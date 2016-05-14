using System;
using System.Linq;
using System.Collections.Generic;

namespace KSPPartRemover
{
    public class CommandLineParser
    {
        private readonly List<Parser> parsers = new List<Parser> ();

        private int currentArgNum = 0;

        private Action<String> handleError;

        public CommandLineParser RequiredArgument (int argIdx, Action<String> handler)
        {
            return Argument (argIdx, true, handler);
        }

        public CommandLineParser OptionalArgument (int argIdx, Action<String> handler)
        {
            return Argument (argIdx, false, handler);
        }

        public CommandLineParser RequiredSwitch (String name, Action handler)
        {
            return Switch (name, true, handler);
        }

        public CommandLineParser OptionalSwitch (String name, Action handler)
        {
            return Switch (name, false, handler);
        }

        public CommandLineParser RequiredSwitchArg <TArg> (String name, Action<TArg> handler)
        {
            return SwitchArg<TArg> (name, true, handler);
        }

        public CommandLineParser OptionalSwitchArg <TArg> (String name, Action<TArg> handler)
        {
            return SwitchArg<TArg> (name, false, handler);
        }

        public CommandLineParser Argument (int argIdx, bool required, Action<String> handler)
        {
            var argHandler = new Parser {
                Parse = argQueue => (!argQueue.Peek ().StartsWith ("-") && currentArgNum++ == argIdx) ? argQueue.Dequeue () : null,
                Accept = arg => handler ((String)arg)
            };

            if (required) {
                argHandler.Error = $"Required argument {argIdx} missing";
            }

            parsers.Add (argHandler);
            return this;
        }

        public CommandLineParser Switch (String name, bool required, Action handler)
        {
            var argHandler = new Parser {
                Parse = argQueue => name.Equals (argQueue.Peek ()) ? argQueue.Dequeue () : null,
                Accept = n => handler ()
            };

            if (required) {
                argHandler.Error = $"Required switch '{name}' missing";
            }

            parsers.Add (argHandler);
            return this;
        }

        public CommandLineParser SwitchArg <TArg> (String name, bool required, Action<TArg> handler)
        {
            var argHandler = new Parser ();
            argHandler.Parse = argQueue => {
                if (!name.Equals (argQueue.Peek ())) {
                    return null;
                }

                if (argQueue.Count < 2) {
                    argHandler.Error = null;
                    return null;
                }

                argQueue.Dequeue ();
                var switchArg = argQueue.Dequeue ();

                TArg arg;
                if (ParseArg (switchArg, out arg)) {
                    return arg;
                }

                argHandler.Error = $"'{name}': '{switchArg}' is not of expected type {typeof(TArg).Name}";
                return null;
            };
            argHandler.Accept = arg => handler ((TArg)arg);

            if (required) {
                argHandler.Error = $"Required switch argument '{name}' missing";
            }

            parsers.Add (argHandler);
            return this;
        }

        public CommandLineParser OnError (Action<String> handleError)
        {
            this.handleError = handleError;
            return this;
        }

        public void Parse (params String[] args)
        {
            currentArgNum = 0;

            var argQueue = new Queue<String> (args);
            while (argQueue.Count > 0) {
                if (!parsers.Any (parser => parser.Handle (argQueue))) {
                    handleError?.Invoke ($"Error while parsing argument '{argQueue.Dequeue()}'");
                }
            }

            parsers.ForEach (handler => handler.Finish (handleError));
        }

        private static bool ParseArg<TArg> (String text, out TArg arg)
        {
            var parse = StringParsers [typeof(TArg)];

            var result = parse (text);
            if (result == null) {
                arg = default(TArg);
                return false;
            }

            arg = (TArg)result;
            return true;
        }

        private static readonly Dictionary<Type, Func<String, Object>> StringParsers = new Dictionary<Type, Func<String, object>> {
            [typeof(String) ] = str => str,
            [typeof(int) ] = str => {
                int result;
                return int.TryParse (str, out result) ? (Object)result : null;
            }
        };

        private class Parser
        {
            public Func<Queue<String>, Object> Parse { get; set; }

            public Action<Object> Accept { get; set; }

            public String Error { get; set; }

            private bool handled;

            public bool Handle (Queue<String> args)
            {
                if (handled || args.Count < 1) {
                    return false;
                }

                var result = Parse (args);
                if (result == null) {
                    return false;
                }

                Accept (result);
                return handled = true;
            }

            public void Finish (Action<String> handleError)
            {
                if (!handled && Error != null) {
                    handleError?.Invoke (Error);
                }
            }
        }
    }
}
