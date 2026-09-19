using UnityEngine;using UnityEngine.EventSystems;
namespace Mosswood {
 public class MossButtonMotion : MonoBehaviour,IPointerEnterHandler,IPointerExitHandler,ISelectHandler,IDeselectHandler {
  bool hover,selected;
  public void OnPointerEnter(PointerEventData e){hover=true;}public void OnPointerExit(PointerEventData e){hover=false;}
  public void OnSelect(BaseEventData e){selected=true;}public void OnDeselect(BaseEventData e){selected=false;}
  void Update(){transform.localScale=Vector3.Lerp(transform.localScale,Vector3.one*(hover||selected?1.018f:1),Time.unscaledDeltaTime*15);}
  void OnDisable(){hover=selected=false;transform.localScale=Vector3.one;}
 }
}
