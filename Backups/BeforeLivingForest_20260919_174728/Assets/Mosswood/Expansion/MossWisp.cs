using UnityEngine;
namespace Mosswood {
 public class MossWisp : MonoBehaviour {
  public MossJourney journey;public float travel=2,rate=.7f;public int health=3;Vector3 origin;float nextAttack;
  void Start(){origin=transform.position;}
  void Update(){if(!journey.IsPlaying)return;transform.position=origin+new Vector3(Mathf.Sin(Time.time*rate)*travel,Mathf.Sin(Time.time*2)*.25f,0);if(Vector2.Distance(transform.position,journey.traveler.transform.position)<9&&Time.time>nextAttack){nextAttack=Time.time+2.8f;MossBolt.Spawn(journey.Combat.lightSprite,journey.Combat.lightMaterial,transform.position,(journey.traveler.transform.position-transform.position).normalized*4,true,journey,1);}}
  public bool TakeHit(int damage){if(!journey.IsPlaying||health<=0)return false;health-=damage;if(health<=0){MossEffect.Burst(journey.Combat.lightSprite,journey.Combat.lightMaterial,transform.position,new Color(1,.7f,.3f),12);Destroy(gameObject);}return true;}
 }
}
