using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebShared.Models
{
    public partial class MediaData
    {
        public string Id_Media { get; set; } = "";
        public string Option { get; set; } = "";
        public string No_Id { get; set; } = "";
        public string Url { get; set; } = "";
        public DateTime? Expired_At { get; set; } = DateTime.Now;
        public bool? IsCompressed { get; set; } = false;
        public string ContentType { get; set; } = "";
        public string? ThumbUrl { get; set; }
        public long? Size { get; set; }
    }
}
