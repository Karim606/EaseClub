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
            var time = DateTime.UtcNow;
            var requestBody = new
            {
                amount = transaction.Amount,
                currency = transaction.Currency,
                merchantReferenceId = transaction.Id.ToString(),
                timestamp = time.ToString("yyyy-MM-ddTHH:mm:ssZ"),
                signature = GenerateSignature(transaction.Amount,transaction.Currency,transaction.Id.ToString(),time.ToString("yyyy-MM-ddTHH:mm:ssZ")),
                paymentOperation = "Pay"
            };

            var response = await PostToGatewayAsync("payment-intent/api/v1/direct/session", requestBody, ct);
            if (!response.IsSuccessStatusCode)
            {
                return await HandleGatewayError<PaymentSessionResult>(response, "Session creation failed");
            }

            using var doc = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync(ct));
            _logger.LogInformation("Geidea Session Response: {Response}", doc.RootElement.ToString());
            var sessionId = doc.RootElement.GetProperty("session").GetProperty("id").GetString();

            return new PaymentSessionResult(sessionId!);
        }

        public async Task<Result<DirectPaymentResult>> ProcessPaymentAsync(PaymentTransaction transaction, CardDetailsDto cardDetails, CancellationToken ct)
        {
            // 1. Session
            var sessionResult = await CreateSessionAsync(transaction, ct);
            if (!sessionResult.IsSuccess) return sessionResult.TopError;
            var sessionId = sessionResult.Value.sessionId;

            // 2. Initiate
            var initiateResult = await InitiateAuthenticationInternalAsync(sessionId, transaction, cardDetails, ct);
            if (!initiateResult.IsSuccess) return initiateResult.TopError;
            var orderId = initiateResult.Value;

            transaction.SetGatewayIdentifiers(sessionId, orderId);

            // 3. Authenticate Payer
            return await AuthenticatePayerInternalAsync(sessionId, orderId, cardDetails, transaction, ct);
        }

        public async Task<Result<DirectPaymentResult>> FinalizePaymentAsync(PaymentTransaction transaction,CardDetailsDto cardDetails, string threeDSecureId, CancellationToken ct)
        {
            var payRequestBody = new
            {
                sessionId = transaction.GatewaySessionId,
                orderId = transaction.GatewayOrderId,
                threeDSecureId = threeDSecureId,
                paymentMethod = new
                {
                    cardNumber = cardDetails.Number,
                    cardholderName = cardDetails.HolderName,
                    expiryDate = new
                    {
                        month = int.Parse(cardDetails.ExpiryMonth),
                        year = int.Parse(cardDetails.ExpiryYear[^2..])
                    },
                    cvv = cardDetails.Cvv
                },
                source = "DirectAPI",
                paymentOperation = "Pay"
            };

            var response = await PostToGatewayAsync("pgw/api/v2/direct/pay", payRequestBody, ct);
            if (!response.IsSuccessStatusCode)
            {
                return await HandleGatewayError<DirectPaymentResult>(response, "Payment finalization failed");
            }

            using var doc = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync(ct));
                _logger.LogInformation("Geidea Finalize Payment Response: {Response}", doc.RootElement.ToString());

            if (doc.RootElement.TryGetProperty("order", out var order) &&
                order.TryGetProperty("status", out var status))
            {
                return new DirectPaymentResult(status.GetString()!, OrderId: transaction.GatewayOrderId);
            }

            return Error.Failure(description: "Payment finalization failed: Missing status in response");
        }

        #region Private Helpers

        private async Task<Result<string>> InitiateAuthenticationInternalAsync(string sessionId, PaymentTransaction transaction, CardDetailsDto cardDetails, CancellationToken ct)
        {
            var body = new
            {
                sessionId,
                cardNumber = cardDetails.Number,
                callbackUrl = _options.CallbackUrl,
                ReturnUrl = _options.SuccessUrl
            };

            var response = await PostToGatewayAsync("pgw/api/v6/direct/authenticate/initiate", body, ct);
            if (!response.IsSuccessStatusCode)
            {
                return await HandleGatewayError<string>(response, "Authentication initiation failed");
            }

            using var doc = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync(ct));
            _logger.LogInformation("Geidea Session Response: {Response}", doc.RootElement.ToString());
            var res = doc.RootElement.TryGetProperty("orderId", out var orderId);
            if (res)
            {
                return orderId.GetString()!;
            }
            return Error.Failure(description: "Authentication initiation failed: Missing orderId in response");
        }

        private async Task<Result<DirectPaymentResult>> AuthenticatePayerInternalAsync(string sessionId, string orderId, CardDetailsDto cardDetails, PaymentTransaction transaction, CancellationToken ct)
        {
            var body = new
            {
                sessionId,
                orderId,
                paymentMethod = new
                {
                    cardNumber = cardDetails.Number,
                    cardholderName = cardDetails.HolderName,
                    expiryDate = new
                    {
                        month = int.Parse(cardDetails.ExpiryMonth),
                        year = int.Parse(cardDetails.ExpiryYear[^2..])
                    },
                    cvv = cardDetails.Cvv
                },
                source = "DirectAPI",
                deviceIdentification = new {
                    providerDeviceId = Guid.NewGuid().ToString(), // In real implementation, this should be a consistent device identifier
                    language = "en",
                    userAgent = "User-Agent: Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/79.0.3945.79 Safari/537.36"

                }
            };

            var response = await PostToGatewayAsync("pgw/api/v6/direct/authenticate/payer", body, ct);
            var content = await response.Content.ReadAsStringAsync(ct);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Geidea Auth Payer failed: {Content}", content);
                return Error.Failure(description: "Authentication step failed.");
            }

            using var doc = JsonDocument.Parse(content);
            var root = doc.RootElement;
            _logger.LogInformation("Geidea Authenticate Payer Response: {Response}", root.ToString());

            var htmlBody = root.TryGetProperty("htmlBodyContent", out var htmlEl)
                ? htmlEl.GetString()
                : null;

            var responseMessage = root.GetProperty("responseMessage").GetString();

            var res = root.TryGetProperty("threeDSecureId", out var threeDSecureId);
    

            // 🟢 FRICITIONLESS SUCCESS (NO 3DS)
            if (!string.IsNullOrEmpty(htmlBody))
            {
                // 3DS required despite success status
                return new DirectPaymentResult(
                    "RequiresAction",
                    RedirectUrl: htmlBody,
                    TransactionId: transaction.Id.ToString(),
                    OrderId: orderId,
                    ThreeDSecureId: threeDSecureId.ValueKind != JsonValueKind.Undefined ? threeDSecureId.GetString() : null);
            }
            else
            {

                if (responseMessage == "Success") 
                {
                    return await FinalizePaymentAsync(transaction,cardDetails, threeDSecureId.GetString()!, ct);
                }
                return Error.Failure(description: $"Payment failed");

            }
            
        }

        private async Task<HttpResponseMessage> PostToGatewayAsync<TRequest>(string endpoint, TRequest body, CancellationToken ct)
        {
            var authInfo = $"{_options.PublicKey}:{_options.ApiPassword}";
            var authHeader = Convert.ToBase64String(Encoding.UTF8.GetBytes(authInfo));

            using var request = new HttpRequestMessage(HttpMethod.Post, $"{_options.BaseUrl.TrimEnd('/')}/{endpoint}");
            request.Headers.Authorization = new AuthenticationHeaderValue("Basic", authHeader);
            request.Content = JsonContent.Create(body);

            return await _httpClient.SendAsync(request, ct);
        }

        private async Task<Result<T>> HandleGatewayError<T>(HttpResponseMessage response, string context)
        {
            var content = await response.Content.ReadAsStringAsync();
            _logger.LogError("Geidea Gateway Error [{Context}]: {Status} - {Content}", context, response.StatusCode, content);
            return Error.Failure(description: $"{context}. Status: {response.StatusCode}");
        }

        #endregion



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
