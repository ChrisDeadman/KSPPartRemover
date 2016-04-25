using System;
using System.Linq;
using NUnit.Framework;
using KSPPartRemover.Extension;
using gen = KSPPartRemover.Tests.TestHelpers.KspObjectGenerator;

namespace KSPPartRemover.Tests.Extension
{
	public class KspPartObjectTest
	{
		[Test]
		public void CanRetrieveNameOfPartFromPartProperty ()
		{
			// given
			var part = KspPartObject.From (
				           gen.Part (gen.Properties (
					           gen.Property ("name", "thisIsIgnoredIfPartPropertyIsFound"),
					           gen.Property ("part", "somePart")
				           )));

			// when / then
			Assert.That (part.Name.Value, Is.EqualTo ("somePart"));
		}

		[Test]
		public void CanRetrieveNameOfPartFromNameProperty ()
		{
			// given
			var part = KspPartObject.From (
				           gen.Part (gen.Properties (
					           gen.Property ("name", "somePart")
				           )));

			// when / then
			Assert.That (part.Name.Value, Is.EqualTo ("somePart"));
		}
	}
}
