namespace ChatServer.Dtos
{
    public sealed record RegisterDto(
        string Name,
        IFormFile File);
    
}
