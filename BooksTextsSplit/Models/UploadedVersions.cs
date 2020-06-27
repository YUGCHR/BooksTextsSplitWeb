using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BooksTextsSplit.Models
{
    public class UploadedVersions
    {
        public UploadedVersions(int[] versions)
        {
            allUploadedVersions = versions;
        }
        public int[] allUploadedVersions { get; set; }
    }
}
