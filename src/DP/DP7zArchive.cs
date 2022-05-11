namespace DAZ_Installer.DP {
    internal class DP7zArchive : DPAbstractArchive
    {

        internal DP7zArchive(string _path,  bool innerArchive = false, string? relativePathBase = null) : base(_path, innerArchive, relativePathBase) {
            
        }

        internal override void Extract()
        {
            throw new System.NotImplementedException();
        }

        internal override void Peek()
        {
            throw new System.NotImplementedException();
        }

        internal override void UpdateParent(DPFolder newParent)
        {
            base.UpdateParent(newParent);
        }
    }
}