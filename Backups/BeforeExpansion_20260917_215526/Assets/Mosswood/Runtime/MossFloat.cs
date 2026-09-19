using UnityEngine;
namespace Mosswood {
 public class MossFloat : MonoBehaviour {
  public float amplitude=.16f, rate=1, phase; Vector3 origin;
  void Awake(){origin=transform.localPosition;}
  void Update(){transform.localPosition=origin+Vector3.up*(Mathf.Sin(Time.time*rate+phase)*amplitude);}
 }
}
