namespace DAZ_Installer.Core
{
    public delegate void DPProcessorEventHandler<T>(IDPProcessor sender, T args);
    public delegate void DPArchiveEventHandler<T>(IDPArchive archive, T args);
    public delegate void DPProcessorEventHandler(IDPProcessor sender);
    public delegate void DPArchiveEventHandler(IDPArchive archive);
}
