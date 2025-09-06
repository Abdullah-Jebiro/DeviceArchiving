using System;

namespace DeviceArchiving.WindowsForm.Dtos
{
    public class ExcelDevice
    {
        public string Source { get; set; }
        public string BrotherName { get; set; }
        public string LaptopName { get; set; }
        public string SystemPassword { get; set; }
        public string WindowsPassword { get; set; }
        public string HardDrivePassword { get; set; }
        public string FreezePassword { get; set; }
        public string Code { get; set; }
        public string Type { get; set; }
        public string SerialNumber { get; set; }
        public string Card { get; set; }
        public string Comment { get; set; }
        public string ContactNumber { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsSelected { get; set; }
        public bool IsDuplicateSerial { get; set; }
        public bool IsDuplicateLaptopName { get; set; }

    }



}

