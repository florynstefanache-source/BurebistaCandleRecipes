using Il2Cpp;
using Il2CppTLD.AddressableAssets;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;
using System.Runtime.CompilerServices;
internal static class BurebistaGear258 {
 static readonly System.Collections.Generic.Dictionary<string, AsyncOperationHandle<GameObject>> handles=new();
 [MethodImpl(MethodImplOptions.NoInlining)]
 internal static GearItem LoadGearItemPrefab(string name) {
  if(string.IsNullOrWhiteSpace(name))return null;
  if(!handles.TryGetValue(name,out var handle)) {
   handle=AssetHelper.SafeLoadAssetAsync<GameObject>(name,false);
   handles[name]=handle;
  }
  var prefab=handle.WaitForCompletion();
  return prefab==null?null:prefab.GetComponent<GearItem>();
 }
}
