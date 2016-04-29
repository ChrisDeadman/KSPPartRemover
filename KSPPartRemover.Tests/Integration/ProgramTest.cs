using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using NUnit.Framework;
using KSPPartRemover.KspObjects;
using KSPPartRemover.KspObjects.Format;
using KSPPartRemover.Extension;

namespace KSPPartRemover.Tests.Integration
{
    public class ProgramTest
    {
        private static readonly StringBuilder StdOutput = new StringBuilder ();
        private static readonly StringWriter StdOutputWriter = new StringWriter (StdOutput);

        [TestFixtureSetUp]
        public static void TestFixtureSetUp ()
        {
            Console.SetOut (StdOutputWriter);
        }

        [TestFixtureTearDown]
        public static void TestFixtureTearDown ()
        {
            StdOutputWriter.Dispose ();
        }

        [TearDown]
        public void TearDown ()
        {
            StdOutput.Clear ();
        }

        [Test]
        public void PrintsUsageOnError ()
        {
            // when
            Program.Main ();

            // then
            Assert.That (StdOutput.ToString (), Is.StringContaining ("usage: "));
        }

        [Test]
        public void PrintsErrorMessageOnError ()
        {
            // when
            Program.Main ();

            // then
            Assert.That (StdOutput.ToString (), Is.StringContaining ("ERROR: "));
        }

        [Test]
        public void HasReturnValueLessThanZeroIfArgumentsAreInvalid ()
        {
            // when
            var returnCode = Program.Main ();

            // then
            Assert.That (returnCode, Is.LessThan (0));
        }

        [Test]
        public void CanRemovePartByIdAndOutputResult ()
        {
            // given
            var inputCraft = new KspCraftObject (isGlobalObject : true).AddProperty (new KspStringProperty ("name", "test"))
                .AddChild (new KspObject ("NOT_A_PART").AddProperty (new KspStringProperty ("name", "fuelTank")))
                .AddChild (new KspPartObject ().AddProperty (new KspStringProperty ("name", "fuelTank")))
                .AddChild (new KspPartObject ().AddProperty (new KspStringProperty ("name", "strut")))
                .AddChild (new KspPartObject ().AddProperty (new KspStringProperty ("name", "fuelTank")));

            var expectedCraft = new KspCraftObject (isGlobalObject : true).AddProperty (new KspStringProperty ("name", "test"))
                .AddChild (new KspObject ("NOT_A_PART").AddProperty (new KspStringProperty ("name", "fuelTank")))
                .AddChild (new KspPartObject ().AddProperty (new KspStringProperty ("name", "strut")))
                .AddChild (new KspPartObject ().AddProperty (new KspStringProperty ("name", "fuelTank")));
            
            var inputText = KspObjectWriter.WriteObject (inputCraft, new StringBuilder ()).ToString ();
            var expectedResult = KspObjectWriter.WriteObject (expectedCraft, new StringBuilder ()).ToString ();

            // when
            File.WriteAllText ("input.txt", inputText);
            var returnCode = Program.Main ("remove-part", "-p", "0", "-i", "input.txt", "-s");

            // then
            Assert.That (StdOutput.ToString (), Is.EqualTo (expectedResult));
            Assert.That (returnCode, Is.EqualTo (0));
        }

        [Test]
        public void CanRemovePartByNameAndOutputResult ()
        {
            // given
            var inputCraft = new KspCraftObject (isGlobalObject : true).AddProperty (new KspStringProperty ("name", "test"))
                .AddChild (new KspObject ("NOT_A_PART").AddProperty (new KspStringProperty ("name", "fuelTank")))
                .AddChild (new KspPartObject ().AddProperty (new KspStringProperty ("name", "fuelTank")))
                .AddChild (new KspPartObject ().AddProperty (new KspStringProperty ("name", "strut")))
                .AddChild (new KspPartObject ().AddProperty (new KspStringProperty ("name", "fuelTank")));

            var expectedCraft = new KspCraftObject (isGlobalObject : true).AddProperty (new KspStringProperty ("name", "test"))
                .AddChild (new KspObject ("NOT_A_PART").AddProperty (new KspStringProperty ("name", "fuelTank")))
                .AddChild (new KspPartObject ().AddProperty (new KspStringProperty ("name", "strut")));
            
            var inputText = KspObjectWriter.WriteObject (inputCraft, new StringBuilder ()).ToString ();
            var expectedResult = KspObjectWriter.WriteObject (expectedCraft, new StringBuilder ()).ToString ();

            // when
            File.WriteAllText ("input.txt", inputText);
            var returnCode = Program.Main ("remove-part", "-p", "fuelTank", "-i", "input.txt", "-s");

            // then
            Assert.That (StdOutput.ToString (), Is.EqualTo (expectedResult));
            Assert.That (returnCode, Is.EqualTo (0));
        }

