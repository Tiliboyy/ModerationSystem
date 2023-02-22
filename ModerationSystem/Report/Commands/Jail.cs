#region

using CustomPlayerEffects;
using Exiled.API.Enums;
using Exiled.API.Extensions;
using Exiled.API.Features;
using Exiled.API.Features.Items;
using Exiled.API.Features.Roles;
using MEC;
using PlayerRoles;
using UnityEngine;
using Log = PluginAPI.Core.Log;

#endregion

namespace ModerationSystem;

public class Jail
{
    public static Dictionary<Player?, JailedPlayer> JailedPlayers = new();

    public static IEnumerator<float> JailPlayer(Player? player)
    {
        
        if (player == null) yield break;
        if (JailedPlayers.TryGetValue(player, out var jailedPlayer))
        {
            player.Role.Set(jailedPlayer.Role, SpawnReason.ForceClass, RoleSpawnFlags.None);
            yield return Timing.WaitForSeconds(0.1f);
            try
            {
                JailedPlayers.Remove(player);
                player.ResetInventory(jailedPlayer.Inventory);
                player.Position = jailedPlayer.Position;
                player.Health = jailedPlayer.HP;
                foreach (var kvp in jailedPlayer.Ammo)
                    player.Ammo[kvp.Key.GetItemType()] = kvp.Value;
            }
            catch (Exception e)
            {
                Log.Error($"{nameof(jailedPlayer)}: {e}");
            }

            foreach (var effect in jailedPlayer.Effects)
            {
                player.EnableEffect(effect);
            }

            yield return 1;
        }
        else
        {
            var ammo = player.Ammo.ToDictionary(kvp => kvp.Key.GetAmmoType(), kvp => kvp.Value);
            List<Item> items = player.Items.ToList();
            JailedPlayers.Add(player, new JailedPlayer()
            {
                Effects = player.ActiveEffects.ToList(),
                Position = player.Position,
                HP = player.Health,
                Inventory = items,
                Role = player.Role,
                Ammo = ammo,
            });
            yield return Timing.WaitForSeconds(0.1f);
            player.ClearInventory(false);
            player.Role.Set(RoleTypeId.Tutorial, SpawnReason.ForceClass, RoleSpawnFlags.UseSpawnpoint);
            yield return 1;
        }
    }

    public struct JailedPlayer
    {
        public List<Item> Inventory { get; set; }
        public Vector3 Position { get; set; }
        public List<StatusEffectBase> Effects { get; set; }
        public float HP { get; set; }
        public Role Role { get; set; }

        public Dictionary<AmmoType, ushort> Ammo { get; set; }
    }
}