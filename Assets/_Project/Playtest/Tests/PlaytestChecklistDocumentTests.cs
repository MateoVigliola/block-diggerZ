using System.Linq;
using NUnit.Framework;

namespace BlockDiggerz.Playtest.Tests
{
    public class PlaytestChecklistDocumentTests
    {
        private const string Sample =
            "# Playtest Checklist\n" +
            "\n" +
            "Intro text.\n" +
            "\n" +
            "## Feature A, 2026-11-14\n" +
            "\n" +
            "Open the scene and press Play.\n" +
            "\n" +
            "- [ ] **Move:** tap A and D.\n" +
            "  - Notes:\n" +
            "- [x] **Drop:** press Space.\n" +
            "  - Notes: felt good\n" +
            "    but a bit slow\n" +
            "- [ ] **No notes line:** just an item.\n" +
            "\n" +
            "## Feature B\n" +
            "\n" +
            "- [ ] **Look:** colors.\n" +
            "  - Notes:\n";

        private static string KeyOf(PlaytestChecklistDocument document, string textStart)
        {
            return document.AllItems().First(item => item.Text.StartsWith(textStart)).Key;
        }

        [Test]
        public void ParsesSectionsItemsAndNotes()
        {
            PlaytestChecklistDocument document = PlaytestChecklistDocument.Parse(Sample);

            Assert.That(document.Sections.Select(section => section.Title), Is.EqualTo(new[] { "Feature A, 2026-11-14", "Feature B" }));
            Assert.That(document.Sections[0].Description, Is.EqualTo("Open the scene and press Play."));
            Assert.That(document.Sections[0].Items.Count, Is.EqualTo(3));

            PlaytestItem drop = document.Sections[0].Items[1];
            Assert.That(drop.Text, Is.EqualTo("**Drop:** press Space."));
            Assert.That(drop.IsChecked, Is.True);
            Assert.That(drop.Notes, Is.EqualTo("felt good\nbut a bit slow"));

            Assert.That(document.Sections[0].Items[0].IsChecked, Is.False);
            Assert.That(document.Sections[0].Items[0].Notes, Is.Empty);
            Assert.That(document.Sections[0].Items[2].Notes, Is.Empty);
        }

        [Test]
        public void UnchangedDocumentRoundTripsExactly()
        {
            Assert.That(PlaytestChecklistDocument.Parse(Sample).ToText(), Is.EqualTo(Sample));
        }

        [Test]
        public void CheckingChangesOnlyTheCheckbox()
        {
            PlaytestChecklistDocument document = PlaytestChecklistDocument.Parse(Sample);

            Assert.That(document.SetChecked(KeyOf(document, "**Move:**"), true), Is.True);

            Assert.That(document.ToText(), Is.EqualTo(Sample.Replace("- [ ] **Move:**", "- [x] **Move:**")));
            Assert.That(document.Find(KeyOf(document, "**Move:**")).IsChecked, Is.True);
        }

        [Test]
        public void UncheckingMovesItemBack()
        {
            PlaytestChecklistDocument document = PlaytestChecklistDocument.Parse(Sample);

            document.SetChecked(KeyOf(document, "**Drop:**"), false);

            Assert.That(document.ToText(), Is.EqualTo(Sample.Replace("- [x] **Drop:**", "- [ ] **Drop:**")));
        }

        [Test]
        public void SettingNotesWritesFirstLineAfterPrefixAndIndentsTheRest()
        {
            PlaytestChecklistDocument document = PlaytestChecklistDocument.Parse(Sample);

            document.SetNotes(KeyOf(document, "**Move:**"), "too slow\nmaybe 0.1s\n\n");

            string expected = Sample.Replace(
                "- [ ] **Move:** tap A and D.\n  - Notes:\n",
                "- [ ] **Move:** tap A and D.\n  - Notes: too slow\n    maybe 0.1s\n");
            Assert.That(document.ToText(), Is.EqualTo(expected));
        }

        [Test]
        public void ReplacingMultiLineNotesRemovesOldLines()
        {
            PlaytestChecklistDocument document = PlaytestChecklistDocument.Parse(Sample);

            document.SetNotes(KeyOf(document, "**Drop:**"), "fine now");

            string expected = Sample.Replace("  - Notes: felt good\n    but a bit slow\n", "  - Notes: fine now\n");
            Assert.That(document.ToText(), Is.EqualTo(expected));
        }

        [Test]
        public void ClearingNotesLeavesEmptyNotesBullet()
        {
            PlaytestChecklistDocument document = PlaytestChecklistDocument.Parse(Sample);

            document.SetNotes(KeyOf(document, "**Drop:**"), string.Empty);

            Assert.That(document.ToText(), Does.Contain("- [x] **Drop:** press Space.\n  - Notes:\n- [ ] **No notes line:**"));
        }

        [Test]
        public void NotesAreAddedWhenTheItemHasNoNotesLine()
        {
            PlaytestChecklistDocument document = PlaytestChecklistDocument.Parse(Sample);

            document.SetNotes(KeyOf(document, "**No notes line:**"), "added");

            Assert.That(document.ToText(), Does.Contain("- [ ] **No notes line:** just an item.\n  - Notes: added\n\n## Feature B"));
            Assert.That(document.Find(KeyOf(document, "**No notes line:**")).Notes, Is.EqualTo("added"));
        }

        [Test]
        public void WindowsLineEndingsArePreserved()
        {
            string windows = Sample.Replace("\n", "\r\n");
            PlaytestChecklistDocument document = PlaytestChecklistDocument.Parse(windows);

            document.SetNotes(KeyOf(document, "**Look:**"), "a\nb");

            Assert.That(document.ToText(), Does.EndWith("- [ ] **Look:** colors.\r\n  - Notes: a\r\n    b\r\n"));
            Assert.That(document.ToText().Replace("\r\n", string.Empty), Does.Not.Contain("\n"));
        }

        [Test]
        public void SameTextInDifferentSectionsHasDifferentKeys()
        {
            PlaytestChecklistDocument document = PlaytestChecklistDocument.Parse(
                "## A\n- [ ] **Look:** x\n  - Notes:\n## B\n- [ ] **Look:** x\n  - Notes:\n");

            string keyB = document.Sections[1].Items[0].Key;
            document.SetChecked(keyB, true);

            Assert.That(document.Sections[0].Items[0].IsChecked, Is.False);
            Assert.That(document.Sections[1].Items[0].IsChecked, Is.True);
        }

        [Test]
        public void UnknownKeyChangesNothing()
        {
            PlaytestChecklistDocument document = PlaytestChecklistDocument.Parse(Sample);

            Assert.That(document.SetChecked("missing", true), Is.False);
            Assert.That(document.SetNotes("missing", "x"), Is.False);
            Assert.That(document.ToText(), Is.EqualTo(Sample));
        }

        [Test]
        public void EmptyTextGivesEmptyChecklist()
        {
            PlaytestChecklistDocument document = PlaytestChecklistDocument.Parse(string.Empty);

            Assert.That(document.Sections, Is.Empty);
            Assert.That(document.ToText(), Is.Empty);
        }
    }
}