        [Test]
        public void CanRemovePartsOfMultipleCraftsAndOutputResult ()
        {
            // given
            var inputCrafts = new KspObject ("GAME")
                .AddChild (new KspCraftObject ().AddProperty (new KspStringProperty ("name", "craft1"))
                    .AddChild (new KspPartObject ().AddProperty (new KspStringProperty ("name", "fuelTank")))
                    .AddChild (new KspPartObject ().AddProperty (new KspStringProperty ("name", "strut")))
                    .AddChild (new KspPartObject ().AddProperty (new KspStringProperty ("name", "fuelTank"))))
                .AddChild (new KspCraftObject ().AddProperty (new KspStringProperty ("name", "craft2"))
                    .AddChild (new KspPartObject ().AddProperty (new KspStringProperty ("name", "fuelTank")))
                    .AddChild (new KspPartObject ().AddProperty (new KspStringProperty ("name", "strut")))
                    .AddChild (new KspPartObject ().AddProperty (new KspStringProperty ("name", "fuelTank"))))
                .AddChild (new KspCraftObject ().AddProperty (new KspStringProperty ("name", "craft3"))
                    .AddChild (new KspPartObject ().AddProperty (new KspStringProperty ("name", "fuelTank")))
                    .AddChild (new KspPartObject ().AddProperty (new KspStringProperty ("name", "strut")))
                    .AddChild (new KspPartObject ().AddProperty (new KspStringProperty ("name", "fuelTank"))));

            var expectedCrafts = new KspObject ("GAME")
                .AddChild (new KspCraftObject ().AddProperty (new KspStringProperty ("name", "craft1"))
                    .AddChild (new KspPartObject ().AddProperty (new KspStringProperty ("name", "strut"))))
                .AddChild (new KspCraftObject ().AddProperty (new KspStringProperty ("name", "craft2"))
                    .AddChild (new KspPartObject ().AddProperty (new KspStringProperty ("name", "fuelTank")))
                    .AddChild (new KspPartObject ().AddProperty (new KspStringProperty ("name", "strut")))
                    .AddChild (new KspPartObject ().AddProperty (new KspStringProperty ("name", "fuelTank"))))
                .AddChild (new KspCraftObject ().AddProperty (new KspStringProperty ("name", "craft3"))
                    .AddChild (new KspPartObject ().AddProperty (new KspStringProperty ("name", "strut"))));
                    
            var inputText = KspObjectWriter.WriteObject (inputCrafts, new StringBuilder ()).ToString ();
            var expectedResult = KspObjectWriter.WriteObject (expectedCrafts, new StringBuilder ()).ToString ();

            // when
            File.WriteAllText ("input.txt", inputText);
            var returnCode = Program.Main ("remove-part", "-p", "fuelTank", "-c", ".*raft[1,3]", "-i", "input.txt", "-s");

            // then
            Assert.That (StdOutput.ToString (), Is.EqualTo (expectedResult));
            Assert.That (returnCode, Is.EqualTo (0));
        }

