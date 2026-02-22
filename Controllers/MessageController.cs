using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BacklogBasement.DTOs;
using BacklogBasement.Exceptions;
using BacklogBasement.Services;

namespace BacklogBasement.Controllers
{
    [Route("api/messages")]
    [ApiController]
    [Authorize]
    public class MessageController : ControllerBase
    {
        private readonly IMessageService _messageService;
        private readonly IUserService _userService;

        public MessageController(IMessageService messageService, IUserService userService)
        {
            _messageService = messageService;
            _userService = userService;
        }

        [HttpGet]
        public async Task<IActionResult> GetConversations()
        {
            try
            {
                var userId = _userService.GetCurrentUserId();
                if (userId == null)
                    return Unauthorized(new { error = "User not found" });

                var conversations = await _messageService.GetConversationsAsync(userId.Value);
                return Ok(conversations);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "An error occurred while retrieving conversations", details = ex.Message });
            }
        }

        [HttpGet("unread-count")]
        public async Task<IActionResult> GetUnreadCount()
        {
            try
            {
                var userId = _userService.GetCurrentUserId();
                if (userId == null)
                    return Unauthorized(new { error = "User not found" });

                var count = await _messageService.GetUnreadMessageCountAsync(userId.Value);
                return Ok(new { count });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "An error occurred while retrieving unread count", details = ex.Message });
            }
        }

        [HttpGet("{userId:guid}")]
        public async Task<IActionResult> GetMessages(Guid userId)
        {
            try
            {
                var currentUserId = _userService.GetCurrentUserId();
                if (currentUserId == null)
                    return Unauthorized(new { error = "User not found" });

                var messages = await _messageService.GetMessagesAsync(currentUserId.Value, userId);
                return Ok(messages);
            }
            catch (BadRequestException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (NotFoundException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "An error occurred while retrieving messages", details = ex.Message });
            }
        }

        [HttpPost("{userId:guid}")]
        public async Task<IActionResult> SendMessage(Guid userId, [FromBody] SendMessageRequest request)
        {
            try
            {
                var currentUserId = _userService.GetCurrentUserId();
                if (currentUserId == null)
                    return Unauthorized(new { error = "User not found" });

                var message = await _messageService.SendMessageAsync(currentUserId.Value, userId, request);
                return Ok(message);
            }
            catch (BadRequestException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (NotFoundException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "An error occurred while sending message", details = ex.Message });
            }
        }

        [HttpPost("{userId:guid}/read")]
        public async Task<IActionResult> MarkAsRead(Guid userId)
        {
            try
            {
                var currentUserId = _userService.GetCurrentUserId();
                if (currentUserId == null)
                    return Unauthorized(new { error = "User not found" });

                await _messageService.MarkConversationAsReadAsync(currentUserId.Value, userId);
                return Ok(new { message = "Conversation marked as read" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "An error occurred while marking conversation as read", details = ex.Message });
            }
        }
    }
}
