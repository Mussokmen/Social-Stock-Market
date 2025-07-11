using System;
namespace BorsaTakip.MVC.Models
{
	public class LoginResponseModel
    {
        public string Token { get; set; }
        public string Expiration { get; set; }
    }
}

