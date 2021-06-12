using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Harmony;
using Microsoft.Xna.Framework;
using Netcode;
using Spacechase.Shared.Harmony;
using SpaceShared;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.Objects;

namespace BuildableLocationsFramework.Patches
{
    /// <summary>Applies Harmony patches to <see cref="CarpenterMenu"/>.</summary>
    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "The naming is determined by Harmony.")]
    internal class CarpenterMenuPatcher : BasePatcher
    {
        /*********
        ** Public methods
        *********/
        /// <inheritdoc />
        public override void Apply(HarmonyInstance harmony, IMonitor monitor)
        {
            harmony.Patch(
                original: this.RequireMethod<CarpenterMenu>(nameof(CarpenterMenu.performHoverAction)),
                transpiler: this.GetHarmonyMethod(nameof(Transpile_PerformHoverAction))
            );

            harmony.Patch(
                original: this.RequireMethod<CarpenterMenu>(nameof(CarpenterMenu.tryToBuild)),
                transpiler: this.GetHarmonyMethod(nameof(Transpile_TryToBuild))
            );

            harmony.Patch(
                original: this.RequireMethod<CarpenterMenu>(nameof(CarpenterMenu.receiveLeftClick)),
                prefix: this.GetHarmonyMethod(nameof(Before_ReceiveLeftClick))
            );
        }


        /*********
        ** Private methods
        *********/
        /// <summary>The method which transpiles <see cref="CarpenterMenu.performHoverAction"/>.</summary>
        private static IEnumerable<CodeInstruction> Transpile_PerformHoverAction(ILGenerator gen, MethodBase original, IEnumerable<CodeInstruction> insns)
        {
            return CarpenterMenuPatcher.Transpile(gen, original, insns);
        }

        /// <summary>The method which transpiles <see cref="CarpenterMenu.tryToBuild"/>.</summary>
        private static IEnumerable<CodeInstruction> Transpile_TryToBuild(ILGenerator gen, MethodBase original, IEnumerable<CodeInstruction> insns)
        {
            return CarpenterMenuPatcher.Transpile(gen, original, insns);
        }

        /// <summary>The method to call before <see cref="CarpenterMenu.receiveLeftClick"/>.</summary>
        public static bool Before_ReceiveLeftClick(CarpenterMenu __instance, int x, int y, bool playSound)
        {
            //
            // Transpiling is difficult because of the lambda and some internal compiler nonsense
            //

            try
            {
                var __instance_freeze = Mod.instance.Helper.Reflection.GetField<bool>(__instance, "freeze");
                var __instance_onFarm = Mod.instance.Helper.Reflection.GetField<bool>(__instance, "onFarm");
                var __instance_moving = Mod.instance.Helper.Reflection.GetField<bool>(__instance, "moving");
                var __instance_upgrading = Mod.instance.Helper.Reflection.GetField<bool>(__instance, "upgrading");
                var __instance_demolishing = Mod.instance.Helper.Reflection.GetField<bool>(__instance, "demolishing");
                var __instance_buildingToMove = Mod.instance.Helper.Reflection.GetField<Building>(__instance, "buildingToMove");
                var __instance_currentBlueprintIndex = Mod.instance.Helper.Reflection.GetField<int>(__instance, "currentBlueprintIndex");
                var __instance_price = Mod.instance.Helper.Reflection.GetField<int>(__instance, "price");
                var __instance_blueprints = Mod.instance.Helper.Reflection.GetField<List<BluePrint>>(__instance, "blueprints");

                if (__instance_freeze.GetValue())
                    goto ret;
                if (!__instance_onFarm.GetValue())
                    CarpenterMenuPatcher.base_receiveLeftClick(__instance, x, y, playSound);
                if (__instance.cancelButton.containsPoint(x, y))
                {
                    if (!__instance_onFarm.GetValue())
                    {
                        __instance.exitThisMenu(true);
                        Game1.player.forceCanMove();
                        Game1.playSound("bigDeSelect");
                    }
                    else
                    {
                        if (__instance_moving.GetValue() && __instance_buildingToMove.GetValue() != null)
                        {
                            Game1.playSound("cancel");
                            goto ret;
                        }
                        __instance.returnToCarpentryMenu();
                        Game1.playSound("smallSelect");
                        goto ret;
                    }
                }
                if (!__instance_onFarm.GetValue() && __instance.backButton.containsPoint(x, y))
                {
                    __instance_currentBlueprintIndex.SetValue(__instance_currentBlueprintIndex.GetValue() - 1);
                    if (__instance_currentBlueprintIndex.GetValue() < 0)
                        __instance_currentBlueprintIndex.SetValue(__instance_blueprints.GetValue().Count - 1);
                    __instance.setNewActiveBlueprint();
                    Game1.playSound("shwip");
                    __instance.backButton.scale = __instance.backButton.baseScale;
                }
                if (!__instance_onFarm.GetValue() && __instance.forwardButton.containsPoint(x, y))
                {
                    __instance_currentBlueprintIndex.SetValue((__instance_currentBlueprintIndex.GetValue() + 1) % __instance_blueprints.GetValue().Count);
                    __instance.setNewActiveBlueprint();
                    __instance.backButton.scale = __instance.backButton.baseScale;
                    Game1.playSound("shwip");
                }
                if (!__instance_onFarm.GetValue() && __instance.demolishButton.containsPoint(x, y) && __instance.demolishButton.visible)
                {
                    Game1.globalFadeToBlack(new Game1.afterFadeFunction(__instance.setUpForBuildingPlacement), 0.02f);
                    Game1.playSound("smallSelect");
                    __instance_onFarm.SetValue(true);
                    __instance_demolishing.SetValue(true);
                }
                if (!__instance_onFarm.GetValue() && __instance.moveButton.containsPoint(x, y) && __instance.moveButton.visible)
                {
                    Game1.globalFadeToBlack(new Game1.afterFadeFunction(__instance.setUpForBuildingPlacement), 0.02f);
                    Game1.playSound("smallSelect");
                    __instance_onFarm.SetValue(true);
                    __instance_moving.SetValue(true);
                }
                if (__instance.okButton.containsPoint(x, y) && !__instance_onFarm.GetValue() && (Game1.player.Money >= __instance_price.GetValue() && __instance_blueprints.GetValue()[__instance_currentBlueprintIndex.GetValue()].doesFarmerHaveEnoughResourcesToBuild()))
                {
                    Game1.globalFadeToBlack(new Game1.afterFadeFunction(__instance.setUpForBuildingPlacement), 0.02f);
                    Game1.playSound("smallSelect");
                    __instance_onFarm.SetValue(true);
                }
                if (!__instance_onFarm.GetValue() || __instance_freeze.GetValue() || Game1.globalFade)
                    goto ret;
                if (__instance_demolishing.GetValue())
                {
                    // MINE - Farm -> BuildableGameLocation
                    BuildableGameLocation farm = CarpenterMenuPatcher.ReturnCurrentLocationAnyways("Farm") as BuildableGameLocation;
                    Building destroyed = farm.getBuildingAt(new Vector2((Game1.viewport.X + Game1.getOldMouseX()) / 64, (Game1.viewport.Y + Game1.getOldMouseY()) / 64));
                    Action buildingLockFailed = () =>
                    {
                        if (!__instance_demolishing.GetValue())
                            return;
                        Game1.addHUDMessage(new HUDMessage(Game1.content.LoadString("Strings\\UI:Carpenter_CantDemolish_LockFailed"), Color.Red, 3500f));
                    };
                    Action continueDemolish = () =>
                    {
                        if (!__instance_demolishing.GetValue() || destroyed == null || !farm.buildings.Contains(destroyed))
                            return;
                        if ((int)destroyed.daysOfConstructionLeft > 0 || (int)destroyed.daysUntilUpgrade > 0)
                            Game1.addHUDMessage(new HUDMessage(Game1.content.LoadString("Strings\\UI:Carpenter_CantDemolish_DuringConstruction"), Color.Red, 3500f));
                        else if (destroyed.indoors.Value != null && destroyed.indoors.Value is AnimalHouse && (destroyed.indoors.Value as AnimalHouse).animalsThatLiveHere.Count > 0)
                            Game1.addHUDMessage(new HUDMessage(Game1.content.LoadString("Strings\\UI:Carpenter_CantDemolish_AnimalsHere"), Color.Red, 3500f));
                        else if (destroyed.indoors.Value != null && destroyed.indoors.Value.farmers.Count<Farmer>() > 0)
                        {
                            Game1.addHUDMessage(new HUDMessage(Game1.content.LoadString("Strings\\UI:Carpenter_CantDemolish_PlayerHere"), Color.Red, 3500f));
                        }
                        else
                        {
                            if (destroyed.indoors.Value != null && destroyed.indoors.Value is Cabin)
                            {
                                foreach (Character allFarmer in Game1.getAllFarmers())
                                {
                                    if (allFarmer.currentLocation.Name == (destroyed.indoors.Value as Cabin).GetCellarName())
                                    {
                                        Game1.addHUDMessage(new HUDMessage(Game1.content.LoadString("Strings\\UI:Carpenter_CantDemolish_PlayerHere"), Color.Red, 3500f));
                                        return;
                                    }
                                }
                            }
                            if (destroyed.indoors.Value is Cabin && (destroyed.indoors.Value as Cabin).farmhand.Value.isActive())
                            {
                                Game1.addHUDMessage(new HUDMessage(Game1.content.LoadString("Strings\\UI:Carpenter_CantDemolish_FarmhandOnline"), Color.Red, 3500f));
                            }
                            else
                            {
                                Chest chest = null;
                                if (destroyed.indoors.Value is Cabin)
                                {
                                    List<Item> objList = (destroyed.indoors.Value as Cabin).demolish();
                                    if (objList.Count > 0)
                                    {
                                        chest = new Chest(true);
                                        chest.fixLidFrame();
                                        chest.items.Set(objList);
                                    }
                                }
                                if (!farm.destroyStructure(destroyed))
                                    return;
                                int tileY = (int)destroyed.tileY;
                                int tilesHigh = (int)destroyed.tilesHigh;
                                Game1.flashAlpha = 1f;
                                destroyed.showDestroyedAnimation(Game1.getFarm());
                                Game1.playSound("explosion");
                                // function does nothing
                                //Utility.spreadAnimalsAround(destroyed, farm);
                                DelayedAction.functionAfterDelay(new DelayedAction.delayedBehavior(__instance.returnToCarpentryMenu), 1500);
                                __instance_freeze.SetValue(true);
                                if (chest == null)
                                    return;
                                farm.objects[new Vector2((int)destroyed.tileX + (int)destroyed.tilesWide / 2, (int)destroyed.tileY + (int)destroyed.tilesHigh / 2)] = chest;
                            }
                        }
                    };
                    if (destroyed != null)
                    {
                        if (destroyed.indoors.Value != null && destroyed.indoors.Value is Cabin && !Game1.IsMasterGame)
                        {
                            Game1.addHUDMessage(new HUDMessage(Game1.content.LoadString("Strings\\UI:Carpenter_CantDemolish_LockFailed"), Color.Red, 3500f));
                            destroyed = null;
                            goto ret;
                        }
                        if (!Game1.IsMasterGame && !__instance.hasPermissionsToDemolish(destroyed))
                        {
                            destroyed = null;
                            goto ret;
                        }
                    }
                    if (destroyed != null && destroyed.indoors.Value is Cabin)
                    {
                        Cabin cabin = destroyed.indoors.Value as Cabin;
                        if (cabin.farmhand.Value != null && (bool)cabin.farmhand.Value.isCustomized)
                        {
                            Game1.currentLocation.createQuestionDialogue(Game1.content.LoadString("Strings\\UI:Carpenter_DemolishCabinConfirm", cabin.farmhand.Value.Name), Game1.currentLocation.createYesNoResponses(), (f, answer) =>
                            {
                                if (answer == "Yes")
                                {
                                    Game1.activeClickableMenu = __instance;
                                    Game1.player.team.demolishLock.RequestLock(continueDemolish, buildingLockFailed);
                                }
                                else
                                    DelayedAction.functionAfterDelay(new DelayedAction.delayedBehavior(__instance.returnToCarpentryMenu), 500);
                            }, null);
                            goto ret;
                        }
                    }
                    if (destroyed == null)
                        goto ret;
                    Game1.player.team.demolishLock.RequestLock(continueDemolish, buildingLockFailed);
                }
                else if (__instance_upgrading.GetValue())
                {
                    Building buildingAt = ((BuildableGameLocation)CarpenterMenuPatcher.ReturnCurrentLocationAnyways("Farm")).getBuildingAt(new Vector2((Game1.viewport.X + Game1.getOldMouseX()) / 64, (Game1.viewport.Y + Game1.getOldMouseY()) / 64));
                    if (buildingAt != null && __instance.CurrentBlueprint.name != null && buildingAt.buildingType.Equals(__instance.CurrentBlueprint.nameOfBuildingToUpgrade))
                    {
                        __instance.CurrentBlueprint.consumeResources();
                        buildingAt.daysUntilUpgrade.Value = 2;
                        buildingAt.showUpgradeAnimation(Game1.getFarm());
                        Game1.playSound("axe");
                        DelayedAction.functionAfterDelay(new DelayedAction.delayedBehavior(__instance.returnToCarpentryMenuAfterSuccessfulBuild), 1500);
                        __instance_freeze.SetValue(true);
                    }
                    else
                    {
                        if (buildingAt == null)
                            goto ret;
                        Game1.addHUDMessage(new HUDMessage(Game1.content.LoadString("Strings\\UI:Carpenter_CantUpgrade_BuildingType"), Color.Red, 3500f));
                    }
                }
                else if (__instance_moving.GetValue())
                {
                    if (__instance_buildingToMove.GetValue() == null)
                    {
                        __instance_buildingToMove.SetValue(((BuildableGameLocation)CarpenterMenuPatcher.ReturnCurrentLocationAnyways("Farm")).getBuildingAt(new Vector2((Game1.viewport.X + Game1.getMouseX()) / 64, (Game1.viewport.Y + Game1.getMouseY()) / 64)));
                        if (__instance_buildingToMove.GetValue() == null)
                            goto ret;
                        if ((int)__instance_buildingToMove.GetValue().daysOfConstructionLeft > 0)
                            __instance_buildingToMove.SetValue(null);
                        else if (!Game1.IsMasterGame && !__instance.hasPermissionsToMove(__instance_buildingToMove.GetValue()))
                        {
                            __instance_buildingToMove.SetValue(null);
                        }
                        else
                        {
                            __instance_buildingToMove.GetValue().isMoving = true;
                            Game1.playSound("axchop");
                        }
                    }
                    else if (((BuildableGameLocation)CarpenterMenuPatcher.ReturnCurrentLocationAnyways("Farm")).buildStructure(__instance_buildingToMove.GetValue(), new Vector2((Game1.viewport.X + Game1.getMouseX()) / 64, (Game1.viewport.Y + Game1.getMouseY()) / 64), Game1.player, false))
                    {
                        __instance_buildingToMove.GetValue().isMoving = false;
                        if (__instance_buildingToMove.GetValue() is ShippingBin)
                            (__instance_buildingToMove.GetValue() as ShippingBin).initLid();
                        __instance_buildingToMove.GetValue().performActionOnBuildingPlacement();
                        __instance_buildingToMove.SetValue(null);
                        Game1.playSound("axchop");
                        DelayedAction.playSoundAfterDelay("dirtyHit", 50, null, -1);
                        DelayedAction.playSoundAfterDelay("dirtyHit", 150, null, -1);
                    }
                    else
                        Game1.playSound("cancel");
                }
                else
                    Game1.player.team.buildLock.RequestLock(() =>
                    {
                        if (__instance_onFarm.GetValue() && Game1.locationRequest == null)
                        {
                            if (__instance.tryToBuild())
                            {
                                __instance.CurrentBlueprint.consumeResources();
                                DelayedAction.functionAfterDelay(new DelayedAction.delayedBehavior(__instance.returnToCarpentryMenuAfterSuccessfulBuild), 2000);
                                __instance_freeze.SetValue(true);
                            }
                            else
                                Game1.addHUDMessage(new HUDMessage(Game1.content.LoadString("Strings\\UI:Carpenter_CantBuild"), Color.Red, 3500f));
                        }
                        Game1.player.team.buildLock.ReleaseLock();
                    }, null);
                ret:
                return false;
            }
            catch (Exception e)
            {
                return true;
            }
        }

        private static void base_receiveLeftClick(IClickableMenu __instance, int x, int y, bool playSound)
        {
            if (__instance.upperRightCloseButton == null || !__instance.readyToClose() || !__instance.upperRightCloseButton.containsPoint(x, y))
                return;
            if (playSound)
                Game1.playSound("bigDeSelect");
            __instance.exitThisMenu(true);
        }

        internal static IEnumerable<CodeInstruction> Transpile(ILGenerator gen, MethodBase original, IEnumerable<CodeInstruction> insns)
        {
            Log.Info("Transpiling " + original);
            List<CodeInstruction> ret = new List<CodeInstruction>();

            foreach (var insn in insns)
            {
                if (insn.operand is MethodInfo info)
                {
                    if (info.DeclaringType == typeof(Game1) && info.Name == "getLocationFromName")
                    {
                        Log.Debug("Found a getLocationFromName, replacing...");
                        insn.operand = PatchHelper.RequireMethod<CarpenterMenuPatcher>(nameof(ReturnCurrentLocationAnyways));
                    }
                }
                else if (insn.operand is TypeInfo tinfo)
                {
                    if (tinfo == typeof(Farm))
                    {
                        insn.operand = typeof(BuildableGameLocation);
                    }
                }
                ret.Add(insn);
            }

            return ret;
        }

        private static GameLocation ReturnCurrentLocationAnyways(string requested)
        {
            // Where this method is referenced, it always casts to BuildableGameLocation
            // Check, just to be sure. (In case the menu is transitioning or something.)
            // Otherwise, return the farm.
            if (Game1.currentLocation is BuildableGameLocation)
            {
                return Game1.currentLocation;
            }
            return Game1.getFarm();
        }
    }
}
