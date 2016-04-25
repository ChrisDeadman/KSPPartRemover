using System;
using System.Collections.Generic;
using System.Linq;
using KSPPartRemover.Extension;
using NUnit.Framework;
using gen = KSPPartRemover.Tests.TestHelpers.KspObjectGenerator;

namespace KSPPartRemover.Tests.Extension
{
	public class KspObjectSearchExtensionsTest
	{
		[Test]
		public void CanFindPropertyOfAnObjectByName ()
		{
			// given
			var obj = gen.Object ("OBJ", gen.Properties (gen.Property ("name", "someObj"), gen.Property ("attN", "bottom, 0"), gen.Property ("attN", "top, 1")));

			// when / then
			Assert.That (obj.FindPropertyByName ("attN"), Is.EquivalentTo (gen.Properties (obj.properties [1], obj.properties [2])));
		}

		[Test]
		public void CanFindChildOfAnObjectByType ()
		{
			// given
			var craft = gen.Object ("OBJ", gen.Properties (),
				             gen.Object ("SEARCHED", gen.Properties (),
					             gen.Object ("SEARCHED", gen.Properties ())),
				             gen.Object ("OTHER", gen.Properties ()),
				             gen.Object ("SEARCHED", gen.Properties (),
					             gen.Object ("OTHER", gen.Properties ())));

			// when / then
			Assert.That (craft.FindChildByType ("SEARCHED"), Is.EquivalentTo (new[] {
				craft.children [0],
				craft.children [2]
			}));
		}

		[Test]
		public void CanRecursivelyFindChildOfAnObjectByType ()
		{
			// given
			var craft = gen.Object ("OBJ", gen.Properties (),
				             gen.Object ("SEARCHED", gen.Properties (),
					             gen.Object ("SEARCHED", gen.Properties ())),
				             gen.Object ("OTHER", gen.Properties ()),
				             gen.Object ("SEARCHED", gen.Properties (),
					             gen.Object ("OTHER", gen.Properties ())));

			// when / then
			Assert.That (craft.FindChildByType ("SEARCHED", recursive: true), Is.EquivalentTo (new[] {
				craft.children [0],
				craft.children [0].children [0],
				craft.children [2]
			}));
		}
	}
}
