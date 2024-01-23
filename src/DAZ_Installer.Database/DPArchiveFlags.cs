namespace DAZ_Installer.Database
{
    [Flags]
    public enum DPArchiveFlags
    {
        None = 0,
        Locked = 1,
        UpdateRequired = 2,
        Corrupted = 4,
        Missing = 8,
        Initialized = 16
    }
}
