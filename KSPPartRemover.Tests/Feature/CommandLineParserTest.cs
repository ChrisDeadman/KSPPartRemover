using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace KSPPartRemover.Tests.Feature
{
    public class CommandLineParserTest
    {
        [Test]
        public void CanParseRequiredArgumentAtSpecifiedPosition ()
        {
            // given
            var args = new [] {
                "arg0",
                "-s", "switchArg",
                "--unknown",
                "arg1"
            };

            var obtainedArgs = new List<String> ();
            var expectedArgs = new[] { "arg0", "arg1" };

            var obtainedErrors = new List<String> ();
            var expectedErrors = new[] {
                "Error while parsing argument '--unknown'",
                "Required argument 2 missing"
            };

            // when
            new CommandLineParser ()
                .OptionalSwitchArg<String> ("-s", s => s = null)
                .RequiredArgument (0, obtainedArgs.Add)
                .RequiredArgument (1, obtainedArgs.Add)
                .RequiredArgument (2, obtainedArgs.Add)
                .OnError (obtainedErrors.Add)
                .Parse (args);

            // then
            Assert.That (obtainedErrors, Is.EqualTo (expectedErrors));
            Assert.That (obtainedArgs, Is.EqualTo (expectedArgs));
        }

        [Test]
        public void CanParseRequiredSwitch ()
        {
            // given
            var args = new [] {
                "s",
                "-s",
                "--switch"
            };

            var obtainedSwitches = new List<String> ();
            var expectedSwitches = new[] { "-s", "--switch" };

            var obtainedErrors = new List<String> ();
            var expectedErrors = new[] {
                "Error while parsing argument 's'",
                "Required switch '--missing' missing"
            };

            // when
            new CommandLineParser ()
                .RequiredSwitch ("-s", () => obtainedSwitches.Add ("-s"))
                .RequiredSwitch ("--switch", () => obtainedSwitches.Add ("--switch"))
                .RequiredSwitch ("--missing", () => obtainedSwitches.Add ("--missing"))
                .OnError (obtainedErrors.Add)
                .Parse (args);

            // then
            Assert.That (obtainedErrors, Is.EqualTo (expectedErrors));
            Assert.That (obtainedSwitches, Is.EqualTo (expectedSwitches));
        }

        [Test]
        public void CanParseRequiredSwitchArg ()
        {
            // given
            var args = new [] {
                "x",
                "-x", "not an integer",
                "--int", "13",
                "--string", "hello",
                "--arg-missing"
            };

            var obtainedSwitchArguments = new List<Object> ();
            var expectedSwitchArguments = new Object[] { 13, "hello" };

            var obtainedErrors = new List<String> ();
            var expectedErrors = new[] {
                "Error while parsing argument 'x'",
                "Error while parsing argument '--arg-missing'",
                "'-x': 'not an integer' is not of expected type Int32",
                "Required switch argument '--missing' missing"
            };

            // when
            new CommandLineParser ()
                .RequiredSwitchArg<int> ("-x", arg => obtainedSwitchArguments.Add (arg))
                .RequiredSwitchArg<int> ("--int", arg => obtainedSwitchArguments.Add (arg))
                .RequiredSwitchArg<String> ("--string", arg => obtainedSwitchArguments.Add (arg))
                .RequiredSwitchArg<String> ("--arg-missing", arg => obtainedSwitchArguments.Add (arg))
                .RequiredSwitchArg<String> ("--missing", arg => obtainedSwitchArguments.Add (arg))
                .OnError (obtainedErrors.Add)
                .Parse (args);

            // then
            Assert.That (obtainedErrors, Is.EqualTo (expectedErrors));
            Assert.That (obtainedSwitchArguments, Is.EqualTo (expectedSwitchArguments));
        }

        [Test]
        public void CanParseOptionalArgumentAtSpecifiedPosition ()
        {
            // given
            var args = new [] {
                "arg0",
                "-s", "switchArg",
                "--unknown",
                "arg1"
            };

            var obtainedArgs = new List<String> ();
            var expectedArgs = new[] { "arg0", "arg1" };

            var obtainedErrors = new List<String> ();
            var expectedErrors = new[] { "Error while parsing argument '--unknown'" };

            // when
            new CommandLineParser ()
                .OptionalSwitchArg<String> ("-s", s => s = null)
                .OptionalArgument (0, obtainedArgs.Add)
                .OptionalArgument (1, obtainedArgs.Add)
                .OptionalArgument (2, obtainedArgs.Add)
                .OnError (obtainedErrors.Add)
                .Parse (args);

            // then
            Assert.That (obtainedErrors, Is.EqualTo (expectedErrors));
            Assert.That (obtainedArgs, Is.EqualTo (expectedArgs));
        }

        [Test]
        public void CanParseOptionalSwitch ()
        {
            // given
            var args = new [] {
                "s",
                "-s",
                "--switch"
            };

            var obtainedSwitches = new List<String> ();
            var expectedSwitches = new[] { "-s", "--switch" };

            var obtainedErrors = new List<String> ();
            var expectedErrors = new[] { "Error while parsing argument 's'" };

            // when
            new CommandLineParser ()
                .OptionalSwitch ("-s", () => obtainedSwitches.Add ("-s"))
                .OptionalSwitch ("--switch", () => obtainedSwitches.Add ("--switch"))
                .OptionalSwitch ("--missing", () => obtainedSwitches.Add ("--missing"))
                .OnError (obtainedErrors.Add)
                .Parse (args);

            // then
            Assert.That (obtainedErrors, Is.EqualTo (expectedErrors));
            Assert.That (obtainedSwitches, Is.EqualTo (expectedSwitches));
        }

        [Test]
        public void CanParseOptionalSwitchArg ()
        {
            // given
            var args = new [] {
                "x",
                "-x", "not an integer",
                "--int", "13",
                "--string", "hello",
                "--arg-missing"
            };

            var obtainedSwitchArguments = new List<Object> ();
            var expectedSwitchArguments = new Object[] { 13, "hello" };

            var obtainedErrors = new List<String> ();
            var expectedErrors = new[] {
                "Error while parsing argument 'x'",
                "Error while parsing argument '--arg-missing'",
                "'-x': 'not an integer' is not of expected type Int32"
            };

            // when
            new CommandLineParser ()
                .OptionalSwitchArg<int> ("-x", arg => obtainedSwitchArguments.Add (arg))
                .OptionalSwitchArg<int> ("--int", arg => obtainedSwitchArguments.Add (arg))
                .OptionalSwitchArg<String> ("--string", arg => obtainedSwitchArguments.Add (arg))
                .OptionalSwitchArg<String> ("--arg-missing", arg => obtainedSwitchArguments.Add (arg))
                .OptionalSwitchArg<String> ("--missing", arg => obtainedSwitchArguments.Add (arg))
                .OnError (obtainedErrors.Add)
                .Parse (args);

            // then
            Assert.That (obtainedErrors, Is.EqualTo (expectedErrors));
            Assert.That (obtainedSwitchArguments, Is.EqualTo (expectedSwitchArguments));
        }
    }
}
