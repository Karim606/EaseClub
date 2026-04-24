using EaseClub.Application.Features.Notifications;
using EaseClub.Infrastructure.Notifications.UserDevices;
using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Infrastructure.Notifications
{
    public class FirebaseNotificationService : IPushNotificationService
    {
        private readonly IDeviceRepository _deviceRepo;
        private readonly ILogger<FirebaseNotificationService> _logger;
        private readonly FirebaseSettings _firebaseSettings;
        public FirebaseNotificationService(
            IDeviceRepository deviceRepo,
            ILogger<FirebaseNotificationService> logger,
            IOptions<FirebaseSettings> firebaseOptions)
        {
            _deviceRepo = deviceRepo;
            _logger = logger;
            _firebaseSettings = firebaseOptions.Value;

            if (FirebaseApp.DefaultInstance == null)
            {
                var credential = GoogleCredential.FromJson($@"
                {{
                    ""type"": ""{_firebaseSettings.Type}"",
                    ""project_id"": ""{_firebaseSettings.ProjectId}"",
                    ""private_key_id"": ""{_firebaseSettings.PrivateKeyId}"",
                    ""private_key"": ""{_firebaseSettings.PrivateKey}"",
                    ""client_email"": ""{_firebaseSettings.ClientEmail}"",
                    ""client_id"": ""{_firebaseSettings.ClientId}"",
                    ""auth_uri"": ""{_firebaseSettings.AuthUri}"",
                    ""token_uri"": ""{_firebaseSettings.TokenUri}""
                }}");

                FirebaseApp.Create(new AppOptions()
                {
                    Credential = credential
                });
            }
        }

        public async Task SendToUserAsync(Guid userId, string title, string message)
        {
            // 1. Fetch ALL tokens for this user (they might have a phone and a tablet)
            var devices = await _deviceRepo.GetDevicesByUserIdAsync(userId);

            if (!devices.Any()) return;

            foreach (var device in devices)
            {
                var fcmMessage = new Message
                {
                    Token = device.FcmToken,
                    Notification = new FirebaseAdmin.Messaging.Notification
                    {
                        Title = title,
                        Body = message
                    }
                };

                try
                {
                    await FirebaseMessaging.DefaultInstance.SendAsync(fcmMessage);
                }
                catch (FirebaseMessagingException ex)
                {
                    // 2. Handle specific "Bad Token" errors
                    if (ex.MessagingErrorCode == MessagingErrorCode.Unregistered ||
                        ex.MessagingErrorCode == MessagingErrorCode.InvalidArgument)
                    {
                        _logger.LogWarning("Removing bad FCM token for user {UserId}", userId);
                        await _deviceRepo.DeleteDeviceAsync(device.FcmToken);
                    }
                    else
                    {
                        _logger.LogError(ex, "Error sending FCM to user {UserId}", userId);
                    }
                }
            }
        }


    }
}
