namespace ChatServer.Models
{
    public sealed class Chat
    {
        public Chat()
        {
            Id=Guid.NewGuid();
        }
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public Guid ToUserId { get; set; }
        public string Message { get; set; }=string.Empty;
        public DateTime Date { get; set; }
        public string? FilePath { get; set; }

    }
}
