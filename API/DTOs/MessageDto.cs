using System;
using System.Text.Json.Serialization;

namespace API.DTOs;

public class MessageDto
{
    public int Id { get; set; }
        public int SenderId { get; set; }
        public required string SenderUsername { get; set; }
        public required string SenderPhotoUrl { get; set; }
        public int RecipientId { get; set; }
        public required string RecipientUsername { get; set; }
        public required string RecipientPhotoUrl { get; set; }
        public required string Content { get; set; }
        public DateTime? DateRead { get; set; }
        public required DateTime MessageSent { get; set; }

        [JsonIgnore]
        public bool SenderDeleted { get; set; }

        [JsonIgnore]
        public bool RecipientDeleted { get; set; }
}
