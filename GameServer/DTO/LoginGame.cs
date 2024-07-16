using System.ComponentModel.DataAnnotations;

namespace GameServer.DTO;

public class LoginRequest
{
    [Required]
    [EmailAddress]
    [MinLength(1, ErrorMessage = "EMAIL CANNOT BE EMPTY")]
    [StringLength(50, ErrorMessage = "EMAIL IS TOO LONG")]
    public required string player_id { get; set; }

    [Required]
    public required string token { get; set; }

    [Required]
    public string AppVersion { get; set; } = "0.1.0"; // �� ���� 0.1.0 ����մϴ�.
    public string DataVersion { get; set; } = "0.1.0"; // ������ ���� 0.1.0 ����մϴ�.
}

public class LoginResponse
{
    [Required]
    public ErrorCode Result { get; set; } = ErrorCode.None;
}
