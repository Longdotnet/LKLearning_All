using System;
using System.Collections.Generic;

namespace WEBAPI.Models;

public partial class Banner
{
    public int BannerId { get; set; }

    public string BannerTitle { get; set; } = null!;

    public string? BannerSubtitle { get; set; }

    public string? BannerDescription { get; set; }

    public string BannerImageUrl { get; set; } = null!;

    public string? LinkUrl { get; set; }

    public bool? IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }
}
