using HarmonyLib;
using Il2Cpp;
using Il2CppTLD.Gear;
namespace BurebistaCandleRecipes;
// Only our gear results bypass the native Addressables registry. Furniture,
// decorations and every other crafting result keep their original path.
[HarmonyPatch(typeof(PlayerManager),nameof(PlayerManager.InstantiateCraftedItemInPlayerInventory))]
static class CandleCraftedDelivery {
 static bool Prefix(ICraftingResult result,int units,float normalizedCondition,ref ICraftingResult __result){
  var gear=result?.TryCast<GearItem>();if(gear==null||!Main.Enabled||!Candles.Is(gear.name))return true;
  MelonLoader.MelonLogger.Msg("Candle crafted result: "+gear.name+"; units="+units);
  GearItem delivered=null;
  for(int i=0;i<units;i++)delivered=Candles.Give(gear.name,normalizedCondition);
  __result=delivered?.Cast<ICraftingResult>();return false;
 }
}
[HarmonyPatch(typeof(PlayerManager),nameof(PlayerManager.InstantiateItemInPlayerInventory),new[]{typeof(GearItem),typeof(int),typeof(float),typeof(PlayerManager.InventoryInstantiateFlags)})]
static class CandleInventoryFromTemplate {
 static bool Prefix(GearItem gearItem,int units,float normalizedCondition,ref GearItem __result){
  if(gearItem==null||!Main.Enabled||!Candles.Is(gearItem.name))return true;
  for(int i=0;i<units;i++)__result=Candles.Give(gearItem.name,normalizedCondition);
  return false;
 }
}
[HarmonyPatch(typeof(PlayerManager),nameof(PlayerManager.InstantiateItemInPlayerInventory),new[]{typeof(AssetReferenceGearItem),typeof(int),typeof(float),typeof(PlayerManager.InventoryInstantiateFlags)})]
static class CandleInventoryFromReference {
 static bool Prefix(AssetReferenceGearItem assetReference,int units,float normalizedCondition,ref GearItem __result){
  if(!Main.Enabled||!Candles.Is(assetReference?.RuntimeKey?.ToString()))return true;
  for(int i=0;i<units;i++)__result=Candles.Give(assetReference.RuntimeKey.ToString(),normalizedCondition);
  return false;
 }
}
