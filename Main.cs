using Il2CppTLD.Gear;
using HarmonyLib;
using Il2Cpp;
using MelonLoader;
using UnityEngine;
[assembly:MelonInfo(typeof(BurebistaCandleRecipes.Main),"Burebista Candle Recipes","0.3.2","Burebista")]
[assembly:MelonGame("Hinterland","TheLongDark")]
namespace BurebistaCandleRecipes;
public sealed class Main:MelonMod {
 internal static bool Enabled=true;float next;int blueprintCount=-1;string language="";bool warning;
 internal static bool English=>(Localization.Language??"").StartsWith("English",StringComparison.OrdinalIgnoreCase);
 internal static string T(string es,string en)=>English?en:es;
 public override void OnInitializeMelon(){Enabled=AccessTools.TypeByName("BurebistaNomadCamp.Main")==null;if(!Enabled)MelonLogger.Warning("Remove the retired Nomad Camp addon before using this standalone recipe; duplicate item registration disabled.");else MelonLogger.Msg("Own candle and wick recipes only. No camp, tipi or map changes.");}
 public override void OnGUI(){if(!Enabled)return;var matrix=GUI.matrix;var color=GUI.color;var background=GUI.backgroundColor;var content=GUI.contentColor;var enabled=GUI.enabled;try{GUI.matrix=Matrix4x4.identity;GUI.color=Color.white;GUI.contentColor=Color.white;GUI.backgroundColor=new Color(.07f,.07f,.07f,.95f);GUI.enabled=true;RollCut.Draw();}finally{GUI.matrix=matrix;GUI.color=color;GUI.backgroundColor=background;GUI.contentColor=content;GUI.enabled=enabled;}}
 public override void OnSceneWasLoaded(int i,string n){next=Time.realtimeSinceStartup+15;blueprintCount=-1;language="";}
 public override void OnUpdate(){RollCut.TickNavigation();try{CandleBurn.Tick();}catch(Exception e){if(!warning){warning=true;MelonLogger.Warning("Candle light: "+e.Message);}}if(!Enabled||Time.realtimeSinceStartup<next||GameManager.IsMainMenuActive()||GameManager.GetPlayerManagerComponent()==null)return;next=Time.realtimeSinceStartup+10;
  try{if(language!=Localization.Language){Candles.Texts();language=Localization.Language;}var manager=BlueprintManager.s_Instance;if(manager==null||!manager.m_IsInitialized||manager.m_AllBlueprints==null)return;Candles.EnsureRecipes();blueprintCount=manager.m_AllBlueprints.Count;}
  catch(Exception e){next=Time.realtimeSinceStartup+60;if(!warning){warning=true;MelonLogger.Warning("Candle recipes pending: "+e.Message);}}
 }
}


