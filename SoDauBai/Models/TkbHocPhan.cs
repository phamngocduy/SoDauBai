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
    
    public partial class TkbHocPhan
    {
        public int id { get; set; }
        public int Tkb_id { get; set; }
        public string MaHP { get; set; }
        public string TenHocPhan { get; set; }
        public int TinChi { get; set; }
        public string NhomTo { get; set; }
        public int Thu { get; set; }
        public string Phong { get; set; }
        public int TietBatDau { get; set; }
        public int SoTiet { get; set; }
        public int SoSV { get; set; }
        public int TuanBatDau { get; set; }
        public int TuanKetThuc { get; set; }
        public string Nganh { get; set; }
        public string MaKhoa { get; set; }
        public string VietTat { get; set; }
    
        public virtual TkbDanhSach TkbDanhSach { get; set; }
    }
}
