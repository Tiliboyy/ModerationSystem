#region

using System.ComponentModel;
using Exiled.API.Interfaces;

#endregion

namespace ModerationSystem;

[Serializable]
public class Config : IConfig
{
    public bool IsEnabled { get; set; } = true;
    public bool Debug { get; set; } = false;

    public string Broadcasttext { get; set; } =
        "<size=80%>Du wurdest <color=#fc1505><b>Verwarnt</b></color>\nNutze .gwarns für mehr Informationen\n\n\n</size>";

    public ushort Broadcasttexttime { get; set; } = 60;

    [Description("RoleID to ping")] public List<string> RoleId { get; set; } = new() { "RoleID" };
}