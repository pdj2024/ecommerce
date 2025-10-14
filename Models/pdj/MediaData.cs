using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Models
{
    public partial class MediaData
    {
        public string Id_Media { get; set; } = "";
        public string Option { get; set; } = "";
        public string No_Id { get; set; } = "";
        public string Url { get; set; } = "";
        public DateTime Expired_At { get; set; }
        public bool IsCompressed { get; set; }
        public string ContentType { get; set; } = "";
        public string? ThumbUrl { get; set; }
        public long Size { get; set; }
    }
}
