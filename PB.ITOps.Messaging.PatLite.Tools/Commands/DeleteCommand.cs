namespace PB.ITOps.Messaging.PatLite.Tools.Commands
{
    public class DeleteCommand: BaseCommand
    {
        public DeleteCommand(PatBootstrapService configurationService)
            : base("delete", "Delete subscription and topic.")
        {
            Command = configurationService.Delete;
        }
    }
}