#region

using Exiled.API.Features;
using HarmonyLib;
using ModerationSystem;
using EventHandler = ModerationSystem.EventHandler;
using MapEvent = Exiled.Events.Handlers.Map;
using Server = Exiled.Events.Handlers.Server;

#endregion

#pragma warning disable CS8618

public class Plugin : Plugin<Config>
{
    public static Plugin Singleton;

    private Harmony _harmony;

    public override string Author => "Tiliboyy";
    public override string Name => "ModerationSystem";

    public override string Prefix => "ModerationSystem";
    public override Version Version => new(1, 0, 0);
    public override Version RequiredExiledVersion => new(5, 0, 0, 0);


    public override void OnEnabled()
    {
        try
        {
            _harmony = new Harmony("Tiliboyy.ModerationSystem.Patches");
            _harmony.PatchAll();
            Singleton = this;
            Server.WaitingForPlayers += EventHandler.OnWaitingForPlayers;
            base.OnEnabled();
        }
        catch (Exception error)
        {
            Log.Error(error);
        }
    }


    public override void OnDisabled()
    {
        _harmony?.UnpatchAll();
        Singleton = null!;
        Server.WaitingForPlayers -= EventHandler.OnWaitingForPlayers;
        base.OnDisabled();
    }
}