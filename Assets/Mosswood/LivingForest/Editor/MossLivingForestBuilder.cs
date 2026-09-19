using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using System.Linq;
using System.Collections.Generic;
using System.IO;
namespace Mosswood.Editor {
 public static class MossLivingForestBuilder {
  const string Root="Assets/Mosswood/LivingForest/";
  static Transform world;static Material spriteMat,shapeMat;static Sprite glow;static int meshId,chapter;static Color fog,shadow,light;static MossTerrainProfile profile;
  static Sprite S(string sheet,int index)=>AssetDatabase.LoadAllAssetsAtPath("Assets/Game/Map/Mossy - "+sheet+".png").OfType<Sprite>().First(s=>s.name=="Mossy - "+sheet+"_"+index);
  static Sprite Imported(string path)=>AssetDatabase.LoadAssetAtPath<Sprite>(Root+"Art/"+path);
  static void Import(){
   foreach(string path in AssetDatabase.FindAssets("t:Texture2D",new[]{Root+"Art"}).Select(AssetDatabase.GUIDToAssetPath)){var imp=(TextureImporter)AssetImporter.GetAtPath(path);imp.textureType=TextureImporterType.Sprite;imp.spriteImportMode=SpriteImportMode.Single;imp.spritePixelsPerUnit=100;imp.alphaIsTransparency=true;imp.mipmapEnabled=false;imp.filterMode=path.Contains("battt")?FilterMode.Point:FilterMode.Bilinear;imp.textureCompression=TextureImporterCompression.Uncompressed;imp.SaveAndReimport();}
   Directory.CreateDirectory(Root+"Meshes");spriteMat=AssetDatabase.LoadAssetAtPath<Material>("Assets/Mosswood/Expansion/SpiritSprite.mat");shapeMat=AssetDatabase.LoadAssetAtPath<Material>(Root+"Art/Silhouette.mat");if(!shapeMat){shapeMat=new Material(Shader.Find("Mosswood/LivingSilhouette"));AssetDatabase.CreateAsset(shapeMat,Root+"Art/Silhouette.mat");}glow=AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Mosswood/Art/Soft light.png");
   var tex=AssetDatabase.LoadAssetAtPath<Texture2D>(Root+"Art/Vander96/battt.png");for(int i=0;i<3;i++){string path=Root+"Art/Vander96/Flight"+i+".asset";if(!AssetDatabase.LoadAssetAtPath<Sprite>(path)){var sp=Sprite.Create(tex,new Rect(i*24,0,24,24),new Vector2(.5f,.5f),24);sp.name="Flight"+i;AssetDatabase.CreateAsset(sp,path);}}
  }
  static GameObject Art(string name,Sprite sprite,Vector2 position,Vector2 size,int order,Color color,Transform parent=null){var g=new GameObject(name);g.transform.SetParent(parent?parent:world);g.transform.position=position;g.transform.localScale=new Vector3(size.x/sprite.bounds.size.x,size.y/sprite.bounds.size.y,1);var r=g.AddComponent<SpriteRenderer>();r.sprite=sprite;r.sharedMaterial=spriteMat;r.sortingOrder=order;r.color=color;return g;}
  static GameObject Natural(string name,Sprite sprite,Vector2 p,float width,int order,Color color,Transform parent=null)=>Art(name,sprite,p,new Vector2(width,width*sprite.bounds.size.y/sprite.bounds.size.x),order,color,parent);
  static Mesh SaveMesh(Mesh m){string path=Root+"Meshes/Chapter"+chapter+"_"+(meshId++)+".asset";m.name=Path.GetFileNameWithoutExtension(path);var old=AssetDatabase.LoadAssetAtPath<Mesh>(path);if(old){EditorUtility.CopySerialized(m,old);Object.DestroyImmediate(m);return old;}AssetDatabase.CreateAsset(m,path);return m;}
  static Transform Shape(string name,Vector2[] points,Transform parent,Color color,int order){var g=new GameObject(name,typeof(MeshFilter),typeof(MeshRenderer));g.transform.SetParent(parent,false);var center=Vector2.zero;foreach(var p in points)center+=p;center/=points.Length;var vertices=new Vector3[points.Length+1];vertices[0]=center;var colors=new Color[vertices.Length];for(int i=0;i<vertices.Length;i++)colors[i]=color;for(int i=0;i<points.Length;i++)vertices[i+1]=points[i];var tris=new int[points.Length*3];for(int i=0;i<points.Length;i++){tris[i*3]=0;tris[i*3+1]=i+1;tris[i*3+2]=(i+1)%points.Length+1;}var mesh=new Mesh{name=name};mesh.vertices=vertices;mesh.triangles=tris;mesh.colors=colors;mesh.RecalculateBounds();g.GetComponent<MeshFilter>().sharedMesh=SaveMesh(mesh);var r=g.GetComponent<MeshRenderer>();r.sharedMaterial=shapeMat;r.sortingOrder=order;return g.transform;}
  static Transform Oval(string name,Vector2 center,Vector2 radius,Transform parent,Color color,int order){var points=new Vector2[40];for(int i=0;i<points.Length;i++){float a=i*2*Mathf.PI/points.Length;points[i]=center+new Vector2(Mathf.Cos(a)*radius.x,Mathf.Sin(a)*radius.y);}return Shape(name,points,parent,color,order);}
  static Transform Branch(string name,Vector2[] points,Transform parent,float width,Color color,int order){var g=new GameObject(name,typeof(LineRenderer));g.transform.SetParent(parent,false);var line=g.GetComponent<LineRenderer>();line.useWorldSpace=false;line.sharedMaterial=shapeMat;line.positionCount=points.Length;line.SetPositions(points.Select(p=>(Vector3)p).ToArray());line.startWidth=width;line.endWidth=width*.09f;line.startColor=line.endColor=color;line.numCapVertices=5;line.numCornerVertices=6;line.sortingOrder=order;return g.transform;}
  [MenuItem("DOAN/Living Forest/Upgrade all campaign scenes")]
  public static void Upgrade(){
   if(EditorApplication.isPlaying||SceneManager.GetActiveScene().isDirty)throw new System.InvalidOperationException("Save scene and exit Play Mode first.");
   Import();string original=SceneManager.GetActiveScene().path;
   for(chapter=0;chapter<3;chapter++){var scene=EditorSceneManager.OpenScene("Assets/Scenes/"+MossJourney.Scenes[chapter]+".unity");UpgradeScene();EditorSceneManager.SaveScene(scene);}
   AssetDatabase.SaveAssets();EditorSceneManager.OpenScene(original);Debug.Log("Living Forest installed in all 3 scenes.");
  }
  static void UpgradeScene(){
   if(GameObject.Find("LIVING FOREST • terrain and creatures"))throw new System.InvalidOperationException("Scene already upgraded; restore its backup before applying again.");
   meshId=0;world=new GameObject("LIVING FOREST • terrain and creatures").transform;
   fog=chapter==2?new Color(.27f,.15f,.20f):new Color(.12f,.29f,.34f);shadow=chapter==2?new Color(.10f,.065f,.115f):new Color(.035f,.105f,.14f);light=chapter==2?new Color(1,.58f,.3f):new Color(.48f,.91f,1);
   profile=world.gameObject.AddComponent<MossTerrainProfile>();
   profile.surface=new[]{new Vector2(-12,-3.3f),new Vector2(8,-3.3f),new Vector2(15,-2.6f),new Vector2(22,chapter==2?-.8f:-1.3f),new Vector2(27,-1.3f),new Vector2(34,-3.9f),new Vector2(41,-3.9f),new Vector2(48,chapter==1?-.4f:-1.2f),new Vector2(53,-1.2f),new Vector2(59,-3.3f),new Vector2(64,-3.3f),new Vector2(70,-1.3f),new Vector2(76,-1.3f),new Vector2(83,-4.2f),new Vector2(88,-4.2f),new Vector2(95,-2.2f),new Vector2(102,-.6f),new Vector2(107,-1.4f),new Vector2(112,-3.3f),new Vector2(160,-3.3f)};
   profile.gaps=chapter==0?new Vector2[0]:chapter==1?new[]{new Vector2(36,38.4f)}:new[]{new Vector2(36,39),new Vector2(88,90.6f)};
   // Deactivate only the old flat surfaces; source assets and the rest of each scene remain intact.
   foreach(var t in Object.FindObjectsByType<Transform>(FindObjectsSortMode.None)){if(t.name=="Continuous gentle trail"||t.name=="Solid trail"||t.name=="Moss trail"||t.name=="Deep earth"||t.name=="Earth depth")t.gameObject.SetActive(false);}
   var j=Object.FindAnyObjectByType<MossJourney>();
   foreach(var r in Object.FindObjectsByType<SpriteRenderer>(FindObjectsSortMode.None)){string n=r.name;float x=r.transform.position.x;if(x>=119)continue;bool place=n=="Fern"||n=="Mossy boulder"||n=="Trail fern"||n=="Moss ledge"||n=="One-way moss ledge"||n=="Hanging vine"||n=="Hanging fern"||n=="Flower stem"||n=="Memory bloom stem"||n=="Briar wisp";if(place)r.transform.position+=Vector3.up*(profile.HeightAt(x)+3.3f);}
   foreach(var s in j.seeds)s.position+=Vector3.up*(profile.HeightAt(s.position.x)+3.3f);
   foreach(var s in j.shrines)s.position+=Vector3.up*(profile.HeightAt(s.position.x)+3.3f);
   float from=-10;foreach(var gap in profile.gaps){Terrain(from,gap.x);from=gap.y;}Terrain(from,158);
   // Low stepping stones clearly mark the few mandatory jumps.
   foreach(var gap in profile.gaps){for(int side=-1;side<=1;side+=2){float x=side<0?gap.x-.5f:gap.y+.5f;Natural("Rift marker",S("Decorations&Hazards",2),new Vector2(x,profile.HeightAt(x)+.2f),.5f,12,light);}}
   var follow=Camera.main.GetComponent<MossCamera>();follow.minY=-.9f;follow.maxY=7.2f;Camera.main.orthographicSize=6.6f;
   foreach(var e in Object.FindObjectsByType<PlatformEffector2D>(FindObjectsSortMode.None)){e.surfaceArc=160;e.useOneWayGrouping=true;}
   Creatures();Landmarks();
   j.description=chapter==0?"Suối rêu, đàn dơi và những triền đất cổ. Có thứ gì đó rất lớn đang nhìn qua màn sương.":chapter==1?"Đi qua đồi rêu và khe thác. Theo đàn cá linh hồn tới nơi thủy quái cổ đang trôi giữa những thân cây.":"Vượt thung lũng gãy và những bậc đá hổ phách. Đôi mắt sau tàn tích dõi theo từng bước chân.";
  }
  [MenuItem("DOAN/Living Forest/Refine traversal")]
  public static void RefineTraversal(){
   if(EditorApplication.isPlaying||SceneManager.GetActiveScene().isDirty)throw new System.InvalidOperationException("Save and exit Play Mode first.");
   Import();string original=SceneManager.GetActiveScene().path;
   for(chapter=0;chapter<3;chapter++){
    var scene=EditorSceneManager.OpenScene("Assets/Scenes/"+MossJourney.Scenes[chapter]+".unity");
    foreach(var e in Object.FindObjectsByType<PlatformEffector2D>(FindObjectsSortMode.None)){e.surfaceArc=160;e.useOneWayGrouping=true;}
    if(chapter==2){world=GameObject.Find("LIVING FOREST • terrain and creatures").transform;profile=world.GetComponent<MossTerrainProfile>();profile.gaps=new[]{new Vector2(36,39),new Vector2(88,90.6f)};foreach(var t in world.Cast<Transform>().ToArray())if(new[]{"Sculpted terrain","Contoured moss lip","Rock strata","Underside roots","Rift marker"}.Contains(t.name))Object.DestroyImmediate(t.gameObject);meshId=0;float from=-10;foreach(var gap in profile.gaps){Terrain(from,gap.x);from=gap.y;}Terrain(from,158);}
    EditorSceneManager.SaveScene(scene);
   }AssetDatabase.SaveAssets();EditorSceneManager.OpenScene(original);
  }
  static void Terrain(float from,float to){
   var top=new List<Vector2>{new Vector2(from,profile.HeightAt(from))};foreach(var p in profile.surface)if(p.x>from&&p.x<to)top.Add(p);top.Add(new Vector2(to,profile.HeightAt(to)));
   var g=new GameObject("Sculpted terrain",typeof(MeshFilter),typeof(MeshRenderer),typeof(PolygonCollider2D));g.transform.SetParent(world,false);g.layer=6;
   var polygon=new List<Vector2>(top);polygon.Add(new Vector2(to,-14));polygon.Add(new Vector2(from,-14));g.GetComponent<PolygonCollider2D>().points=polygon.ToArray();
   var v=new List<Vector3>();var colors=new List<Color>();var indices=new List<int>();for(int i=0;i<top.Count;i++){v.Add(top[i]);v.Add(new Vector3(top[i].x,-14));colors.Add(new Color(.052f,.067f,.082f));colors.Add(new Color(.008f,.016f,.029f));if(i>0){int k=i*2;indices.AddRange(new[]{k-2,k,k-1,k,k+1,k-1});}}
   var mesh=new Mesh{name="Sculpted soil"};mesh.SetVertices(v);mesh.SetColors(colors);mesh.SetTriangles(indices,0);mesh.RecalculateBounds();g.GetComponent<MeshFilter>().sharedMesh=SaveMesh(mesh);var renderer=g.GetComponent<MeshRenderer>();renderer.sharedMaterial=shapeMat;renderer.sortingOrder=5;
   for(float x=from;x<to;){float end=Mathf.Min(x+1.25f,to);foreach(var knot in top)if(knot.x>x+.01f&&knot.x<end)end=knot.x;float mid=(x+end)/2;float y0=profile.HeightAt(x),y1=profile.HeightAt(end);float angle=Mathf.Atan2(y1-y0,end-x)*Mathf.Rad2Deg;var tile=Art("Contoured moss lip",S("TileSet",1),new Vector2(mid,(y0+y1)/2-.52f),new Vector2(Vector2.Distance(new Vector2(x,y0),new Vector2(end,y1))+.035f,1.12f),10,chapter==2?new Color(.88f,.72f,.53f):new Color(.62f,.85f,.78f));tile.transform.rotation=Quaternion.Euler(0,0,angle);x=end;}
   for(float x=from+2;x<to-1;x+=4.2f){float h=profile.HeightAt(x);Natural("Rock strata",S("Decorations&Hazards",1),new Vector2(x,h-1.2f),2.3f,7,new Color(.22f,.29f,.31f));Natural("Underside roots",S("Hanging Plants",2),new Vector2(x,h-1.7f),.7f,11,new Color(.4f,.55f,.5f));}
  }
  static void Creatures(){
   Sprite[] bats=Enumerable.Range(0,3).Select(i=>AssetDatabase.LoadAssetAtPath<Sprite>(Root+"Art/Vander96/Flight"+i+".asset")).ToArray();
   for(int i=0;i<18;i++){float x=9+i*7.4f;float y=profile.HeightAt(x)+3.8f+(i%3)*.6f;var g=Natural("Cave bat • ambient",bats[0],new Vector2(x,y),.5f+(i%3)*.12f,chapter==2?-18:-28,new Color(.24f,.34f,.42f,.9f));var w=g.AddComponent<MossWildlife>();w.species=MossWildlife.Species.Bat;w.frames=bats;w.range=2.3f;w.speed=.6f;w.phase=i*.83f;}
   foreach(float center in new[]{32f,79f,107f})for(int i=0;i<6;i++){var sprite=Imported(i%2==0?"Kenney/fish_blue_skeleton.png":"Kenney/fish_pink_skeleton.png");var g=Natural("Spirit fish • drifting school",sprite,new Vector2(center+i*.75f,1+i%3*.6f),.65f,-38,new Color(.5f,.8f,.91f,.65f));var w=g.AddComponent<MossWildlife>();w.species=MossWildlife.Species.SpiritFish;w.range=3;w.speed=.18f;w.phase=i*.15f;w.amplitude=.55f;}
   for(int i=0;i<32;i++){float x=-2+i*4.7f;var g=Art("Bioluminescent spore",glow,new Vector2(x,profile.HeightAt(x)+.7f+(i%4)*.6f),Vector2.one*.16f,18,new Color(.55f,1,.74f,.8f));var w=g.AddComponent<MossWildlife>();w.species=MossWildlife.Species.Spore;w.range=.45f;w.phase=i;w.speed=.22f;}
  }
  static void Landmarks(){
   var far=new GameObject("Mythic distance • parallax").transform;far.SetParent(world,false);far.gameObject.AddComponent<MossParallax>().factor=.12f;
   if(chapter!=1){Stag(new Vector2(chapter==0?25:33,2),far);Leviathan(new Vector2(91,4),far);}else{Leviathan(new Vector2(29,4.4f),far);Stag(new Vector2(97,2.2f),far);}
   for(int i=0;i<15;i++){float x=-5+i*12;var g=Art("Moving veil of mist",glow,new Vector2(x,1.2f+(i%3)*1.8f),new Vector2(24,5),-48,new Color(fog.r,fog.g,fog.b,.3f));var mist=g.AddComponent<MossMist>();mist.phase=i;mist.drift=2;}
   foreach(float x in new[]{19f,47f,70f,102f}){float y=profile.HeightAt(x);for(int i=0;i<4;i++){float px=x+i*.36f;Natural("Glow fungus stem",S("Decorations&Hazards",27),new Vector2(px,y+.35f),.22f,12,new Color(.5f,.68f,.65f));Art("Glow fungus cap",glow,new Vector2(px,y+.7f),new Vector2(.5f,.2f),16,light);}}
   // Tiny motes around the colossal eyes and contours establish scale.
  }
  static Transform Eye(Vector2 local,Transform parent,float size){var g=Art("Distant watchful eye",glow,(Vector2)parent.position+local,new Vector2(size,size*.5f),-55,new Color(light.r,light.g,light.b,.9f),parent);return g.transform;}
  static void Stag(Vector2 pos,Transform parent){
   var root=new GameObject("The Hollow Stag • colossal silhouette").transform;root.SetParent(parent,false);root.localPosition=pos;var rig=root.gameObject.AddComponent<MossColossus>();rig.phase=chapter*2;rig.sway=1.2f;
   Oval("Shoulders",new Vector2(0,-2),new Vector2(5.2f,3.2f),root,shadow,-61);
   Shape("Long veiled neck",new[]{new Vector2(-1.6f,-3),new Vector2(-1.2f,1),new Vector2(-.8f,2.4f),new Vector2(.6f,2.5f),new Vector2(1.1f,1),new Vector2(1.8f,-3)},root,shadow,-60);
   var head=new GameObject("Slow head rig").transform;head.SetParent(root,false);head.localPosition=new Vector3(0,1.1f);
   Shape("Ancient mask",new[]{new Vector2(-1.1f,1.5f),new Vector2(-1.45f,.55f),new Vector2(-.82f,-.7f),new Vector2(-.34f,-1.9f),new Vector2(.28f,-2.15f),new Vector2(.8f,-.9f),new Vector2(1.4f,.55f),new Vector2(1,1.5f)},head,Color.Lerp(shadow,fog,.13f),-59);
   var limbs=new List<Transform>{head};
   for(int side=-1;side<=1;side+=2){
    var horn=new GameObject("Branching antler").transform;horn.SetParent(head,false);horn.localPosition=new Vector3(side*.9f,1.1f);limbs.Add(horn);
    Branch("Antler trunk",new[]{Vector2.zero,new Vector2(side*1.1f,.65f),new Vector2(side*2,1.5f),new Vector2(side*2.3f,2.7f),new Vector2(side*2.9f,3.4f)},horn,.35f,shadow,-60);
    Branch("Antler crown",new[]{new Vector2(side*1.7f,1.25f),new Vector2(side*3.1f,1.8f),new Vector2(side*4.4f,2),new Vector2(side*5.2f,2.8f)},horn,.23f,shadow,-60);
    for(int i=0;i<3;i++)Branch("Antler tine",new[]{new Vector2(side*(1.2f+i*.8f),.9f+i*.42f),new Vector2(side*(1.1f+i*.8f),2.1f+i*.35f),new Vector2(side*(.8f+i*.8f),2.9f+i*.25f)},horn,.15f,shadow,-60);
    Branch("Ancient foreleg",new[]{new Vector2(side*3,-2.3f),new Vector2(side*3.5f,-4),new Vector2(side*3.1f,-6.5f)},root,.65f,shadow,-61);
   }
   rig.limbs=limbs.ToArray();rig.eyes=new[]{Eye(new Vector2(-.62f,.15f),head,.47f),Eye(new Vector2(.62f,.15f),head,.47f)};
  }
  static void Leviathan(Vector2 pos,Transform parent){
   var root=new GameObject("The Sky Leviathan • colossal silhouette").transform;root.SetParent(parent,false);root.localPosition=pos;var rig=root.gameObject.AddComponent<MossColossus>();rig.leviathan=true;rig.phase=1.7f;rig.sway=2.5f;
   var parts=new List<Transform>();for(int i=0;i<9;i++){var segment=new GameObject("Undulating body "+i).transform;segment.SetParent(root,false);segment.localPosition=new Vector3(-i*1.5f,Mathf.Sin(i*.5f)*.4f);float radius=Mathf.Lerp(1.9f,.35f,i/8f);Oval("Ancient armored segment",Vector2.zero,new Vector2(1.65f,radius),segment,shadow,-61);Shape("Dorsal fin",new[]{new Vector2(-.9f,radius*.5f),new Vector2(-1.7f,radius+1.6f),new Vector2(.7f,radius*.5f)},segment,shadow,-62);parts.Add(segment);}
   var head=parts[0];Shape("Leviathan jaw",new[]{new Vector2(-.6f,1.2f),new Vector2(1.7f,1),new Vector2(3.4f,.15f),new Vector2(2.4f,-.75f),new Vector2(.8f,-1.4f),new Vector2(-.9f,-1)},head,Color.Lerp(shadow,fog,.10f),-60);
   for(int i=0;i<4;i++)parts.Add(Branch("Trailing whisker",new[]{new Vector2(1.8f,-.5f-i*.1f),new Vector2(1.5f-i*.4f,-2.4f),new Vector2(-1-i,-3.4f),new Vector2(-2.5f-i,-4.1f)},head,.12f,shadow,-61));rig.limbs=parts.ToArray();rig.eyes=new[]{Eye(new Vector2(1.7f,.35f),head,.55f)};
  }
 }
}


