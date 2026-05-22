using EaseClub.Application.Features.Payment.Commands.AttachOrderToTransaction;
//using EaseClub.Application.Features.Payment.Commands.IntiatePayment;
using EaseClub.Application.Features.Payment.Commands.ProcessPaymentGatewayWebhookCommand;
using EaseClub.Application.Features.Payment.Commands.ProcessDirectPayment;
using EaseClub.Application.Features.Payment.Commands.FinalizeDirectPayment;
using EaseClub.Application.Features.Payment;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EaseClub.Api.Controllers
{
    [Route("api/v{version:ApiVersion}/payments")]
    public class PaymentGatewaysController(ISender sender) : ApiController
    {
        [HttpPost("process")]
        [ProducesResponseType(typeof(DirectPaymentResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [EndpointName("ProcessDirectPayment")]
        [EndpointSummary("Processes a direct server-to-server payment (Step 1-3)")]
        [EndpointDescription("Creates a transaction and processes it with the gateway. Returns Success if immediate or RedirectUrl if 3DS is required.")]
        public async Task<IActionResult> ProcessPayment([FromBody] ProcessDirectPaymentCommand command)
        {
            var result = await sender.Send(command);

            return result.Match(
                res => Ok(res),
                Problem
            );
        }

        [HttpPost("finalize")]
        [ProducesResponseType(typeof(DirectPaymentResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [EndpointName("FinalizeDirectPayment")]
        [EndpointSummary("Finalizes a payment after 3DS authentication")]
        [EndpointDescription("Calls the 'Pay' endpoint with the threeDSecureId obtained from the bank challenge.")]
        public async Task<IActionResult> FinalizePayment([FromBody] FinalizeDirectPaymentCommand command)
        {
            var result = await sender.Send(command);

            return result.Match(
                res => Ok(res),
                Problem
            );
        }

        // [HttpPost("initiate/{invoiceId}")]

        // [ProducesResponseType(typeof(IntiatePaymentResponse), StatusCodes.Status200OK)]
        // [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        // [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]

        // [EndpointName("InitiatePayment")]
        // [EndpointSummary("Starts a new payment journey")]
        // [EndpointDescription("Creates a new internal PaymentTransaction linked to the Invoice and requests a SessionId from the chosen gateway.\n\n" +
        //     "Behavior:\n" +
        //     "- Validates invoice status (must be Unpaid).\n" +
        //     "- Records a new 'Pending' attempt in the database.\n" +
        //     "- Returns a TransactionId (used as MerchantReference) and a Gateway SessionId.")]
        // public async Task<IActionResult> InitiatePayment(Guid invoiceId,InitiatePaymentRequest request)
        // {
        //     var command = new InitiatePaymentCommand(invoiceId,request.Gateway);
        //     var result = await sender.Send(command);

        //     return result.Match(
        //         res => Ok(res),
        //         Problem
        //     );
        // }

        // Webhook endpoint for a specific gateway
        [HttpPost("webhook")]

        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]

        [EndpointName("PaymentWebhook")]
        [EndpointSummary("Gateway Callback Handler")]
        [EndpointDescription("Asynchronous callback endpoint for payment gateways (e.g., Geidea).\n\n" +
            "Behavior:\n" +
            "- Validates the HMAC signature using the API secret.\n" +
            "- Matches the callback to a transaction using the MerchantReferenceId.\n" +
            "- Updates Transaction status and confirms the linked Invoice if successful.")]
        public async Task<IActionResult> Webhook([FromBody] object webhookRequest)
        {
            var command = new ProcessPaymentGatewayWebhookCommand(webhookRequest);
            var result = await sender.Send(command);

            return result.Match(
                success => Ok(),
                Problem
            );
        }

        // [HttpPatch("transactions/{transactionId:guid}/attach-order")]

        // [ProducesResponseType(StatusCodes.Status204NoContent)]
        // [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        // [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]

        // [EndpointName("AttachOrderId")]
        // [EndpointSummary("Links Gateway OrderId to Transaction")]
        // [EndpointDescription("Updates a pending transaction with the permanent OrderId received from the gateway's frontend SDK.\n\n" +
        //     "Behavior:\n" +
        //     "- Idempotent: If the OrderId matches the existing one, returns 204.\n" +
        //     "- If the transaction already has a different OrderId, returns 409 Conflict.")]
        // public async Task<IActionResult> AttachOrderId(Guid transactionId, [FromBody] AttachOrderRequest request)
        // {
        //     // Map the DTO and TransactionId to your Command
        //     var command = new AttachOrderToTransactionCommand(transactionId, request.OrderId);

        //     var result = await sender.Send(command);

        //     return result.Match(
        //         _ => NoContent(), // 204 No Content is standard for successful updates
        //         Problem
        //     );
        // }
    }
}
