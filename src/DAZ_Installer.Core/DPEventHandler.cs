namespace DAZ_Installer.Core
{
    public delegate Task DPProcessorEventHandler<T>(IDPProcessor sender, T args);
    public delegate Task DPArchiveEventHandler<T>(IDPArchive archive, T args);
}
