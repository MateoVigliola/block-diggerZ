using System;
using System.Collections.Generic;

namespace BlockDiggerz.DebugTools
{
    /// <summary>One feature's group of debug controls, built through the fluent methods below.</summary>
    public sealed class DebugSection
    {
        internal DebugSection(object owner, string title)
        {
            Owner = owner;
            Title = title;
        }

        /// <summary>The object that registered the section; used to unregister it.</summary>
        public object Owner { get; }

        /// <summary>Foldout title and the CLI's --section key.</summary>
        public string Title { get; }

        /// <summary>The section's controls in the order they were added.</summary>
        public List<DebugControl> Controls { get; } = new();

        /// <summary>Adds a button that runs <paramref name="onClick"/>.</summary>
        public DebugSection Button(string label, Action onClick)
        {
            Controls.Add(new DebugControl(DebugControlKind.Button, label) { OnClick = onClick });
            return this;
        }

        /// <summary>Adds a checkbox bound to a getter and setter.</summary>
        public DebugSection Toggle(string label, Func<bool> get, Action<bool> set)
        {
            Controls.Add(new DebugControl(DebugControlKind.Toggle, label) { GetBool = get, SetBool = set });
            return this;
        }

        /// <summary>Adds a slider bound to a getter and setter.</summary>
        public DebugSection Slider(string label, float min, float max, Func<float> get, Action<float> set)
        {
            Controls.Add(new DebugControl(DebugControlKind.Slider, label)
            {
                Min = min,
                Max = max,
                GetFloat = get,
                SetFloat = set
            });
            return this;
        }

        /// <summary>Adds a read-only line of live state, refreshed a few times a second.</summary>
        public DebugSection Readout(string label, Func<string> get)
        {
            Controls.Add(new DebugControl(DebugControlKind.Readout, label) { GetText = get });
            return this;
        }
    }
}
