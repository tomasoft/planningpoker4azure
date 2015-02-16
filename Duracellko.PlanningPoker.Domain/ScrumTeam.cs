﻿// <copyright>
// Copyright (c) 2012 Rasto Novotny
// </copyright>

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Duracellko.PlanningPoker.Domain
{
    /// <summary>
    /// Scrum team is a group of members, who play planning poker, and observers, who watch the game.
    /// </summary>
    [Serializable]
    [SuppressMessage("StyleCop.CSharp.OrderingRules", "SA1201:ElementsMustAppearInTheCorrectOrder", Justification = "Events are placed together with protected methods.")]
    public class ScrumTeam
    {
        #region Fields

        private readonly List<Member> members = new List<Member>();
        private readonly List<Observer> observers = new List<Observer>();

        private readonly Estimation[] availableEstimations = new Estimation[]
        {
            new Estimation(0.0),
            new Estimation(0.5),
            new Estimation(1.0),
            new Estimation(2.0),
            new Estimation(3.0),
            new Estimation(5.0),
            new Estimation(8.0),
            new Estimation(13.0),
            new Estimation(20.0),
            new Estimation(40.0),
            new Estimation(100.0),
            new Estimation(double.PositiveInfinity),
            new Estimation()
        };

        private EstimationResult estimationResult;

        [NonSerialized]
        private DateTimeProvider dateTimeProvider;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="ScrumTeam"/> class.
        /// </summary>
        /// <param name="name">The team name.</param>
        public ScrumTeam(string name)
            : this(name, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ScrumTeam"/> class.
        /// </summary>
        /// <param name="name">The team name.</param>
        /// <param name="dateTimeProvider">The date time provider to provide current time. If null is specified, then default date time provider is used.</param>
        public ScrumTeam(string name, DateTimeProvider dateTimeProvider)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException("name");
            }

            this.dateTimeProvider = dateTimeProvider ?? Duracellko.PlanningPoker.Domain.DateTimeProvider.Default;
            this.Name = name;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the Scrum team name.
        /// </summary>
        /// <value>The Scrum team name.</value>
        public string Name { get; private set; }

        /// <summary>
        /// Gets the observers watching planning poker game of the Scrum team.
        /// </summary>
        /// <value>The observers collection.</value>
        public IEnumerable<Observer> Observers
        {
            get
            {
                return this.observers;
            }
        }

        /// <summary>
        /// Gets the collection members joined to the Scrum team.
        /// </summary>
        /// <value>The members collection.</value>
        public IEnumerable<Member> Members
        {
            get
            {
                return this.members;
            }
        }

        /// <summary>
        /// Gets the scrum master of the team.
        /// </summary>
        /// <value>The Scrum master.</value>
        public ScrumMaster ScrumMaster
        {
            get
            {
                return this.Members.OfType<ScrumMaster>().FirstOrDefault();
            }
        }

        /// <summary>
        /// Gets the available estimations the members can pick from.
        /// </summary>
        /// <value>The collection of available estimations.</value>
        public IEnumerable<Estimation> AvailableEstimations
        {
            get
            {
                return this.availableEstimations;
            }
        }

        /// <summary>
        /// Gets the current Scrum team state.
        /// </summary>
        /// <value>The team state.</value>
        public TeamState State { get; private set; }

        /// <summary>
        /// Gets the estimation result, when <see cref="P:State"/> is EstimationFinished.
        /// </summary>
        /// <value>The estimation result.</value>
        public EstimationResult EstimationResult
        {
            get
            {
                return this.State == TeamState.EstimationFinished ? this.estimationResult : null;
            }
        }

        /// <summary>
        /// Gets the collection of participants in current estimation.
        /// </summary>
        /// <value>
        /// The estimation participants.
        /// </value>
        public IEnumerable<EstimationParticipantStatus> EstimationParticipants
        {
            get
            {
                if (this.State == TeamState.EstimationInProgress)
                {
                    return this.estimationResult.Select(p => new EstimationParticipantStatus(p.Key.Name, p.Value != null)).ToList();
                }
                else
                {
                    return null;
                }
            }
        }

        /// <summary>
        /// Gets the date time provider used by the Scrum team.
        /// </summary>
        /// <value>The date-time provider.</value>
        public DateTimeProvider DateTimeProvider
        {
            get
            {
                return this.dateTimeProvider;
            }
        }

        #endregion

        #region Public methods

        /// <summary>
        /// Sets new scrum master of the team.
        /// </summary>
        /// <param name="name">The Scrum master name.</param>
        /// <returns>The new Scrum master.</returns>
        public ScrumMaster SetScrumMaster(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException("name");
            }

            if (this.FindMemberOrObserver(name) != null)
            {
                throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, Properties.Resources.Error_MemberAlreadyExists, name), "name");
            }

            if (this.ScrumMaster != null)
            {
                throw new InvalidOperationException(Properties.Resources.Error_ScrumMasterAlreadyExists);
            }

            var scrumMaster = new ScrumMaster(this, name);
            this.members.Add(scrumMaster);

            var recipients = this.UnionMembersAndObservers().Where(m => m != scrumMaster);
            this.SendMessage(recipients, () => new MemberMessage(MessageType.MemberJoined) { Member = scrumMaster });

            return scrumMaster;
        }

        /// <summary>
        /// Connects new member or observer with specified name.
        /// </summary>
        /// <param name="name">The member name.</param>
        /// <param name="asObserver">If set to <c>true</c> then connect new observer, otherwise member.</param>
        /// <returns>The observer or member, who joined the Scrum team.</returns>
        public Observer Join(string name, bool asObserver)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException("name");
            }

            if (this.FindMemberOrObserver(name) != null)
            {
                throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, Properties.Resources.Error_MemberAlreadyExists, name), "name");
            }

            Observer result;
            if (asObserver)
            {
                var observer = new Observer(this, name);
                this.observers.Add(observer);
                result = observer;
            }
            else
            {
                var member = new Member(this, name);
                this.members.Add(member);
                result = member;
            }

            var recipients = this.UnionMembersAndObservers().Where(m => m != result);
            this.SendMessage(recipients, () => new MemberMessage(MessageType.MemberJoined) { Member = result });

            return result;
        }

        /// <summary>
        /// Disconnects member with specified name from the Scrum team.
        /// </summary>
        /// <param name="name">The member name.</param>
        public void Disconnect(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException("name");
            }

            var observer = this.observers.FirstOrDefault(o => MatchObserverName(o, name));
            if (observer != null)
            {
                this.observers.Remove(observer);

                var recipients = this.UnionMembersAndObservers();
                this.SendMessage(recipients, () => new MemberMessage(MessageType.MemberDisconnected) { Member = observer });

                // Send message to disconnecting observer, so that he/she stops waiting for messages.
                observer.SendMessage(new Message(MessageType.Empty));
            }
            else
            {
                var member = this.members.FirstOrDefault(o => MatchObserverName(o, name));
                if (member != null)
                {
                    this.members.Remove(member);

                    if (this.State == TeamState.EstimationInProgress)
                    {
                        // Check if all members picked estimations. If member disconnects then his/her estimation is null.
                        this.UpdateEstimationResult(null);
                    }

                    var recipients = this.UnionMembersAndObservers();
                    this.SendMessage(recipients, () => new MemberMessage(MessageType.MemberDisconnected) { Member = member });

                    // Send message to disconnecting member, so that he/she stops waiting for messages.
                    member.SendMessage(new Message(MessageType.Empty));
                }
            }
        }

        /// <summary>
        /// Finds existing member or observer with specified name.
        /// </summary>
        /// <param name="name">The member name.</param>
        /// <returns>The member or observer.</returns>
        public Observer FindMemberOrObserver(string name)
        {
            var allObservers = this.Observers.Union(this.Members);
            return allObservers.FirstOrDefault(o => MatchObserverName(o, name));
        }

        /// <summary>
        /// Disconnects inactive observers, who did not checked for messages for specified period of time.
        /// </summary>
        /// <param name="inactivityTime">The inactivity time.</param>
        public void DisconnectInactiveObservers(TimeSpan inactivityTime)
        {
            var lastInactivityTime = this.DateTimeProvider.UtcNow - inactivityTime;
            var isObserverActive = new Func<Observer, bool>(o => o.LastActivity < lastInactivityTime);
            var inactiveObservers = this.Observers.Where(isObserverActive).ToArray();
            var inactiveMembers = this.Members.Where<Member>(isObserverActive).ToArray();

            if (inactiveObservers.Length > 0 || inactiveMembers.Length > 0)
            {
                foreach (var observer in inactiveObservers)
                {
                    this.observers.Remove(observer);
                }

                foreach (var member in inactiveMembers)
                {
                    this.members.Remove(member);
                }

                var recipients = this.UnionMembersAndObservers();
                foreach (var member in inactiveObservers.Union(inactiveMembers))
                {
                    this.SendMessage(recipients, () => new MemberMessage(MessageType.MemberDisconnected) { Member = member });
                }

                if (inactiveMembers.Length > 0)
                {
                    if (this.State == TeamState.EstimationInProgress)
                    {
                        // Check if all members picked estimations. If member disconnects then his/her estimation is null.
                        this.UpdateEstimationResult(null);
                    }
                }
            }
        }

        #endregion

        #region Internal methods

        /// <summary>
        /// Starts new estimation.
        /// </summary>
        internal void StartEstimation()
        {
            this.State = TeamState.EstimationInProgress;

            foreach (var member in this.Members)
            {
                member.ResetEstimation();
            }

            this.estimationResult = new EstimationResult(this.Members);

            var recipients = this.UnionMembersAndObservers();
            this.SendMessage(recipients, () => new Message(MessageType.EstimationStarted));
        }

        /// <summary>
        /// Cancels current estimation.
        /// </summary>
        internal void CancelEstimation()
        {
            this.State = TeamState.EstimationCanceled;
            this.estimationResult = null;

            var recipients = this.UnionMembersAndObservers();
            this.SendMessage(recipients, () => new Message(MessageType.EstimationCanceled));
        }

        /// <summary>
        /// Notifies that a member has placed estimation.
        /// </summary>
        /// <param name="member">The member, who estimated.</param>
        internal void OnMemberEstimated(Member member)
        {
            var recipients = this.UnionMembersAndObservers();
            this.SendMessage(recipients, () => new MemberMessage(MessageType.MemberEstimated) { Member = member });
            this.UpdateEstimationResult(member);
        }

        /// <summary>
        /// Notifies that a member is still active.
        /// </summary>
        /// <param name="observer">The observer.</param>
        internal void OnObserverActivity(Observer observer)
        {
            this.SendMessage(new MemberMessage(MessageType.MemberActivity) { Member = observer });
        }

        #endregion

        #region Events

        /// <summary>
        /// Occurs when a new message is received.
        /// </summary>
        [field: NonSerialized]
        public event EventHandler<MessageReceivedEventArgs> MessageReceived;

        /// <summary>
        /// Raises the <see cref="E:MessageReceived"/> event.
        /// </summary>
        /// <param name="e">The <see cref="MessageReceivedEventArgs"/> instance containing the event data.</param>
        protected virtual void OnMessageReceived(MessageReceivedEventArgs e)
        {
            if (this.MessageReceived != null)
            {
                this.MessageReceived(this, e);
            }
        }

        #endregion

        #region Private methods

        private static bool MatchObserverName(Observer observer, string name)
        {
            return string.Equals(observer.Name, name, StringComparison.OrdinalIgnoreCase);
        }

        private IEnumerable<Observer> UnionMembersAndObservers()
        {
            foreach (var member in this.Members)
            {
                yield return member;
            }

            foreach (var observer in this.Observers)
            {
                yield return observer;
            }
        }

        private void SendMessage(Message message)
        {
            if (message != null)
            {
                this.OnMessageReceived(new MessageReceivedEventArgs(message));
            }
        }

        private void SendMessage(IEnumerable<Observer> recipients, Func<Message> messageFactory)
        {
            this.SendMessage(messageFactory());
            foreach (var recipient in recipients)
            {
                recipient.SendMessage(messageFactory());
            }
        }

        /// <summary>
        /// Checks if all members picked an estimation. If yes, then finishes the estimation.
        /// </summary>
        /// <param name="member">The who initiated member updating of estimation results.</param>
        private void UpdateEstimationResult(Member member)
        {
            if (member != null)
            {
                if (this.estimationResult.ContainsMember(member))
                {
                    this.estimationResult[member] = member.Estimation;
                }
            }

            if (this.estimationResult.All(p => p.Value != null || !this.Members.Contains(p.Key)))
            {
                this.estimationResult.SetReadOnly();
                this.State = TeamState.EstimationFinished;

                var recipients = this.UnionMembersAndObservers();
                this.SendMessage(recipients, () => new EstimationResultMessage(MessageType.EstimationEnded) { EstimationResult = this.estimationResult });
            }
        }

        #endregion

        #region Serialization

        [OnDeserialized]
        private void OnDeserialized(StreamingContext context)
        {
            var dateTimeProvider = context.Context as DateTimeProvider;
            this.dateTimeProvider = dateTimeProvider ?? Duracellko.PlanningPoker.Domain.DateTimeProvider.Default;
        }

        #endregion
    }
}
