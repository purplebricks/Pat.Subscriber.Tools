namespace Pat.Subscriber.Tools.Commands
{
    public class DeleteSubscriptionCommand: BaseCommand
    {
        public DeleteSubscriptionCommand(PatBootstrapService configurationService)
            : base("deleteSubscription", "Delete subscription.")
        {
            Command = configurationService.DeleteSubscription;
        }
    }
}