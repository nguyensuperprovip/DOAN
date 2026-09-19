using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
namespace Mosswood {
 public class MossJourney : MonoBehaviour {
  public MossTraveler traveler;
  public Transform[] seeds, shrines;
  public Text progress, area, hint, panelTitle, panelBody, actionText;
  public GameObject panel; public Button actionButton, restartButton, soundButton;
  public Text soundText;
  public bool Started {get;private set;} public bool Completed {get;private set;}
  int seedBits, shrineBits; Vector3 checkpoint=new Vector3(0,-1.9f,0); bool paused, muted; AudioSource sound; AudioClip ambience,chime; float messageUntil; string message;
  const string SaveKey="DOAN.Mosswood.v1.";
  public int SeedCount {get {int n=0;for(int i=0;i<seeds.Length;i++)if((seedBits&(1<<i))!=0)n++;return n;}}
  public int ShrineCount {get {int n=0;for(int i=0;i<shrines.Length;i++)if((shrineBits&(1<<i))!=0)n++;return n;}}
  void Start(){
   Time.timeScale=1;seedBits=PlayerPrefs.GetInt(SaveKey+"seeds",0);shrineBits=PlayerPrefs.GetInt(SaveKey+"shrines",0);
   checkpoint.x=PlayerPrefs.GetFloat(SaveKey+"x",0);checkpoint.y=PlayerPrefs.GetFloat(SaveKey+"y",-1.9f);
   for(int i=0;i<seeds.Length;i++)if((seedBits&(1<<i))!=0)seeds[i].gameObject.SetActive(false);
   traveler.Warp(checkpoint); traveler.Locked=true;
   actionButton.onClick.AddListener(Continue);restartButton.onClick.AddListener(Restart);soundButton.onClick.AddListener(ToggleSound);
   panelTitle.text="RỪNG THÌ THẦM"; panelBody.text="Một chuyến đi nhỏ giữa miền rêu xanh.\n\nĐánh thức ba hoa ánh sáng và tìm về Cây Ký Ức.\nDạo bước, khám phá những lối cao, gom đom đóm.\nKhông giới hạn thời gian. Hãy đi theo nhịp của bạn.";
   actionText.text=shrineBits!=0||seedBits!=0?"TIẾP TỤC HÀNH TRÌNH":"BƯỚC VÀO KHU RỪNG";
   MakeSound();Refresh();
  }
  public void Continue(){Started=true;paused=false;panel.SetActive(false);traveler.Locked=false;Time.timeScale=1;}
  void Update(){
   if(Input.GetKeyDown(KeyCode.M))ToggleSound();
   if(Input.GetKeyDown(KeyCode.Escape)&&Started){if(paused)Continue();else{paused=true;traveler.Locked=true;Time.timeScale=0;panel.SetActive(true);panelTitle.text="NGHỈ CHÂN MỘT CHÚT";panelBody.text="Khu rừng vẫn ở đây, chờ bạn.\nTiến độ được lưu khi thu thập và ghé hoa ánh sáng.";actionText.text="TIẾP TỤC";}}
   if(!Started||paused)return;
   if(traveler.transform.position.y < -8)Respawn();
   for(int i=0;i<seeds.Length;i++)if(seeds[i].gameObject.activeSelf&&Vector2.Distance(traveler.transform.position,seeds[i].position)<1.15f){seedBits|=1<<i;seeds[i].gameObject.SetActive(false);sound.PlayOneShot(chime,.35f);Save();Refresh();}
   string nearby="";
   for(int i=0;i<shrines.Length;i++)if(Vector2.Distance(traveler.transform.position,shrines[i].position)<2.5f){nearby="E  ·  Nghỉ bên hoa ánh sáng";if(Input.GetKeyDown(KeyCode.E))ActivateShrine(i);}
   if(traveler.transform.position.x>144){nearby=ShrineCount==3?"E  ·  Gửi ánh sáng về Cây Ký Ức":"Hãy đánh thức cả 3 hoa ánh sáng trước khi trở về.";if(Input.GetKeyDown(KeyCode.E))TryComplete();}
   hint.text=Time.unscaledTime<messageUntil?message:nearby!=""?nearby:"A / D  Di chuyển     SPACE  Nhảy     SHIFT  Lướt     ESC  Nghỉ     M  Âm thanh";
   area.text=traveler.transform.position.x<43?"01  /  LỐI VÀO MIỀN RÊU":traveler.transform.position.x<100?"02  /  THUNG LŨNG ĐOM ĐÓM":"03  /  VƯỜN KÝ ỨC";
  }
  public bool TryComplete(){if(ShrineCount!=3||traveler.transform.position.x<=144||Completed)return false;Completed=true;panel.SetActive(true);traveler.Locked=true;panelTitle.text="KHU RỪNG ĐÃ THỨC GIẤC";panelBody.text="Ba ngọn sáng đã tìm được đường về.\nBạn đã mang về "+SeedCount+" / "+seeds.Length+" đom đóm.\n\nCảm ơn bạn đã chậm lại và lắng nghe.\nBạn vẫn có thể tiếp tục khám phá khu rừng.";actionText.text="DẠO CHƠI THÊM";return true;}
  public void ActivateShrine(int i){shrineBits|=1<<i;checkpoint=new Vector3(shrines[i].position.x,-1.9f,0);Save();Refresh();sound.PlayOneShot(chime,.5f);message="Hoa ánh sáng đã thức giấc · Đã lưu điểm nghỉ";messageUntil=Time.unscaledTime+3.5f;}
  public void Respawn(){traveler.Warp(checkpoint);message="Một làn gió đưa bạn về điểm nghỉ gần nhất.";messageUntil=Time.unscaledTime+3;}
  void Refresh(){progress.text="HOA ÁNH SÁNG   "+ShrineCount+" / 3       ĐOM ĐÓM   "+SeedCount+" / "+seeds.Length;for(int i=0;i<shrines.Length;i++){var r=shrines[i].GetComponent<SpriteRenderer>();if(r)r.color=(shrineBits&(1<<i))!=0?new Color(1,1,.65f):new Color(.6f,.9f,1);}}
  void Save(){PlayerPrefs.SetInt(SaveKey+"seeds",seedBits);PlayerPrefs.SetInt(SaveKey+"shrines",shrineBits);PlayerPrefs.SetFloat(SaveKey+"x",checkpoint.x);PlayerPrefs.SetFloat(SaveKey+"y",checkpoint.y);PlayerPrefs.Save();}
  public void Restart(){foreach(var suffix in new[]{"seeds","shrines","x","y"})PlayerPrefs.DeleteKey(SaveKey+suffix);PlayerPrefs.Save();Time.timeScale=1;SceneManager.LoadScene(SceneManager.GetActiveScene().name);}
  void ToggleSound(){muted=!muted;if(sound)sound.mute=muted;soundText.text=muted?"ÂM THANH: TẮT":"ÂM THANH: BẬT";}
  void MakeSound(){sound=gameObject.AddComponent<AudioSource>();sound.loop=true;sound.volume=.22f; const int sr=22050;var data=new float[sr*24];var rng=new System.Random(81);float noise=0;for(int i=0;i<data.Length;i++){float t=(float)i/sr;noise=noise*.985f+((float)rng.NextDouble()*2-1)*.015f;float fade=Mathf.Sin(Mathf.PI*t/24);data[i]=(noise*.45f+(Mathf.Sin(t*2*Mathf.PI*220)+Mathf.Sin(t*2*Mathf.PI*330)*.4f+Mathf.Sin(t*2*Mathf.PI*440)*.2f)*.075f)*fade*fade;}ambience=AudioClip.Create("Mosswood - wind and soft harmonics",data.Length,1,sr,false);ambience.SetData(data,0);sound.clip=ambience;sound.Play();var notes=new float[sr];for(int i=0;i<notes.Length;i++){float t=(float)i/sr;notes[i]=Mathf.Sin(2*Mathf.PI*880*t)*Mathf.Exp(-7*t)*.25f;}chime=AudioClip.Create("Firefly chime",sr,1,sr,false);chime.SetData(notes,0);}
  void OnDestroy(){Time.timeScale=1;if(ambience)Destroy(ambience);if(chime)Destroy(chime);}
 }
}
