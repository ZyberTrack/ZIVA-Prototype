namespace ZIVA_Prototype.Components.Models.Enums
{
    public enum ArtifactRelationType
    {
        Unknown,

        // =====================================================
        // DOMAIN / HISTORY
        // =====================================================

        DomainToHistory,

        CookieToDomain,

        CookieToHistory,

        StorageToHistory,

        ExtensionToWebStore,

        UserInputToHistory,

        AutofillToHistory,

        FaviconToHistory,

        HistoryReferrer,


        // =====================================================
        // CORRELATION
        // =====================================================

        SharedDomain,

        SharedTimestamp,

        SharedSession,

        SharedStorageOrigin,


        // =====================================================
        // ANOMALY / INVESTIGATION
        // =====================================================

        DeletedHistoryEvidence,

        SuspiciousCorrelation,

        LocalhostCommunication,

        TrackingRelation,

        AuthenticationRelation
    }
}