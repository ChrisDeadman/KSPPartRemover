using System.IO;
using System.Text;
using System.Reflection;
using System.Linq;
using NUnit.Framework;
using KSPPartRemover.KspFormat;
using KSPPartRemover.KspFormat.Objects;

namespace KSPPartRemover.Tests.KspFormat
{
    public class KspFormatTest
    {
        [Test]
        public void CanReadAndWriteEmptyObject()
        {
            // given
            var textIn = "";

            // when
            var tokenIn = KspTokenReader.ReadToken(textIn);
            var kspObject = KspObjectReader.ReadObject(tokenIn);

            // then
            Assert.That(kspObject.Type, Is.Empty);
            Assert.That(kspObject.Children, Is.Empty);

            // and when
            var tokenOut = KspObjectWriter.WriteObject(kspObject);
            var textOut = KspTokenWriter.WriteToken(tokenOut, new StringBuilder()).ToString();

            // then
            Assert.That(textOut, Is.EqualTo(textIn));
        }

        [Test]
        public void CanReadAndWriteKspObjectsFromCraftFileString()
        {
            // given
            var textIn = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("KSPPartRemover.Tests.Resources.Mün Mk I.in.craft")).ReadToEnd();

            // when
            var tokenIn = KspTokenReader.ReadToken(textIn);
            var kspObject = KspObjectReader.ReadObject(tokenIn) as KspCraftObject;

            // then
            Assert.That(kspObject.Name, Is.EqualTo("Mün Mk I"));
            Assert.That(kspObject.Properties<KspStringProperty>("version").First().Text, Is.EqualTo("0.23.5"));
            Assert.That(kspObject.Properties<KspStringProperty>("type").First().Text, Is.EqualTo("VAB"));
            Assert.That(kspObject.Children<KspObject>().First().Properties<KspStringProperty>("pos").First().Text, Is.EqualTo("-0.2561799,3.782969,0.2440315"));
            Assert.That(kspObject.Children<KspObject>().Skip(1).First().Properties<KspStringProperty>("pos").First().Text, Is.EqualTo("-0.2561799,5.661159,0.2440315"));
            Assert.That(kspObject.Children<KspObject>().Skip(2).First().Properties<KspStringProperty>("pos").First().Text, Is.EqualTo("-0.2561799,7.539349,0.2440315"));

            // and given
            textIn = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("KSPPartRemover.Tests.Resources.Super-Heavy Lander.in.craft")).ReadToEnd();

            // when
            tokenIn = KspTokenReader.ReadToken(textIn);
            kspObject = KspObjectReader.ReadObject(tokenIn) as KspCraftObject;
            var tokenOut = KspObjectWriter.WriteObject(kspObject);
            var textOut = KspTokenWriter.WriteToken(tokenOut, new StringBuilder()).ToString();

            // then
            Assert.That(textOut, Is.EqualTo(textIn));
        }

        [Test]
        public void CanReadAndWriteKspObjectsFromSaveFileString()
        {
            // given
            var textIn = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("KSPPartRemover.Tests.Resources.Refuel at Minmus.in.sfs")).ReadToEnd();

            // when
            var tokenIn = KspTokenReader.ReadToken(textIn);
            var kspObject = KspObjectReader.ReadObject(tokenIn);

            // then
            Assert.That(kspObject.Children<KspCraftObject>(recursive: true).Select(craft => craft.Name), Is.EqualTo(new[] {
                "Ast. HSJ-227",
                "Ast. LHV-865",
                "Ast. IQY-452",
                "Aeris 4B \"Maybe the Sky\"",
                "Ast. JCK-736",
                "Ast. JIH-531",
                "Ast. AYF-000",
                "Bowser 1",
                "Bowser 1 Debris",
                "Ast. JMV-788",
                "Bowser 1 Debris"
            }));

            // and given
            textIn = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("KSPPartRemover.Tests.Resources.Dynawing.sfs")).ReadToEnd();

            // when
            tokenIn = KspTokenReader.ReadToken(textIn);
            kspObject = KspObjectReader.ReadObject(tokenIn);
            var tokenOut = KspObjectWriter.WriteObject(kspObject);
            var textOut = KspTokenWriter.WriteToken(tokenOut, new StringBuilder()).ToString();

            // then
            Assert.That(textOut, Is.EqualTo(textIn));
        }

        [Test]
        public void CanReadAndWriteKspObjectsFromPartFileString()
        {
            // given
            var textIn = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("KSPPartRemover.Tests.Resources.Size3LargeTankPart.cfg")).ReadToEnd();

            // when
            var tokenIn = KspTokenReader.ReadToken(textIn);
            var kspObject = KspObjectReader.ReadObject(tokenIn) as KspPartObject;

            // then
            Assert.That(kspObject.Name, Is.EqualTo("Size3LargeTank"));
            Assert.That(kspObject.Properties<KspStringProperty>("rescaleFactor").First().Text, Is.EqualTo("1"));
            Assert.That(kspObject.Properties<KspStringProperty>("bulkheadProfiles").First().Text, Is.EqualTo("size3, srf"));
            Assert.That(kspObject.Children<KspObject>().First().Properties<KspStringProperty>("model").First().Text, Is.EqualTo("Squad/Parts/FuelTank/Size3Tanks/Size3LargeTank"));
            Assert.That(kspObject.Children<KspObject>().Skip(1).First().Properties<KspStringProperty>("name").First().Text, Is.EqualTo("LiquidFuel"));
            Assert.That(kspObject.Children<KspObject>().Skip(2).First().Properties<KspStringProperty>("name").First().Text, Is.EqualTo("Oxidizer"));

            // and when
            var tokenOut = KspObjectWriter.WriteObject(kspObject);
            var textOut = KspTokenWriter.WriteToken(tokenOut, new StringBuilder()).ToString();

            // then
            // A lot of comments are not written out so it cannot be compared here.
            // So just check if reading it returns same properties
            tokenIn = KspTokenReader.ReadToken(textOut);
            kspObject = KspObjectReader.ReadObject(tokenIn) as KspPartObject;
            Assert.That(kspObject.Name, Is.EqualTo("Size3LargeTank"));
            Assert.That(kspObject.Properties<KspStringProperty>("rescaleFactor").First().Text, Is.EqualTo("1"));
            Assert.That(kspObject.Properties<KspStringProperty>("bulkheadProfiles").First().Text, Is.EqualTo("size3, srf"));
            Assert.That(kspObject.Children<KspObject>().First().Properties<KspStringProperty>("model").First().Text, Is.EqualTo("Squad/Parts/FuelTank/Size3Tanks/Size3LargeTank"));
            Assert.That(kspObject.Children<KspObject>().Skip(1).First().Properties<KspStringProperty>("name").First().Text, Is.EqualTo("LiquidFuel"));
            Assert.That(kspObject.Children<KspObject>().Skip(2).First().Properties<KspStringProperty>("name").First().Text, Is.EqualTo("Oxidizer"));
        }
    }
}
