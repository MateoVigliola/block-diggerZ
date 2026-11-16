using System.Text;
using BlockDiggerz.DebugTools;
using BlockDiggerz.Input;
using UnityEngine;

namespace BlockDiggerz.MobileControls
{
    /// <summary>
    /// The "Touch controls" debug menu section: force the touch layout on desktop and read its state. Sits next to
    /// <see cref="TouchControls"/> on the MobileControls prefab. Labels are the CLI's keys
    /// (`unity command debugmenu do --section "Touch controls" --label "Force touch controls"`).
    /// </summary>
    public sealed class TouchControlsDebugControls : MonoBehaviour
    {
        /// <summary>Section title in the debug menu and the CLI.</summary>
        public const string SectionTitle = "Touch controls";

        /// <summary>Label of the toggle that shows the touch controls on desktop.</summary>
        public const string ForceLabel = "Force touch controls";

        private const string Yes = "yes";
        private const string No = "no";
        private const string NoValue = "-";
        private const string Separator = " ";

        [SerializeField, Tooltip("Touch controls the section reads and toggles.")]
        private TouchControls _controls;

#if UNITY_EDITOR || DEBUG
        private void OnEnable()
        {
            if (_controls == null)
            {
                return;
            }

            DebugMenu.Register(this, SectionTitle, section =>
            {
                section
                    .Toggle(ForceLabel, () => _controls.ForceShow, v => _controls.ForceShow = v)
                    .Readout("Mobile platform", () => TouchControls.IsMobilePlatform ? Yes : No)
                    .Readout("Active", () => _controls.IsActive ? Yes : No)
                    .Readout("Shown", () => _controls.IsShown ? Yes : No)
                    .Readout("Touch pad", () => _controls.TouchPad.IsConnected ? "connected" : "not connected")
                    .Readout("Pressed", DescribePressed)
                    .Readout("Board on screen", DescribeBoardRect)
                    .Readout("Screen orientation", () => Screen.orientation.ToString())
                    .Readout("Rotate message", () => _controls.IsRotateMessageShown ? Yes : No);
            });
        }

        private void OnDisable()
        {
            DebugMenu.Unregister(this);
        }

        private string DescribePressed()
        {
            uint mask = _controls.TouchPad.Buttons;
            if (mask == TouchPadMask.None)
            {
                return NoValue;
            }

            var text = new StringBuilder();
            foreach (TouchPadButton button in System.Enum.GetValues(typeof(TouchPadButton)))
            {
                if (TouchPadMask.IsPressed(mask, button))
                {
                    text.Append(button).Append(Separator);
                }
            }

            return text.ToString().TrimEnd();
        }

        private string DescribeBoardRect()
        {
            if (!_controls.IsActive)
            {
                return NoValue;
            }

            Rect rect = _controls.BoardPanelRect;
            return $"{rect.xMin:0},{rect.yMin:0} {rect.width:0}x{rect.height:0}";
        }
#endif
    }
}
