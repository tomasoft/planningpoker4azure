﻿using System;
using System.Collections.Generic;
using System.Text.Encodings.Web;
using PlanningPoker.Client.Service;
using PlanningPoker.Service;

namespace PlanningPoker.Client.Controllers
{
    /// <summary>
    /// Helper functions used by UI logic.
    /// </summary>
    internal static class ControllerHelper
    {
        /// <summary>
        /// Gets collection of available estimation decks, which can be selected, when creating new team.
        /// </summary>
        public static IReadOnlyDictionary<Deck, string> EstimationDecks { get; } = new SortedDictionary<Deck, string>
        {
            { Deck.Standard, Resources.EstimationDeck_Standard },
            { Deck.Fibonacci, Resources.EstimationDeck_Fibonacci },
            { Deck.Rating, Resources.EstimationDeck_Rating },
            { Deck.Tshirt, Resources.EstimationDeck_Tshirt },
            { Deck.RockPaperScissorsLizardSpock, Resources.EstimationDeck_RockPaperScissorsLizardSpock },
        };

        /// <summary>
        /// Gets user friendly message from <see cref="PlanningPokerException"/> object.
        /// </summary>
        /// <param name="exception">Exception to get message for.</param>
        /// <returns>User friendly text message.</returns>
        public static string GetErrorMessage(PlanningPokerException exception)
        {
            if (exception == null)
            {
                throw new ArgumentNullException(nameof(exception));
            }

            var result = exception.Message;
            var newLineIndex = result.IndexOf('\n', StringComparison.Ordinal);
            if (newLineIndex >= 0)
            {
                result = result.Substring(0, newLineIndex);
            }

            return result;
        }

        /// <summary>
        /// Opens PlanningPoker page with specified team name and member name.
        /// </summary>
        /// <param name="navigationManager">Helper to navigate to URL.</param>
        /// <param name="team">Scrum team name to include in URL.</param>
        /// <param name="member">Member name to include in URL.</param>
        public static void OpenPlanningPokerPage(INavigationManager navigationManager, ScrumTeam team, string member)
        {
            if (navigationManager == null)
            {
                throw new ArgumentNullException(nameof(navigationManager));
            }

            var urlEncoder = UrlEncoder.Default;
            var uri = $"PlanningPoker/{urlEncoder.Encode(team.Name)}/{urlEncoder.Encode(member)}";
            navigationManager.NavigateTo(uri);
        }

        /// <summary>
        /// Opens Index page with prefilled team name and member name.
        /// </summary>
        /// <param name="navigationManager">Helper to navigate to URL.</param>
        /// <param name="team">Scrum team name to include in URL.</param>
        /// <param name="member">Member name to include in URL.</param>
        public static void OpenIndexPage(INavigationManager navigationManager, string? team, string? member)
        {
            if (navigationManager == null)
            {
                throw new ArgumentNullException(nameof(navigationManager));
            }

            var urlEncoder = UrlEncoder.Default;
            var uri = "Index";

            if (!string.IsNullOrEmpty(team))
            {
                uri += '/' + urlEncoder.Encode(team);

                if (!string.IsNullOrEmpty(member))
                {
                    uri += '/' + urlEncoder.Encode(member);
                }
            }

            navigationManager.NavigateTo(uri);
        }
    }
}