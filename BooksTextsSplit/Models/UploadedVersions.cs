using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BooksTextsSplit.Models
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
