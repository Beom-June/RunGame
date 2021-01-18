using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyMove : MonoBehaviour
{
    Rigidbody2D rigid;
    Animator animator;
    SpriteRenderer spriterenderer;
    CapsuleCollider2D capsuleCollider;

    public int NextMove;

    void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriterenderer = GetComponent<SpriteRenderer>();
        capsuleCollider = GetComponent<CapsuleCollider2D>();
        Invoke("Think", 3);
    }

    void FixedUpdate()
    {
        // Move
        rigid.velocity = new Vector2(NextMove, rigid.velocity.y);

        // 지형 체크 (Platform Check)
        Vector2 FrontVec = new Vector2(rigid.position.x + NextMove * 0.2f, rigid.position.y);
        Debug.DrawRay(FrontVec, Vector3.down, new Color(0, 1, 0));
        RaycastHit2D RayHit = Physics2D.Raycast(FrontVec, Vector3.down, 1, LayerMask.GetMask("Platform"));

        // RayHit를 맞으면
        if (RayHit.collider == null)
        {
            if(spriterenderer.flipY != true)
            {
                Turn();
            }
        }
    }

    void Think()
    {
        // 다음 활동 (Set Next Active)
        NextMove = Random.Range(-1, 2);

        // 몬스터의 좌우 변경 (Sprite Animation)
        animator.SetInteger("WalkSpeed", NextMove);
        // Flip Sprite
        if (NextMove != 0)
        {
            spriterenderer.flipX = NextMove == 1;
        }

        // 재귀함수
        float NextThinkTime = Random.Range(1f, 4f);
        Invoke("Think", NextThinkTime);
    }

    void Turn()                                                     // 몬스터 좌우 회전
    {
        NextMove *= -1;
        spriterenderer.flipX = NextMove == 1;
        CancelInvoke();
        Invoke("Think", 2);
    }

    public void OnDamaged()
    {
        // Sprite Alpha
        spriterenderer.color = new Color(1, 1, 1, 0.5f);
        // Sprite Flip Y
        spriterenderer.flipY = true;
        // Collider Disable
        capsuleCollider.enabled = false;
        // Die Effect Jump
        rigid.AddForce(Vector2.up * 5, ForceMode2D.Impulse);
        // Destroy
        Invoke("DeActive", 5);
    }

     void DeActive()
    {
        gameObject.SetActive(false);
    }
}
