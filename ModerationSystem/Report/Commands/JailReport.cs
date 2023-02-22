#region

using CommandSystem;
using Exiled.API.Features;
using Exiled.Permissions.Extensions;
using MEC;

#endregion
/*
namespace ModerationSystem;

[CommandHandler(typeof(RemoteAdminCommandHandler))]
internal class JailReport : ICommand
{
    public string Command { get; } = "jailreport";

    public string[] Aliases { get; } = Array.Empty<string>();

    public string Description { get; } = "Jails all players from a report";

    public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
    {
        if (sender.CheckPermission("rmd.jailreport"))
        {
            response = "You do not have permission to use this command";
            return false;
        }
        if (arguments.Count != 1)
        {
            response = "Usage: JailReport <Reportid>";
            return true;
        }

        if (!int.TryParse(arguments.At(0), out int i))
        {
            response = arguments.At(0) + " is not a Number";
            return true;
        }

        var report = ReportDatabase.GetReport(i);
        if (report.Id == -1)
        {
            response = "Report not Found";
        }

        var admin = Player.Get(sender);
        var reporter = Player.Get(report.Reporter);
        var reported = Player.Get(report.Reperted);
        if (admin == null)
        {
            response = "You can only run this as a player";
            return true;
        }

        if (reporter == null)
        {
            response = "Der Reporter wurde nicht auf dem Server gefunden";
            return true;
        }

        if (reported == null)
        {
            response = "Die Reportete Person wurde nicht auf dem Server gefunden";
            return true;
        }


        if (admin.RawUserId == reporter.RawUserId || admin.RawUserId == reported.RawUserId)
        {
            Timing.RunCoroutine(Jail.JailPlayer(reporter));
            Timing.RunCoroutine(Jail.JailPlayer(reported));
            response = $"{reporter.Nickname} und {reported.Nickname} wurden Jailed";
        }
        else
        {
            Timing.RunCoroutine(Jail.JailPlayer(admin));
            Timing.RunCoroutine(Jail.JailPlayer(reporter));
            Timing.RunCoroutine(Jail.JailPlayer(reported));
            response = $"{reporter.Nickname}, {reported.Nickname} und {admin.Nickname} wurden Jailed";
        }


        return true;
    }
*/