﻿using System.Linq;
using NUnit.Framework;
using KSPPartRemover.KspFormat.Objects;
using KSPPartRemover.Feature;

namespace KSPPartRemover.Tests.Feature
{
    public class CraftEditorTest
    {
        [Test]
        public void RemovesPartsAndCorrespondingLinks()
        {
            // given
            var craft = new KspCraftObject()
                .AddChild(new KspPartObject().AddProperty(new KspStringProperty("name", "part1")))
                .AddChild(new KspPartObject().AddProperty(new KspStringProperty("name", "part2")))
                .AddChild(new KspPartObject().AddProperty(new KspStringProperty("name", "part3")))
                .AddChild(new KspPartObject().AddProperty(new KspStringProperty("name", "part4"))) as KspCraftObject;

            var parts = craft.Children<KspPartObject>().ToArray();

            parts[0]
                .AddProperty(new KspPartLinkProperty(KspPartLinkProperty.Types.Link, "", parts[1]))
                .AddProperty(new KspPartLinkProperty(KspPartLinkProperty.Types.Link, "", parts[2]))
                .AddProperty(new KspPartLinkProperty(KspPartLinkProperty.Types.Link, "", parts[3]))
                .AddProperty(new KspPartLinkProperty(KspPartLinkProperty.Types.Parent, "", parts[1]))
                .AddProperty(new KspPartLinkProperty(KspPartLinkProperty.Types.Parent, "", parts[2]))
                .AddProperty(new KspPartLinkProperty(KspPartLinkProperty.Types.Parent, "", parts[3]))
                .AddProperty(new KspPartLinkProperty(KspPartLinkProperty.Types.Sym, "top", parts[1]))
                .AddProperty(new KspPartLinkProperty(KspPartLinkProperty.Types.Sym, "bottom", parts[2]))
                .AddProperty(new KspPartLinkProperty(KspPartLinkProperty.Types.Sym, "left", parts[3]))
                .AddProperty(new KspPartLinkProperty(KspPartLinkProperty.Types.SrfN, "srfAttach", parts[1]))
                .AddProperty(new KspPartLinkProperty(KspPartLinkProperty.Types.SrfN, "srfAttach", parts[2]))
                .AddProperty(new KspPartLinkProperty(KspPartLinkProperty.Types.SrfN, "srfAttach", parts[3]))
                .AddProperty(new KspPartLinkProperty(KspPartLinkProperty.Types.AttN, "left", parts[1]))
                .AddProperty(new KspPartLinkProperty(KspPartLinkProperty.Types.AttN, "bottom", parts[2]))
                .AddProperty(new KspPartLinkProperty(KspPartLinkProperty.Types.AttN, "top", parts[3]));

            parts[3]
                .AddProperty(new KspPartLinkProperty(KspPartLinkProperty.Types.Link, "", parts[1]))
                .AddProperty(new KspPartLinkProperty(KspPartLinkProperty.Types.Link, "", parts[2]))
                .AddProperty(new KspPartLinkProperty(KspPartLinkProperty.Types.Link, "", parts[0]))
                .AddProperty(new KspPartLinkProperty(KspPartLinkProperty.Types.Parent, "", parts[1]))
                .AddProperty(new KspPartLinkProperty(KspPartLinkProperty.Types.Parent, "", parts[2]))
                .AddProperty(new KspPartLinkProperty(KspPartLinkProperty.Types.Parent, "", parts[0]))
                .AddProperty(new KspPartLinkProperty(KspPartLinkProperty.Types.Sym, "top", parts[1]))
                .AddProperty(new KspPartLinkProperty(KspPartLinkProperty.Types.Sym, "bottom", parts[2]))
                .AddProperty(new KspPartLinkProperty(KspPartLinkProperty.Types.Sym, "left", parts[0]))
                .AddProperty(new KspPartLinkProperty(KspPartLinkProperty.Types.SrfN, "srfAttach", parts[1]))
                .AddProperty(new KspPartLinkProperty(KspPartLinkProperty.Types.SrfN, "srfAttach", parts[2]))
                .AddProperty(new KspPartLinkProperty(KspPartLinkProperty.Types.SrfN, "srfAttach", parts[0]))
                .AddProperty(new KspPartLinkProperty(KspPartLinkProperty.Types.AttN, "left", parts[1]))
                .AddProperty(new KspPartLinkProperty(KspPartLinkProperty.Types.AttN, "bottom", parts[2]))
                .AddProperty(new KspPartLinkProperty(KspPartLinkProperty.Types.AttN, "top", parts[0]));

            var partsToRemove = new[] { parts[1], parts[2] };

            var expectedRemainingParts = new[] { parts[0], parts[3] };

            var expectedPropertiesPart1 = new KspProperty[] {
                parts [0].Properties [0],
                parts [0].Properties [3],
                parts [0].Properties [6],
                parts [0].Properties [9],
                parts [0].Properties [12],
                parts [0].Properties [15]
            };

            var expectedPropertiesPart2 = new KspProperty[] {
                parts [3].Properties [0],
                parts [3].Properties [3],
                parts [3].Properties [6],
                parts [3].Properties [9],
                parts [3].Properties [12],
                parts [3].Properties [15]
            };

            // when
            craft.Edit().RemoveParts(partsToRemove);

            var remainingParts = craft.Children<KspPartObject>().ToArray();

            // then
            Assert.That(remainingParts, Is.EqualTo(expectedRemainingParts));
            Assert.That(remainingParts[0].Properties, Is.EqualTo(expectedPropertiesPart1));
            Assert.That(remainingParts[1].Properties, Is.EqualTo(expectedPropertiesPart2));
        }
    }
}
