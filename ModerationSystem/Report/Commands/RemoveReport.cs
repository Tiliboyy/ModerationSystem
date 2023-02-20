#region

using CommandSystem;
using ModerationSystem;

#endregion

[CommandHandler(typeof(RemoteAdminCommandHandler))]
internal class RemoveReport : ICommand
{
    public string Command { get; } = "RemoveReport";

    public string[] Aliases { get; } = { "rreport, delreport" };

    public string Description { get; } = "Removes reports";

    public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
    {
        if (arguments.Count != 1)
        {
            response = "Usage: RemoveReport <ReportID>";
            return true;
        }

        if (int.TryParse(arguments.At(0), out var id))
        {
            response = "Usage: RemoveReport <ReportID>";
            return true;
        }

        if (ReportDatabase.DeleteReport(id))
        {
            response = "Report wurde entfernt";
            return true;
        }

        response = "Report wurden nicht gefunden";
        return true;
    }
}