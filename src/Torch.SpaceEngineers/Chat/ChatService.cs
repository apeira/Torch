using NLog;
using Sandbox.Engine.Multiplayer;
using Sandbox.Game.Multiplayer;
using Torch.Core;
using Torch.Core.Commands;
using Torch.SpaceEngineers.Patches;
using Torch.SpaceEngineers.Players;
using VRage.Game;
using VRage.Network;
using VRageMath;

namespace Torch.SpaceEngineers.Chat
{
    public delegate void ChatMessageReceivedDel(ref ChatMessageEvent data);
    
    public class ChatService
    {
        private const string SERVER_NAME = "Server";
        private static readonly Color DefaultColor = Color.White;

        private static readonly Logger Log = LogManager.GetCurrentClassLogger();
        private static readonly Logger ChatLog = LogManager.GetLogger("Chat");
        private ICommandService _commands;

        public event ChatMessageReceivedDel ChatMessageReceived;

        public char CommandPrefix { get; } = '!';

        private ChatService(ICommandService commands = null)
        {
            // Hook into chat system
            ChatInterceptPatch.ChatMessageReceived += OnChatMessage;

            _commands = commands;
        }

        public void SendMessage(string message)
            => SendMessageTo(SERVER_NAME, message, DefaultColor, 0);

        public void SendMessage(string message, Color color)
            => SendMessageTo(SERVER_NAME, message, color, 0);

        public void SendMessage(string authorName, string message, Color color)
            => SendMessageTo(authorName, message, color, 0);

        public void SendMessageTo(string message, ulong targetSteamId)
            => SendMessageTo(SERVER_NAME, message, DefaultColor, targetSteamId);

        public void SendMessageTo(string message, Color color, ulong targetSteamId)
            => SendMessageTo(SERVER_NAME, message, color, targetSteamId);

        public void SendMessageTo(string authorName, string message, Color color, ulong targetSteamId)
        {
            var msg = new ScriptedChatMsg
            {
                Author = authorName,
                Text = message,
                Font = MyFontEnum.White,
                Color = color,
                Target = Sync.Players.TryGetIdentityId(targetSteamId),
            };
            MyMultiplayerBase.SendScriptedChatMessage(ref msg);
        }

        private void OnChatMessage(ref ChatMsg message, ref bool cancel)
        {
            // TODO player manager to cache these objects
            var player = new Player {Name = message.CustomAuthorName, SteamId = message.Author};

            var text = message.Text;
            if (_commands.IsCommand(text))
            {
                _commands.Execute(player, text.TrimStart(CommandPrefix), s => SendMessageTo(s, player.SteamId));
                cancel = true;
            }
            else
            {
                var e = new ChatMessageEvent(message.Author, message.CustomAuthorName, text);
                ChatMessageReceived?.Invoke(ref e);
                cancel |= e.IsCancelled;

                if (!cancel)
                    ChatLog.Info($"{e.SenderName}: {e.Content}");
            }
        }
    }
}
