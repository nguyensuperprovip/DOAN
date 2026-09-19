using UnityEngine;using UnityEditor;using UnityEngine.SceneManagement;using System;using System.Collections;using System.Collections.Generic;using System.IO;
namespace Mosswood.Editor {
 public static class MossCampaignChecks {
  const string Output="C:/Users/nguye/Documents/Codex/2026-09-17/h-y-c-to-n-b-2/outputs/";
  static IEnumerator routine;static double next;static List<string> report;static Dictionary<string,string> saved;static Dictionary<string,bool> existed;static string[] suffixes={"seeds","shrines","x","y","boss","complete"};static string[] prefixes={"DOAN.Mosswood.v1.","DOAN.Mosswood.v2.1.","DOAN.Mosswood.v2.2."};static int errors;
  [MenuItem("DOAN/Expansion/Verify campaign (Play Mode)")]
  public static void Run(){if(!EditorApplication.isPlaying)throw new InvalidOperationException("Enter Play Mode first");Application.runInBackground=true;report=new List<string>();saved=new Dictionary<string,string>();existed=new Dictionary<string,bool>();foreach(var p in prefixes)foreach(var s in suffixes){string k=p+s;existed[k]=PlayerPrefs.HasKey(k);saved[k]=(s=="x"||s=="y")?PlayerPrefs.GetFloat(k).ToString(System.Globalization.CultureInfo.InvariantCulture):PlayerPrefs.GetInt(k).ToString();PlayerPrefs.DeleteKey(k);}errors=0;Application.logMessageReceived+=Log;routine=Suite();next=0;EditorApplication.update+=Tick;}
  static void Log(string condition,string stack,LogType type){if(type==LogType.Error||type==LogType.Exception||type==LogType.Assert){errors++;report.Add("RUNTIME ERROR: "+condition);}}
  static void Check(bool value,string name){report.Add((value?"PASS: ":"FAIL: ")+name);}
  static void Tick(){EditorApplication.QueuePlayerLoopUpdate();if(EditorApplication.timeSinceStartup<next)return;if(!EditorApplication.isPlaying){Finish();return;}try{if(!routine.MoveNext()){Finish();return;}next=EditorApplication.timeSinceStartup+(routine.Current is float f?f:.1f);}catch(Exception e){report.Add("FAIL: "+e);Finish();}}
  static IEnumerator Suite(){
   for(int map=0;map<3;map++){
    SceneManager.LoadScene(MossJourney.Scenes[map]);yield return 1.2f;var j=UnityEngine.Object.FindAnyObjectByType<MossJourney>();
    Check(j!=null&&j.chapter==map,"Map "+map+" scene transition");Check(j.panel.activeSelf&&j.traveler.Locked,"Map "+map+" starts in menu");Check(j.SeedCount==0&&j.ShrineCount==0,"Map "+map+" separate clean save");
    int missing=0;foreach(var g in UnityEngine.Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None))missing+=GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(g);Check(missing==0,"Map "+map+" no missing scripts");
    j.actionButton.onClick.Invoke();yield return 1f;Check(j.IsPlaying&&!j.panel.activeSelf,"Map "+map+" start button");var rb=j.traveler.GetComponent<Rigidbody2D>();Check(Mathf.Abs(rb.position.y+2.48f)<.18f,"Map "+map+" player lands on trail");
    j.Pause();var p=rb.position;int hp=j.Combat.Health;yield return .3f;Check(Time.timeScale==0&&rb.position==p&&!j.Combat.Hurt(1)&&j.Combat.Health==hp,"Map "+map+" pause freezes player and damage");j.Continue();
    j.traveler.Warp(j.seeds[0].position);yield return .2f;Check(j.SeedCount==1&&PlayerPrefs.GetInt(j.SavePrefix+"seeds")==1,"Map "+map+" pickup saves");
    j.traveler.Warp(new Vector3(j.finishX+1,-1.9f));Check(!j.TryComplete(),"Map "+map+" completion blocked before shrines");
    for(int i=0;i<j.shrines.Length;i++)j.ActivateShrine(i);Check(j.ShrineCount==3&&PlayerPrefs.GetInt(j.SavePrefix+"shrines")==7,"Map "+map+" checkpoints save");j.Respawn();Check(Mathf.Abs(rb.position.x-j.shrines[2].position.x)<.05f&&j.Combat.Health==5,"Map "+map+" respawn restores position and health");
    // Check a real jump passes through and lands on the first one-way platform.
    j.traveler.enabled=false;j.traveler.Warp(new Vector3(map==0?13:12,-2.48f));rb.gravityScale=3;Physics2D.SyncTransforms();yield return .35f;rb.linearVelocity=new Vector2(0,11.5f);yield return 2f;Check(rb.position.y> -1&&rb.position.y<0,"Map "+map+" one-way ledge landing (y="+rb.position.y+")");j.traveler.enabled=true;
    if(map>0){var boss=j.guardian;j.traveler.Warp(new Vector3(147,-2.4f));Check(!j.TryComplete(),"Map "+map+" boss gates completion");j.traveler.Warp(new Vector3(128,-2.4f));yield return .2f;Check(boss.Engaged,"Map "+map+" boss engagement");int before=boss.Health;Check(j.Combat.Fire(),"Map "+map+" spirit weapon fires");yield return .5f;Check(boss.Health<before,"Map "+map+" projectile sweep damages boss");
     boss.TakeHit(boss.maxHealth/2);Check(boss.Enraged,"Map "+map+" second phase");yield return 1.2f;Check(!string.IsNullOrEmpty(boss.Cue),"Map "+map+" attack telegraph");
     j.Respawn();Check(boss.Health==boss.maxHealth&&!boss.Engaged,"Map "+map+" failed encounter resets");yield return 1.3f;Check(j.Combat.Hurt(1)&&j.Combat.Health==4,"Map "+map+" player damage");Check(!j.Combat.Hurt(1)&&j.Combat.Health==4,"Map "+map+" invulnerability window");j.traveler.Warp(new Vector3(128,-2.4f));yield return .2f;boss.TakeHit(100);Check(boss.Defeated&&!boss.hitbox.enabled&&PlayerPrefs.GetInt(j.SavePrefix+"boss")==1,"Map "+map+" defeat persists and disables hitbox");}
    j.traveler.Warp(new Vector3(j.finishX+1,-2.4f));Check(j.TryComplete()&&j.Completed&&j.panel.activeSelf&&Time.timeScale==0,"Map "+map+" victory menu");j.Continue();Check(j.IsPlaying,"Map "+map+" exploration after victory");
    SceneManager.LoadScene(MossJourney.Scenes[map]);yield return 1f;j=UnityEngine.Object.FindAnyObjectByType<MossJourney>();Check(j.SeedCount==1&&j.ShrineCount==3&&j.Completed,"Map "+map+" save restored after reload");if(map>0)Check(j.guardian.Defeated,"Map "+map+" defeated guardian stays defeated");
   }
   Check(errors==0,"No runtime errors during campaign checks");
  }
  static void Finish(){EditorApplication.update-=Tick;Application.logMessageReceived-=Log;foreach(var p in prefixes)foreach(var s in suffixes){string k=p+s;if(!existed[k])PlayerPrefs.DeleteKey(k);else if(s=="x"||s=="y")PlayerPrefs.SetFloat(k,float.Parse(saved[k],System.Globalization.CultureInfo.InvariantCulture));else PlayerPrefs.SetInt(k,int.Parse(saved[k]));}PlayerPrefs.Save();Directory.CreateDirectory(Output);File.WriteAllLines(Output+"verification.txt",report);Debug.Log(string.Join("\n",report));EditorApplication.isPlaying=false;}
 }
}




