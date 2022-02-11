using System;
using System.Collections.Generic;

namespace KSPPartRemover
{
    public class CommandLineParser
    {
        private static readonly String ArgPrefix = "-";

        private readonly List<Parser> parsers = new List<Parser>();
        private readonly List<Parser> handledParsers = new List<Parser>();

        private int currentArgNum;

        private Action<String> handleError;

        public CommandLineParser RequiredArgument(int argIdx, Action<String> handler) => Argument(argIdx, true, handler);

        public CommandLineParser OptionalArgument(int argIdx, Action<String> handler) => Argument(argIdx, false, handler);

        public CommandLineParser RequiredSwitch(String name, Action handler) => Switch(name, null, true, handler);

        public CommandLineParser RequiredSwitch(String name, String longName, Action handler) => Switch(name, longName, true, handler);

        public CommandLineParser OptionalSwitch(String name, Action handler) => Switch(name, null, false, handler);

        public CommandLineParser OptionalSwitch(String name, String longName, Action handler) => Switch(name, longName, false, handler);

        public CommandLineParser RequiredSwitchArg<TArg>(String name, Action<TArg> handler) => SwitchArg<TArg>(name, null, true, handler);

        public CommandLineParser RequiredSwitchArg<TArg>(String name, String longName, Action<TArg> handler) => SwitchArg<TArg>(name, longName, true, handler);

        public CommandLineParser OptionalSwitchArg<TArg>(String name, Action<TArg> handler) => SwitchArg<TArg>(name, null, false, handler);

        public CommandLineParser OptionalSwitchArg<TArg>(String name, String longName, Action<TArg> handler) => SwitchArg<TArg>(name, longName, false, handler);

        public CommandLineParser Argument(int argIdx, bool required, Action<String> handler)
        {
            var argHandler = new Parser {
                Parse = argQueue => (!argQueue.Peek().StartsWith(ArgPrefix) && currentArgNum++ == argIdx) ? argQueue.Dequeue() : null,
                Accept = arg => handler((String)arg)
            };

            if (required) {
                argHandler.Error = $"Required argument {argIdx} missing";
            }

            parsers.Add(argHandler);
            return this;
        }

        public CommandLineParser Switch(String name, String longName, bool required, Action handler)
        {
            var argHandler = new Parser {
                Parse = argQueue => IsExpectedSwitch(name, longName, argQueue.Peek()) ? argQueue.Dequeue() : null,
                Accept = n => handler()
            };

            if (required) {
                argHandler.Error = $"Required switch '{name}' missing";
            }

            parsers.Add(argHandler);
            return this;
        }

        public CommandLineParser SwitchArg<TArg>(String name, String longName, bool required, Action<TArg> handler)
        {
            var argHandler = new Parser();
            argHandler.Parse = argQueue => {
                if (!IsExpectedSwitch(name, longName, argQueue.Peek())) {
                    return null;
                }

                argHandler.Error = null;

                if (argQueue.Count < 2) {
                    return null;
                }

                argQueue.Dequeue();
                var switchArg = argQueue.Dequeue();

                TArg arg;
                if (ParseArg(switchArg, out arg)) {
                    return arg;
                }

                argHandler.Error = $"'{name}': '{switchArg}' is not of expected type {typeof(TArg).Name}";
                return null;
            };
            argHandler.Accept = arg => handler((TArg)arg);

            if (required) {
                argHandler.Error = $"Required switch argument '{name}' missing";
            }

            parsers.Add(argHandler);
            return this;
        }

        public CommandLineParser OnError(Action<String> handleError)
        {
            this.handleError = handleError;
            return this;
        }

        public void Parse(params String[] args)
        {
            currentArgNum = 0;
            handledParsers.Clear();

            var argQueue = new Queue<String>(args);
            while (argQueue.Count > 0) {
                try {
                    var handled = false;

                    foreach (var parser in parsers) {
                        if (handledParsers.Contains(parser)) {
                            continue;
                        }

                        if (handled = parser.Handle(argQueue)) {
                            handledParsers.Add(parser);
                            break;
                        }
                    }

                    if (!handled) {
                        handleError?.Invoke($"Error while parsing argument '{argQueue.Dequeue()}'");
                    }
                } catch (Exception ex) {
                    handleError?.Invoke(ex.Message);
                }
            }

            foreach (var parser in parsers) {
                if (!handledParsers.Contains(parser) && parser.Error != null) {
                    handleError?.Invoke(parser.Error);
                }
            }
        }

        private static bool IsExpectedSwitch(String name, String longName, String cmdLine)
        {
            if (name != null && IsExpectedSwitch(name, cmdLine)) {
                return true;
            }
            if (longName != null && IsExpectedSwitch(longName, cmdLine)) {
                return true;
            }
            return false;
        }

        private static bool IsExpectedSwitch(String name, String cmdLine)
        {
            var expected = (name.Length <= 1)
                ? $"{ArgPrefix}{name}"
                : $"{ArgPrefix}{ArgPrefix}{name}";

            return expected.Equals(cmdLine);
        }

        private static bool ParseArg<TArg>(String text, out TArg arg)
        {
            var parse = Deserializers[typeof(TArg)];

            var result = parse(text);
            if (result == null) {
                arg = default(TArg);
                return false;
            }

            arg = (TArg)result;
            return true;
        }

        private static readonly Dictionary<Type, Func<String, Object>> Deserializers = new Dictionary<Type, Func<String, object>> {
            [typeof(String)] = str => str,
            [typeof(int)] = str => {
                int result;
                return int.TryParse(str, out result) ? (Object)result : null;
            }
        };

        private class Parser
        {
            public Func<Queue<String>, Object> Parse { get; set; }

            public Action<Object> Accept { get; set; }

            public String Error { get; set; }

            public bool Handle(Queue<String> args)
            {
                if (args.Count < 1) {
                    return false;
                }

                var result = Parse(args);
                if (result == null) {
                    return false;
                }

                Accept(result);
                return true;
            }
        }
    }
}
