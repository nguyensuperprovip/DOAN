using UnityEngine;
namespace Mosswood {
 public class MossMist : MonoBehaviour {
  public float phase;public float drift=.45f;Vector3 origin;SpriteRenderer art;Color tint;
  void Awake(){origin=transform.localPosition;art=GetComponent<SpriteRenderer>();if(art)tint=art.color;}
  void Update(){transform.localPosition=origin+new Vector3(Mathf.Sin(Time.time*.08f+phase)*drift,Mathf.Sin(Time.time*.12f+phase)*.12f,0);if(art){var c=tint;c.a*=.8f+.2f*Mathf.Sin(Time.time*.2f+phase);art.color=c;}}
 }
}
