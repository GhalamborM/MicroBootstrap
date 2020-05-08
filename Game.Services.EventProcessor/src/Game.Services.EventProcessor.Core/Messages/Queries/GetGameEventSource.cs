using System;
using Common.Types;
using Game.Services.EventProcessor.Core.DTO;

namespace Game.Services.EventProcessor.Core.Messages.Queries
{
    public class GetGameEventSource : IQuery<GameEventSourceDto>
    {
        public Guid Id { get; set; } 
    }
}