using System;
using System.Collections.Generic;
using AutoMapper;
using PlanningPoker.Domain;

namespace PlanningPoker.Service
{
    /// <summary>
    /// Maps planning poker domain entities to planning poker service data entities.
    /// </summary>
    internal static class ServiceEntityMapper
    {
        private static readonly Lazy<IConfigurationProvider> _configuration = new Lazy<IConfigurationProvider>(CreateMapperConfiguration);
        private static readonly Lazy<IMapper> _mappingEngine = new Lazy<IMapper>(() => new Mapper(_configuration.Value));

        /// <summary>
        /// Maps the specified source entity to destination entity.
        /// </summary>
        /// <typeparam name="TSource">The type of the source entity.</typeparam>
        /// <typeparam name="TDestination">The type of the destination entity.</typeparam>
        /// <param name="source">The source entity to map.</param>
        /// <returns>The mapped destination entity.</returns>
        public static TDestination Map<TSource, TDestination>(TSource source)
        {
            return _mappingEngine.Value.Map<TSource, TDestination>(source);
        }

        /// <summary>
        /// Filters or transforms message before sending to client.
        /// MemberDisconnected message of ScrumMaster is transformed to Empty message,
        /// because that is internal message and ScrumMaster is not actually disconnected.
        /// </summary>
        /// <param name="message">The message to transform.</param>
        /// <returns>The transformed message.</returns>
        public static Domain.Message FilterMessage(Domain.Message message)
        {
            if (message.MessageType == Domain.MessageType.MemberDisconnected)
            {
                var memberMessage = (Domain.MemberMessage)message;
                if (memberMessage.Member is ScrumMaster)
                {
                    return new Domain.Message(Domain.MessageType.Empty, message.Id);
                }
            }

            return message;
        }

        /// <summary>
        /// Maps service Deck value to domain Deck value.
        /// </summary>
        /// <param name="value">Service deck value.</param>
        /// <returns>Domain deck value.</returns>
        public static Domain.Deck Map(Deck value)
        {
            return (Domain.Deck)value;
        }

        private static IConfigurationProvider CreateMapperConfiguration()
        {
            var result = new MapperConfiguration(config =>
            {
                config.AllowNullCollections = true;
                config.CreateMap<Domain.ScrumTeam, ScrumTeam>();
                config.CreateMap<Observer, TeamMember>()
                    .ForMember(m => m.Type, mc => mc.MapFrom((s, d, m) => s.GetType().Name));
                config.CreateMap<Domain.Message, Message>()
                    .Include<Domain.MemberMessage, MemberMessage>()
                    .Include<Domain.EstimationResultMessage, EstimationResultMessage>()
                    .Include<Domain.EstimationSetMessage, EstimationSetMessage>()
                    .Include<Domain.TimerMessage, TimerMessage>()
                    .ForMember(m => m.Type, mc => mc.MapFrom(m => m.MessageType));
                config.CreateMap<Domain.MemberMessage, MemberMessage>();
                config.CreateMap<Domain.EstimationResultMessage, EstimationResultMessage>();
                config.CreateMap<Domain.EstimationSetMessage, EstimationSetMessage>();
                config.CreateMap<Domain.TimerMessage, TimerMessage>();
                config.CreateMap<KeyValuePair<Member, Domain.Estimation>, EstimationResultItem>()
                    .ForMember(i => i.Member, mc => mc.MapFrom(p => p.Key))
                    .ForMember(i => i.Estimation, mc => mc.MapFrom(p => p.Value));
                config.CreateMap<Domain.EstimationParticipantStatus, EstimationParticipantStatus>();
                config.CreateMap<Domain.Estimation, Estimation>()
                    .ForMember(e => e.Value, mc => mc.MapFrom((s, d, m) => MapEstimationValue(s.Value)));
            });

            result.AssertConfigurationIsValid();
            return result;
        }

        private static double? MapEstimationValue(double? value) =>
            value.HasValue && double.IsPositiveInfinity(value.Value) ? Estimation.PositiveInfinity : value;
    }
}
