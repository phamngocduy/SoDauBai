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
    
    public partial class TroGiang
    {
        public int id { get; set; }
        public string Email { get; set; }
        public int idTKB { get; set; }
    
        public virtual ThoiKhoaBieu ThoiKhoaBieu { get; set; }
    }
}
