using System.IO;
using System.Text;
using System.Reflection;
using NUnit.Framework;
using KSPPartRemover.KspFormat;

namespace KSPPartRemover.Tests.KspFormat
{
    public class KspFormatTest
    {
        [Test]
        public void CanReadAndWriteKspObjectsFromCraftFileString()
        {
            // given
            var textIn = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("KSPPartRemover.Tests.Resources.Super-Heavy Lander.in.craft")).ReadToEnd();

            // when
            var tokenIn = KspTokenReader.ReadToken(textIn);
            var kspObject = KspObjectReader.ReadObject(tokenIn);
            var tokenOut = KspObjectWriter.WriteObject(kspObject);
            var textOut = KspTokenWriter.WriteToken(tokenOut, new StringBuilder()).ToString();

            // then
            Assert.That(textOut, Is.EqualTo(textIn));
        }

        [Test]
        public void CanReadAndWriteKspObjectsFromSaveFileString()
        {
            // given
            var textIn = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("KSPPartRemover.Tests.Resources.Dynawing.sfs")).ReadToEnd();

            // when
            var tokenIn = KspTokenReader.ReadToken(textIn);
            var kspObject = KspObjectReader.ReadObject(tokenIn);
            var tokenOut = KspObjectWriter.WriteObject(kspObject);
            var textOut = KspTokenWriter.WriteToken(tokenOut, new StringBuilder()).ToString();

            // then
            Assert.That(textOut, Is.EqualTo(textIn));
        }

        [Test]
        public void ReturnsEmptyObjectForEmptyFile()
        {
            // given
            var textIn = "";

            // when
            var tokenIn = KspTokenReader.ReadToken(textIn);
            var kspObject = KspObjectReader.ReadObject(tokenIn);
            var tokenOut = KspObjectWriter.WriteObject(kspObject);
            var textOut = KspTokenWriter.WriteToken(tokenOut, new StringBuilder()).ToString();

            // then
            Assert.That(textOut, Is.EqualTo(textIn));
        }
    }
}
