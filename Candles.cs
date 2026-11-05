using System.Reflection;
using HarmonyLib;
using Il2Cpp;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Il2CppTLD.Gear;
using Il2CppTLD.IntBackedUnit;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Object=UnityEngine.Object;
namespace BurebistaCandleRecipes;
internal static class Candles {
 internal const string Candle="GEAR_BureNomadCandle",Wick="GEAR_BureNomadWick",CutWick="GEAR_BureCandleWickPiece",Native="GEAR_Cloth";
 static readonly Dictionary<string,GearItem> templates=new();static readonly Dictionary<string,GearItemData> data=new();
 static readonly Dictionary<string,BlueprintData> recipes=new();
 static readonly Dictionary<string,GearItem> inventoryFatTemplates=new();
 static Material wax,thread;static Texture2D icon,wickIcon,pieceIcon;
 internal static bool Is(string name)=>(name??"").Replace("(Clone)","").Trim() is Candle or Wick or CutWick;
 internal static Texture2D Icon(){if(icon!=null)return icon;using var stream=typeof(Main).Assembly.GetManifestResourceStream("Burebista.Candle.png");using var bytes=new MemoryStream();stream.CopyTo(bytes);icon=new Texture2D(2,2,TextureFormat.RGBA32,false);ImageConversion.LoadImage(icon,bytes.ToArray(),false);Object.DontDestroyOnLoad(icon);return icon;}
 internal static Texture2D WickIcon(){if(wickIcon!=null)return wickIcon;using var stream=typeof(Main).Assembly.GetManifestResourceStream("Burebista.Wick.png");using var bytes=new MemoryStream();stream.CopyTo(bytes);wickIcon=new Texture2D(2,2,TextureFormat.RGBA32,false);ImageConversion.LoadImage(wickIcon,bytes.ToArray(),false);Object.DontDestroyOnLoad(wickIcon);return wickIcon;}
 internal static Texture2D PieceIcon(){if(pieceIcon!=null)return pieceIcon;using var stream=typeof(Main).Assembly.GetManifestResourceStream("Burebista.WickPiece.png");using var bytes=new MemoryStream();stream.CopyTo(bytes);pieceIcon=new Texture2D(2,2,TextureFormat.RGBA32,false);ImageConversion.LoadImage(pieceIcon,bytes.ToArray(),false);Object.DontDestroyOnLoad(pieceIcon);return pieceIcon;}
 internal static float Weight(string key)=>key==Candle?.25f:key==Wick?.04f:.01f;
 internal static GearItem Give(string key,float condition){
  MelonLoader.MelonLogger.Msg("Candle delivery begin: "+key);
  var prefab=Template(key);var gear=Object.Instantiate(prefab.gameObject).GetComponent<GearItem>();
  MelonLoader.MelonLogger.Msg("Candle delivery cloned: "+key);
  Configure(gear,key);gear.m_StackableItem.m_Units=1;gear.gameObject.SetActive(true);gear.SetNormalizedHP(Mathf.Clamp01(condition),false);ObjectGuid.MaybeAttachObjectGuidAddOrReplace(gear.gameObject,Guid.NewGuid().ToString());
  MelonLoader.MelonLogger.Msg("Candle delivery adding to inventory: "+key);
  var added=GameManager.GetPlayerManagerComponent().AddItemToPlayerInventory(gear,false,true);
  if(added==null||!added.m_InPlayerInventory){Object.Destroy(gear.gameObject);throw new IOException("Candle/wick inventory delivery failed");}
  MelonLoader.MelonLogger.Msg("Candle delivery complete: "+key+"; stack="+(added.m_StackableItem?.m_Units??1));return added;
 }
 internal static GearItem Template(string key){
  if(templates.TryGetValue(key,out var cached)&&cached!=null)return cached;
  var native=BurebistaGear258.LoadGearItemPrefab(Native)??throw new InvalidOperationException("Native candle template unavailable");
  var root=Object.Instantiate(native.gameObject);root.SetActive(false);Object.DontDestroyOnLoad(root);
  var item=Object.Instantiate(native.m_GearItemData);item.name=key+"Data";Object.DontDestroyOnLoad(item);item.m_PrefabReference=new AssetReferenceGearItem(key);item.m_BaseWeight=ItemWeight.FromKilograms(Weight(key));
  item.m_Type=key==Candle?GearType.Firestarting:GearType.Material;
  item.m_LocalizedName=new LocalizedString{m_LocalizationID=key+"Name"};item.m_LocalizedDescription=new LocalizedString{m_LocalizationID=key+"Description"};
  var image=ScriptableObject.CreateInstance<GearItemInventoryIconSimpleData>();image.m_Icon=key==Wick?WickIcon():key==CutWick?PieceIcon():Icon();Object.DontDestroyOnLoad(image);item.m_IconData=image;
  data[key]=item;var gear=root.GetComponent<GearItem>();templates[key]=gear;Configure(gear,key);return gear;
 }
 internal static GameObject Visual(string key){
  if(wax==null){wax=new Material(Shader.Find("Standard")??Shader.Find("Diffuse")){color=new Color(.83f,.75f,.57f)};thread=new Material(wax){color=new Color(.10f,.075f,.04f)};Object.DontDestroyOnLoad(wax);Object.DontDestroyOnLoad(thread);}
  var g=new GameObject("BureNomadCandleVisual");
  void Part(string name,PrimitiveType shape,Vector3 p,Vector3 scale,Material m){var part=GameObject.CreatePrimitive(shape);part.name=name;part.transform.SetParent(g.transform,false);part.transform.localPosition=p;part.transform.localScale=scale;part.GetComponent<Renderer>().sharedMaterial=m;var c=part.GetComponent<Collider>();c.enabled=false;Object.Destroy(c);}
  if(key==Candle){
   Part("TallowBody",PrimitiveType.Cylinder,new Vector3(0,.065f,0),new Vector3(.075f,.065f,.075f),wax);
   for(int i=0;i<7;i++){float angle=i*2.4f;float y=.07f+(i%3)*.014f;Part("UnevenTallowDrip",PrimitiveType.Capsule,new Vector3(Mathf.Sin(angle)*.034f,y,Mathf.Cos(angle)*.034f),new Vector3(.009f,.025f+(i%2)*.01f,.008f),wax);}
  }
  if(key==Candle)Part("CottonWick",PrimitiveType.Cylinder,new Vector3(0,.141f,0),new Vector3(.004f,.011f,.004f),thread);
  else CordModel.Add(g,key==Wick,wax);
  return g;
 }
 internal static void Configure(GearItem gear,string key){if(gear==null)return;if(!data.ContainsKey(key))Template(key);gear.name=key;gear.m_GearItemData=data[key];foreach(var component in gear.GetComponents<HarvestBase>())Object.DestroyImmediate(component);
  foreach(var component in gear.GetComponents<ToolsItem>())Object.DestroyImmediate(component);

  gear.SyncAudioValues(BurebistaGear258.LoadGearItemPrefab(Native).gameObject);gear.m_DefaultLocalScale=Vector3.one;gear.transform.localScale=Vector3.one;gear.WeightKG=ItemWeight.FromKilograms(Weight(key));gear.m_ToolsItem=null;gear.m_Harvest=null;
  var stack=gear.GetComponent<StackableItem>();if(stack==null){stack=gear.gameObject.AddComponent<StackableItem>();stack.m_Units=1;}gear.m_StackableItem=stack;
  stack.m_DefaultUnitsInItem=1;stack.m_LocalizedSingleUnitText=new LocalizedString{m_LocalizationID=key+"Name"};stack.m_LocalizedMultipleUnitText=stack.m_LocalizedSingleUnitText;stack.m_ShareStackWithGear=new Il2CppReferenceArray<StackableItem>(0);stack.m_StackSpriteName="";stack.m_InstantiateStackItem=null;stack.m_StackConditionDifferenceConstraint=key==Candle?0f:1f;
 if(key==Candle){foreach(var fuel in gear.GetComponents<FuelSourceItem>())Object.DestroyImmediate(fuel);gear.m_FuelSourceItem=null;if(gear.GetNormalizedCondition()<=0)CandleBurn.SpentFuel(gear);}
 gear.SetHaltDecay(true);CandleBurn.Register(gear);if(key==Wick)RollCut.Configure(gear);
  var visual=gear.transform.Find("BureNomadCandleVisual");if(visual!=null)return;
  foreach(var r in gear.GetComponentsInChildren<Renderer>(true))r.enabled=false;foreach(var c in gear.GetComponentsInChildren<Collider>(true))c.enabled=false;
  var model=Visual(key);model.transform.SetParent(gear.transform,false);var box=gear.gameObject.AddComponent<BoxCollider>();box.center=new Vector3(0,key==Candle?.07f:.006f,0);box.size=key==Candle?new Vector3(.08f,.16f,.08f):new Vector3(.08f,.014f,.08f);
  gear.m_MeshRenderers=model.GetComponentsInChildren<MeshRenderer>(true).ToArray();gear.m_SkinnedMeshRenderers=new SkinnedMeshRenderer[0];
 }
 internal static void Texts(){var table=Localization.s_CurrentLanguageStringTable;if(table==null)return;foreach(string key in new[]{Candle,Wick,CutWick})foreach(bool title in new[]{true,false}){string text=key==Candle?(title?Main.T("Vela de sebo","Tallow candle"):Main.T("Aproximadamente 500 g de grasa (2 porciones de castor) y 1 mecha cortada. Dura 24 horas encendida; el resto agotado sirve como combustible. Botón central cerca de la vela colocada: encender/apagar; se enciende con una llama cercana o una cerilla.","About 500 g of fat (2 beaver portions) and 1 cut wick. Burns for 24 hours; the exhausted stub can fuel a fire. Middle mouse near the placed candle: light/extinguish; use a lit flame nearby or one inventory match.")):key==Wick?(title?Main.T("Rollo de mechas","Wick roll"):Main.T("Fabricado con 1 tela. Acciones > Cortar/Desmontar: produce 4 mechas. Requiere cuchillo o sílex; no consume la herramienta.","Made from 1 cloth. Actions > Harvest: produces 4 wicks. Requires a knife or flint; does not consume the tool.")):(title?Main.T("Mecha cortada","Cut wick"):Main.T("Trozo de mecha de algodón. Una mecha y 2 porciones de grasa producen una vela.","A cut cotton wick. One wick and 2 fat portions make one candle."));string id=key+(title?"Name":"Description");var e=table.GetEntryFromKey(id)??table.AddEntryForKey(id);for(int i=0;i<e.m_Languages.Count;i++)e.m_Languages[i]=text;}}
 internal static void EnsureRecipes(){
  var manager=BlueprintManager.s_Instance;if(manager==null||!manager.m_IsInitialized||manager.m_AllBlueprints==null)return;
  RollCut.RefreshInventoryRolls();AddRecipe("BureNomadWickRecipe",null);AddRecipe("BureCutWickRecipe",null);
  var fats=new Dictionary<string,GearItem>();
  var own=AccessTools.TypeByName("BurebistaBeaver.TrapItems");var beaver=own==null?null:AccessTools.Method(own,"Template")?.Invoke(null,new object[]{4,true}) as GearItem;if(beaver!=null)fats["GEAR_BureBeaverFat"]=beaver;
  foreach(var bp in manager.m_AllBlueprints){if(bp==null)continue;void Add(GearItem item){if(item==null||item.m_InPlayerInventory)return;string key=(item.name??"").Replace("(Clone)","").Trim();if(FatRules.IsAnimalFat(key)&&!fats.ContainsKey(key))fats[key]=item;}
   Add(bp.m_CraftedResultGear);if(bp.m_RequiredGear!=null)foreach(var ingredient in bp.m_RequiredGear)if(ingredient!=null)Add(ingredient.m_Item);
  }
  var inv=GameManager.GetInventoryComponent();
  if(inv!=null)foreach(var entry in inv.m_Items){var item=entry?.m_GearItem;if(item==null)continue;string key=(item.name??"").Replace("(Clone)","").Trim();if(!FatRules.IsAnimalFat(key)||fats.ContainsKey(key))continue;
   if(!inventoryFatTemplates.TryGetValue(key,out var template)||template==null){var material=BurebistaGear258.LoadGearItemPrefab(Native);var root=Object.Instantiate(material.gameObject);root.SetActive(false);Object.DontDestroyOnLoad(root);template=root.GetComponent<GearItem>();template.name=key;template.m_GearItemData=item.m_GearItemData;template.m_InPlayerInventory=false;template.m_InsideContainer=false;inventoryFatTemplates[key]=template;MelonLoader.MelonLogger.Msg("Candle ingredient discovered in inventory: "+key);}
   fats[key]=template;
  }
  foreach(var fat in fats){string id=fat.Key=="GEAR_BureBeaverFat"?"BureNomadCandleRecipe":"BureCandleFrom_"+fat.Key;AddRecipe(id,fat.Value);}
 }
 static void AddRecipe(string id,GearItem fat){
  var manager=BlueprintManager.s_Instance;foreach(var bp in manager.m_AllBlueprints)if(bp!=null&&bp.name==id){if(id=="BureCutWickRecipe"){bp.m_OptionalTools=new Il2CppReferenceArray<ToolsItem>(RollCut.Alternatives());bp.InitializeOptionalTools();}return;}bool cut=id=="BureCutWickRecipe";bool wick=fat==null;
  if(!recipes.TryGetValue(id,out var recipe)){
   int fatCount=wick?0:fat.name.Replace("(Clone)","").Trim()=="GEAR_BureBeaverFat"?2:Math.Clamp(Mathf.CeilToInt(ItemWeight.FromKilograms(.5f)/fat.m_GearItemData.m_BaseWeight),1,100);
   var required=wick?new[]{new BlueprintData.RequiredGearItem{m_Item=cut?Template(Wick):BurebistaGear258.LoadGearItemPrefab("GEAR_Cloth"),m_Count=1}}:new[]{new BlueprintData.RequiredGearItem{m_Item=fat,m_Count=fatCount},new BlueprintData.RequiredGearItem{m_Item=Template(CutWick),m_Count=1}};if(required.Any(x=>x.m_Item==null))return;
   recipe=ScriptableObject.CreateInstance<BlueprintData>();recipe.name=id;recipe.SetCraftedResult(Template(cut?CutWick:wick?Wick:Candle));recipe.m_CraftedResultCount=cut?4:1;recipe.m_DurationMinutes=cut?5:wick?10:60;
   recipe.m_RequiredGear=new Il2CppReferenceArray<BlueprintData.RequiredGearItem>(required);recipe.m_RequiredCraftingLocation=CraftingLocation.Anywhere;recipe.m_RequiresLitFire=!wick;recipe.m_RequiresLight=true;
   recipe.m_CraftingAudio=new Il2CppAK.Wwise.Event{WwiseObjectReference=ScriptableObject.CreateInstance<WwiseEventReference>()};recipe.m_CraftingAudio.WwiseObjectReference.objectName="NULL_WWISEEVENT";
   recipe.m_XPModesToDisable=new Il2CppSystem.Collections.Generic.List<ExperienceModeType>();recipe.m_RequiredPowder=new Il2CppReferenceArray<BlueprintData.RequiredPowder>(0);recipe.m_RequiredLiquid=new Il2CppReferenceArray<BlueprintData.RequiredLiquid>(0);recipe.m_OptionalTools=new Il2CppReferenceArray<ToolsItem>(0);recipe.m_FilteredOptionalTools=new Il2CppSystem.Collections.Generic.List<ToolsItem>();recipe.m_AppliedSkill=(SkillType)(-1);recipe.m_ImprovedSkill=(SkillType)(-1);recipe.m_AppearsInSurvivalOnly=true;recipe.m_Locked=false;recipe.Locked=false;recipe.m_IgnoreLockInSurvival=true;Object.DontDestroyOnLoad(recipe);recipes[id]=recipe;
  }if(cut){recipe.m_RequiredTool=RollCut.MainKnife();recipe.m_OptionalTools=new Il2CppReferenceArray<ToolsItem>(RollCut.Alternatives());recipe.InitializeOptionalTools();}manager.m_AllBlueprints.Add(recipe);MelonLoader.MelonLogger.Msg("Candle recipe registered: "+id);
 }

}
[HarmonyPatch]
static class CandleFactory {
 static IEnumerable<MethodBase> TargetMethods()=>typeof(GearItem).GetMethods().Where(m=>(m.Name=="InstantiateGearItem"||m.Name=="InstantiateAndDeserializeGearItem")&&m.GetParameters().Length>0&&m.GetParameters()[0].ParameterType==typeof(string));
 static void Prefix(ref string __0,out string __state){__state=Main.Enabled&&Candles.Is(__0)?__0.Replace("(Clone)","").Trim():null;if(__state!=null)__0=Candles.Native;}
 static void Postfix(GearItem __result,string __state){if(__state!=null)Candles.Configure(__result,__state);}
}
[HarmonyPatch(typeof(GearItem),nameof(GearItem.CreateSaveData))]
static class CandleSave {static void Postfix(GearItem __instance,GearSaveData __result){if(Main.Enabled&&Candles.Is(__instance.name)&&__result!=null)__result.m_PrefabName=__instance.name.Replace("(Clone)","").Trim();}}
[HarmonyPatch(typeof(BlueprintData),nameof(BlueprintData.GetCraftingIcon))]
static class CandleRecipeIcon {static bool Prefix(BlueprintData __instance,ref Texture2D __result){if(!Main.Enabled||(__instance.name is not ("BureNomadWickRecipe" or "BureNomadCandleRecipe" or "BureCutWickRecipe")&&!__instance.name.StartsWith("BureCandleFrom_",StringComparison.Ordinal)))return true;__result=__instance.name=="BureNomadWickRecipe"?Candles.WickIcon():__instance.name=="BureCutWickRecipe"?Candles.PieceIcon():Candles.Icon();return false;}}

[HarmonyPatch]
static class CandleReferenceFactory {
 static IEnumerable<MethodBase> TargetMethods()=>typeof(GearItem).GetMethods().Where(m=>(m.Name=="InstantiateGearItem"||m.Name=="InstantiateAndDeserializeGearItem")&&m.GetParameters().Length>0&&m.GetParameters()[0].ParameterType==typeof(AssetReference));
 static void Prefix(ref AssetReference __0,out string __state){var key=__0?.RuntimeKey?.ToString();__state=Main.Enabled&&Candles.Is(key)?key:null;if(__state!=null)__0=BurebistaGear258.LoadGearItemPrefab(Candles.Native).m_GearItemData.m_PrefabReference;}
 static void Postfix(GearItem __result,string __state){if(__state!=null)Candles.Configure(__result,__state);}
}
