using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SikayetAIWeb.Models
{
    [Table("conversations")]
    public class Conversation
    {
        [Key]
        [Column("conversation_id")]
        public int ConversationId { get; set; }

        [Column("participant1_id")]
        public int Participant1Id { get; set; }

        [Column("participant2_id")]
        public int Participant2Id { get; set; }

        // Navigation properties
        [ForeignKey("Participant1Id")]
        public User Participant1 { get; set; }

        [ForeignKey("Participant2Id")]
        public User Participant2 { get; set; }

        public List<Message> Messages { get; set; } = new List<Message>();
    }
}
