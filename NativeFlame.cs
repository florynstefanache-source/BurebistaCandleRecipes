using Il2Cpp;
using UnityEngine;
using Object=UnityEngine.Object;
namespace BurebistaCandleRecipes;
internal static class NativeFlame {
 internal static GameObject Create(Transform parent){
  var torch=BurebistaGear258.LoadGearItemPrefab("GEAR_Torch");var source=torch?.m_TorchItem?.m_FXGameObject;
  if(source==null)throw new InvalidOperationException("Native torch flame unavailable");
  var root=Object.Instantiate(source);root.name="BureSmallNativeFlame";root.SetActive(false);root.transform.SetParent(parent,false);root.transform.localPosition=Vector3.zero;root.transform.localRotation=Quaternion.identity;root.transform.localScale=Vector3.one*.025f;
  foreach(var light in root.GetComponentsInChildren<Light>(true))light.enabled=false;
  foreach(var audio in root.GetComponentsInChildren<AudioSource>(true))audio.enabled=false;
  foreach(var script in root.GetComponentsInChildren<MonoBehaviour>(true))script.enabled=false;
  foreach(var transform in root.GetComponentsInChildren<Transform>(true)){if(transform==root.transform)continue;transform.localPosition=Vector3.zero;transform.localRotation=Quaternion.identity;}
  var systems=root.GetComponentsInChildren<ParticleSystem>(true);var kept=new List<ParticleSystem>();var subSystems=new HashSet<IntPtr>();foreach(var system in systems){var subs=system.subEmitters;for(int i=0;i<subs.subEmittersCount;i++){var child=subs.GetSubEmitterSystem(i);if(child!=null)subSystems.Add(child.Pointer);}}
  foreach(var ps in systems){var renderer=ps.GetComponent<ParticleSystemRenderer>();string label=(ps.name+" "+(renderer?.sharedMaterial?.name??"")).ToLowerInvariant();var main=ps.main;main.playOnAwake=false;
   if(subSystems.Contains(ps.Pointer)||label.Contains("spark")||label.Contains("ember")||label.Contains("smoke")||label.Contains("distort")||label.Contains("heat")||label.Contains("ash")||label.Contains("glow")){var emission=ps.emission;emission.enabled=false;ps.Stop(false,ParticleSystemStopBehavior.StopEmittingAndClear);if(renderer!=null)renderer.enabled=false;continue;}
   main.scalingMode=ParticleSystemScalingMode.Hierarchy;main.simulationSpace=ParticleSystemSimulationSpace.Local;var shape=ps.shape;shape.position=Vector3.zero;kept.Add(ps);
  }
  root.SetActive(true);foreach(var ps in kept){ps.gameObject.SetActive(true);ps.Clear(false);ps.Play(false);}
  return root;
 }
}

