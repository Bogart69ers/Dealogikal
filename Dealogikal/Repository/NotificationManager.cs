using System;
using System.Collections.Generic;
using System.Linq;
using Dealogikal.Utils;
using Dealogikal.Database;

namespace Dealogikal.Repository
{
    public class NotificationManager
    {
        private readonly BaseRepository<notification> _notif;
        private readonly ImageManager _ImageManager;

        public NotificationManager()
        {
            _notif = new BaseRepository<notification>();
            _ImageManager = new ImageManager();
        }

        // ✅ Get single notification by Id
        public notification GetNotificationById(int id)
        {
            return _notif.Get(id);
        }

        // ✅ 1. Get unread notifications (optionally by employeeId)
        public List<notification> GetUnreadNotifications(string employeeId = null)
        {
            var query = _notif.GetAll().Where(n => n.isRead == false);

            if (!string.IsNullOrEmpty(employeeId))
            {
                query = query.Where(n => n.employeeId == employeeId);
            }

            return query.OrderByDescending(n => n.createdAt).ToList();
        }

        // ✅ 2. Mark notification as read
    
        public ErrorCode MarkAsRead(int notificationId, ref string errMsg)
        {
            try
            {
                var notification = _notif.Get(notificationId);
                if (notification == null)
                {
                    errMsg = "Notification not found";
                    return ErrorCode.Error;
                }

                notification.isRead = true; 

                return _notif.Update(notificationId,notification, out errMsg);
            }
            catch (Exception ex)
            {
                errMsg = $"An error occurred: {ex.Message}";
                return ErrorCode.Error; ;
            }
        }

        // ✅ 3. Create a notification
        public bool CreateNotification(string employeeId, string title, string message, ref string errorMessage)
        {
            try
            {
                var notification = new notification
                {
                    employeeId = employeeId,
                    title = title,
                    message = message,
                    isRead = false, // new notifications are unread
                    createdAt = DateTime.Now
                };

                return _notif.Create(notification, out errorMessage) == ErrorCode.Success;
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                return false;
            }
        }

        public List<notification> GetAllNotifications(string employeeId, int page = 1, int pageSize = 10)
        {
            var query = _notif._table.Where(e => e.employeeId == employeeId)
                                     .OrderByDescending(n => n.createdAt);

            return query.Skip((page - 1) * pageSize)
                        .Take(pageSize)
                        .ToList();
        }


        public IEnumerable<object> GetNotificationsWithProfile(string employeeId, bool unreadOnly = true)
        {
            var notifications = unreadOnly ? GetUnreadNotifications(employeeId) : GetAllNotifications(employeeId);

            var employeeImage = _ImageManager.ListImageByEmployeeId(employeeId)?.FirstOrDefault();

            string profilePicture = employeeImage != null
                ? employeeImage.imageFile
                : "profile.jpg";

            return notifications.Select(n => new
            {
                id = n.id,
                title = n.title,
                message = n.message,
                createdAt = n.createdAt?.ToString("MMM dd, yyyy hh:mm tt"),
                employeePictureUrl = "/UploadedFiles/" + profilePicture
            });
        }

    }
}
