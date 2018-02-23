namespace PB.ITOps.Messaging.PatLite.Tools.Commands
{
    public class DeleteTopicCommand: BaseCommand
    {
        public DeleteTopicCommand(PatBootstrapService configurationService)
            : base("deleteTopic", "Delete topic and all its subscriptions.")
        {
            Command = configurationService.DeleteTopic;
        }
    }
}