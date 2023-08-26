namespace DAZ_Installer.Core
{
    public delegate void DPProcessorEventHandler<T>(DPProcessor sender, T args);
    public delegate void DPArchiveEventHandler<T>(DPArchive archive, T args);
    public delegate void DPProcessorEventHandler(DPProcessor sender);
    public delegate void DPArchiveEventHandler(DPArchive archive);

}
