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
    
    public partial class ThoiKhoaBieu
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public ThoiKhoaBieu()
        {
            this.BanCanSus = new HashSet<BanCanSu>();
            this.PhongDayBus = new HashSet<PhongDayBu>();
            this.PhuGiangs = new HashSet<PhuGiang>();
            this.SoGhiBais = new HashSet<SoGhiBai>();
            this.SoGhiChus = new HashSet<SoGhiChu>();
            this.TroGiangs = new HashSet<TroGiang>();
        }
    
        public int id { get; set; }
        public string MaMH { get; set; }
        public string TenMH { get; set; }
        public byte SoTinChi { get; set; }
        public string NhomTo { get; set; }
        public string ToTH { get; set; }
        public string TenToHop { get; set; }
        public string MaNganh { get; set; }
        public string MaLop { get; set; }
        public string TenLop { get; set; }
        public short TongSoSV { get; set; }
        public byte ThuKieuSo { get; set; }
        public byte TietBD { get; set; }
        public byte SoTiet { get; set; }
        public string MaGV { get; set; }
        public string MaPH { get; set; }
        public byte HocKy { get; set; }
        public byte TuanBD { get; set; }
        public byte TuanKT { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<BanCanSu> BanCanSus { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<PhongDayBu> PhongDayBus { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<PhuGiang> PhuGiangs { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<SoGhiBai> SoGhiBais { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<SoGhiChu> SoGhiChus { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TroGiang> TroGiangs { get; set; }
    }
}
