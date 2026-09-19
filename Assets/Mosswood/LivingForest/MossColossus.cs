using UnityEngine;
namespace Mosswood {
 // Rigs use persistent meshes and line renderers, so no video decoding or runtime texture allocation is needed.
 public class MossColossus : MonoBehaviour {
  public Transform[] limbs;public Transform[] eyes;public float phase,breath=.025f,sway=2;public bool leviathan;
  Vector3 origin,scale;Quaternion[] rotations;Vector3[] positions;Vector3[] eyeScales;
  void Awake(){origin=transform.localPosition;scale=transform.localScale;rotations=new Quaternion[limbs.Length];positions=new Vector3[limbs.Length];for(int i=0;i<limbs.Length;i++){rotations[i]=limbs[i].localRotation;positions[i]=limbs[i].localPosition;}eyeScales=new Vector3[eyes.Length];for(int i=0;i<eyes.Length;i++)eyeScales[i]=eyes[i].localScale;}
  void Update(){float t=Time.time*.35f+phase;transform.localPosition=origin+new Vector3(Mathf.Sin(t*.4f)*.7f,Mathf.Sin(t)*.23f,0);transform.localScale=Vector3.Scale(scale,new Vector3(1+Mathf.Sin(t)*breath,1+Mathf.Sin(t+.7f)*breath,1));for(int i=0;i<limbs.Length;i++){limbs[i].localRotation=rotations[i]*Quaternion.Euler(0,0,Mathf.Sin(t+i*.7f)*sway);if(leviathan)limbs[i].localPosition=positions[i]+Vector3.up*Mathf.Sin(t*1.3f-i*.5f)*.3f;}float blink=Mathf.Repeat(Time.time+phase,11);for(int i=0;i<eyes.Length;i++)eyes[i].localScale=Vector3.Scale(eyeScales[i],new Vector3(1,blink>10.7f?.15f:1,1));}
 }
}
