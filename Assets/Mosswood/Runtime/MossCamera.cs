using UnityEngine;
namespace Mosswood {
 public class MossCamera : MonoBehaviour {
  public Transform target;public float minX=4,maxX=145,minY=1.3f,maxY=5;Vector3 velocity;float shake;
  public void Shake(float amount){shake=Mathf.Max(shake,amount);}
  public void Snap(){if(!target)return;transform.position=Desired();velocity=Vector3.zero;}
  Vector3 Desired()=>new Vector3(Mathf.Clamp(target.position.x+2.8f,minX,maxX),Mathf.Clamp(target.position.y+2.1f,minY,maxY),-10);
  void LateUpdate(){if(!target||Time.timeScale==0)return;var p=Vector3.SmoothDamp(transform.position,Desired(),ref velocity,.35f);if(PlayerPrefs.GetInt("DOAN.Mosswood.shake",1)==1&&shake>0){p+=(Vector3)Random.insideUnitCircle*shake;shake=Mathf.MoveTowards(shake,0,Time.deltaTime);}transform.position=p;}
 }
}
