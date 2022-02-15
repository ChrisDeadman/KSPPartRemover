using System.Linq;
using NUnit.Framework;
using KSPPartRemover.Feature;
using KSPPartRemover.KspFormat.Objects;

namespace KSPPartRemover.Tests.Feature
{
    public class ObjectLoaderTest
    {
        [Test]
        public void CanLoadAndSaveFromFiles()
        {
            // given / when
            ObjectLoader.SaveToFile("test.sfs", new KspObject("GAME")
                .AddChild(new KspCraftObject().AddProperty(new KspStringProperty("name", "craft1")))
                .AddChild(new KspCraftObject().AddProperty(new KspStringProperty("name", "craft2")))
                .AddChild(new KspCraftObject().AddProperty(new KspStringProperty("name", "craft3"))));

            // and when
            var kspObjTree = ObjectLoader.LoadFromFile("test.sfs");

            // then
            Assert.That(kspObjTree.Children<KspCraftObject>(recursive: true).Select(craft => craft.Name), Is.EqualTo(new[] { "craft1", "craft2", "craft3" }));
        }
    }
}
