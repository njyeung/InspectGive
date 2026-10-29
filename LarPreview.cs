using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Commands;
using Microsoft.Extensions.Logging;

namespace InspectGive;

/// <summary>
/// Experiment: preview the unreleased "LAR" rifle. Its model, skeleton and anims
/// ship in pak01 (build 26281801) but there's no items_game definition or
/// weapon_lar entity class yet, so it can't be given directly. Instead we give a
/// stock rifle and swap its model. CS2 no longer has a server-side viewmodel
/// entity — the client draws first person from the weapon itself — so the single
/// SetModel is all we can do; whether the first-person view follows is what
/// we're testing.
/// </summary>
public sealed partial class InspectGivePlugin
{
    private const string LarModel = "weapons/models/lar/weapon_rif_lar.vmdl";
    private const string LarDefaultHost = "weapon_ssg08";

    /// <summary>Precache only runs on map load — after a hot-load, change map before
    /// using !lar or SetModel has nothing to point at.</summary>
    private void LoadLarPreview()
    {
        RegisterListener<Listeners.OnServerPrecacheResources>(manifest => manifest.AddResource(LarModel));
        AddCommand("css_lar", "Give a rifle wearing the unreleased LAR model: !lar [weapon_x]", OnLarCommand);
    }

    private void OnLarCommand(CCSPlayerController? player, CommandInfo info)
    {
        if (player is null || !player.IsValid)
            return;

        var host = info.ArgCount > 1 ? info.GetArg(1).Trim().ToLowerInvariant() : LarDefaultHost;
        if (!host.StartsWith("weapon_"))
        {
            player.PrintToChat($" \x02[LAR]\x01 Usage: !lar [weapon_x] (default {LarDefaultHost})");
            return;
        }

        var pawn = player.PlayerPawn.Value;
        if (pawn is null || !pawn.IsValid
            || pawn.LifeState != (byte)LifeState_t.LIFE_ALIVE
            || pawn.ItemServices is null)
        {
            player.PrintToChat(" \x02[LAR]\x01 You need to be alive.");
            return;
        }

        var weapon = new CCSPlayer_ItemServices(pawn.ItemServices.Handle).GiveNamedItem<CBasePlayerWeapon>(host);
        if (weapon is null || !weapon.IsValid)
        {
            player.PrintToChat($" \x02[LAR]\x01 Couldn't give {host}.");
            return;
        }

        // The give's own model setup runs this frame; swap on the next one so ours wins.
        var steamId = player.SteamID;
        Server.NextFrame(() =>
        {
            if (!weapon.IsValid)
                return;
            weapon.SetModel(LarModel);
            Logger.LogInformation("LAR model applied to {host} for {steamId}", host, steamId);
            if (player.IsValid)
                player.PrintToChat($" \x04[LAR]\x01 Gave {host} with the LAR model.");
        });
    }
}
