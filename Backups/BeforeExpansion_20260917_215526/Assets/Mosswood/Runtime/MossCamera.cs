using UnityEngine;
namespace Mosswood {
 public class MossCamera : MonoBehaviour {
  public Transform target; Vector3 velocity;
  void LateUpdate(){if(!target)return; var p=new Vector3(Mathf.Clamp(target.position.x+2.8f,4,145),Mathf.Clamp(target.position.y+2.1f,1.3f,5),-10);transform.position=Vector3.SmoothDamp(transform.position,p,ref velocity,.55f,Mathf.Infinity,Time.unscaledDeltaTime);}
 }
}
