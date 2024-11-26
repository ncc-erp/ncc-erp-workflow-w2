using System;

namespace W2.Komu
{
    public class KomuMessageLogDto
    {
        public Guid Id { get; set; }
        public string SendTo { get; set; }
        public string Message { get; set; }
        public int Status { get; set; }
        public string SystemResponse { get; set; }
        public DateTime CreationTime { get; set; }
        public Guid? CreatorId { get; set; }
    }
}
