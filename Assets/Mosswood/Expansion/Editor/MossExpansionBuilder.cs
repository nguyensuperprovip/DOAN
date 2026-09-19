using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using UnityEditor.Animations;
using System.Linq;
using System.Collections.Generic;
namespace Mosswood.Editor {
 public static class MossExpansionBuilder {
  const string Root="Assets/Mosswood/Expansion/";static Material material;static Sprite glow,square;static Transform world;static Color accent;static System.Random random;
  static float R(float a,float b)=>Mathf.Lerp(a,b,(float)random.NextDouble());
  static Sprite S(string sheet,int n)=>AssetDatabase.LoadAllAssetsAtPath("Assets/Game/Map/Mossy - "+sheet+".png").OfType<Sprite>().First(s=>s.name=="Mossy - "+sheet+"_"+n);
  static GameObject Art(string name,Sprite sprite,Vector2 p,Vector2 size,int order,Color color,Transform parent=null){var g=new GameObject(name);g.transform.SetParent(parent?parent:world);g.transform.position=p;g.transform.localScale=new Vector3(size.x/sprite.bounds.size.x,size.y/sprite.bounds.size.y,1);var r=g.AddComponent<SpriteRenderer>();r.sprite=sprite;r.sharedMaterial=material;r.color=color;r.sortingOrder=order;return g;}
  static GameObject Natural(string name,Sprite sprite,Vector2 p,float width,int order,Color color,Transform parent=null)=>Art(name,sprite,p,new Vector2(width,width*sprite.bounds.size.y/sprite.bounds.size.x),order,color,parent);
  static void Ledge(float x,float top,float width,List<Transform> seeds,bool seed=true){var g=Natural("One-way moss ledge",S("FloatingPlatforms",2),new Vector2(x,top-.55f),width,10,Color.white);g.layer=6;var b=g.AddComponent<BoxCollider2D>();b.size=new Vector2((width-.25f)/g.transform.localScale.x,.2f/g.transform.localScale.y);b.offset=new Vector2(0,.45f/g.transform.localScale.y);b.usedByEffector=true;g.AddComponent<PlatformEffector2D>().useOneWay=true;Natural("Hanging fern",S("Hanging Plants",2),new Vector2(x+.6f,top-1.75f),1.1f,8,accent);if(seed){var f=Art("Firefly "+(seeds.Count+1),glow,new Vector2(x,top+1.1f),Vector2.one*.9f,30,new Color(1,1,.65f));f.AddComponent<MossFloat>().phase=seeds.Count;seeds.Add(f.transform);}}
  static void Ground(float from,float to,int chapter){var g=new GameObject("Solid trail");g.transform.SetParent(world);g.transform.position=new Vector3((from+to)/2,-5,0);g.layer=6;g.AddComponent<BoxCollider2D>().size=new Vector2(to-from,3.4f);Art("Earth depth",square,new Vector2((from+to)/2,-8),new Vector2(to-from,9.4f),4,new Color(.025f,.04f,.065f));for(float x=from;x<to;x+=2.5f)Art("Moss trail",S("TileSet",1),new Vector2(x+1.25f,-4.35f),new Vector2(Mathf.Min(2.52f,to-x),2.5f),10,chapter==2?new Color(1,.8f,.55f):new Color(.8f,1,1));}
  static void LoadArt(){glow=AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Mosswood/Art/Soft light.png");square=AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Mosswood/Art/Solid.png");material=AssetDatabase.LoadAssetAtPath<Material>(Root+"SpiritSprite.mat");if(!material){material=new Material(Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit-Default"));AssetDatabase.CreateAsset(material,Root+"SpiritSprite.mat");}}
  [MenuItem("DOAN/Expansion/Build Campaign")]
  public static void Build(){
   if(EditorApplication.isPlaying||SceneManager.GetActiveScene().isDirty)throw new System.InvalidOperationException("Exit Play Mode and save your scene first.");LoadArt();
   EditorSceneManager.OpenScene("Assets/Scenes/SampleScene.unity");var player=GameObject.Find("Player");
   // Dedicated controller leaves the source animation assets intact; movement explicitly chooses states.
   var controller=AssetDatabase.LoadAssetAtPath<AnimatorController>(Root+"Traveler.controller");if(!controller){controller=AnimatorController.CreateAnimatorControllerAtPath(Root+"Traveler.controller");foreach(var pair in new[]{new[]{"playeridle","playeridle"},new[]{"playerrun","playerrun"},new[]{"playerjump","playerjump"},new[]{"Playerdash","Playerdash"}}){var state=controller.layers[0].stateMachine.AddState(pair[0]);state.motion=AssetDatabase.LoadAssetAtPath<AnimationClip>("Assets/Game/Animation/"+pair[1]+".anim");}}
   player.GetComponent<Animator>().runtimeAnimatorController=controller;
   var combat=player.GetComponent<MossCombat>()??player.AddComponent<MossCombat>();combat.lightSprite=glow;combat.lightMaterial=material;
   PrefabUtility.SaveAsPrefabAsset(player,Root+"Traveler.prefab");
   UpgradeIntro();CreateMap(1);CreateMap(2);
   EditorBuildSettings.scenes=MossJourney.Scenes.Select(s=>new EditorBuildSettingsScene("Assets/Scenes/"+s+".unity",true)).ToArray();AssetDatabase.SaveAssets();EditorSceneManager.OpenScene("Assets/Scenes/SampleScene.unity");Debug.Log("Campaign ready: prologue + Moonfall + Amber Ruins.");
  }
  static void Interface(MossJourney journey){var canvas=new GameObject("Mosswood • Campaign interface",typeof(RectTransform));var ui=canvas.AddComponent<MossInterface>();ui.journey=journey;ui.glow=glow;journey.ui=ui;journey.traveler.GetComponent<MossCombat>().journey=journey;}
  static void UpgradeIntro(){var j=Object.FindAnyObjectByType<MossJourney>();j.chapter=0;j.chapterTitle="RỪNG THÌ THẦM";j.description="Gom đom đóm trên những lối cao. Đánh thức đủ ba hoa ký ức để bước qua cánh cổng cuối rừng.";var old=GameObject.Find("Mosswood UI");if(old)Object.DestroyImmediate(old);var existing=Object.FindAnyObjectByType<MossInterface>();if(existing)Object.DestroyImmediate(existing.gameObject);Interface(j);
   // Use the render pipeline's sprite shader to avoid material/texture batching artifacts.
   foreach(var r in Object.FindObjectsByType<SpriteRenderer>(FindObjectsSortMode.None)){if(r.gameObject==j.traveler.gameObject)continue;Color tint=r.sharedMaterial&&r.sharedMaterial.HasProperty("_Tint")?r.sharedMaterial.GetColor("_Tint"):Color.white;r.sharedMaterial=material;r.color*=tint;if(r.sortingOrder< -30)r.color=new Color(.3f,.62f,.62f,.8f);}
   Camera.main.orthographicSize=6.3f;ApplyAmbient();EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
  }
  static void CreateMap(int chapter){
   var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);random=new System.Random(703+chapter);accent=chapter==1?new Color(.4f,.85f,1):new Color(1,.68f,.3f);world=new GameObject(chapter==1?"MAP 01 • Moonfall terraces":"MAP 02 • Amber ruins").transform;
   var cam=new GameObject("Main Camera",typeof(Camera),typeof(AudioListener));cam.tag="MainCamera";var camera=cam.GetComponent<Camera>();camera.orthographic=true;camera.orthographicSize=6.3f;camera.backgroundColor=chapter==1?new Color(.025f,.07f,.15f):new Color(.12f,.075f,.10f);cam.transform.position=new Vector3(4,1.3f,-10);
   var player=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(Root+"Traveler.prefab"));player.name="Player";player.transform.position=new Vector3(0,-1.9f,0);var traveler=player.GetComponent<MossTraveler>();var follow=cam.AddComponent<MossCamera>();follow.target=player.transform;
   var j=new GameObject("Journey • "+chapter).AddComponent<MossJourney>();j.chapter=chapter;j.chapterTitle=chapter==1?"THÁC NGUYỆT QUANG":"TÀN TÍCH HỔ PHÁCH";j.description=chapter==1?"Đi theo bậc đá bên thác. Đánh thức ba hoa ký ức và giải thoát Người Giữ Rễ ở cuối thung lũng.":"Vượt những cầu đá đổ, tìm ba hoa ký ức và đánh thức Vệ Binh Hổ Phách khỏi lời nguyền.";j.traveler=traveler;j.finishX=146;
   // Broad soft lights create depth without covering gameplay silhouettes.
   for(int i=0;i<22;i++){Art("Atmospheric glow",glow,new Vector2(-10+i*8,R(1,8)),new Vector2(20,23),-100,new Color(accent.r*.35f,accent.g*.4f,accent.b*.55f,.55f));}
   var far=new GameObject("Distant forest • parallax").transform;far.SetParent(world);far.gameObject.AddComponent<MossParallax>().factor=.13f;
   for(int i=0;i<33;i++){float x=-20+i*6;Natural("Ancient tree",S("Decorations&Hazards",18+i%4),new Vector2(x,3),R(2,3.5f),-70,new Color(accent.r*.32f,accent.g*.35f,accent.b*.4f,.8f),far);Natural("Distant moss hill",S("BackgroundDecoration",3),new Vector2(x,-3),R(8,12),-75,new Color(accent.r*.22f,accent.g*.3f,accent.b*.35f),far);}
   Ground(-10,chapter==2?32:158,chapter);if(chapter==2){Ground(35,73,chapter);Ground(76,99,chapter);Ground(102,158,chapter);}
   var seeds=new List<Transform>();float[] heights=chapter==1?new[]{-1.4f,.1f,1.6f,3.0f,1.6f,.1f,-1.4f,.1f,1.6f,3f,1.6f,.1f,-1.4f,.1f,1.6f,.1f}:new[]{-1.4f,.1f,1.6f,.1f,-1.4f,.1f,1.6f,3f,1.6f,.1f,-1.4f,.1f,1.6f,3f,1.6f,.1f};
   for(int i=0;i<heights.Length;i++)Ledge(12+i*6.3f,heights[i],5,seeds);j.seeds=seeds.ToArray();
   var shrines=new List<Transform>();foreach(float x in new[]{5f,60f,112f}){Natural("Memory bloom stem",S("Decorations&Hazards",27),new Vector2(x,-2),1,12,accent);var f=Art("Memory flower",glow,new Vector2(x,-.65f),Vector2.one*2.1f,33,accent);Art("Flower heart",glow,new Vector2(x,-.65f),Vector2.one*.7f,34,Color.white,f.transform);shrines.Add(f.transform);}j.shrines=shrines.ToArray();
   for(int i=0;i<100;i++){float x=R(-8,157);Natural("Trail fern",S("Decorations&Hazards",new[]{9,22,23,25,26,28}[i%6]),new Vector2(x,-2.7f),R(.45f,1.2f),i%4==0?13:-5,Color.Lerp(accent,Color.white,.45f));}
   for(int i=0;i<18;i++){Natural("Canopy vine",S("Hanging Plants",i%7),new Vector2(i*9,7.4f),R(1,2),-8,accent);var f=Art("Drifting firefly",glow,new Vector2(R(-5,154),R(-1,6)),Vector2.one*R(.12f,.35f),5,accent);var motion=f.AddComponent<MossFloat>();motion.rate=R(.5f,1);motion.phase=i;motion.amplitude=.4f;}
   if(chapter==1){foreach(float x in new[]{29f,66f,102f}){for(int i=0;i<6;i++)Art("Moonlit waterfall",glow,new Vector2(x+i*.35f,2),new Vector2(.8f,20),-40,new Color(.28f,.7f,1,.55f));Art("Waterfall mist",glow,new Vector2(x+1,-2),new Vector2(10,2),-12,new Color(.55f,.85f,1,.7f));}}
   else {foreach(float x in new[]{25f,65f,99f,130f}){Natural("Ruined arch",S("Decorations&Hazards",5),new Vector2(x,2),9,-30,new Color(.62f,.5f,.33f));for(int side=-1;side<=1;side+=2)Natural("Ancient pillar",S("Decorations&Hazards",18),new Vector2(x+side*4,0),1.7f,-25,new Color(.6f,.47f,.3f));Art("Amber sanctuary",glow,new Vector2(x,3),new Vector2(8,12),-35,new Color(1,.5f,.15f,.5f));}}
   foreach(float x in new[]{24f,44f,72f,91f}){var w=Natural("Briar wisp",S("Decorations&Hazards",6),new Vector2(x,-.8f),1.1f,22,chapter==1?new Color(.8f,.6f,1):new Color(1,.55f,.3f));w.AddComponent<CircleCollider2D>().isTrigger=true;var enemy=w.AddComponent<MossWisp>();enemy.journey=j;Art("Wisp heart",glow,w.transform.position,Vector2.one*.55f,23,accent,w.transform);}
   MakeGuardian(j,chapter);
   for(int side=-1;side<=1;side+=2){var p=Natural("Sanctuary gate",S("Decorations&Hazards",18),new Vector2(150+side*1.6f,-.2f),1,14,accent);}Art("Portal light",glow,new Vector2(150,-.2f),new Vector2(3,6),13,accent);
   foreach(float x in new[]{-8f,158f}){var wall=new GameObject("Map boundary");wall.transform.SetParent(world);wall.transform.position=new Vector3(x,2,0);wall.layer=6;wall.AddComponent<BoxCollider2D>().size=new Vector2(1,30);}
   Interface(j);new GameObject("EventSystem",typeof(EventSystem),typeof(StandaloneInputModule));ApplyAmbient();EditorSceneManager.SaveScene(scene,"Assets/Scenes/"+MossJourney.Scenes[chapter]+".unity");
  }
  public static void ApplyAmbient(){foreach(var r in Object.FindObjectsByType<SpriteRenderer>(FindObjectsSortMode.None)){if(r.name.Contains("vine")||r.name.Contains("fern")||r.name.Contains("waterfall")){var m=r.GetComponent<MossAmbientMotion>()??r.gameObject.AddComponent<MossAmbientMotion>();m.waterfall=r.name.Contains("waterfall");m.phase=r.transform.position.x;m.angle=2;m.rate=.8f;}}}
  static void MakeGuardian(MossJourney j,int chapter){var root=new GameObject(chapter==1?"Người Giữ Rễ":"Vệ Binh Hổ Phách");root.transform.SetParent(world);root.transform.position=new Vector3(135,-1.1f,0);var boss=root.AddComponent<MossGuardian>();boss.journey=j;boss.guardianName=chapter==1?"NGƯỜI GIỮ RỄ":"VỆ BINH HỔ PHÁCH";boss.maxHealth=chapter==1?24:34;j.guardian=boss;
   var body=new GameObject("Animated guardian body").transform;body.SetParent(root.transform,false);boss.body=body;var p=(Vector2)root.transform.position;
   Natural("Stone mantle",S("Decorations&Hazards",0),p+new Vector2(0,-.2f),4.5f,23,accent,body);Natural("Crowned head",S("Decorations&Hazards",2),p+new Vector2(0,1.55f),2.4f,24,Color.Lerp(accent,Color.white,.4f),body);
   boss.leftArm=Natural("Left root arm",S("Decorations&Hazards",20),p+new Vector2(-2,.3f),1.1f,22,accent,body).transform;boss.rightArm=Natural("Right root arm",S("Decorations&Hazards",19),p+new Vector2(2,.3f),1.1f,22,accent,body).transform;
   boss.core=Art("Luminous core",glow,p+new Vector2(0,.3f),Vector2.one*1.7f,28,Color.white,body).transform;
   for(int side=-1;side<=1;side+=2)Art("Guardian eye",glow,p+new Vector2(side*.4f,1.7f),new Vector2(.36f,.16f),29,new Color(1,.92f,.55f),body);
   var box=root.AddComponent<BoxCollider2D>();box.isTrigger=true;box.size=new Vector2(3.7f,4.5f);box.offset=new Vector2(0,.3f);boss.hitbox=box;var warning=Art("Attack warning",square,new Vector2(130,-3),new Vector2(5,.13f),35,new Color(1,.28f,.15f,.85f));boss.warning=warning.GetComponent<SpriteRenderer>();boss.warning.enabled=false;
   Ledge(124,-1.1f,4,new List<Transform>(),false);Ledge(142,-1.1f,4,new List<Transform>(),false);
  }
 }
}

