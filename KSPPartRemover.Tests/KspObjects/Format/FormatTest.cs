using System;
using System.IO;
using System.Reflection;
using NUnit.Framework;
using KSPPartRemover.KspObjects.Format;

namespace KSPPartRemover.Tests.KspObjects.Format
{
    public class FormatTest
    {
        [Test]
        public void CanReadAndWriteKspObjectsFromCraftFileString ()
        {
            // given
            var inputCraftText = new StreamReader (Assembly.GetExecutingAssembly ().GetManifestResourceStream ("KSPPartRemover.Tests.Resources.Mün Mk I.in.craft")).ReadToEnd ();

            // when
            var deserialized = KspObjectReader.ReadObject (inputCraftText);
            var serialized = KspObjectWriter.ToString (deserialized);

            // then
            Assert.That (serialized, Is.EqualTo (inputCraftText));
        }

        [Test]
        public void CanReadAndWriteKspObjectsFromSaveFileString ()
        {
            // given
            var inputCraftText = new StreamReader (Assembly.GetExecutingAssembly ().GetManifestResourceStream ("KSPPartRemover.Tests.Resources.Refuel at Minmus.in.sfs")).ReadToEnd ();

            // when
            var deserialized = KspObjectReader.ReadObject (inputCraftText);
            var serialized = KspObjectWriter.ToString (deserialized);

            // then
            Assert.That (serialized, Is.EqualTo (inputCraftText));
        }
    }
}
