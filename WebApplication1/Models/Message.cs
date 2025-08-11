using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SikayetAIWeb.Models
{
    [Table("messages")]
    public class Message
    {
        [Key]
        [Column("message_id")]
        public int MessageId { get; set; }

        [Column("conversation_id")]
        public int ConversationId { get; set; }

        [Column("sender_id")]
        public int SenderId { get; set; }

        [Required]
        [Column("content")]
        public string Content { get; set; }

        [Column("sent_at")]
        public DateTime SentAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey("ConversationId")]
        public Conversation Conversation { get; set; }

        [ForeignKey("SenderId")]
        public User Sender { get; set; }


        [Column("is_read")]
        public bool IsRead { get; set; } = false;
    }
}