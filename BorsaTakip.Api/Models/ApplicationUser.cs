using System;
using Microsoft.AspNetCore.Identity;

namespace BorsaTakip.Api.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string? Bio { get; set; }
        public string? PhotoUrl { get; set; }
    }
}
