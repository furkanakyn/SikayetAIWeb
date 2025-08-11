using SikayetAIWeb.Models;
using System.Collections.Generic;

namespace SikayetAIWeb.ViewModels
{
    public class MessageIndexViewModel
    {
        public List<Department> Departments { get; set; }
        public List<Conversation> Conversations { get; set; }
    }
}

