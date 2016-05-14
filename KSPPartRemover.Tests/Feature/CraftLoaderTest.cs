using System;
using System.IO;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using KSPPartRemover.Feature;
using KSPPartRemover.KspFormat.Objects;

namespace KSPPartRemover.Tests.Feature
{
    public class CraftLoaderTest
    {
        [Test]
        public void CanLoadCraftsFromCraftFileString ()
        {
            // given
            var textIn = new StreamReader (Assembly.GetExecutingAssembly ().GetManifestResourceStream ("KSPPartRemover.Tests.Resources.Mün Mk I.in.craft")).ReadToEnd ();

            KspObject kspObjTree;

            // when
            var crafts = CraftLoader.LoadFromText (textIn, out kspObjTree);

            // then
            Assert.That (crafts.Select (craft => craft.Name), Is.EqualTo (new[] { "Mün Mk I" }));
            Assert.That (kspObjTree, Is.EqualTo (crafts.Single ()));
        }

        [Test]
        public void CanLoadCraftsFromSaveFileString ()
        {
            // given
            var textIn = new StreamReader (Assembly.GetExecutingAssembly ().GetManifestResourceStream ("KSPPartRemover.Tests.Resources.Refuel at Minmus.in.sfs")).ReadToEnd ();

            KspObject kspObjTree;

            // when
            var crafts = CraftLoader.LoadFromText (textIn, out kspObjTree);

            // then
            Assert.That (kspObjTree.Children<KspCraftObject> (recursive: true), Is.EqualTo (crafts));
            Assert.That (crafts.Select (craft => craft.Name), Is.EqualTo (new[] {
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
        }

        [Test]
        public void ReturnsEmptyObjectTreeForEmptyFile ()
        {
            // given
            var textIn = "";

            KspObject kspObjTree;

            // when
            var crafts = CraftLoader.LoadFromText (textIn, out kspObjTree);

            // then
            Assert.That (crafts, Is.Empty);
            Assert.That (kspObjTree.Children, Is.Empty);
        }

        [Test]
        public void CanLoadAndSaveFromFiles ()
        {
            // given / when
            CraftLoader.SaveToFile ("test.sfs", new KspObject ("GAME")
                .AddChild (new KspCraftObject ().AddProperty (new KspStringProperty ("name", "craft1")))
                .AddChild (new KspCraftObject ().AddProperty (new KspStringProperty ("name", "craft2")))
                .AddChild (new KspCraftObject ().AddProperty (new KspStringProperty ("name", "craft3"))));

            // and when
            KspObject kspObjTreeLoaded;
            var crafts = CraftLoader.LoadFromFile ("test.sfs", out kspObjTreeLoaded);

            // then
            Assert.That (kspObjTreeLoaded.Children<KspCraftObject> (recursive: true), Is.EqualTo (crafts));
            Assert.That (crafts.Select (craft => craft.Name), Is.EqualTo (new[] { "craft1", "craft2", "craft3" }));
        }
    }
}
