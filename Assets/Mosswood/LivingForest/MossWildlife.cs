using UnityEngine;
namespace Mosswood {
 [RequireComponent(typeof(SpriteRenderer))]
 public class MossWildlife : MonoBehaviour {
  public enum Species {Bat,SpiritFish,Spore}
  public Species species;public Sprite[] frames;public float range=3,speed=.4f,phase,amplitude=.35f;
  Vector3 origin;SpriteRenderer art;Transform traveler;float alarm;float clock;
  void Awake(){origin=transform.localPosition;art=GetComponent<SpriteRenderer>();var t=FindAnyObjectByType<MossTraveler>();if(t)traveler=t.transform;clock=phase;}
  void Update(){float dt=Time.deltaTime;if(dt<=0)return;clock+=dt;bool near=traveler&&Vector2.Distance(traveler.position,transform.position)<3.2f;alarm=Mathf.MoveTowards(alarm,near?1:0,dt*2);float t=clock*speed+phase;float x=Mathf.Sin(t)*range;float y=Mathf.Sin(t*2.1f)*amplitude+alarm*.85f;transform.localPosition=origin+new Vector3(x,y,0);if(species!=Species.Spore)art.flipX=Mathf.Cos(t)<0;if(frames!=null&&frames.Length>0)art.sprite=frames[Mathf.FloorToInt(clock*(alarm>0?13:9))%frames.Length];transform.localRotation=Quaternion.Euler(0,0,species==Species.Spore?clock*12:Mathf.Sin(t*2)*7);}
 }
}
