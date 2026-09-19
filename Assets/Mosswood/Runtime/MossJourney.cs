using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
namespace Mosswood {
 public class MossJourney : MonoBehaviour {
  public MossTraveler traveler;public Transform[] seeds,shrines;
  public Text progress,area,hint,panelTitle,panelBody,actionText,soundText;
  public GameObject panel;public Button actionButton,restartButton,soundButton;
  public int chapter;public string chapterTitle="RỪNG THÌ THẦM";public string description;public float finishX=144;public MossGuardian guardian;
  public MossInterface ui;public MossCombat Combat {get;private set;}
  public bool Started {get;private set;}public bool Completed {get;private set;}public bool IsPlaying=>Started&&!paused&&!traveler.Locked&&Time.timeScale>0;
  public string SavePrefix=>chapter==0?"DOAN.Mosswood.v1.":"DOAN.Mosswood.v2."+chapter+".";
  public static readonly string[] Scenes={"SampleScene","Map1_Moonfall","Map2_AmberRuins"};
  int seedBits,shrineBits;Vector3 checkpoint;bool paused,muted;AudioSource sound;AudioClip ambience,chime;float messageUntil;string message;
  public int SeedCount {get{int n=0;for(int i=0;i<seeds.Length;i++)if((seedBits&(1<<i))!=0)n++;return n;}}
  public int ShrineCount {get{int n=0;for(int i=0;i<shrines.Length;i++)if((shrineBits&(1<<i))!=0)n++;return n;}}
  void Start(){
   Combat=traveler.GetComponent<MossCombat>();Time.timeScale=1;
   seedBits=PlayerPrefs.GetInt(SavePrefix+"seeds",0);shrineBits=PlayerPrefs.GetInt(SavePrefix+"shrines",0);Completed=PlayerPrefs.GetInt(SavePrefix+"complete",0)==1;
   checkpoint=new Vector3(Mathf.Clamp(PlayerPrefs.GetFloat(SavePrefix+"x",0),-2,finishX),Mathf.Clamp(PlayerPrefs.GetFloat(SavePrefix+"y",-1.9f),-3,6),0);
   for(int i=0;i<seeds.Length;i++)if((seedBits&(1<<i))!=0)seeds[i].gameObject.SetActive(false);
   traveler.Warp(checkpoint);traveler.Locked=true;Camera.main.GetComponent<MossCamera>()?.Snap();
   MakeSound();muted=PlayerPrefs.GetInt("DOAN.Mosswood.muted",0)==1;ApplySound();Refresh();ui.ShowMenu(false);
  }
  public void Continue(){Started=true;paused=false;panel.SetActive(false);traveler.Locked=false;Time.timeScale=1;}
  public void Pause(){if(!Started)return;paused=true;traveler.Locked=true;Time.timeScale=0;ui.ShowMenu(true);}
  void Update(){
   if(Input.GetKeyDown(KeyCode.M))ToggleSound();if(Input.GetKeyDown(KeyCode.Escape)&&Started){if(panel.activeSelf)Continue();else Pause();}
   if(!IsPlaying)return;if(traveler.transform.position.y< -8){Respawn();return;}
   for(int i=0;i<seeds.Length;i++)if(seeds[i].gameObject.activeSelf&&Vector2.Distance(traveler.transform.position,seeds[i].position)<1.15f){seedBits|=1<<i;seeds[i].gameObject.SetActive(false);sound.PlayOneShot(chime,.35f);if(Combat)MossEffect.Burst(Combat.lightSprite,Combat.lightMaterial,seeds[i].position,new Color(.7f,1,.6f));Save();Refresh();}
   string nearby="";
   for(int i=0;i<shrines.Length;i++)if(Vector2.Distance(traveler.transform.position,shrines[i].position)<2.7f){nearby="E · ĐÁNH THỨC HOA / HỒI MÁU / LƯU ĐIỂM NGHỈ";if(Input.GetKeyDown(KeyCode.E))ActivateShrine(i);}
   if(traveler.transform.position.x>finishX){nearby=ShrineCount!=shrines.Length?"Đánh thức đủ hoa ánh sáng để mở cổng.":guardian&&!guardian.Defeated?"Đánh bại người canh giữ để mở cổng.":"E · HOÀN THÀNH MÀN";if(Input.GetKeyDown(KeyCode.E))TryComplete();}
   hint.text=Time.unscaledTime<messageUntil?message:nearby!=""?nearby:"A / D  DI CHUYỂN     SPACE  NHẢY     SHIFT  LƯỚT     J  BẮN     ESC  MENU";
  }
  public bool TryComplete(){if(ShrineCount!=shrines.Length||traveler.transform.position.x<=finishX||(guardian&&!guardian.Defeated))return false;Completed=true;PlayerPrefs.SetInt(SavePrefix+"complete",1);Save();paused=true;traveler.Locked=true;Time.timeScale=0;ui.ShowCompletion();return true;}
  public void ActivateShrine(int i){if(i<0||i>=shrines.Length)return;shrineBits|=1<<i;checkpoint=new Vector3(shrines[i].position.x,shrines[i].position.y-1.25f,0);Combat?.Heal();Save();Refresh();sound.PlayOneShot(chime,.5f);Notify("Hoa đã thức giấc · Hồi đầy sinh lực · Đã lưu");}
  public void Respawn(){MossBolt.Clear();guardian?.ResetEncounter();traveler.Warp(checkpoint);Combat?.Heal();Camera.main.GetComponent<MossCamera>()?.Snap();Notify("Làn gió đưa bạn về điểm nghỉ gần nhất.");}
  public void Notify(string text){message=text;messageUntil=Time.unscaledTime+3.5f;}
  void Refresh(){progress.text="HOA  "+ShrineCount+" / "+shrines.Length+"     ĐOM ĐÓM  "+SeedCount+" / "+seeds.Length;area.text=(chapter==0?"MỞ ĐẦU":("MAP 0"+chapter))+"  /  "+chapterTitle;for(int i=0;i<shrines.Length;i++){var r=shrines[i].GetComponent<SpriteRenderer>();if(r)r.color=(shrineBits&(1<<i))!=0?new Color(1,1,.65f):new Color(.6f,.9f,1);}}
  void Save(){PlayerPrefs.SetInt(SavePrefix+"seeds",seedBits);PlayerPrefs.SetInt(SavePrefix+"shrines",shrineBits);PlayerPrefs.SetFloat(SavePrefix+"x",checkpoint.x);PlayerPrefs.SetFloat(SavePrefix+"y",checkpoint.y);PlayerPrefs.Save();}
  public void Restart(){foreach(var suffix in new[]{"seeds","shrines","x","y","boss","complete"})PlayerPrefs.DeleteKey(SavePrefix+suffix);PlayerPrefs.Save();LoadChapter(chapter);}
  public void LoadChapter(int index){if(index<0||index>=Scenes.Length)return;Time.timeScale=1;SceneManager.LoadScene(Scenes[index]);}
  public void ToggleSound(){muted=!muted;PlayerPrefs.SetInt("DOAN.Mosswood.muted",muted?1:0);PlayerPrefs.Save();ApplySound();}
  void ApplySound(){if(sound){sound.mute=muted;sound.volume=PlayerPrefs.GetFloat("DOAN.Mosswood.volume",.35f);}if(soundText)soundText.text=muted?"ÂM THANH  /  TẮT":"ÂM THANH  /  BẬT";}
  public void SetVolume(float value){PlayerPrefs.SetFloat("DOAN.Mosswood.volume",value);ApplySound();}
  void MakeSound(){sound=gameObject.AddComponent<AudioSource>();sound.loop=true;sound.volume=.22f; const int sr=22050;var data=new float[sr*24];var rng=new System.Random(81);float noise=0;for(int i=0;i<data.Length;i++){float t=(float)i/sr;noise=noise*.985f+((float)rng.NextDouble()*2-1)*.015f;float fade=Mathf.Sin(Mathf.PI*t/24);data[i]=(noise*.45f+(Mathf.Sin(t*2*Mathf.PI*220)+Mathf.Sin(t*2*Mathf.PI*330)*.4f+Mathf.Sin(t*2*Mathf.PI*440)*.2f)*.075f)*fade*fade;}ambience=AudioClip.Create("Mosswood - wind and soft harmonics",data.Length,1,sr,false);ambience.SetData(data,0);sound.clip=ambience;sound.Play();var notes=new float[sr];for(int i=0;i<notes.Length;i++){float t=(float)i/sr;notes[i]=Mathf.Sin(2*Mathf.PI*880*t)*Mathf.Exp(-7*t)*.25f;}chime=AudioClip.Create("Firefly chime",sr,1,sr,false);chime.SetData(notes,0);}
  void OnDestroy(){Time.timeScale=1;PlayerPrefs.Save();if(ambience)Destroy(ambience);if(chime)Destroy(chime);}
 }
}
