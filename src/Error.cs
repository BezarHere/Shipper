namespace Shipper;

enum Error
{
	Ok = 0,
	Fault,
	UnexpectedArgument,
	ExpectedArgument,
	FileDoesNotExist,
	DirectoryDoesNotExist,
	UnknownCommand,
	UnauthorizedAccess,
	PathTooLong,
	UnsupportedOperation,
}

static class ErrorUtility
{

	public static string GetName(Error error)
	{
		return error switch
		{
			Error.Ok => "Ok",
			Error.Fault => "Fault",
			Error.UnexpectedArgument => "UnexpectedArgument",
			Error.ExpectedArgument => "ExpectedArgument",
			Error.FileDoesNotExist => "FileDoesNotExist",
			Error.DirectoryDoesNotExist => "DirectoryDoesNotExist",
			Error.UnknownCommand => "UnknownCommand",
			Error.UnauthorizedAccess => "UnauthorizedAccess",
			Error.PathTooLong => "PathTooLong",
			Error.UnsupportedOperation => "UnsupportedOperation",
			_ => "UNKNOWN",
		};
	}
}
