namespace OmokClient;

public enum ErrorCode : UInt16
{
    None = 0,
    InvalidCredentials = 1,
    UserNotFound = 2,
    ServerError = 3,
    InternalServerError = 4,
    RequestTurnTimeout = 2505,
    TurnChangedByTimeout = 2510,
    RequestFailed = 10000,

    FailToDeleteMailItemNotReceived = 8021,
    AttendanceCheckFailAlreadyChecked = 9002,
}
