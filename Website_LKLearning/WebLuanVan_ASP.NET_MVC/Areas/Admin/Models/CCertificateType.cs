using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WebLuanVan_ASP.NET_MVC.Areas.Admin.Models
{
   public class CCertificateType
    {
        public int CertificateTypeId { get; set; }
        [Display(Name ="Tên loại chứng chỉ")]
        public string TypeName { get; set; } = null!;

    }
}