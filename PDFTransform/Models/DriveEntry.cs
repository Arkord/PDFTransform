using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PDFTransform.Models
{
    public class DriveEntry
    {
        public string Id { get; set; }
        public string Type { get; set; }

        public DriveEntry()
        {
            Id = string.Empty;
            Type = string.Empty;
        }
    }
}
