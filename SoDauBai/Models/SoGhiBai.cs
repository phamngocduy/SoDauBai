//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace SoDauBai.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class SoGhiBai
    {
        public int id { get; set; }
        public System.DateTime NgayTao { get; set; }
        public System.DateTime NgaySua { get; set; }
        public System.DateTime NgayDay { get; set; }
        public System.TimeSpan ThoiGianBD { get; set; }
        public System.TimeSpan ThoiGianKT { get; set; }
        public string NDGiangDay { get; set; }
        public byte SoTietDay { get; set; }
        public string MaPhong { get; set; }
        public string NhanXetSV { get; set; }
        public short TongSoSV { get; set; }
        public string DeXuat { get; set; }
        public string Email { get; set; }
        public int idTKB { get; set; }
        public byte Loai { get; set; }
        public string XemDeXuat { get; set; }
    
        public virtual ThoiKhoaBieu ThoiKhoaBieu { get; set; }
    }
}
