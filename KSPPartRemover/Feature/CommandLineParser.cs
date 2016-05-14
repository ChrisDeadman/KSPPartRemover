using System;
using System.Linq;
using System.Collections.Generic;

namespace KSPPartRemover
{
    public class CommandLineParser
    {
        private static readonly Dictionary<Type, Func<String, Object>> ArgumentParsers = new Dictionary<Type, Func<String, object>> {
            [typeof(String) ] = str => str,
            [typeof(int) ] = str => {
                int result;
                return int.TryParse (str, out result) ? (Object)result : null;
            }
        };

        private readonly List<ArgumentHandler> argumentHandlers = new List<ArgumentHandler> ();

        private Action<String> errorHandler;

        public CommandLineParser Command (Action<String> handler)
        {
            String error = "Command argument missing";

            argumentHandlers.Add (new ArgumentHandler (
                (idx, current, next) => {
                    if (current == null || idx != 0) {
                        return error;
                    }
                    handler (current);
                    return error = null;
                }
            ));

            return this;
        }

        public CommandLineParser Required (String name, Action handler)
        {
            String error = $"Required argument '{name}' missing";

            argumentHandlers.Add (new ArgumentHandler (
                (idx, current, next) => {
                    if (current == null || !name.Equals (current)) {
                        return error;
                    }
                    handler ();
                    return error = null;
                }
            ));

            return this;
        }

        public CommandLineParser Required <TArg> (String name, Action<TArg> handler)
        {
            String error = $"Required argument '{name}' missing";

            argumentHandlers.Add (new ArgumentHandler (
                (idx, current, next) => {
                    if (current == null || next == null || !name.Equals (current)) {
                        return error;
                    }
                    TArg arg;
                    if (!ParseArg (next, out arg)) {
                        return error = $"Argument '{name}': '{next}' is not of expected type {typeof(TArg).Name}";
                    }

                    handler (arg);
                    return error = null;
                }
            ));

            return this;
        }

        public CommandLineParser Optional (String name, Action handler)
        {
            argumentHandlers.Add (new ArgumentHandler (
                (idx, current, next) => {
                    if (current == null || !name.Equals (current)) {
                        return null;
                    }
                    handler ();
                    return null;
                }
            ));

            return this;
        }

        public CommandLineParser Optional <TArg> (String name, Action<TArg> handler)
        {
            String error = null;

            argumentHandlers.Add (new ArgumentHandler (
                (idx, current, next) => {
                    if (current == null || next == null || !name.Equals (current)) {
                        return error;
                    }
                    TArg arg;
                    if (!ParseArg (next, out arg)) {
                        return error = $"Argument '{name}': '{next}' is not of expected type {typeof(TArg).Name}";
                    }

                    handler (arg);
                    return error = null;
                }
            ));

            return this;
        }

        public CommandLineParser Error (Action<String> handler)
        {
            errorHandler = handler;
            return this;
        }

        public void Parse (params String[] args)
        {
            var enumerator = args.ToList ().GetEnumerator ();
            var current = enumerator.MoveNext () ? enumerator.Current : null;

            var idx = 0;
            do {
                var next = enumerator.MoveNext () ? enumerator.Current : null;

                foreach (var handler in argumentHandlers) {
                    try {
                        handler.Handle (idx, current, next);
                    } catch (Exception ex) {
                        errorHandler?.Invoke (ex.Message);
                        return;
                    }
                }

                current = next;
                idx++;
            } while (current != null);

            argumentHandlers.ForEach (handler => handler.Finish (errorHandler));
        }

        private static bool ParseArg<TArg> (String text, out TArg arg)
        {
            var parse = ArgumentParsers [typeof(TArg)];

            var result = parse (text);
            if (result == null) {
                arg = default(TArg);
                return false;
            }

            arg = (TArg)result;
            return true;
        }

        private class ArgumentHandler
        {
            private readonly Func<int, String, String, String> handle;

            private String error;

            public ArgumentHandler (Func<int, String, String, String> handle)
            {
                this.handle = handle;
            }

            public void Handle (int idx, String current, String next)
            {
                error = handle?.Invoke (idx, current, next);
            }

            public void Finish (Action<String> errorHandler)
            {
                if (error != null) {
                    errorHandler?.Invoke (error);
                }
            }
        }
    }
}
