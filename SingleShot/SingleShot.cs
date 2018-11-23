using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CitizenFX.Core;
using static CitizenFX.Core.Native.API;

namespace SingleShot
{
    public class SingleShot : BaseScript
    {
        #region General variables
        private bool weaponSafety = false;
        private int firemode = 2;
        List<string> automaticWeapons = new List<string>{
            "WEAPON_COMBATPISTOL",
            "WEAPON_STUNGUN",
            "WEAPON_PISTOL",
            "WEAPON_APPISTOL",
            "WEAPON_PISTOL50",
            "WEAPON_MICROSMG",
            "WEAPON_ASSAULTSMG",
            "WEAPON_SMG",
            "WEAPON_ASSAULTRIFLE",
            "WEAPON_CARBINERIFLE",
            "WEAPON_ADVANCEDRIFLE",
            "WEAPON_MG",
            "WEAPON_COMBATMG",
            "WEAPON_PUMPSHOTGUN",
            "WEAPON_SAWNOFFSHOTGUN",
            "WEAPON_ASSAULTSHOTGUN",
            "WEAPON_BULLPUPSHOTGUN",
            "WEAPON_SNIPERRIFLE",
            "WEAPON_REMOTESNIPER",
            "WEAPON_SNSPISTOL",
            "WEAPON_BULLPUPRIFLE",
            "WEAPON_GUSENBERG",
            "WEAPON_SPECIALCARBINE",
            "WEAPON_HEAVYPISTOL",
            "WEAPON_VINTAGEPISTOL",
            "WEAPON_HEAVYSHOTGUN",
            "WEAPON_MARKSMANRIFLE",
            "WEAPON_COMBATPDW",
            "WEAPON_MACHINEPISTOL",
            "WEAPON_REVOLVER",
            "WEAPON_COMPACTRIFLE",
            "WEAPON_DBSHOTGUN",
            "WEAPON_MINISMG",
            "WEAPON_AUTOSHOTGUN",
            "WEAPON_CARBINERIFLE_MK2",
            "WEAPON_ASSAULTRIFLE_MK2",
            "WEAPON_SPECIALCARBINE_MK2",
            "WEAPON_SMG_MK2",
            "WEAPON_BULLPUPRIFLE_MK2",
            "WEAPON_COMBATMG_MK2",
            "WEAPON_MARKSMANRIFLE_MK2",
            "WEAPON_PUMPSHOTGUN_MK2",
            "WEAPON_BULLPUPRIFLE_MK2",
            "WEAPON_PISTOL_MK2",
        };
        #endregion


        /// <summary>
        /// Constructor.
        /// </summary>
        public SingleShot()
        {
            Tick += OnTick;
            Tick += ShowCurrentMode;
        }

        /// <summary>
        /// OnTick async Task.
        /// </summary>
        /// <returns></returns>
        private async Task OnTick()
        {
            // Load the texture dictionary.
            if (!HasStreamedTextureDictLoaded("mpweaponsgang0"))
            {
                RequestStreamedTextureDict("mpweaponsgang0", true);
                while (!HasStreamedTextureDictLoaded("mpweaponsgang0"))
                {
                    await Delay(0);
                }
            }

            // Only run the rest of the code when the player is holding an automatic weapon.
            if (IsAutomaticWeapon(GetSelectedPedWeapon(PlayerPedId())))
            {

                // If the weapon safety feature is turned on, disable the weapon from firing.
                if (weaponSafety)
                {
                    // Disable shooting.
                    DisablePlayerFiring(PlayerId(), true);

                    // If the user tries to shoot while the safety is enabled, notify them.
                    if (IsDisabledControlJustPressed(0, 24))
                    {
                        CitizenFX.Core.UI.Screen.ShowNotification("~r~Weapon safety mode is enabled!~n~~w~Press ~y~K ~w~to switch it off.", true);
                    }
                }

                // If the player pressed K (311/Rockstar Editor Keyframe Help display button) ON KEYBOARD ONLY(!) then toggle the safety mode.
                if (IsInputDisabled(2) && IsControlJustPressed(0, 311))
                {
                    weaponSafety = !weaponSafety;
                    CitizenFX.Core.UI.Screen.ShowSubtitle("~y~Weapon safety mode ~g~" + (weaponSafety ? "~g~enabled" : "~r~disabled") + "~y~.", 3000);
                }

                // (2) Single shot firing mode
                else if (firemode == 2)
                {
                    // If the player starts shooting...
                    if (IsControlJustPressed(0, 24))
                    {
                        // ...disable the weapon after the first shot and keep it disabled as long as the trigger is being pulled.
                        // once the player lets go of the trigger, the loop will stop and they can pull it again.
                        while (IsControlPressed(0, 24) || IsDisabledControlPressed(0, 24))
                        {
                            DisablePlayerFiring(PlayerId(), true);

                            // Because we're now in a while loop, we need to add a delay to prevent the game from freezing up/crashing.
                            await Delay(0);
                        }
                    }
                }
                // We don't need to have a function that handles firing mode 0, since that's full auto mode and that's enabled by default anyway.
            }
        }

        /// <summary>
        /// Checks if the given weapon hash (int) is present in the weapons list defined at the top of this file.
        /// This also returns false regardless of the weapon the player has equipped, if the player is in a vehicle.
        /// Meaning the fire mode in a vehicle will always be full auto for all weapons.
        /// </summary>
        /// <param name="weaponHash"></param>
        /// <returns>true/false (bool), true if the weapon is found in the list, false if it's not.</returns>
        private bool IsAutomaticWeapon(int weaponHash)
        {
            if (IsPedInAnyVehicle(PlayerPedId(), false))
            {
                return false;
            }
            else
            {
                foreach (string weapon in automaticWeapons)
                {
                    if (GetHashKey(weapon) == weaponHash)
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        /// <summary>
        /// Used to draw text ont the screen on the specified x,y
        /// </summary>
        /// <param name="text"></param>
        /// <param name="posx"></param>
        /// <param name="posy"></param>
        private void ShowText(string text, float posx, float posy)
        {
            SetTextFont(4);
            SetTextScale(0.0f, 0.31f);
            SetTextJustification(1);
            SetTextColour(250, 250, 120, 255);
            SetTextDropshadow(1, 255, 255, 255, 255);
            SetTextEdge(1, 0, 0, 0, 205);
            BeginTextCommandDisplayText("STRING");
            AddTextComponentSubstringPlayerName(text);
            EndTextCommandDisplayText(posx, posy);
        }

        /// <summary>
        /// Show the current firing mode visually just below the ammo count.
        /// Called every frame.
        /// </summary>
        private async Task ShowCurrentMode()
        {
            // Just add a wait in here when it's not being displayed, to remove the async warnings. 
            if (!IsAutomaticWeapon(GetSelectedPedWeapon(PlayerPedId())))
            {
                await Delay(0);
            }
            // If the weapon is a valid weapon that has different firing modes, then this will be shown.
            else
            {
                if (weaponSafety)
                {
                    ShowText(" ~r~X", 0.975f, 0.065f);
                }
                else
                {
                    switch (firemode)
                    {
                        case 1:
                            ShowText("|", 0.975f, 0.065f);
                            break;
                        case 2:
                            ShowText("|", 0.975f, 0.065f);
                            break;
                        case 0:
                        default:
                            ShowText("|", 0.975f, 0.065f);
                            break;
                    }
                }
                DrawSprite("mpweaponsgang0", "w_ar_carbinerifle_mag1", 0.975f, 0.06f, 0.099f, 0.099f, 0.0f, 200, 200, 200, 255);
            }
        }
    }
}
