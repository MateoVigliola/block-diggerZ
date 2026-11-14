namespace BlockDiggerz.AssetTracking
{
    /// <summary>What a pull from the sheet changed in the local database.</summary>
    public struct PullSummary
    {
        /// <summary>Rows that exist only in the sheet (planned assets) and were added locally.</summary>
        public int Added;

        /// <summary>Local entries whose sheet-owned fields were updated from the sheet.</summary>
        public int Updated;

        /// <summary>Unlinked entries removed locally because someone deleted their row in the sheet.</summary>
        public int Forgotten;

        /// <summary>Linked entries whose sheet row was deleted; they are written back in full on push.</summary>
        public int Restored;

        /// <summary>Rows whose Status text matched no known status; the local status was kept.</summary>
        public int UnknownStatuses;
    }
}
