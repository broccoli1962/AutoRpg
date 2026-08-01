namespace Backend.Meta.StoreCompliance
{
    /// <summary>
    /// ATT/UMP 광고 식별자 동의 결과.
    /// </summary>
    public enum AdConsentStatus
    {
        Unknown = 0,
        NotRequired = 1,
        Obtained = 2,
        Denied = 3,
    }
}
