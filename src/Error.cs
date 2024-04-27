namespace Shipper;

enum Error
{
	Ok = 0,
	Fault,
	UnexpectedArgument,
	UnexpectedArguments,
	ExpectedArgument,
	ExpectedArguments,
	FileDoesNotExist,
	UnknownCommand,
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
			Error.UnexpectedArguments => "UnexpectedArguments",
			Error.ExpectedArgument => "ExpectedArgument",
			Error.ExpectedArguments => "ExpectedArguments",
			Error.FileDoesNotExist => "FileDoesNotExist",
			Error.UnknownCommand => "UnknownCommand",
			_ => "UNKNOWN",
		};
	}
}
