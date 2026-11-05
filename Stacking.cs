using HarmonyLib;
using Il2Cpp;
namespace BurebistaCandleRecipes;
[HarmonyPatch(typeof(StackableItem),nameof(StackableItem.CanStackItems))]
static class CandleStackSeparation {
 static bool Prefix(GearItem __0,GearItem __1,ref bool __result){if(!Main.Enabled||__0==null||__1==null)return true;bool first=Candles.Is(__0.name),second=Candles.Is(__1.name);if(!first&&!second)return true;__result=__0.Pointer!=__1.Pointer&&first&&second&&__0.name.Replace("(Clone)","").Trim()==__1.name.Replace("(Clone)","").Trim()&&(__0.name.Replace("(Clone)","").Trim()!=Candles.Candle||Math.Abs(__0.GetNormalizedCondition()-__1.GetNormalizedCondition())<.000001f);return false;}
}

