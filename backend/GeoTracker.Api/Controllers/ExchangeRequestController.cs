using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GeoTracker.Api.Models;
using GeoTracker.Api.DTOs.ExchangeRequest;
using GeoTracker.Api.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace GeoTracker.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ExchangeRequestsController : BaseApiController
    {
        private readonly IExchangeRequestRepository _exchangeRepo;

        public ExchangeRequestsController(IExchangeRequestRepository exchangeRepo)
        {
            _exchangeRepo = exchangeRepo;
        }

        // Get all request
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ExchangeResponse>>> GetExchanges()
        {
            var exchanges = await _exchangeRepo.GetAllAsync();
            var response = exchanges.Select(ToExchangeResponse).ToList();

            return Ok(response);
        }

        // Get request by id
        [HttpGet("{id:int}")]
        public async Task<ActionResult<ExchangeResponse>> GetExchangeById(int id)
        {
            var exchange = await _exchangeRepo.GetByIdAsync(id);
            if (exchange == null)
            {
                return NotFound(ErrorResponse(404, "Not Found", $"Exchange request {id} not found."));
            }

            var response = ToExchangeResponse(exchange);
            return Ok(response);
        }

        // Get request by initiator id
        [HttpGet("initiator/{initId:int}")]
        public async Task<ActionResult<IEnumerable<ExchangeResponse>>> GetExchangeByInitId(int initId)
        {
            var exchanges = await _exchangeRepo.GetByInitIdAsync(initId);
            var response = exchanges.Select(ToExchangeResponse).ToList();

            return Ok(response);
        }

        // Get request by receiver id
        [HttpGet("receiver/{receiveId:int}")]
        public async Task<ActionResult<IEnumerable<ExchangeResponse>>> GetExchangeByReceiveId(int receiveId)
        {
            var exchanges = await _exchangeRepo.GetByReceiveIdAsync(receiveId);
            var response = exchanges.Select(ToExchangeResponse).ToList();

            return Ok(response);
        }

        // Get pending requests by receiver id
        [HttpGet("receiver/{receiveId:int}/pending")]
        public async Task<ActionResult<IEnumerable<ExchangeResponse>>> GetPendingExchangeByReceiveId(int receiveId)
        {
            var exchanges = await _exchangeRepo.GetPendingByReceiveIdAsync(receiveId);
            var response = exchanges.Select(ToExchangeResponse).ToList();

            return Ok(response);
        }        

        // Create Exchange
        [Authorize]
        [HttpPost]
        public async Task<ActionResult<ExchangeResponse>> CreateExchange(CreateExchangeRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int currentUserId))
            {
                return Unauthorized(ErrorResponse(401, "Invalid token", "User ID claim is missing or invalid."));
            }

            var newExchange = await _exchangeRepo.CreateAsync(currentUserId, request);
            var response = ToExchangeResponse(newExchange);

            return CreatedAtAction(nameof(GetExchangeById), new { id = newExchange.Id }, response);
        }

        // Accept exchange status (by receiver)
        [Authorize]
        [HttpPost("{receiverId:int}/accept/{requestId:int}")]
        public async Task<ActionResult> AcceptExchange(int receiverId, int requestId)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int currentUserId))
            {
                return Unauthorized(ErrorResponse(401, "Invalid token", "User ID claim is missing or invalid."));
            }
            if (currentUserId != receiverId)
            {                
                return StatusCode(403, ErrorResponse(403, "Forbidden", "No authorization."));
            }

            var exchange = await _exchangeRepo.GetByIdAsync(requestId);
            if (exchange == null) 
            {
                return NotFound(ErrorResponse(404, "Not Found", $"Exchange request {requestId} not found."));
            }
            if (exchange.ReceiverUserId != currentUserId) 
            {                
                return StatusCode(403, ErrorResponse(403, "Forbidden", "No authorization."));
            }
            
            try
            {
                exchange = await _exchangeRepo.AcceptAsync(exchange);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ErrorResponse(409, "Conflict", ex.Message));
            }

            var response = ToExchangeResponse(exchange);

            return Ok(response);
        }

        // Reject exchange status (by receiver)
        [Authorize]
        [HttpPost("{receiverId:int}/reject/{requestId:int}")]
        public async Task<ActionResult> RejectExchange(int receiverId, int requestId)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int currentUserId))
            {
                return Unauthorized(ErrorResponse(401, "Invalid token", "User ID claim is missing or invalid."));
            }
            if (currentUserId != receiverId)
            {
                return StatusCode(403, ErrorResponse(403, "Forbidden", "No authorization."));
            }

            var exchange = await _exchangeRepo.GetByIdAsync(requestId);
            if (exchange == null)
            {
                return NotFound(ErrorResponse(404, "Not Found", $"Exchange request {requestId} not found."));
            }
            if (exchange.ReceiverUserId != currentUserId)
            {
                return StatusCode(403, ErrorResponse(403, "Forbidden", "No authorization."));
            }

            try
            {
                exchange = await _exchangeRepo.RejectAsync(exchange);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ErrorResponse(409, "Conflict", ex.Message));
            }

            var response = ToExchangeResponse(exchange);

            return Ok(response);
        }

        // Cancel exchange status (by initiator)
        [Authorize]
        [HttpPost("{initId:int}/cancel/{requestId:int}")]
        public async Task<ActionResult> CancelExchange(int initId, int requestId)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int currentUserId))
            {
                return Unauthorized(ErrorResponse(401, "Invalid token", "User ID claim is missing or invalid."));
            }
            if (currentUserId != initId)
            {
                return StatusCode(403, ErrorResponse(403, "Forbidden", "No authorization."));
            }

            var exchange = await _exchangeRepo.GetByIdAsync(requestId);
            if (exchange == null)
            {
                return NotFound(ErrorResponse(404, "Not Found", $"Exchange request {requestId} not found."));
            }
            if (exchange.InitiatorUserId != currentUserId)
            {
                return StatusCode(403, ErrorResponse(403, "Forbidden", "No authorization."));
            }

            try
            {
                exchange = await _exchangeRepo.CancelAsync(exchange);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ErrorResponse(409, "Conflict", ex.Message));
            }

            var response = ToExchangeResponse(exchange);

            return Ok(response);
        }

        // Delete exchange 
        // TODO: authorize and only for status = Cancelled
        [HttpDelete("{requestId:int}")]
        public async Task<ActionResult> DeleteExchange(int requestId)
        {
            var exchange = await _exchangeRepo.GetByIdAsync(requestId);
            if (exchange == null)
            {
                return NotFound(ErrorResponse(404, "Not Found", $"Exchange request {requestId} not found."));
            }

            var isDeleted = await _exchangeRepo.DeleteAsync(exchange);
            if (!isDeleted)
            {
                return StatusCode(500, ErrorResponse(500, "Internal Server Error", "Failed to delete request."));
            }

            return NoContent();
        }

        private static ExchangeResponse ToExchangeResponse(ExchangeRequest ex)
        {
            return new ExchangeResponse
            {
                Id = ex.Id,
                InitiatorUserId = ex.InitiatorUserId,
                ReceiverUserId = ex.ReceiverUserId,
                OfferedCollectibleId = ex.OfferedCollectibleId,
                OfferedQuantity = ex.OfferedQuantity,
                Status = ex.Status,
                CreatedAt = ex.CreatedAt,
                ResolvedAt = ex.ResolvedAt
            };
        }   
    }
}