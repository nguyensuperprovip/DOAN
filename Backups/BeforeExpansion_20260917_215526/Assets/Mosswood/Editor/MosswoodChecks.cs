using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.Build.Reporting;
namespace Mosswood.Editor {
 public static class MosswoodChecks {
  const string Output="C:/Users/nguye/Documents/Codex/2026-09-17/haxy/outputs/";
  static MossJourney j; static Rigidbody2D rb; static double next;static int stage;static List<string> report;
  static int savedSeeds,savedShrines;static float savedX,savedY;static bool[] had;static readonly string[] keys={"seeds","shrines","x","y"};
  static string Key(string k)=>"DOAN.Mosswood.v1."+k;
  static void Check(bool ok,string what){report.Add((ok?"PASS: ":"FAIL: ")+what);}
  [MenuItem("DOAN/Verify Mosswood (Play Mode)")]
  public static void Verify(){
   if(!EditorApplication.isPlaying)throw new InvalidOperationException("Enter Play Mode first.");
   report=new List<string>();j=UnityEngine.Object.FindAnyObjectByType<MossJourney>();rb=j.traveler.GetComponent<Rigidbody2D>();
   had=Array.ConvertAll(keys,k=>PlayerPrefs.HasKey(Key(k)));savedSeeds=PlayerPrefs.GetInt(Key("seeds"));savedShrines=PlayerPrefs.GetInt(Key("shrines"));savedX=PlayerPrefs.GetFloat(Key("x"));savedY=PlayerPrefs.GetFloat(Key("y"));
   Check(j.seeds.Length==13&&j.shrines.Length==3,"13 collectibles and 3 rest points configured");
   int missing=0;foreach(var go in UnityEngine.Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None))missing+=GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(go);
   Check(missing==0,"No missing scripts in playable scene");
   bool shaders=true;foreach(var r in UnityEngine.Object.FindObjectsByType<SpriteRenderer>(FindObjectsSortMode.None))shaders&=r.sharedMaterial!=null&&r.sharedMaterial.shader.isSupported;
   Check(shaders,"All sprite shaders supported");
   j.Continue();j.enabled=false;j.traveler.enabled=false;j.traveler.Warp(new Vector3(0,0,0));rb.gravityScale=3;
   stage=0;next=Time.time+2;EditorApplication.update+=Tick;
  }
  static void Tick(){if(Time.time<next)return;try{
   switch(stage++){
    case 0:Check(Mathf.Abs(rb.position.y+2.48f)<.12f,"Player settles on lower trail, y="+rb.position.y);j.traveler.Warp(new Vector3(28,4,0));next=Time.time+2;break;
    case 1:Check(Mathf.Abs(rb.position.y-2.63f)<.15f,"Player lands on optional upper ledge, y="+rb.position.y);j.traveler.Warp(new Vector3(13,-2.48f,0));rb.linearVelocity=new Vector2(0,11.5f);next=Time.time+2;break;
    case 2:Check(rb.position.y>-.9f&&rb.position.y<-.2f,"Jump passes up through one-way ledge and lands on it");j.enabled=true;rb.simulated=false;int idx=Array.FindIndex(j.seeds,t=>t.gameObject.activeSelf);if(idx>=0){j.traveler.transform.position=j.seeds[idx].position;report.Add("Collectible inventory before: "+j.SeedCount);}next=Time.time+.3;break;
    case 3:Check(PlayerPrefs.GetInt(Key("seeds"))!=savedSeeds||j.SeedCount==13,"Collectible pickup persists progress");for(int i=0;i<3;i++)j.ActivateShrine(i);Check(j.ShrineCount==3,"All three rest flowers activate");Check(PlayerPrefs.GetInt(Key("shrines"))==7,"Rest flower state persists");rb.simulated=true;j.traveler.Warp(new Vector3(10,-20,0));j.Respawn();Check(Vector2.Distance(rb.position,new Vector2(138,-1.9f))<.1f,"Fall recovery returns to last checkpoint");j.traveler.Warp(new Vector3(150,-1.9f,0));Check(j.TryComplete()&&j.Completed&&j.panel.activeSelf,"Three flowers unlock ending screen");j.Continue();Check(!j.panel.activeSelf&&!j.traveler.Locked,"Exploration resumes after ending");Finish();break;
   }
  }catch(Exception e){report.Add("FAIL: "+e);Finish();}}
  static void Finish(){EditorApplication.update-=Tick;PlayerPrefs.SetInt(Key("seeds"),savedSeeds);PlayerPrefs.SetInt(Key("shrines"),savedShrines);PlayerPrefs.SetFloat(Key("x"),savedX);PlayerPrefs.SetFloat(Key("y"),savedY);for(int i=0;i<keys.Length;i++)if(!had[i])PlayerPrefs.DeleteKey(Key(keys[i]));PlayerPrefs.Save();Directory.CreateDirectory(Output);File.WriteAllLines(Output+"verification.txt",report);EditorApplication.isPlaying=false;Debug.Log(string.Join("\n",report));}
  [MenuItem("DOAN/Build Windows Player")]
  public static void BuildWindows(){
   if(EditorApplication.isPlaying)throw new InvalidOperationException("Exit Play Mode before building.");
   Directory.CreateDirectory(Output+"Mosswood");PlayerSettings.productName="Rung Thi Tham - Mosswood";PlayerSettings.companyName="DOAN";PlayerSettings.defaultScreenWidth=1600;PlayerSettings.defaultScreenHeight=900;PlayerSettings.fullScreenMode=FullScreenMode.Windowed;
   var r=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=new[]{"Assets/Scenes/SampleScene.unity"},locationPathName=Output+"Mosswood/DOAN-Mosswood.exe",target=BuildTarget.StandaloneWindows64,options=BuildOptions.None});
   File.WriteAllText(Output+"build-report.txt","Result: "+r.summary.result+"\nErrors: "+r.summary.totalErrors+"\nWarnings: "+r.summary.totalWarnings+"\nSize: "+r.summary.totalSize+" bytes\nDuration: "+r.summary.totalTime);
   Debug.Log("Mosswood Windows build: "+r.summary.result);
  }
 }
}
