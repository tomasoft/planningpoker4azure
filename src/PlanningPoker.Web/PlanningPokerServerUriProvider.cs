using System;
using PlanningPoker.Client.Service;

namespace PlanningPoker.Web
{
    public class PlanningPokerServerUriProvider : IPlanningPokerUriProvider
    {
        public Uri? BaseUri { get; private set; }

        public void InitializeBaseUri(Uri baseUri)
        {
            BaseUri = baseUri;
        }
    }
}
