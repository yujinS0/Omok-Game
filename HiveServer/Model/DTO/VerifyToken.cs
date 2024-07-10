using System.ComponentModel.DataAnnotations;

namespace HiveServer.Model.DTO
{
    public class VerifyTokenRequest
    {
        [Required]
        public string hive_player_id { get; set; }
        [Required]
        public required string hive_token { get; set; }
    }

    public class VerifyTokenResponse
    {
        [Required]
        public ErrorCode Result { get; set; } = ErrorCode.None;
    }
}
