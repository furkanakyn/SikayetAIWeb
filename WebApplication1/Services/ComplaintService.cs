using Microsoft.EntityFrameworkCore;
using SikayetAIWeb.Models;
using System;
using System.Collections.Generic;
using System.Linq;

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
                .Include(c => c.AssignedDepartment)
                .FirstOrDefault(c => c.ComplaintId == complaintId);
        }


        public Complaint CreateComplaint(Complaint complaint)
        {
            try
            {

                complaint.User = null;
                complaint.Responses = null;

                if (string.IsNullOrWhiteSpace(complaint.Category2))
                {
                    complaint.Category2 = null;
                }


                if (complaint.CreatedAt == default)
                {
                    complaint.CreatedAt = DateTime.UtcNow;
                }

                if (complaint.UpdatedAt == default)
                {
                    complaint.UpdatedAt = DateTime.UtcNow;
                }

                var mapping = _context.CategoryDepartmentMappings
                .FirstOrDefault(cd => cd.CategoryName.ToLower() == complaint.Category.ToLower());

                if (mapping != null)
                {
                    complaint.AssignedDepartmentId = mapping.DepartmentId;
                }

                _context.Complaints.Add(complaint);
                _context.SaveChanges();
                return complaint;
            }
            catch (Exception ex)
            {
                throw new Exception("Şikayet oluşturulamadı: " + ex.Message, ex);
            }
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
            try
            {
                _context.Responses.Add(response);

             
                var complaint = _context.Complaints.Find(response.ComplaintId);
                if (complaint != null && complaint.Status == ComplaintStatus.pending)
                {
                    complaint.Status = ComplaintStatus.in_progress;
                    complaint.UpdatedAt = DateTime.UtcNow;
                }

                _context.SaveChanges();
                return response;
            }
            catch (Exception ex)
            {
                throw new Exception("Yanıt eklenemedi: " + ex.Message, ex);
            }
        }

        public IEnumerable<Response> GetComplaintResponses(int complaintId, int userId)
        {
            var responses = _context.Responses
                .Where(r => r.ComplaintId == complaintId)
                .OrderBy(r => r.CreatedAt)
                .ToList();

            var complaint = _context.Complaints.Find(complaintId);
            if (complaint != null && complaint.UserId == userId)
            {
                var unreadResponses = responses.Where(r => r.UserId != userId ).ToList();
               
                if (unreadResponses.Any())
                {
                    try
                    {
                        _context.SaveChanges();
                    }
                    catch
                    {
                        
                    }
                }
            }

            return responses;
        }
    }
}