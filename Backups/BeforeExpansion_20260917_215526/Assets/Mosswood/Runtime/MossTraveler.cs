using UnityEngine;
namespace Mosswood {
 [RequireComponent(typeof(Rigidbody2D),typeof(BoxCollider2D))]
 public class MossTraveler : MonoBehaviour {
  public float speed=5.8f, jumpSpeed=11.5f;
  public LayerMask groundMask=64;
  public bool grounded;
  Rigidbody2D body; BoxCollider2D box; Animator animator; SpriteRenderer art;
  float axis, grace, buffer, dash, cooldown; int facing=1;
  public bool Locked {get;set;}
  void Awake(){body=GetComponent<Rigidbody2D>();box=GetComponent<BoxCollider2D>();animator=GetComponent<Animator>();art=GetComponent<SpriteRenderer>();}
  void Update(){
   if(Locked || Time.timeScale==0){axis=0;return;}
   axis=Input.GetAxisRaw("Horizontal");
   var b=box.bounds;
   grounded=Physics2D.OverlapBox(new Vector2(b.center.x,b.min.y-.045f),new Vector2(b.size.x*.8f,.1f),0,groundMask)!=null;
   grace=grounded?.14f:grace-Time.deltaTime; buffer-=Time.deltaTime; cooldown-=Time.deltaTime;dash-=Time.deltaTime;
   if(Input.GetButtonDown("Jump"))buffer=.16f;
   if(buffer>0&&grace>0){body.linearVelocity=new Vector2(body.linearVelocity.x,jumpSpeed);buffer=0;grace=0;}
   if(Input.GetButtonUp("Jump")&&body.linearVelocity.y>0)body.linearVelocity=new Vector2(body.linearVelocity.x,body.linearVelocity.y*.55f);
   if(axis!=0){facing=axis>0?1:-1;art.flipX=facing<0;}
   if(Input.GetKeyDown(KeyCode.LeftShift)&&cooldown<=0){dash=.16f;cooldown=.85f;}
   if(animator){animator.SetBool("isrunning",Mathf.Abs(axis)>.1f&&grounded);animator.SetBool("isjumping",!grounded);animator.SetBool("isdashing",dash>0);}
  }
  void FixedUpdate(){if(Locked){body.linearVelocity=Vector2.zero;body.gravityScale=0;return;}body.gravityScale=dash>0?0:3;body.linearVelocity=new Vector2(dash>0?facing*13:Mathf.MoveTowards(body.linearVelocity.x,axis*speed,55*Time.fixedDeltaTime),dash>0?0:Mathf.Max(body.linearVelocity.y,-20));}
  public void Warp(Vector3 p){transform.position=p;body.position=p;body.linearVelocity=Vector2.zero;dash=0;grace=0;buffer=0;}
 }
}
