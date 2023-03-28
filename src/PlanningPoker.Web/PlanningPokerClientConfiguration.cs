using PlanningPoker.Web.Model;

namespace PlanningPoker.Web
{
    public class PlanningPokerClientConfiguration
    {
        public ServerSideConditions UseServerSide { get; set; }

        public bool UseHttpClient { get; set; }
    }
}
