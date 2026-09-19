using UnityEngine;
namespace Mosswood {
 public class MossCombat : MonoBehaviour {
  public Sprite lightSprite; public Material lightMaterial;public MossJourney journey;public int maxHealth=5;
  public int Health {get;private set;}public float FireReady=>Mathf.Clamp01(1-(nextFire-Time.time)/.32f);
  MossTraveler traveler;SpriteRenderer art;float nextFire,invulnerable,ghostAt;Color original;
  void Awake(){traveler=GetComponent<MossTraveler>();art=GetComponent<SpriteRenderer>();original=art.color;Health=maxHealth;}
  void Update(){
   if(traveler.Locked||Time.timeScale==0)return;
   art.color=Time.time<invulnerable?Color.Lerp(original,new Color(1,.4f,.45f,.4f),.5f+.5f*Mathf.Sin(Time.time*35)):original;
   if(Input.GetKey(KeyCode.J)||Input.GetMouseButton(0))Fire();
   if(traveler.IsDashing&&Time.time>ghostAt){ghostAt=Time.time+.035f;var g=new GameObject("Dash echo");g.transform.position=transform.position;g.transform.localScale=transform.localScale;var s=g.AddComponent<SpriteRenderer>();s.sprite=art.sprite;s.sharedMaterial=art.sharedMaterial;s.flipX=art.flipX;s.sortingOrder=19;s.color=new Color(.3f,1,1,.5f);g.AddComponent<MossEffect>().duration=.2f;}
  }
  public bool Fire(){if(traveler.Locked||Time.timeScale==0||Time.time<nextFire||!lightSprite)return false;nextFire=Time.time+.32f;Vector2 direction=Vector2.right*traveler.Facing;MossBolt.Spawn(lightSprite,lightMaterial,transform.position+(Vector3)direction*.65f,direction*16,false,journey,1);MossEffect.Burst(lightSprite,lightMaterial,transform.position+(Vector3)direction*.6f,new Color(.5f,1,.9f),3);return true;}
  public bool Hurt(int amount){if(traveler.Locked||Time.timeScale==0||traveler.IsDashing||Time.time<invulnerable||amount<=0)return false;Health=Mathf.Max(0,Health-amount);invulnerable=Time.time+1.15f;Camera.main.GetComponent<MossCamera>()?.Shake(.15f);MossEffect.Burst(lightSprite,lightMaterial,transform.position,new Color(1,.3f,.35f));if(Health==0)journey.Respawn();return true;}
  public void Heal(){Health=maxHealth;invulnerable=Time.time+1.2f;art.color=original;}
 }
}
