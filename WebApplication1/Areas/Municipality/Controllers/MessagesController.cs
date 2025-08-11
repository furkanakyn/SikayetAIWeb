using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SikayetAIWeb.ViewModels;
using SikayetAIWeb.Models;
using System.Collections.Generic;

namespace SikayetAIWeb.Areas.Municipality.Controllers
{
    [Area("Municipality")]
    [Authorize]
    public class MessagesController : Controller
    {
        private readonly ApplicationDbContext _dbContext;

        public MessagesController(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<IActionResult> Index()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                return Unauthorized();
            }

            var userId = int.Parse(userIdClaim);

            var viewModel = new MessageIndexViewModel
            {
                Departments = await _dbContext.Departments.ToListAsync(),
                Conversations = await _dbContext.Conversations
                    .Include(c => c.Participant1)
                    .Include(c => c.Participant2)
                    .Where(c => c.Participant1Id == userId || c.Participant2Id == userId)
                    .ToListAsync()
            };

            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> GetUsersByDepartment(int departmentId)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                return Unauthorized();
            }
            var currentUserId = int.Parse(userIdClaim);

            var users = await _dbContext.Users
                .Where(u => u.DepartmentId == departmentId && u.Id != currentUserId)
                .Select(u => new { id = u.Id, fullName = u.FullName })
                .ToListAsync();

            return Json(users);
        }

        public async Task<IActionResult> Chat(int recipientId)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                return Unauthorized();
            }
            var senderId = int.Parse(userIdClaim);

            var conversation = await _dbContext.Conversations
                .Include(c => c.Messages.OrderBy(m => m.SentAt))
                .ThenInclude(m => m.Sender)
                .FirstOrDefaultAsync(c =>
                    (c.Participant1Id == senderId && c.Participant2Id == recipientId) ||
                    (c.Participant1Id == recipientId && c.Participant2Id == senderId));

            if (conversation == null)
            {
                conversation = new Conversation
                {
                    Participant1Id = senderId,
                    Participant2Id = recipientId,
                    Messages = new List<Message>()
                };
                _dbContext.Conversations.Add(conversation);
                await _dbContext.SaveChangesAsync();
            }
            else
            {
                // Gelen mesajları okundu olarak işaretle
                var unreadMessages = await _dbContext.Messages
                    .Where(m => m.ConversationId == conversation.ConversationId && m.SenderId != senderId && !m.IsRead)
                    .ToListAsync();

                foreach (var message in unreadMessages)
                {
                    message.IsRead = true;
                }
                await _dbContext.SaveChangesAsync();
            }

            var recipientUser = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == recipientId);
            var currentUser = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == senderId);

            var viewModel = new ChatViewModel
            {
                Conversation = conversation,
                RecipientUser = recipientUser,
                CurrentUser = currentUser
            };

            return PartialView("_ChatPartial", viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> SendMessage(int recipientId, string content)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                return Unauthorized();
            }
            var senderId = int.Parse(userIdClaim);

            var conversation = await _dbContext.Conversations
                .FirstOrDefaultAsync(c =>
                    (c.Participant1Id == senderId && c.Participant2Id == recipientId) ||
                    (c.Participant1Id == recipientId && c.Participant2Id == senderId));

            if (conversation == null)
            {
                conversation = new Conversation
                {
                    Participant1Id = senderId,
                    Participant2Id = recipientId,
                    Messages = new List<Message>()
                };
                _dbContext.Conversations.Add(conversation);
                await _dbContext.SaveChangesAsync();
            }

            var message = new Message
            {
                SenderId = senderId,
                ConversationId = conversation.ConversationId,
                Content = content,
                SentAt = DateTime.Now,
                IsRead = false // Yeni gönderilen mesaj okundu değil
            };

            _dbContext.Messages.Add(message);
            await _dbContext.SaveChangesAsync();

            return Ok();
        }

        [HttpGet]
        public async Task<IActionResult> GetUnreadMessageCount()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                return Unauthorized();
            }
            var currentUserId = int.Parse(userIdClaim);

            var unreadCount = await _dbContext.Messages
                .Where(m => m.SenderId != currentUserId && !m.IsRead)
                .Where(m => m.Conversation.Participant1Id == currentUserId || m.Conversation.Participant2Id == currentUserId)
                .CountAsync();

            return Json(new { count = unreadCount });
        }
    }
}