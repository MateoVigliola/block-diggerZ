using System;

namespace BlockDiggerz.AssetTracking
{
    /// <summary>
    /// Columns that people edit in the Google Sheet. Unity only writes one of these to the sheet when it
    /// changed the value itself since the last sync (for example a deleted file setting Status to Missing).
    /// </summary>
    [Flags]
    public enum SheetField
    {
        None = 0,
        Name = 1 << 0,
        Description = 1 << 1,
        Requirements = 1 << 2,
        Status = 1 << 3,
        Notes = 1 << 4,
        All = Name | Description | Requirements | Status | Notes
    }
}
