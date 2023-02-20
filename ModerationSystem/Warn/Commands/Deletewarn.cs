#region

using CommandSystem;
using Exiled.Permissions.Extensions;
using GameStore;

#endregion

namespace ModerationSystem
{
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    public class delwarn : ICommand
    {
        public string Command { get; } = "removewarn";
        public string[] Aliases { get; } = new string[] { "rwarn" };
        public string Description { get; } = "Usage: removewarn <Steam64ID> <WARN ID>)";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (!sender.CheckPermission("ws.delete"))
            {
                response = "You do not have permission to use this command";
                return false;
            }

            if (arguments.Count != 2)
            {
                response = "Usage: removewarn <Steam64ID> <WARN ID>";
                return true;
            }

            if (int.TryParse(arguments.At(1), out var id))
            {
                string e = WarnDatabase.Database.RemoveWarn(arguments.At(0), id);
                response = e;
                return true;
            }

            response = "ERROR: ID ist keine Zahl";
            return true;
        }
    }
}