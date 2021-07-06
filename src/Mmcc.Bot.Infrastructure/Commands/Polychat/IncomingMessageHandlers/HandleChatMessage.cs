﻿using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Mmcc.Bot.Core;
using Mmcc.Bot.Core.Models.Settings;
using Mmcc.Bot.Protos;
using Mmcc.Bot.Infrastructure.Services;
using Remora.Discord.API.Abstractions.Rest;

namespace Mmcc.Bot.Infrastructure.Commands.Polychat.IncomingMessageHandlers
{
    public class HandleChatMessage
    {
        public class Handler : AsyncRequestHandler<TcpRequest<ChatMessage>>
        {
            private readonly IPolychatService _polychatService;
            private readonly IDiscordRestChannelAPI _channelApi;
            private readonly PolychatSettings _polychatSettings;

            public Handler(IPolychatService polychatService, IDiscordRestChannelAPI channelApi, PolychatSettings polychatSettings)
            {
                _polychatService = polychatService;
                _channelApi = channelApi;
                _polychatSettings = polychatSettings;
            }
            
            protected override async Task Handle(TcpRequest<ChatMessage> request, CancellationToken cancellationToken)
            {
                var id = new PolychatServerIdString(request.Message.ServerId);
                var sanitisedId = id.ToSanitisedUppercase();
                await _polychatService.ForwardMessage(sanitisedId, request.Message);
                
                var getChatChannelResult =
                    await _channelApi.GetChannelAsync(new(_polychatSettings.ChatChannelId), cancellationToken);
                
                if (!getChatChannelResult.IsSuccess)
                {
                    throw new Exception(getChatChannelResult.Error.Message);
                }

                var messageStr = new PolychatChatMessageString(request.Message.Message);
                var sendMessageResult = await _channelApi.CreateMessageAsync(
                        new(_polychatSettings.ChatChannelId),
                        messageStr.ToDiscordFormattedString(),
                        ct: cancellationToken);
                    
                if (!sendMessageResult.IsSuccess)
                {
                    throw new Exception(getChatChannelResult.Error?.Message ?? "Could not forward message to Discord.");
                }
            }
        }
    }
}