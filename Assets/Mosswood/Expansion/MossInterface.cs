using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
namespace Mosswood {
 public class MossInterface : MonoBehaviour {
  public MossJourney journey;public Sprite glow;Text health,bossLabel,settingsText;Image bossFill;GameObject bossPanel;Button next;Text restartLabel;bool confirming;CanvasGroup menuGroup;
  static Color ink=new Color(.025f,.065f,.095f,.96f),mint=new Color(.64f,.96f,.84f),gold=new Color(.98f,.79f,.43f);
  public static RectTransform Rect(Transform parent,string name,Vector2 position,Vector2 size,Vector2 anchor){var g=new GameObject(name,typeof(RectTransform));g.transform.SetParent(parent,false);var r=g.GetComponent<RectTransform>();r.anchorMin=r.anchorMax=anchor;r.pivot=new Vector2(.5f,.5f);r.anchoredPosition=position;r.sizeDelta=size;return r;}
  public static Text Label(Transform parent,string name,string value,int size,Vector2 pos,Vector2 bounds,TextAnchor align=TextAnchor.MiddleLeft){var r=Rect(parent,name,pos,bounds,new Vector2(.5f,.5f));var t=r.gameObject.AddComponent<Text>();t.font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");t.text=value;t.fontSize=size;t.color=mint;t.alignment=align;t.raycastTarget=false;return t;}
  static Image Block(Transform p,string name,Vector2 pos,Vector2 size,Color color){var r=Rect(p,name,pos,size,new Vector2(.5f,.5f));var i=r.gameObject.AddComponent<Image>();i.color=color;return i;}
  Button Button(Transform p,string value,Vector2 pos,Vector2 size,UnityEngine.Events.UnityAction action,out Text label){var bg=Block(p,value,pos,size,new Color(.1f,.25f,.27f,.96f));var b=bg.gameObject.AddComponent<Button>();b.targetGraphic=bg;var colors=b.colors;colors.highlightedColor=new Color(1.3f,1.4f,1.25f);colors.selectedColor=colors.highlightedColor;colors.pressedColor=new Color(.65f,.85f,.8f);b.colors=colors;b.onClick.AddListener(action);label=Label(bg.transform,"Label",value,18,Vector2.zero,size-new Vector2(28,4),TextAnchor.MiddleCenter);bg.gameObject.AddComponent<MossButtonMotion>();return b;}
  void Awake(){
   journey.ui=this;var canvas=gameObject.AddComponent<Canvas>();canvas.renderMode=RenderMode.ScreenSpaceOverlay;var scaler=gameObject.AddComponent<CanvasScaler>();scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;scaler.referenceResolution=new Vector2(1600,900);scaler.matchWidthOrHeight=.5f;gameObject.AddComponent<GraphicRaycaster>();
   var top=Block(transform,"HUD top",new Vector2(0,388),new Vector2(1536,72),ink);top.raycastTarget=false;
   journey.area=Label(top.transform,"Chapter","",19,new Vector2(-450,12),new Vector2(570,30));health=Label(top.transform,"Health","",15,new Vector2(-450,-17),new Vector2(570,25));journey.progress=Label(top.transform,"Inventory","",18,new Vector2(460,0),new Vector2(540,50),TextAnchor.MiddleRight);
   var foot=Block(transform,"HUD controls",new Vector2(0,-410),new Vector2(1400,42),ink);foot.raycastTarget=false;journey.hint=Label(foot.transform,"Hint","",16,Vector2.zero,new Vector2(1370,40),TextAnchor.MiddleCenter);
   bossPanel=Block(transform,"Guardian health",new Vector2(0,304),new Vector2(700,74),ink).gameObject;bossLabel=Label(bossPanel.transform,"Name","",16,new Vector2(0,13),new Vector2(660,36),TextAnchor.MiddleCenter);var track=Block(bossPanel.transform,"Track",new Vector2(0,-20),new Vector2(640,8),new Color(.2f,.16f,.18f));bossFill=Block(track.transform,"Fill",Vector2.zero,new Vector2(640,8),gold);bossFill.rectTransform.pivot=new Vector2(0,.5f);bossFill.rectTransform.anchoredPosition=new Vector2(-320,0);bossPanel.SetActive(false);
   var panel=Block(transform,"Journey menu",Vector2.zero,new Vector2(1600,900),new Color(.015f,.04f,.065f,.78f));journey.panel=panel.gameObject;menuGroup=panel.gameObject.AddComponent<CanvasGroup>();
   var left=Block(panel.transform,"Menu card",new Vector2(-440,5),new Vector2(590,748),ink);
   Label(left.transform,"Edition","D O A N   /   M O S S W O O D",16,new Vector2(0,314),new Vector2(490,30)).color=gold;
   journey.panelTitle=Label(left.transform,"Title","RỪNG\nTHÌ THẦM",49,new Vector2(0,219),new Vector2(490,140));journey.panelTitle.fontStyle=FontStyle.Bold;
   journey.panelBody=Label(left.transform,"Story","",20,new Vector2(0,62),new Vector2(490,145));journey.panelBody.color=new Color(.69f,.78f,.8f);
   journey.actionButton=Button(left.transform,"BƯỚC VÀO KHU RỪNG",new Vector2(0,-64),new Vector2(490,55),()=>journey.Continue(),out journey.actionText);
   Text nextLabel;next=Button(left.transform,"ĐẾN MAP KẾ TIẾP  →",new Vector2(0,-130),new Vector2(490,49),()=>journey.LoadChapter(journey.chapter+1),out nextLabel);
   journey.restartButton=Button(left.transform,"CHƠI LẠI MÀN NÀY",new Vector2(0,-190),new Vector2(490,45),()=>{if(confirming)journey.Restart();else{confirming=true;restartLabel.text="BẤM LẦN NỮA ĐỂ XÓA TIẾN ĐỘ MÀN";}},out restartLabel);
   journey.soundButton=Button(left.transform,"ÂM THANH / BẬT",new Vector2(-127,-248),new Vector2(236,40),()=>journey.ToggleSound(),out journey.soundText);
   Text quitLabel;Button(left.transform,"THOÁT GAME",new Vector2(127,-248),new Vector2(236,40),Quit,out quitLabel);
   Button(left.transform,"",new Vector2(0,-303),new Vector2(490,36),()=>{int v=1-PlayerPrefs.GetInt("DOAN.Mosswood.shake",1);PlayerPrefs.SetInt("DOAN.Mosswood.shake",v);PlayerPrefs.Save();SettingsLabel();},out settingsText);SettingsLabel();
   Label(panel.transform,"Chapters title","NHỮNG MIỀN ĐẤT ĐANG CHỜ",21,new Vector2(310,329),new Vector2(740,40)).color=gold;
   string[] names={"MỞ ĐẦU  /  RỪNG THÌ THẦM","MAP 01  /  THÁC NGUYỆT QUANG","MAP 02  /  TÀN TÍCH HỔ PHÁCH"};string[] desc={"Lối mòn phủ rêu · Học nhảy và lướt\n13 đom đóm  /  3 hoa ký ức","Bậc đá bên thác · Linh hồn tuần tra\nNgười Giữ Rễ  /  2 giai đoạn chiến đấu","Cầu đá cổ · Những vòm rễ hóa thạch\nVệ Binh Hổ Phách  /  Mưa gai cuồng nộ"};
   for(int i=0;i<3;i++){int index=i;float y=208-i*185;Text t;var b=Button(panel.transform,"",new Vector2(310,y),new Vector2(740,162),()=>journey.LoadChapter(index),out t);Object.Destroy(t.gameObject);var strip=Block(b.transform,"Accent",new Vector2(-363,0),new Vector2(4,160),i==2?gold:mint);strip.raycastTarget=false;Label(b.transform,"Chapter",names[i],23,new Vector2(15,45),new Vector2(655,42)).fontStyle=FontStyle.Bold;Label(b.transform,"Description",desc[i],17,new Vector2(15,-12),new Vector2(655,65)).color=new Color(.75f,.83f,.84f);string key=i==0?"DOAN.Mosswood.v1.":"DOAN.Mosswood.v2."+i+".";Label(b.transform,"Status",i==journey.chapter?"ĐANG CHỌN   /   TIẾP TỤC  →":PlayerPrefs.GetInt(key+"complete",0)==1?"ĐÃ HOÀN THÀNH   /   KHÁM PHÁ LẠI  →":"KHÁM PHÁ  →",13,new Vector2(15,-61),new Vector2(655,23)).color=gold;}
   Label(panel.transform,"Keys","A / D   di chuyển     SPACE   nhảy     SHIFT   lướt né\nJ / chuột trái   bắn ánh sáng     E   tương tác     ESC   nghỉ",17,new Vector2(310,-334),new Vector2(740,65));
  }
  void SettingsLabel(){settingsText.text="RUNG CAMERA  /  "+(PlayerPrefs.GetInt("DOAN.Mosswood.shake",1)==1?"BẬT":"TẮT");}
  public void ShowMenu(bool pause){confirming=false;restartLabel.text="CHƠI LẠI MÀN NÀY";journey.panel.SetActive(true);menuGroup.alpha=0;journey.panelTitle.text=pause?"DỪNG CHÂN\nMỘT CHÚT":"RỪNG\nTHÌ THẦM";journey.panelBody.text=journey.chapterTitle+"\n\n"+(string.IsNullOrEmpty(journey.description)?"Đánh thức những hoa ký ức. Gom ánh sáng và tìm đường về ngôi nhà của khu rừng.":journey.description);journey.actionText.text=pause?"TIẾP TỤC HÀNH TRÌNH":"BẮT ĐẦU / TIẾP TỤC";next.interactable=journey.Completed&&journey.chapter<2;EventSystem.current?.SetSelectedGameObject(journey.actionButton.gameObject);}
  public void ShowCompletion(){ShowMenu(false);journey.panelTitle.text="ÁNH SÁNG\nĐÃ TRỞ VỀ";journey.panelBody.text=journey.chapterTitle+"\n"+journey.SeedCount+" / "+journey.seeds.Length+" đom đóm đã tìm về.\n\n"+(journey.chapter<2?"Một miền đất mới đang chờ bước chân bạn.":"Bạn đã đánh thức những miền ký ức. Cảm ơn bạn đã đi hết hành trình.");journey.actionText.text="KHÁM PHÁ THÊM";}
  void Update(){if(journey.panel.activeSelf)menuGroup.alpha=Mathf.MoveTowards(menuGroup.alpha,1,Time.unscaledDeltaTime*5);if(journey.Combat)health.text="SINH LỰC  "+new string('●',journey.Combat.Health)+new string('○',journey.Combat.maxHealth-journey.Combat.Health)+"       LƯỚT  "+Mathf.RoundToInt(journey.traveler.DashReady*100)+"%";var boss=journey.guardian;bool active=boss&&boss.Engaged&&!boss.Defeated;bossPanel.SetActive(active);if(active){bossLabel.text=boss.guardianName+"  /  "+(boss.Enraged?"GIAI ĐOẠN II":"GIAI ĐOẠN I")+"\n"+boss.Cue;bossFill.rectTransform.sizeDelta=new Vector2(640f*boss.Health/boss.maxHealth,8);}}
  void Quit(){
#if UNITY_EDITOR
   UnityEditor.EditorApplication.isPlaying=false;
#else
   Application.Quit();
#endif
  }
 }
}
