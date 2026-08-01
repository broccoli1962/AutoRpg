namespace Backend.Services.Save
{
    /// <summary>
    /// 로컬·클라우드 충돌 시 사용자 선택.
    /// </summary>
    public enum SaveConflictChoice
    {
        UseLocal = 0,
        UseCloud = 1,
        Cancel = 2,
    }
}
