namespace Pat.Subscriber.Tools.Commands
{
    public class CreateCommand: BaseCommand
    {
        public CreateCommand(PatBootstrapService configurationService) 
            : base("create", "Create subscription and topic.")
        {
            Command = configurationService.Create;
        }
    }
}