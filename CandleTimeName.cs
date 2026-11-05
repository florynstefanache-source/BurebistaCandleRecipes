using System.Reflection;
using HarmonyLib;
using Il2Cpp;
namespace BurebistaCandleRecipes;
[HarmonyPatch]
static class CandleTimeName {
 static IEnumerable<MethodBase> TargetMethods(){yield return AccessTools.PropertyGetter(typeof(GearItem),"DisplayName");yield return AccessTools.PropertyGetter(typeof(GearItem),"DisplayNameWithCondition");}
 static void Postfix(GearItem __instance,ref string __result){if(!Main.Enabled||__instance==null||__instance.name.Replace("(Clone)","").Trim()!=Candles.Candle||string.IsNullOrEmpty(__result)||__result.Contains(" min]"))return;int minutes=(int)Math.Ceiling(1440*Math.Clamp(__instance.GetNormalizedCondition(),0,1));__result+=" ["+(minutes/60)+" h "+(minutes%60).ToString("00")+" min]";}
}
