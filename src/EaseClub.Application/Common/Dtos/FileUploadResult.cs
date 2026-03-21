using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Application.Common.Dtos
{
    public class FileUploadResult
    {
       
            public string FileId { get; set; }
            public string FileName { get; set; }
            public string FilePath { get; set; }
            public string Url { get; set; }
            public string ThumbnailUrl { get; set; }
            public int Width { get; set; }
            public int Height { get; set; }
            public long Size { get; set; }
            public bool IsPrivateFile { get; set; }
        
    }
}
