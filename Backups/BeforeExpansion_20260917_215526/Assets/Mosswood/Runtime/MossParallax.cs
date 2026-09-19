using UnityEngine;
namespace Mosswood {
 public class MossParallax : MonoBehaviour {
  public float factor=.15f; Vector3 origin; Transform cam;
  void Start(){origin=transform.position;cam=Camera.main.transform;}
  void LateUpdate(){if(cam)transform.position=origin+new Vector3((cam.position.x-4)*factor,(cam.position.y-1.3f)*factor,0);}
 }
}
