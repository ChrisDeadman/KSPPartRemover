using NUnit.Framework;
using KSPPartRemover.KspFormat.Objects;

namespace KSPPartRemover.Tests.KspFormat.Objects
{
    public class KspObjectTest
    {
        [Test]
        public void CanAddProperties()
        {
            // given
            var properties = new[] {
                new KspStringProperty ("property1", "property1Text"),
                new KspStringProperty ("property2", "property2Text"),
                new KspStringProperty ("property3", "property3Text")
            };

            // when
            var obj = new KspObject("OBJ")
                .AddProperty(properties[0])
                .AddProperty(properties[1])
                .AddProperty(properties[2]);

            // when / then
            Assert.That(obj.Properties, Is.EqualTo(properties));
        }

        [Test]
        public void CanInsertProperties()
        {
            // given
            var properties = new[] {
                new KspStringProperty ("property1", "property1Text"),
                new KspStringProperty ("property2", "property2Text"),
                new KspStringProperty ("property3", "property3Text")
            };

            // when
            var obj = new KspObject("OBJ")
                .InsertProperty(1, properties[1])
                .InsertProperty(1, properties[2])
                .InsertProperty(0, properties[0]);

            // when / then
            Assert.That(obj.Properties, Is.EqualTo(properties));
        }

        [Test]
        public void CanRemoveProperties()
        {
            // given
            var properties = new[] {
                new KspStringProperty ("property1", "property1Text"),
                new KspStringProperty ("property2", "property2Text"),
                new KspStringProperty ("property3", "property3Text")
            };

            // when
            var obj = new KspObject("OBJ")
                .AddProperty(properties[0])
                .AddProperty(properties[1])
                .AddProperty(properties[2])
                .RemoveProperty(properties[1]);

            // when / then
            Assert.That(obj.Properties, Is.EqualTo(new[] { properties[0], properties[2] }));
        }

        [Test]
        public void CanAddChildren()
        {
            // given
            var children = new[] {
                new KspObject ("child1"),
                new KspObject ("child2"),
                new KspObject ("child3")
            };

            // when
            var obj = new KspObject("OBJ")
                .AddChild(children[0])
                .AddChild(children[1])
                .AddChild(children[2]);

            // when / then
            Assert.That(obj.Children, Is.EqualTo(children));
        }

        [Test]
        public void CanRemoveChildren()
        {
            // given
            var children = new[] {
                new KspObject ("child1"),
                new KspObject ("child2"),
                new KspObject ("child3")
            };

            // when
            var obj = new KspObject("OBJ")
                .AddChild(children[0])
                .AddChild(children[1])
                .AddChild(children[2])
                .RemoveChild(children[1]);

            // when / then
            Assert.That(obj.Children, Is.EqualTo(new[] { children[0], children[2] }));
        }
    }
}
