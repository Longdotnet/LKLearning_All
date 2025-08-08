using System;
using System.Collections.Generic;

namespace WEBAPI.Models;

public partial class User
{
    public int UserId { get; set; }

    public string? Fullname { get; set; }

    public string? Email { get; set; }

    public string? Password { get; set; }

    public int? Age { get; set; }

    public string? Phone { get; set; }

    public bool? Gender { get; set; }

    public DateOnly? DateofBirth { get; set; }

    public int? RoleId { get; set; }

    public DateTime? LastLoginDate { get; set; }

    public int? AvatarId { get; set; }

    public virtual ICollection<AcademicResult> AcademicResults { get; set; } = new List<AcademicResult>();

    public virtual Avatar? Avatar { get; set; }

    public virtual ICollection<Certificate> Certificates { get; set; } = new List<Certificate>();

    public virtual ICollection<Conversation> ConversationAdmins { get; set; } = new List<Conversation>();

    public virtual ICollection<Conversation> ConversationUsers { get; set; } = new List<Conversation>();

    public virtual ICollection<Message> Messages { get; set; } = new List<Message>();

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();

    public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();

    public virtual Role? Role { get; set; }

    public virtual ICollection<UserExamHistory> UserExamHistories { get; set; } = new List<UserExamHistory>();

    public virtual ICollection<UserPackageRegistration> UserPackageRegistrations { get; set; } = new List<UserPackageRegistration>();
}
