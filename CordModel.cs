using UnityEngine;
namespace BurebistaCandleRecipes;
internal static class CordModel {
 internal static void Add(GameObject root,bool roll,Material material){
  const int segments=192,sides=10;float radius=roll?.003f:.002f;
  var vertices=new Vector3[(segments+1)*sides];var uv=new Vector2[vertices.Length];var triangles=new int[segments*sides*6];
  Vector3 Path(float t){if(!roll)return new Vector3((t-.5f)*.075f,.003f,Mathf.Sin(t*Mathf.PI)*.008f);float angle=t*Mathf.PI*10;float r=.008f+t*.027f;return new Vector3(Mathf.Cos(angle)*r,.004f,Mathf.Sin(angle)*r);}
  for(int i=0;i<=segments;i++){float t=(float)i/segments;var pos=Path(t);var tangent=(Path(t+.001f)-Path(t-.001f)).normalized;var side=Vector3.Cross(tangent,Vector3.up).normalized;for(int j=0;j<sides;j++){float a=j*2*Mathf.PI/sides;float texture=1+.07f*Mathf.Sin(t*360+a*3);int n=i*sides+j;vertices[n]=pos+(Vector3.up*Mathf.Cos(a)+side*Mathf.Sin(a))*radius*texture;uv[n]=new Vector2(t*30,(float)j/sides);if(i<segments){int k=(i*sides+j)*6;int next=i*sides+(j+1)%sides;triangles[k]=n;triangles[k+1]=n+sides;triangles[k+2]=next;triangles[k+3]=next;triangles[k+4]=n+sides;triangles[k+5]=next+sides;}}}
  var mesh=new Mesh();mesh.vertices=vertices;mesh.triangles=triangles;mesh.uv=uv;mesh.RecalculateNormals();mesh.RecalculateBounds();var obj=new GameObject(roll?"CottonWickRoll":"CutCottonWick");obj.transform.SetParent(root.transform,false);obj.AddComponent<MeshFilter>().sharedMesh=mesh;obj.AddComponent<MeshRenderer>().sharedMaterial=material;
 }
}
