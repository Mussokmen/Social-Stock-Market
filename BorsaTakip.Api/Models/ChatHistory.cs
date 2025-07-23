using System;

namespace BorsaTakip.Api.Models
{
    public class ChatHistory
    {
        public int Id { get; set; }
        public string Question { get; set; }
        public string Answer { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