        [Test]
        public void CanPrintCraftList ()
        {
            // given
            var inputCrafts = new KspObject ("GAME")
                .AddChild (new KspCraftObject ().AddProperty (new KspStringProperty ("name", "someCraft")))
                .AddChild (new KspCraftObject ().AddProperty (new KspStringProperty ("name", "anotherCraft")))
                .AddChild (new KspCraftObject ().AddProperty (new KspStringProperty ("name", "ignored")));
            
            var inputText = KspObjectWriter.WriteObject (inputCrafts, new StringBuilder ()).ToString ();

            var expectedResult = "someCraft" + Environment.NewLine + "anotherCraft" + Environment.NewLine;

            // when
            File.WriteAllText ("input.txt", inputText);
            var returnCode = Program.Main ("list-crafts", "-c", ".*Craft", "-i", "input.txt");

            // then
            Assert.That (StdOutput.ToString (), Is.StringEnding (expectedResult));
            Assert.That (returnCode, Is.EqualTo (0));
        }

        [Test]
        public void CanPrintPartList ()
        {
            // given
            var inputCrafts = new KspObject ("GAME")
                .AddChild (new KspCraftObject ().AddProperty (new KspStringProperty ("name", "someCraft"))
                    .AddChild (new KspPartObject ().AddProperty (new KspStringProperty ("name", "fuelTank")))
                    .AddChild (new KspPartObject ().AddProperty (new KspStringProperty ("name", "strut"))))
                .AddChild (new KspCraftObject ().AddProperty (new KspStringProperty ("name", "anotherCraft"))
                    .AddChild (new KspPartObject ().AddProperty (new KspStringProperty ("name", "strut")))
                    .AddChild (new KspPartObject ().AddProperty (new KspStringProperty ("name", "fuelTank")))
                    .AddChild (new KspPartObject ().AddProperty (new KspStringProperty ("name", "ignored"))))
                .AddChild (new KspCraftObject ().AddProperty (new KspStringProperty ("name", "ignored"))
                    .AddChild (new KspPartObject ().AddProperty (new KspStringProperty ("name", "somePart"))));
            
            var inputText = KspObjectWriter.WriteObject (inputCrafts, new StringBuilder ()).ToString ();

            var expectedResult =
                "someCraft:" + Environment.NewLine +
                "\t[0]fuelTank" + Environment.NewLine +
                "\t[1]strut" + Environment.NewLine +
                "anotherCraft:" + Environment.NewLine +
                "\t[0]strut" + Environment.NewLine +
                "\t[1]fuelTank" + Environment.NewLine;

            // when
            File.WriteAllText ("input.txt", inputText);
            var returnCode = Program.Main ("list-parts", "-p", "[s,f].*", "-c", ".*Craft", "-i", "input.txt");

            // then
            Assert.That (StdOutput.ToString (), Is.StringEnding (expectedResult));
            Assert.That (returnCode, Is.EqualTo (0));
        }


        [Test]
        public void CanPrintPartDependencies ()
        {
            // given
            var inputCrafts = new KspObject ("GAME")
                .AddChild (new KspCraftObject ().AddProperty (new KspStringProperty ("name", "someCraft"))
                    .AddChild (new KspPartObject ().AddProperty (new KspStringProperty ("name", "fuelTank1")))
                    .AddChild (new KspPartObject ().AddProperty (new KspStringProperty ("name", "strut"))))
                .AddChild (new KspCraftObject ().AddProperty (new KspStringProperty ("name", "anotherCraft"))
                    .AddChild (new KspPartObject ().AddProperty (new KspStringProperty ("name", "strut")))
                    .AddChild (new KspPartObject ().AddProperty (new KspStringProperty ("name", "fuelTank2"))))
                .AddChild (new KspCraftObject ().AddProperty (new KspStringProperty ("name", "ignored"))
                    .AddChild (new KspPartObject ().AddProperty (new KspStringProperty ("name", "somePart"))));

            var craft1 = inputCrafts.Children [0];
            var craft2 = inputCrafts.Children [1];

            craft1.Children [0].AddProperty (new KspPartLinkProperty ("link", null, (KspPartObject)craft1.Children [1], true));
            craft1.Children [1].AddProperty (new KspPartLinkProperty ("parent", null, (KspPartObject)craft1.Children [0], false));
            craft1.Children [1].AddProperty (new KspPartLinkProperty ("sym", "top", (KspPartObject)craft1.Children [0], false));
            craft2.Children [0].AddProperty (new KspPartLinkProperty ("attN", "bottom", (KspPartObject)craft2.Children [1], false));

            var inputText = KspObjectWriter.WriteObject (inputCrafts, new StringBuilder ()).ToString ();

            var expectedResult =
                "someCraft:" + Environment.NewLine +
                "\t[1]strut:" + Environment.NewLine +
                "\t\t[0]fuelTank1[parent]" + Environment.NewLine +
                "\t\t[0]fuelTank1[sym(top)]" + Environment.NewLine +
                "anotherCraft:" + Environment.NewLine +
                "\t[0]strut:" + Environment.NewLine +
                "\t\t[1]fuelTank2[attN(bottom)]" + Environment.NewLine;
            
            // when
            File.WriteAllText ("input.txt", inputText);
            var returnCode = Program.Main ("list-partdeps", "-p", ".*uelTank.*", "-c", ".*Craft", "-i", "input.txt");

            // then
            Assert.That (StdOutput.ToString (), Is.StringEnding (expectedResult));
            Assert.That (returnCode, Is.EqualTo (0));
        }

