using System.Collections.Generic;

namespace BlockDiggerz.Playtest
{
    /// <summary>
    /// A <c>##</c> section of the playtest checklist: usually one feature, with its intro text and items.
    /// </summary>
    public sealed class PlaytestSection
    {
        public PlaytestSection(string title, string description, IReadOnlyList<PlaytestItem> items)
        {
            Title = title;
            Description = description;
            Items = items;
        }

        public string Title { get; }

        /// <summary>Text between the heading and the first item, joined into one paragraph.</summary>
        public string Description { get; }

        public IReadOnlyList<PlaytestItem> Items { get; }
    }
}
