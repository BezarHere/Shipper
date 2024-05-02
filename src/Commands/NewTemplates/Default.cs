namespace Shipper.Commands.NewTemplates;
internal class DefaultTemplate : IProjectNewTemplate
{
	public string Name => "default";

	public string Description => "the default project configuration";

	public string Generate(in ProjectNewParameters parameters)
	{
		throw new NotImplementedException();
	}
}
