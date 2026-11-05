using Il2CppTLD.Gear;
using Il2Cpp;
using UnityEngine;
using Object=UnityEngine.Object;
namespace BurebistaCandleRecipes;
internal static class CandleBurn {
 sealed class State {internal GearItem Gear;internal Light Glow;internal GameObject Flame;internal bool On;internal float Last;}
 static readonly List<State> states=new();
 internal static void Register(GearItem gear){if(gear.name!=Candles.Candle||states.Any(s=>s.Gear==gear))return;states.Add(new State{Gear=gear});}
 internal static void SpentFuel(GearItem gear){if(gear.m_FuelSourceItem!=null)return;var fuel=gear.gameObject.AddComponent<FuelSourceItem>();fuel.m_BurnDurationHours=2f/60f;fuel.m_HeatIncrease=.5f;fuel.m_HeatInnerRadius=1;fuel.m_HeatOuterRadius=2;fuel.m_FireAgeMinutesBeforeAdding=0;fuel.m_IsTinder=false;fuel.m_IsWet=false;gear.m_FuelSourceItem=fuel;}
 internal static void DrawHUD(){
  if(GameManager.IsMainMenuActive()||GameManager.GetPlayerManagerComponent()==null||InterfaceManager.IsPanelEnabled<Panel_Inventory>()||InterfaceManager.IsPanelEnabled<Panel_Crafting>())return;
  var pos=GameManager.GetPlayerTransform().position;var near=states.Where(s=>s.Gear!=null&&s.Gear.gameObject.activeInHierarchy&&!s.Gear.m_InPlayerInventory&&!s.Gear.IsInsideContainer()&&Vector3.Distance(s.Gear.transform.position,pos)<3f).OrderBy(s=>Vector3.Distance(s.Gear.transform.position,pos)).FirstOrDefault();if(near==null)return;var gear=near.Gear;
  int minutes=Mathf.CeilToInt(24*60*Mathf.Clamp01(gear.GetNormalizedCondition()));string remaining=(minutes/60)+" h "+(minutes%60).ToString("00")+" min";bool on=states.Any(s=>s.Gear==gear&&s.On);GUI.depth=-1000;
  var rect=new Rect(Screen.width*.5f-230,Screen.height*.68f,460,55);GUI.Box(rect,(minutes==0?Main.T("Resto de vela: combustible para el fuego", "Candle stub: campfire fuel"):Main.T("Combustible restante: ","Fuel remaining: ")+remaining)+"\n"+Main.T("Boton central: ","Middle mouse: ")+(on?Main.T("apagar","extinguish"):Main.T("encender","light")));
 }
 internal static void Tick(){
  if(!Main.Enabled||GameManager.IsMainMenuActive()||GameManager.GetPlayerManagerComponent()==null)return;
  float now=GameManager.GetTimeOfDayComponent().GetHoursPlayedNotPaused();
  for(int i=states.Count-1;i>=0;i--){var s=states[i];if(s.Gear==null){states.RemoveAt(i);continue;}float elapsed=Mathf.Max(0,now-s.Last);s.Last=now;
   if(s.Gear.m_InPlayerInventory||s.Gear.IsInsideContainer()||!s.Gear.gameObject.activeInHierarchy)s.On=false;
   if(s.On&&elapsed>0)s.Gear.SetNormalizedHP(Mathf.Max(0,s.Gear.GetNormalizedCondition()-elapsed/24f),false);
   float remaining=Mathf.Clamp01(s.Gear.GetNormalizedCondition());float height=.12f+.88f*remaining;
   var model=s.Gear.transform.Find("BureNomadCandleVisual");if(model!=null)model.localScale=new Vector3(1,height,1);
   s.Gear.WeightKG=Il2CppTLD.IntBackedUnit.ItemWeight.FromKilograms(.25f*height);
   if(s.Glow!=null)s.Glow.transform.localPosition=new Vector3(0,.141f*height+.007f,0);
   if(remaining<=0){s.On=false;SpentFuel(s.Gear);}
   if(s.Glow!=null){s.Glow.enabled=s.On;s.Glow.intensity=.43f+.025f*Mathf.Sin(Time.time*7.3f)+.018f*Mathf.Sin(Time.time*11.7f);}if(s.Flame!=null)s.Flame.SetActive(s.On);
  }
  if(InterfaceManager.IsOverlayActiveCached()||!Input.GetMouseButtonDown(2))return;
  var pos=GameManager.GetPlayerTransform().position;var nearest=states.Where(s=>s.Gear!=null&&!s.Gear.m_InPlayerInventory&&!s.Gear.IsInsideContainer()&&s.Gear.gameObject.activeInHierarchy&&Vector3.Distance(pos,s.Gear.transform.position)<2.5f).OrderBy(s=>Vector3.Distance(pos,s.Gear.transform.position)).FirstOrDefault();if(nearest==null)return;
  if(nearest.On){nearest.On=false;HUDMessage.AddMessage(Main.T("Vela apagada","Candle extinguished"));return;}
  if(nearest.Gear.m_StackableItem!=null&&nearest.Gear.m_StackableItem.m_Units>1){HUDMessage.AddMessage(Main.T("Suelta una sola vela para encenderla.","Drop one candle to light it."));return;}
  if(nearest.Gear.GetNormalizedCondition()<=0){HUDMessage.AddMessage(Main.T("La vela está consumida","Candle exhausted"));return;}
  bool Close(Component c,float distance)=>Vector3.Distance(c.transform.position,pos)<distance||Vector3.Distance(c.transform.position,nearest.Gear.transform.position)<distance;
  bool fire=Object.FindObjectsOfType<Fire>().Any(f=>f.IsBurning()&&Close(f,8))||Object.FindObjectsOfType<TorchItem>().Any(t=>t.IsBurning()&&Close(t,3))||Object.FindObjectsOfType<KeroseneLampItem>().Any(l=>l.IsOn()&&Close(l,3))||Object.FindObjectsOfType<MatchesItem>().Any(m=>m.IsBurning()&&Close(m,3))||states.Any(s=>s.On&&s.Gear!=null&&Close(s.Gear,2.5f));
  var inventory=GameManager.GetInventoryComponent();var match=fire?null:inventory.GetBestMatches();
  if(!fire&&(match==null||match.GetNormalizedCondition()<=0)){HUDMessage.AddMessage(Main.T("Necesitas una llama encendida o cerillas.","You need a lit flame or matches."));return;}
  if(nearest.Glow==null){var root=new GameObject("BureCandleGlow");root.transform.SetParent(nearest.Gear.transform,false);root.transform.localPosition=new Vector3(0,.15f,0);nearest.Glow=root.AddComponent<Light>();nearest.Glow.color=new Color(1,.67f,.32f);nearest.Glow.range=2.5f;nearest.Glow.intensity=.45f;nearest.Glow.shadows=LightShadows.None;
   nearest.Flame=NativeFlame.Create(root.transform);

  }
  if(!fire){int before=inventory.GetNumMatches();inventory.RemoveUnits(match,1);if(inventory.GetNumMatches()!=before-1){HUDMessage.AddMessage(Main.T("No se pudo usar la cerilla.","Could not use the match."));return;}}
  nearest.Last=now;nearest.On=true;HUDMessage.AddMessage(Main.T("Vela encendida — ","Candle lit — ")+(24*nearest.Gear.GetNormalizedCondition()).ToString("0.0")+Main.T(" horas restantes"," hours remaining"));
 }
}