        [Test]
        public void PrintsAndReturnsErrorIfPartIdToRemoveIsNotFound ()
        {
            // given
            var inputCraft = new KspCraftObject (isGlobalObject : true).AddProperty (new KspStringProperty ("name", "test"))
                .AddChild (new KspObject ("NOT_A_PART").AddProperty (new KspStringProperty ("name", "notAPart")))
                .AddChild (new KspPartObject ().AddProperty (new KspStringProperty ("name", "somePart")))
                .AddChild (new KspPartObject ().AddProperty (new KspStringProperty ("name", "anotherPart")));
            
            var inputText = KspObjectWriter.WriteObject (inputCraft, new StringBuilder ()).ToString ();

            // when
            File.WriteAllText ("input.txt", inputText);
            var returnCode = Program.Main ("remove-part", "-p", "2", "-i", "input.txt");

            // then
            Assert.That (StdOutput.ToString (), Is.StringContaining ("No parts matching '2' found"));
            Assert.That (returnCode, Is.LessThan (0));
        }

        [Test]
        public void PrintsAndReturnsErrorIfPartNameToRemoveIsNotFound ()
        {
            // given
            var inputCraft = new KspCraftObject (isGlobalObject : true).AddProperty (new KspStringProperty ("name", "test"))
                .AddChild (new KspObject ("NOT_A_PART").AddProperty (new KspStringProperty ("name", "notAPart")))
                .AddChild (new KspPartObject ().AddProperty (new KspStringProperty ("name", "somePart")))
                .AddChild (new KspPartObject ().AddProperty (new KspStringProperty ("name", "anotherPart")));

            var inputText = KspObjectWriter.WriteObject (inputCraft, new StringBuilder ()).ToString ();

            // when
            File.WriteAllText ("input.txt", inputText);
            var returnCode = Program.Main ("remove-part", "-p", "nonExistingPart", "-i", "input.txt");

            // then
            Assert.That (StdOutput.ToString (), Is.StringContaining ("No parts matching 'nonExistingPart' found"));
            Assert.That (returnCode, Is.LessThan (0));
        }

        [Test]
        public void PrintsAndReturnsErrorIfNoCraftWithMatchingCraftNameIsFound ()
        {
            // given
            var inputCrafts = new KspObject ("GAME")
                .AddChild (new KspCraftObject ().AddProperty (new KspStringProperty ("name", "someCraft")))
                .AddChild (new KspCraftObject ().AddProperty (new KspStringProperty ("name", "anotherCraft")));
            
            var inputText = KspObjectWriter.WriteObject (inputCrafts, new StringBuilder ()).ToString ();

            // when
            File.WriteAllText ("input.txt", inputText);
            var returnCode = Program.Main ("remove-part", "-p", "somePart", "--craft", "nonExistingCraft", "-i", "input.txt");

            // then
            Assert.That (StdOutput.ToString (), Is.StringContaining ("No craft matching 'nonExistingCraft' found, aborting"));
            Assert.That (returnCode, Is.LessThan (0));
        }
    }
}
