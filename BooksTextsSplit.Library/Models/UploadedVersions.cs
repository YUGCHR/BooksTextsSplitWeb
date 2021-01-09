namespace BooksTextsSplit.Library.Models
{
    public class UploadedVersions
    {
        public UploadedVersions(int[] versions, int maxUploadedVersion)
        {
            AllUploadedVersions = versions;            
            MaxUploadedVersion = maxUploadedVersion;
        }
        public int[] AllUploadedVersions { get; set; }
        public int MaxUploadedVersion { get; set; }
    }
}
