namespace Shipper;

enum Error
{
	Ok = 0,
	Fault,
	UnexpectedArgument,
	UnexpectedArguments,
	ExpectedArgument,
	ExpectedArguments,
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
			Error.UnknownCommand => "UnknownCommand",
			_ => "UNKNOWN",
		};
	}
}
