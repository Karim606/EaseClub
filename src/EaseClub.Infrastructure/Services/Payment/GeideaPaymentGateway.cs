using EaseClub.Application.Features.Payment;
using EaseClub.Application.Features.Payment.Commands.ProcessPaymentGatewayWebhookCommand;
using EaseClub.Domain.Common;
using EaseClub.Domain.Common.Results;
using EaseClub.Domain.Payment;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace EaseClub.Infrastructure.Services.Payment
{
    public class GeideaPaymentGateway : IPaymentGateway
    {
        private readonly HttpClient _httpClient;
        private readonly GeideaOptions _options;
        private readonly ILogger<GeideaPaymentGateway> _logger;
        public GeideaPaymentGateway(HttpClient httpClient, IOptions<GeideaOptions> options, ILogger<GeideaPaymentGateway> logger)
        {
            _httpClient = httpClient;
            _options = options.Value;
            _logger = logger;
        }

        public async Task<Result<PaymentSessionResult>> CreateSessionAsync(PaymentTransaction transaction, CancellationToken ct)
        {
            var timestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ");

            var signature = GenerateSignature(
                transaction.Amount,
                _options.Currency,
                transaction.Id.ToString(), // Using transactionId as MerchantReferenceId
                timestamp
                );

            var requestBody = new
            {
                amount = transaction.Amount,
                currency = "EGP",
                merchantReferenceId = transaction.Id.ToString(), // Link Geidea to our InvoiceId
                timestamp,
                signature,
                callbackUrl = _options.CallbackUrl,
                returnUrl = _options.SuccessUrl,
                paymentOperation = "Pay"
            };
            // 3. ADD BASIC AUTHENTICATION HEADER
            var authInfo = $"{_options.PublicKey}:{_options.ApiPassword}";
            var authHeader = Convert.ToBase64String(Encoding.ASCII.GetBytes(authInfo));

            using var request = new HttpRequestMessage(HttpMethod.Post, $"{_options.BaseUrl}/payment-intent/api/v2/direct/session");
            request.Headers.Authorization = new AuthenticationHeaderValue("Basic", authHeader);
            request.Content = JsonContent.Create(requestBody);

            var response = await _httpClient.SendAsync(request, ct);
            using var doc = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync(ct));
            var root = doc.RootElement;

            // Navigate the JSON tree: session -> id
            var sessionId = root.GetProperty("session").GetProperty("id").GetString();


            if (!response.IsSuccessStatusCode || string.IsNullOrEmpty(sessionId))
            {
                var content = await response.Content.ReadAsStringAsync();
                // Log EVERYTHING
                _logger.LogError("Geidea request failed. Status: {StatusCode}, Response: {Content}",
                    response.StatusCode, content);

                throw new Exception($"Payment request failed: {response.StatusCode} - {content}");
            }

            return new PaymentSessionResult(sessionId);
        }



        public Result<CallBackResultDto> ValidateCallbackSignature(object payload)
        {

            var geideaPayload = payload switch
            {
                GeideaCallbackDto dto => dto,
                JsonElement json => JsonSerializer.Deserialize<GeideaCallbackDto>(json.GetRawText()),
                string str => JsonSerializer.Deserialize<GeideaCallbackDto>(str),
                _ => throw new ArgumentException("Unsupported payload type")
            };
            // 1. Build raw string EXACTLY as per the new documentation order
            // Order: PublicKey + Amount + Currency + OrderId + Status + RefId + Timestamp
            var amountStr = geideaPayload.OrderAmount.ToString("F2", CultureInfo.InvariantCulture);

            var rawString = string.Concat(
                _options.PublicKey,
                amountStr,
                geideaPayload.OrderCurrency,
                geideaPayload.OrderId,
                geideaPayload.Status,
                geideaPayload.MerchantReferenceId,
                geideaPayload.TimeStamp
            );

            // 2. Geidea "New Signature" usually expects HMACSHA256 
            // using the ApiPassword as the KEY
            var hashed = ComputeHmacSha256(rawString, _options.ApiPassword);
            var generatedSignature = Convert.ToBase64String(hashed);

            // 3. Compare
            var isValid = generatedSignature == geideaPayload.Signature;

            if (!isValid)
            {
                _logger.LogWarning("Invalid Webhook Signature. Expected: {Exp}, Got: {Got}",
                    generatedSignature, geideaPayload.Signature);
                return Error.Failure(description: "Invalid Webhook Signature");

            }
            var isSuccess = geideaPayload.Status.Equals("Success", StringComparison.OrdinalIgnoreCase);

            return new CallBackResultDto(Guid.Parse(geideaPayload.MerchantReferenceId), geideaPayload.PaymentMethod,geideaPayload.OrderId,isSuccess,geideaPayload.DetailedResponseMessage);
        }


        private string GenerateSignature(
            decimal orderAmount,
            string orderCurrency,
            string orderMerchantReferenceId,
            string timestamp)
        {
            // Format amount to 2 decimal places like PHP number_format
            string amountStr = orderAmount.ToString("F2", CultureInfo.InvariantCulture);

            // Build data string
            string data = $"{_options.PublicKey}{amountStr}{orderCurrency}{orderMerchantReferenceId}{timestamp}";

            var hash = ComputeHmacSha256(data, _options.ApiPassword);

            return Convert.ToBase64String(hash);
        }

        private byte[] ComputeHmacSha256(string data, string key)
        {
            var keyBytes = Encoding.UTF8.GetBytes(key);
            var dataBytes = Encoding.UTF8.GetBytes(data);

            using var hmac = new HMACSHA256(keyBytes);
            return hmac.ComputeHash(dataBytes);
        }
    }


}
