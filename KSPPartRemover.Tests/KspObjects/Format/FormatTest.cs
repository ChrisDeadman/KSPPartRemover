using System;
using System.IO;
using System.Text;
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
            var textIn = new StreamReader (Assembly.GetExecutingAssembly ().GetManifestResourceStream ("KSPPartRemover.Tests.Resources.Mün Mk I.in.craft")).ReadToEnd ();

            // when
            var tokenIn = KspTokenReader.ReadToken (textIn);
            var kspObject = KspObjectReader.ReadObject (tokenIn);
            var tokenOut = KspObjectWriter.WriteObject (kspObject);
            var textOut = KspTokenWriter.WriteToken (tokenOut, new StringBuilder ()).ToString ();

            // then
            Assert.That (textOut, Is.EqualTo (textIn));
        }

        [Test]
        public void CanReadAndWriteKspObjectsFromSaveFileString ()
        {
            // given
            var textIn = new StreamReader (Assembly.GetExecutingAssembly ().GetManifestResourceStream ("KSPPartRemover.Tests.Resources.Refuel at Minmus.in.sfs")).ReadToEnd ();

            // when
            var tokenIn = KspTokenReader.ReadToken (textIn);
            var kspObject = KspObjectReader.ReadObject (tokenIn);
            var tokenOut = KspObjectWriter.WriteObject (kspObject);
            var textOut = KspTokenWriter.WriteToken (tokenOut, new StringBuilder ()).ToString ();

            // then
            Assert.That (textOut, Is.EqualTo (textIn));
        }
    }
}
