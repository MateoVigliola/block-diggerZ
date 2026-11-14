using System;

namespace BlockDiggerz.DebugTools
{
    /// <summary>Description of one debug control; the menu turns it into UI and the CLI drives it by label.</summary>
    public sealed class DebugControl
    {
        internal DebugControl(DebugControlKind kind, string label)
        {
            Kind = kind;
            Label = label;
        }

        /// <summary>What kind of control this is.</summary>
        public DebugControlKind Kind { get; }

        /// <summary>Display text and the CLI's --label key; keep it stable and unique in its section.</summary>
        public string Label { get; }

        /// <summary>Button action.</summary>
        public Action OnClick { get; internal set; }

        /// <summary>Toggle getter.</summary>
        public Func<bool> GetBool { get; internal set; }

        /// <summary>Toggle setter.</summary>
        public Action<bool> SetBool { get; internal set; }

        /// <summary>Slider minimum.</summary>
        public float Min { get; internal set; }

        /// <summary>Slider maximum.</summary>
        public float Max { get; internal set; }

        /// <summary>Slider getter.</summary>
        public Func<float> GetFloat { get; internal set; }

        /// <summary>Slider setter.</summary>
        public Action<float> SetFloat { get; internal set; }

        /// <summary>Readout getter.</summary>
        public Func<string> GetText { get; internal set; }
    }
}
