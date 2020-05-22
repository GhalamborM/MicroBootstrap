using System;

namespace Game.API.DTO
{
    public class GameEventSourceDto
    {
        public Guid Id { get; set; }
        public bool IsWin { get; set; }
        public int Score { get; set; }
    }
}