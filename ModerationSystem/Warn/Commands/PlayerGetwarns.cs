#nullable enable

#region

using CommandSystem;
using Exiled.API.Features;
using GameStore;

#endregion

namespace ModerationSystem
{
    [CommandHandler(typeof(ClientCommandHandler))]
    public class Getwarnsplayer : ICommand
    {
        public string Command { get; } = "getwarns";

        public string[] Aliases { get; } = new[]
        {
            "gwarns", "showwarns"
        };

        public string Description { get; } = "Usage: .getwarns";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            var player = Player.Get(sender);
            if (player == null)
            {
                response = "ERROR";
                return true;
            }

            string str = WarnDatabase.Database.GetWarns(player.UserId, true, false);
            response = str;
            return true;
        }
    }
}