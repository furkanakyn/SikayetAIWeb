using Microsoft.EntityFrameworkCore;
using SikayetAIWeb.Models;
using Microsoft.EntityFrameworkCore;

namespace SikayetAIWeb.Services
{
    public class ComplaintService
    {
        private readonly ApplicationDbContext _context;

        public ComplaintService(ApplicationDbContext context)
        {
            _context = context;
        }
        

        public Complaint? GetComplaintDetails(int complaintId)
        {
        return _context.Complaints
            .Include(c => c.Responses)
            .Include(c => c.User)
            .FirstOrDefault(c => c.Id == complaintId);
        }

        public Complaint CreateComplaint(Complaint complaint)
        {
            _context.Complaints.Add(complaint);
            _context.SaveChanges();
            return complaint;
        }

        public IEnumerable<Complaint> GetUserComplaints(int userId)
        {
            return _context.Complaints
                .Where(c => c.UserId == userId)
                .OrderByDescending(c => c.CreatedAt)
                .ToList();
        }

        public IEnumerable<Complaint> GetDepartmentComplaints(UserType userType, string? category = null)
        {
            var query = _context.Complaints.AsQueryable();

            if (userType == UserType.municipality && !string.IsNullOrEmpty(category))
            {
                query = query.Where(c => c.Category == category);
            }

            return query.OrderByDescending(c => c.CreatedAt).ToList();
        }

        public Response AddResponse(Response response)
        {
            _context.Responses.Add(response);

            // Update complaint status
            var complaint = _context.Complaints.Find(response.ComplaintId);
            if (complaint != null && complaint.Status == ComplaintStatus.Pending)
            {
                complaint.Status = ComplaintStatus.InProgress;
                complaint.UpdatedAt = DateTime.UtcNow;
            }

            _context.SaveChanges();
            return response;
        }

        public IEnumerable<Response> GetComplaintResponses(int complaintId, int userId)
        {
            var responses = _context.Responses
                .Where(r => r.ComplaintId == complaintId)
                .OrderBy(r => r.CreatedAt)
                .ToList();

            // Mark responses as read if viewed by complaint owner
            var unreadResponses = responses.Where(r => !r.IsRead && r.Complaint.UserId == userId).ToList();
            foreach (var res in unreadResponses)
            {
                res.IsRead = true;
            }

            if (unreadResponses.Any())
                _context.SaveChanges();

            return responses;
        }

    }
}