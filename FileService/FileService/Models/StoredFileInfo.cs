using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FileService.Models
{
    public class StoredFileInfo
    {
        public string FilePath { get; set; }
        public string FileName { get; set; }
        public string UserId { get; set; }
        public string Comment { get; set; }
        public string Id { get; set; } //= System.Guid.NewGuid().ToString();
    }
}
