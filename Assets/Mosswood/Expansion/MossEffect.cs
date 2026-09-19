using UnityEngine;
namespace Mosswood {
 public class MossEffect : MonoBehaviour {
  public Vector2 velocity;public float duration=.5f;float age;SpriteRenderer art;Vector3 size;
  void Start(){art=GetComponent<SpriteRenderer>();size=transform.localScale;}
  void Update(){age+=Time.deltaTime;transform.position+=(Vector3)(velocity*Time.deltaTime);if(art){var c=art.color;c.a=Mathf.Clamp01(1-age/duration);art.color=c;}transform.localScale=size*(1+age/duration*.4f);if(age>=duration)Destroy(gameObject);}
  public static void Burst(Sprite sprite,Material material,Vector3 position,Color color,int count=8){for(int i=0;i<count;i++){var g=new GameObject("Light mote");g.transform.position=position;g.transform.localScale=Vector3.one*Random.Range(.12f,.3f);var s=g.AddComponent<SpriteRenderer>();s.sprite=sprite;s.sharedMaterial=material;s.color=color;s.sortingOrder=45;var e=g.AddComponent<MossEffect>();e.velocity=Random.insideUnitCircle*3;e.duration=Random.Range(.3f,.65f);}}
 }
}
