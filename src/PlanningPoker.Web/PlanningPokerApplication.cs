using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using PlanningPoker.Azure;
using PlanningPoker.Controllers;

namespace PlanningPoker.Web
{
    public class PlanningPokerApplication : IApplicationModelConvention
    {
        public void Apply(ApplicationModel application)
        {
            if (application == null)
            {
                throw new ArgumentNullException(nameof(application));
            }

            // Remove PlanningPokerController, because it is not MVC controller.
            var planningPokerController = application.Controllers.FirstOrDefault(c => c.ControllerType == typeof(PlanningPokerController));
            if (planningPokerController != null)
            {
                application.Controllers.Remove(planningPokerController);
            }

            planningPokerController = application.Controllers.FirstOrDefault(c => c.ControllerType == typeof(AzurePlanningPokerController));
            if (planningPokerController != null)
            {
                application.Controllers.Remove(planningPokerController);
            }
        }
    }
}
