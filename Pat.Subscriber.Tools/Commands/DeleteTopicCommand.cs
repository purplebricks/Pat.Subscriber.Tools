namespace Pat.Subscriber.Tools.Commands
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