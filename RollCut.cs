using HarmonyLib;
using Il2Cpp;
using Il2CppTLD.Gear;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using UnityEngine;
using Object=UnityEngine.Object;
namespace BurebistaCandleRecipes;
internal static class RollCut {
 internal static void Configure(GearItem gear){
  var harvest=gear.gameObject.AddComponent<Harvest>();harvest.m_DurationMinutes=5;harvest.m_AppliedSkillType=(SkillType)(-1);harvest.m_RequiredTools=new Il2CppReferenceArray<ToolsItem>(new[]{MainKnife()}.Concat(Alternatives()).ToArray());
  harvest.m_YieldGear=new Il2CppReferenceArray<GearItem>(new[]{Candles.Template(Candles.CutWick)});harvest.m_YieldGearUnits=new Il2CppStructArray<int>(new[]{4});harvest.m_YieldPowder=new Il2CppReferenceArray<PowderType>(0);harvest.m_YieldPowderAmount=new Il2CppStructArray<Il2CppTLD.IntBackedUnit.ItemWeight>(0);
  harvest.m_Audio=BurebistaGear258.LoadGearItemPrefab(Candles.Native).m_GearItemData.m_PickupAudio?.WwiseObjectReference?.objectName??"NULL_WWISEEVENT";gear.m_Harvest=harvest;
 }
 internal static void RefreshInventoryRolls(){var inv=GameManager.GetInventoryComponent();if(inv==null)return;var tools=new[]{MainKnife()}.Concat(Alternatives()).ToArray();foreach(var entry in inv.m_Items){var gear=entry?.m_GearItem;if(gear==null||gear.name.Replace("(Clone)","").Trim()!=Candles.Wick)continue;if(gear.m_Harvest==null)Configure(gear);gear.m_Harvest.m_RequiredTools=new Il2CppReferenceArray<ToolsItem>(tools);}}

 internal static void Draw(){}
 internal static bool NavigationRequested;static float retry;
 internal static bool IsRoll(GearItem gear)=>gear!=null&&gear.name.Replace("(Clone)","").Trim()==Candles.Wick;
 internal static void TickNavigation(){if(!NavigationRequested||Time.realtimeSinceStartup<retry)return;retry=Time.realtimeSinceStartup+.25f;var manager=BlueprintManager.s_Instance;if(manager==null||!manager.m_IsInitialized)return;
  try{Candles.EnsureRecipes();BlueprintData recipe=null;foreach(var bp in manager.m_AllBlueprints)if(bp!=null&&bp.name=="BureCutWickRecipe"){recipe=bp;break;}if(recipe==null)return;
   NavigationRequested=false;
   InterfaceManager.TrySetPanelEnabled<Panel_Inventory_Examine>(false);InterfaceManager.TrySetPanelEnabled<Panel_Harvest>(false);InterfaceManager.TrySetPanelEnabled<Panel_Inventory>(false);
   var panel=InterfaceManager.GetPanel<Panel_Crafting>();panel.Enable(true);panel.SetFilter(Panel_Crafting.Filter.All);panel.RefreshAvailableBlueprints();panel.ApplyFilter();
   int index=-1;for(int i=0;i<panel.m_FilteredBlueprints.Count;i++)if(panel.m_FilteredBlueprints[i].name==recipe.name){index=i;break;}
   if(index<0)throw new InvalidOperationException("Cut wick recipe not found in native crafting list");panel.m_ScrollBehaviour.SetSelectedIndex(index,true,true);panel.SelectedBPI=recipe;panel.RefreshSelectedBlueprint(index);
   MelonLoader.MelonLogger.Msg("Roll action opened native Cut wick recipe with tool selector.");
  }catch(Exception e){NavigationRequested=false;MelonLoader.MelonLogger.Warning("Cut wick navigation: "+e);HUDMessage.AddMessage(Main.T("Abre Fabricacion > Mecha cortada.","Open Crafting > Cut wick."));}
 }

 static readonly Dictionary<string,GearItem> templates=new();
 internal static readonly string[] ToolKeys={"GEAR_Knife","GEAR_KnifeImprovised","GEAR_Flint"};
 internal static ToolsItem MainKnife()=>BurebistaGear258.LoadGearItemPrefab("GEAR_Knife").m_ToolsItem;
 static ToolsItem AttachFlint(GearItem gear){var tool=gear.m_ToolsItem;if(tool==null){tool=gear.gameObject.AddComponent<ToolsItem>();gear.m_ToolsItem=tool;}var knife=MainKnife();tool.m_ToolType=knife.m_ToolType;tool.m_CuttingToolType=knife.m_CuttingToolType;tool.m_CraftingAndRepairTimeModifier=1;tool.m_CraftingAndRepairSkillModifier=0;tool.m_DegradePerHourCrafting=0;tool.m_CanOnlyCraftAndRepairClothes=false;return tool;}
 internal static ToolsItem[] Alternatives(){var tools=new List<ToolsItem>();var improvised=BurebistaGear258.LoadGearItemPrefab("GEAR_KnifeImprovised")?.m_ToolsItem;if(improvised!=null)tools.Add(improvised);var inv=GameManager.GetInventoryComponent();if(inv==null)return tools.ToArray();
  foreach(var key in ToolKeys.Skip(2)){var actual=inv.GetHighestConditionGearThatMatchesName(key);if(actual==null)continue;if(key=="GEAR_Flint")AttachFlint(actual);
   if(!templates.TryGetValue(key,out var prefab)||prefab==null){var root=Object.Instantiate(BurebistaGear258.LoadGearItemPrefab(Candles.Native).gameObject);root.SetActive(false);Object.DontDestroyOnLoad(root);prefab=root.GetComponent<GearItem>();prefab.name=key;prefab.m_GearItemData=actual.m_GearItemData;prefab.m_InPlayerInventory=false;prefab.m_ToolsItem=root.AddComponent<ToolsItem>();var source=actual.m_ToolsItem??MainKnife();prefab.m_ToolsItem.m_ToolType=source.m_ToolType;prefab.m_ToolsItem.m_CuttingToolType=source.m_CuttingToolType;prefab.m_ToolsItem.m_CraftingAndRepairTimeModifier=1;prefab.m_ToolsItem.m_DegradePerHourCrafting=0;templates[key]=prefab;}
   tools.Add(prefab.m_ToolsItem);
  }return tools.ToArray();
 }
}


[HarmonyPatch(typeof(Panel_Inventory_Examine),nameof(Panel_Inventory_Examine.OnSelectHarvestButton))]
static class OpenRollNativeRecipe {
 static bool Prefix(Panel_Inventory_Examine __instance){if(!Main.Enabled||!RollCut.IsRoll(__instance.m_GearItem))return true;RollCut.NavigationRequested=true;return false;}
}
[HarmonyPatch(typeof(Panel_Inventory_Examine),nameof(Panel_Inventory_Examine.OnHarvest))]
static class OpenRollNativeRecipeFallback {
 static bool Prefix(Panel_Inventory_Examine __instance){if(!Main.Enabled||!RollCut.IsRoll(__instance.m_GearItem))return true;RollCut.NavigationRequested=true;return false;}
}

