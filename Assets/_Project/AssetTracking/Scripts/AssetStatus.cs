namespace BlockDiggerz.AssetTracking
{
    /// <summary>
    /// Production state of a tracked asset. See <see cref="AssetStatusNames"/> for the text shown in the sheet.
    /// </summary>
    public enum AssetStatus
    {
        Missing,
        Placeholder,
        NeedsReplacement,
        Final
    }
}
