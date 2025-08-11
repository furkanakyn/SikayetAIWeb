using SikayetAIWeb.Models;
using System.Collections.Generic;

namespace SikayetAIWeb.ViewModels
{
    public class ChatViewModel
    {
        public Conversation Conversation { get; set; }
        public User RecipientUser { get; set; }
        public User CurrentUser { get; set; }
        public string NewMessageContent { get; set; }
    }
}