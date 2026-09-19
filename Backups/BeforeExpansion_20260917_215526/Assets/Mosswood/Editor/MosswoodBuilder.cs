using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Linq;
using System.Collections.Generic;
namespace Mosswood.Editor {
 public static class MosswoodBuilder {
  const string Root="Assets/Mosswood/";
  static Dictionary<string,Material> textureMaterials = new Dictionary<string,Material>();
  static Transform world; static Material ground, middle, far, plain, depth; static Sprite glow, square; static System.Random random;
  static float R(float a,float b)=>Mathf.Lerp(a,b,(float)random.NextDouble());
  static Sprite Sprite(string sheet,int index)=>AssetDatabase.LoadAllAssetsAtPath("Assets/Game/Map/Mossy - "+sheet+".png").OfType<Sprite>().First(s=>s.name=="Mossy - "+sheet+"_"+index);
  static Material Mat(string name,Color tint,Color fog,float mist){var m=new Material(Shader.Find("Mosswood/AtmosphereSprite"));m.SetColor("_Tint",tint);m.SetColor("_Fog",fog);m.SetFloat("_Mist",mist);AssetDatabase.CreateAsset(m,Root+"Art/"+name+".mat");return m;}
  static GameObject Art(string name,Sprite sprite,Vector2 position,Vector2 scale,int order,Material mat,Transform parent=null){var g=new GameObject(name);g.transform.SetParent(parent?parent:world);g.transform.position=position;g.transform.localScale=scale;var r=g.AddComponent<SpriteRenderer>();r.sprite=sprite;r.sortingOrder=order;r.sharedMaterial=TextureMaterial(mat,sprite);return g;}
  static Material TextureMaterial(Material mat,Sprite sprite){string key=mat.name+"-"+sprite.texture.name;if(!textureMaterials.TryGetValue(key,out var m)){m=new Material(mat);m.SetTexture("_MainTex",sprite.texture);AssetDatabase.CreateAsset(m,Root+"Art/"+key+".mat");textureMaterials[key]=m;}return m;}
  static Sprite Texture(string name,int size,System.Func<int,int,Color> pixel){var t=new Texture2D(size,size,TextureFormat.RGBA32,false);var c=new Color[size*size];for(int y=0;y<size;y++)for(int x=0;x<size;x++)c[y*size+x]=pixel(x,y);t.SetPixels(c);t.Apply();string p=Root+"Art/"+name+".png";System.IO.File.WriteAllBytes(p,t.EncodeToPNG());Object.DestroyImmediate(t);AssetDatabase.ImportAsset(p);var imp=(TextureImporter)AssetImporter.GetAtPath(p);imp.textureType=TextureImporterType.Sprite;imp.spritePixelsPerUnit=size;imp.alphaIsTransparency=true;imp.mipmapEnabled=false;imp.SaveAndReimport();return AssetDatabase.LoadAssetAtPath<Sprite>(p);}
  static GameObject Platform(float x,float top,float width,int variant=2){var s=Sprite("FloatingPlatforms",variant);float scale=width/s.bounds.size.x;float height=s.bounds.size.y*scale;var g=Art("Moss ledge",s,new Vector2(x,top-height*.5f+.18f),Vector2.one*scale,10,ground);g.layer=6;var b=g.AddComponent<BoxCollider2D>();b.size=new Vector2((width-.35f)/scale,(height-.5f)/scale);b.offset=new Vector2(0,-.07f/scale);return g;}
  static Text Label(Transform parent,string name,string text,int size,Vector2 anchor,Vector2 position,Vector2 dimensions,TextAnchor align){var g=new GameObject(name,typeof(RectTransform));g.transform.SetParent(parent,false);var rt=g.GetComponent<RectTransform>();rt.anchorMin=rt.anchorMax=anchor;rt.pivot=anchor;rt.anchoredPosition=position;rt.sizeDelta=dimensions;var t=g.AddComponent<Text>();t.font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");t.text=text;t.fontSize=size;t.color=new Color(.85f,.96f,.92f);t.alignment=align;t.raycastTarget=false;return t;}
  static Button Button(Transform parent,string text,Vector2 position,out Text label){var g=new GameObject(text,typeof(RectTransform),typeof(Image),typeof(Button));g.transform.SetParent(parent,false);var rt=g.GetComponent<RectTransform>();rt.anchorMin=rt.anchorMax=rt.pivot=new Vector2(.5f,.5f);rt.anchoredPosition=position;rt.sizeDelta=new Vector2(360,48);g.GetComponent<Image>().color=new Color(.13f,.37f,.36f,.95f);label=Label(g.transform,"Label",text,17,new Vector2(.5f,.5f),Vector2.zero,new Vector2(350,46),TextAnchor.MiddleCenter);return g.GetComponent<Button>();}
  [MenuItem("DOAN/Build Mosswood Adventure")]
  public static void Build(){
   if(EditorApplication.isPlaying)throw new System.InvalidOperationException("Exit Play Mode before rebuilding.");
   var scene=SceneManager.GetActiveScene();if(scene.isDirty)throw new System.InvalidOperationException("Save scene changes before rebuilding.");
   random=new System.Random(1709);textureMaterials.Clear();
   // The original scene is backed up outside Assets before this builder is run.
   foreach(var root in scene.GetRootGameObjects())if(root.name!="Player"&&root.name!="Main Camera")Object.DestroyImmediate(root);
   world=new GameObject("MOSSWOOD · Handcrafted forest").transform;
   ground=Mat("Moss foreground",new Color(.76f,1.08f,1.06f),new Color(.012f,.023f,.085f),0);
   middle=Mat("Moss middle",new Color(.35f,1.2f,1.2f),new Color(.02f,.29f,.33f),.35f);
   far=Mat("Moss distant",new Color(.2f,1.1f,1.2f),new Color(.045f,.29f,.34f),.48f);
   plain=Mat("Luminous",Color.white,Color.black,0);depth=Mat("Forest depth",new Color(.012f,.023f,.085f),Color.black,0);
   glow=Texture("Soft light",128,(x,y)=>{float d=Vector2.Distance(new Vector2(x,y),new Vector2(63.5f,63.5f))/64;return new Color(.62f,1,.91f,Mathf.Pow(Mathf.Clamp01(1-d),3));});
   square=Texture("Solid",4,(x,y)=>Color.white);
   var bg=Texture("Forest haze",256,(x,y)=>{float u=x/255f,v=y/255f;float beam=Mathf.Pow(.5f+.5f*Mathf.Sin(u*19+v*3),6);return Color.Lerp(new Color(.008f,.10f,.19f),new Color(.11f,.52f,.54f),Mathf.Clamp01(.25f+v*.48f+beam*.18f));});
   Art("Deep turquoise atmosphere",bg,new Vector2(72,4),new Vector2(210,50),-100,plain);
   var farLayer=new GameObject("01 · Distant silhouettes").transform;farLayer.SetParent(world);farLayer.gameObject.AddComponent<MossParallax>().factor=.16f;
   var midLayer=new GameObject("02 · Ferns and ancient trunks").transform;midLayer.SetParent(world);midLayer.gameObject.AddComponent<MossParallax>().factor=.07f;
   for(int i=0;i<25;i++){float x=-32+i*9;Art("Distant moss pillar",Sprite("BackgroundDecoration",i%2),new Vector2(x,R(2,10)),Vector2.one*R(.30f,.60f),-80,far,farLayer);}
   for(int i=0;i<33;i++){float x=-18+i*6;int index=new[]{0,1,2,3,4,5}[i%6];Art("Layered moss",Sprite("BackgroundDecoration",index),new Vector2(x,R(-5,0)),Vector2.one*R(.23f,.45f),-55,middle,midLayer);if(i%3==0)Art("Twisting trunk",Sprite("Decorations&Hazards",18+i%4),new Vector2(x,3),Vector2.one*R(.5f,.85f),-48,middle,midLayer);}
   for(int i=0;i<31;i++){float x=-12+i*5.6f;var s=Sprite("Decorations&Hazards",i%3);float k=R(.28f,.48f);Art("Mossy boulder",s,new Vector2(x,-2.9f+s.bounds.size.y*k*.5f),Vector2.one*k,-25,middle);}
   for(int i=0;i<180;i++){float x=R(-15,162);var s=Sprite("Decorations&Hazards",new[]{9,22,23,25,26,28}[i%6]);float k=R(.22f,.48f);Art("Fern",s,new Vector2(x,-3+s.bounds.size.y*k*.5f),Vector2.one*k,-15,i%3==0?far:middle);}
   // Continuous accessible lower trail. Upper ledges are optional exploration.
   for(int i=0;i<66;i++){float x=-19+i*2.8f;var tile=Sprite("TileSet",1);Art("Moss trail",tile,new Vector2(x,-4.4f),Vector2.one*(2.8f/tile.bounds.size.x),10,ground);} var trail=new GameObject("Continuous gentle trail");trail.transform.SetParent(world);trail.transform.position=new Vector3(72,-5,0);trail.layer=6;trail.AddComponent<BoxCollider2D>().size=new Vector2(185,3.4f);Art("Deep earth",square,new Vector2(72,-10),new Vector2(186,10),5,plain).GetComponent<SpriteRenderer>().sharedMaterial=TextureMaterial(depth,square);
   var ledges=new[]{new Vector3(13,-1.35f,5.5f),new Vector3(20,.35f,5.5f),new Vector3(28,1.95f,7),new Vector3(37,.3f,6),new Vector3(47,-1.25f,6),new Vector3(69,-1.3f,6),new Vector3(77,.4f,6),new Vector3(85,2,7),new Vector3(94,.3f,6),new Vector3(109,-1.3f,6),new Vector3(117,.3f,6),new Vector3(125,1.9f,6),new Vector3(133,.2f,6)};
   foreach(var l in ledges){var ledge=Platform(l.x,l.y,l.z);var lc=ledge.GetComponent<BoxCollider2D>();lc.size=new Vector2(lc.size.x,.25f/ledge.transform.localScale.y);lc.offset=new Vector2(0,(l.y-.28f-ledge.transform.position.y)/ledge.transform.localScale.y);lc.usedByEffector=true;ledge.AddComponent<PlatformEffector2D>().useOneWay=true;var s=Sprite("Hanging Plants",random.Next(0,5));Art("Hanging vine",s,new Vector2(l.x+1,l.y-2.6f),Vector2.one*.27f,8,ground);}
   for(int i=0;i<17;i++){float x=-8+i*10;var g=Art("Overhanging canopy",Sprite("FloatingPlatforms",9),new Vector2(x,8.8f+R(-.8f,.8f)),new Vector2(.7f,-.9f),25,ground);Art("Canopy vines",Sprite("Hanging Plants",i%7),new Vector2(x+2,6.8f),Vector2.one*.3f,24,ground);}
   Art("Canopy depth",square,new Vector2(72,15),new Vector2(186,12),26,plain).GetComponent<SpriteRenderer>().sharedMaterial=TextureMaterial(depth,square);
   for(int i=0;i<25;i++){var g=Art("Drifting light",glow,new Vector2(R(-10,156),R(-1,10)),Vector2.one*R(.2f,.55f),-12,plain);var f=g.AddComponent<MossFloat>();f.phase=R(0,6);f.rate=R(.3f,.8f);f.amplitude=.6f;}
   for(int i=0;i<24;i++)Art("Soft pool of light",glow,new Vector2(-10+i*7,R(0,9)),Vector2.one*R(8,15),-65,plain).GetComponent<SpriteRenderer>().color=new Color(.4f,.9f,.85f,.4f);
   var player=GameObject.Find("Player");GameObjectUtility.RemoveMonoBehavioursWithMissingScript(player);
   
   player.transform.position=new Vector3(0,-1.9f,0);player.transform.localScale=Vector3.one*.6f;
   var traveler=player.GetComponent<MossTraveler>()??player.AddComponent<MossTraveler>();
   var box=player.GetComponent<BoxCollider2D>();box.size=new Vector2(1.15f,2.65f);box.offset=new Vector2(0,-.04f);
   var rb=player.GetComponent<Rigidbody2D>();rb.gravityScale=3;rb.constraints=RigidbodyConstraints2D.FreezeRotation;rb.collisionDetectionMode=CollisionDetectionMode2D.Continuous;rb.interpolation=RigidbodyInterpolation2D.Interpolate;
   var physics=new PhysicsMaterial2D("Gentle traveler"){friction=0,bounciness=0};AssetDatabase.CreateAsset(physics,Root+"Art/Traveler.physicsMaterial2D");box.sharedMaterial=physics;
   var pr=player.GetComponent<SpriteRenderer>();pr.sortingLayerName="Default";pr.sortingOrder=20;pr.sharedMaterial=new Material(Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit-Default"));AssetDatabase.CreateAsset(pr.sharedMaterial,Root+"Art/Traveler.mat");
   var cam=Camera.main;foreach(var c in cam.GetComponents<Component>())if(c.GetType().Namespace=="Unity.Cinemachine")Object.DestroyImmediate(c);
   cam.orthographic=true;cam.orthographicSize=7.5f;cam.backgroundColor=new Color(.01f,.12f,.2f);cam.transform.position=new Vector3(4,1.3f,-10);var follow=cam.GetComponent<MossCamera>()??cam.gameObject.AddComponent<MossCamera>();follow.target=player.transform;
   var manager=new GameObject("Journey · checkpoints, fireflies and sound").AddComponent<MossJourney>();manager.traveler=traveler;
   var seeds=new List<Transform>();for(int i=0;i<ledges.Length;i++){var p=ledges[i];var g=Art("Firefly "+(i+1),glow,new Vector2(p.x,p.y+1),Vector2.one*.8f,30,plain);var core=Art("Light core",glow,g.transform.position,Vector2.one*.23f,31,plain,g.transform);core.transform.localScale=Vector3.one*.3f;g.AddComponent<MossFloat>().phase=i;seeds.Add(g.transform);}manager.seeds=seeds.ToArray();
   var shrines=new List<Transform>();foreach(float x in new[]{5f,59f,138f}){var stem=Sprite("Decorations&Hazards",27);Art("Flower stem",stem,new Vector2(x,-1.8f),Vector2.one*.55f,11,middle);var g=Art("Memory flower",glow,new Vector2(x,-.65f),Vector2.one*2.2f,32,plain);Art("Flower heart",glow,new Vector2(x,-.65f),Vector2.one*.6f,33,plain,g.transform);shrines.Add(g.transform);}manager.shrines=shrines.ToArray();
   for(int i=0;i<5;i++)Art("Memory tree",Sprite("Decorations&Hazards",18+i%4),new Vector2(150+(i-2)*.65f,.5f),Vector2.one*(.6f+i*.04f),-10,middle);
   Art("Home light",glow,new Vector2(150,3.8f),Vector2.one*9,0,plain);
   foreach(float x in new[]{-8f,158f}){var wall=new GameObject("Forest boundary");wall.transform.SetParent(world);wall.transform.position=new Vector3(x,3,0);wall.layer=6;wall.AddComponent<BoxCollider2D>().size=new Vector2(1,30);}
   var canvas=new GameObject("Mosswood UI",typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster));canvas.GetComponent<Canvas>().renderMode=RenderMode.ScreenSpaceOverlay;var scaler=canvas.GetComponent<CanvasScaler>();scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;scaler.referenceResolution=new Vector2(1600,900);scaler.matchWidthOrHeight=.5f;
   manager.area=Label(canvas.transform,"Area","01  /  LỐI VÀO MIỀN RÊU",19,new Vector2(0,1),new Vector2(42,-35),new Vector2(560,40),TextAnchor.MiddleLeft);
   manager.progress=Label(canvas.transform,"Progress","",17,new Vector2(1,1),new Vector2(-40,-35),new Vector2(650,40),TextAnchor.MiddleRight);
   manager.hint=Label(canvas.transform,"Hint","",18,new Vector2(.5f,0),new Vector2(0,26),new Vector2(1440,50),TextAnchor.MiddleCenter);
   var panel=new GameObject("Rest overlay",typeof(RectTransform),typeof(Image));panel.transform.SetParent(canvas.transform,false);var rect=panel.GetComponent<RectTransform>();rect.anchorMin=Vector2.zero;rect.anchorMax=Vector2.one;rect.offsetMin=rect.offsetMax=Vector2.zero;panel.GetComponent<Image>().color=new Color(.012f,.045f,.075f,.87f);manager.panel=panel;
   Label(panel.transform,"Eyebrow","D O A N    /    M O S S W O O D",17,new Vector2(.5f,.5f),new Vector2(0,240),new Vector2(800,40),TextAnchor.MiddleCenter);
   manager.panelTitle=Label(panel.transform,"Title","RỪNG THÌ THẦM",48,new Vector2(.5f,.5f),new Vector2(0,165),new Vector2(1100,80),TextAnchor.MiddleCenter);
   manager.panelBody=Label(panel.transform,"Story","",22,new Vector2(.5f,.5f),new Vector2(0,25),new Vector2(1000,180),TextAnchor.MiddleCenter);
   manager.actionButton=Button(panel.transform,"BƯỚC VÀO KHU RỪNG",new Vector2(0,-120),out manager.actionText);
   Text restartLabel;manager.restartButton=Button(panel.transform,"HÀNH TRÌNH MỚI · XÓA TIẾN ĐỘ",new Vector2(0,-184),out restartLabel);
   manager.soundButton=Button(panel.transform,"ÂM THANH: BẬT",new Vector2(0,-248),out manager.soundText);
   new GameObject("EventSystem",typeof(EventSystem),typeof(StandaloneInputModule));
   EditorSceneManager.SaveScene(scene,"Assets/Scenes/SampleScene.unity");EditorBuildSettings.scenes=new[]{new EditorBuildSettingsScene("Assets/Scenes/SampleScene.unity",true)};
   AssetDatabase.SaveAssets();Selection.activeGameObject=world.gameObject;SceneView.lastActiveSceneView?.LookAt(new Vector3(4,1.3f,0),Quaternion.identity,15);Debug.Log("MOSSWOOD BUILD COMPLETE: 3 memory flowers, "+seeds.Count+" fireflies, accessible forest trail.");
  }
 }
}
