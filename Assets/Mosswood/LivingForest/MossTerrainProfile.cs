using UnityEngine;
namespace Mosswood {
 // Shared surface profile for authored terrain, safe checkpoints and wildlife placement.
 public class MossTerrainProfile : MonoBehaviour {
  public Vector2[] surface; public Vector2[] gaps;
  public float HeightAt(float x){if(surface==null||surface.Length==0)return -3.3f;for(int i=1;i<surface.Length;i++)if(x<=surface[i].x)return Mathf.Lerp(surface[i-1].y,surface[i].y,Mathf.InverseLerp(surface[i-1].x,surface[i].x,x));return surface[surface.Length-1].y;}
  public bool IsGap(float x){if(gaps!=null)foreach(var gap in gaps)if(x>gap.x&&x<gap.y)return true;return false;}
 }
}
