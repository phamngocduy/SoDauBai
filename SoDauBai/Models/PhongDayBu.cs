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
    
    public partial class PhongDayBu
    {
        public int id { get; set; }
        public System.DateTime Date { get; set; }
        public System.DateTime NgayBD { get; set; }
        public string GhiChu1 { get; set; }
        public int idTKB { get; set; }
        public System.DateTime NgayDay { get; set; }
        public byte TietBD { get; set; }
        public string MaPH { get; set; }
        public string GhiChu2 { get; set; }
        public Nullable<System.DateTime> status { get; set; }
        public string email1 { get; set; }
        public string email2 { get; set; }
    
        public virtual ThoiKhoaBieu ThoiKhoaBieu { get; set; }
    }
}
