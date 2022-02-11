using NUnit.Framework;
using KSPPartRemover.Feature;

namespace KSPPartRemover.Tests.Feature
{
    public class RegexFilterTest
    {
        [Test]
        public void CanApplyRegularExpressionFilterOnCollection()
        {
            // given
            var target = new RegexFilter("[Bb]ert");

            var herbert = new { Name = "Herbert" };
            var franz = new { Name = "Franz" };
            var berthold = new { Name = "Berthold" };

            // when
            var filtered = target.Apply(new[] { herbert, franz, berthold }, user => user.Name);

            // then
            Assert.That(filtered, Is.EquivalentTo(new[] { herbert, berthold }));
        }

        [Test]
        public void SupportsInverseMatching()
        {
            // given
            var target = new RegexFilter("![Bb]ert");

            // when / then
            Assert.That(target.Matches("Herbert"), Is.False);
            Assert.That(target.Matches("Berthold"), Is.False);
            Assert.That(target.Matches("Franz"), Is.True);
        }
    }
}
