
using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
  public class MessagesController : BaseApiController
  {
    private readonly IMessageRepository _messageRepository;
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;
    public MessagesController(IUserRepository userRepository, IMessageRepository messageRepository, IMapper mapper)
    {
      _mapper = mapper;
      _userRepository = userRepository;
      _messageRepository = messageRepository;
    }

    [HttpPost]
    public async Task<ActionResult<MessageDto>> CreateMessage(CreateMessageDto createMessageDto)
    {
      var username = User.GetUsername();

      if (username == createMessageDto.RecipientUsername.ToLower())
      {
        return BadRequest("You cannot send messages to yourself");
      }

      var sender = await _userRepository.GetUserByUsernameAsync(username);
      var recipient = await _userRepository.GetUserByUsernameAsync(createMessageDto.RecipientUsername);

      var message = new Message
      {
        Sender = sender,
        Recipient = recipient,
        SenderUsername = username,
        RecipientUsername = recipient.UserName,
        Content = createMessageDto.Content
      };

      _messageRepository.AddMessage(message);
      if (await _messageRepository.SavaAllAsync())
      {
        return Ok(_mapper.Map<MessageDto>(message));
      }

      return BadRequest("Failed to send the message");
    }
  }
}