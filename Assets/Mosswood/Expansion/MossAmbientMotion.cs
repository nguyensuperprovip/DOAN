using UnityEngine;
namespace Mosswood {
 // Gentle periodic movement for hanging vegetation and water. No texture copies at runtime.
 public class MossAmbientMotion : MonoBehaviour {
  public float angle=3,rate=.7f,phase;public bool waterfall;Vector3 origin;Quaternion rotation;SpriteRenderer art;Color tint;
  void Awake(){origin=transform.localPosition;rotation=transform.localRotation;art=GetComponent<SpriteRenderer>();if(art)tint=art.color;}
  void Update(){float t=Time.time*rate+phase;if(waterfall){transform.localPosition=origin+Vector3.up*Mathf.Sin(t)*.2f;if(art){var c=tint;c.a=tint.a*(.8f+.2f*Mathf.Sin(t*2));art.color=c;}}else transform.localRotation=rotation*Quaternion.Euler(0,0,Mathf.Sin(t)*angle);}
 }
}
