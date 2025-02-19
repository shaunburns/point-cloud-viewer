namespace PointCloudViewer.Server.DTOs
{
    public class RegisterNewUserRequestDTO
    {
        public required string Username { get; set; }
        public required string Email { get; set; }
        public required string Password { get; set; }
    }
}
