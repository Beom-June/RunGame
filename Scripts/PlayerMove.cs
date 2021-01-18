using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMove : MonoBehaviour
{
    public GameManager gameManager;
    public AudioClip AudioJump;
    public AudioClip AudioAttack;
    public AudioClip AudioDamaged;
    public AudioClip AudioItem;
    public AudioClip AudioDie;
    public AudioClip AudioFinish;

    public float MaxSpeed;
    public float JumpPower;

    Rigidbody2D rigid;
    SpriteRenderer spriterenderer;
    Animator animator;
    CapsuleCollider2D capsulePlayer;
    AudioSource audioSource;

    void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        spriterenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        capsulePlayer = GetComponent<CapsuleCollider2D>();
        audioSource = GetComponent<AudioSource>();
    }

    void Update()                                                           // 단발적인 키입력은 Update
    {
        // 점프.... && 이후는 무한 점프 막기
        if(Input.GetButtonDown("Jump") && !animator.GetBool("isJumping"))
        {
            rigid.AddForce(Vector2.up * JumpPower, ForceMode2D.Impulse);
            animator.SetBool("isJumping", true);
            PlaySound("JUMP");
        }
        // 멈추는 속도
        if(Input.GetButtonUp("Horizontal"))
        {
            rigid.velocity = new Vector2(rigid.velocity.normalized.x * 0.5f, rigid.velocity.y);
        }

        // 방향 전환
        if (Input.GetButton("Horizontal"))
        {
            spriterenderer.flipX = Input.GetAxisRaw("Horizontal") == -1;
        }

        // 애니메이션. Mathf : 수학함수. Abs : 절대값
        if(Mathf.Abs (rigid.velocity.x) < 0.3)
        {
            animator.SetBool("isWalking", false);
        }
        else
        {
            animator.SetBool("isWalking", true);
        }
    }

    void FixedUpdate()
    {
        // 키 컨트롤 움직임
        float h = Input.GetAxisRaw("Horizontal");

        rigid.AddForce(Vector2.right * h, ForceMode2D.Impulse);

        // 최대 속도 제한
        if(rigid.velocity.x > MaxSpeed)                                         // 오른쪽 속도 제한
        {
            rigid.velocity = new Vector2(MaxSpeed, rigid.velocity.y);
        }
        else if (rigid.velocity.x < MaxSpeed * (-1))                                   // 왼쪽 속도 제한
        {
            rigid.velocity = new Vector2(MaxSpeed * (-1), rigid.velocity.y);
        }

        // RayCast.... Lading Platform
        if(rigid.velocity.y < 0)                                        // 착지 할 때만
        {
            Debug.DrawRay(rigid.position, Vector3.down, new Color(0, 1, 0));
            RaycastHit2D RayHit = Physics2D.Raycast(rigid.position, Vector3.down, 1, LayerMask.GetMask("Platform"));

            // RayHit를 맞으면
            if (RayHit.collider != null)
            {
                if (RayHit.distance < 0.5f)
                {
                    //Debug.Log(RayHit.collider.name);
                    animator.SetBool("isJumping", false);
                }
            }
        }
    }

    // 충돌 이벤트
     void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.gameObject.tag == "Enemy")
        {
            // 오류 검토
            // 몬스터를 밟았을 때 공격 (Attack) >> 바닥 위치 &&뒤는 우리의 현재 위치
            if (rigid.velocity.y < 0 && transform.position.y > collision.transform.position.y)
            {
                Debug.Log("공격");
                OnAttack(collision.transform);
                PlaySound("ATTACK");
            }
            // 데미지를 입음 (Damaged)
            else 
            {
                Debug.Log("데미지를 입음");
                OnDamaged(collision.transform.position);
                PlaySound("DAMAGED");
            }
        }
    }

     void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.tag == "Item")
        {
            // Point
            bool isBronze = collision.gameObject.name.Contains("Bronze");
            bool isSilver = collision.gameObject.name.Contains("Silver");
            bool isGold = collision.gameObject.name.Contains("Gold");

            if (isBronze)
            {
                gameManager.StagePoint += 50;
            }
            else if (isSilver)
            {
                gameManager.StagePoint += 100;
            }
            else if (isGold)
            {
                gameManager.StagePoint += 200;
            }
            /*else
            {
                gameManager.StagePoint += 150;
            }*/
            // Item 사라짐 (Deactive)
            collision.gameObject.SetActive(false);
            PlaySound("ITEM");
        }
        else if(collision.gameObject.tag == "Finish")
        {
            // Next Stage
            gameManager.NextStage();
            PlaySound("FINISH");
        }
    }

    void OnAttack(Transform enemy)
    {
        // 포인트 (Point)
        gameManager.StagePoint += 100;

        // 밟았을 때 반발력
        rigid.AddForce(Vector2.up * 5, ForceMode2D.Impulse);
        // 적 죽음 (Enemy Die)
        EnemyMove enemyMove = enemy.GetComponent<EnemyMove>();
        enemyMove.OnDamaged();
    }

    // 충돌시 무적 시간
    void OnDamaged(Vector2 TargetPos)
    {
        // 체력 감소 (Health --)
        gameManager.HealthDown();
        // Layer 변경
        gameObject.layer = 11;
        spriterenderer.color = new Color(1,1,1,0.4f);

        // 튕겨나가는 액션
        int dirc = transform.position.x - TargetPos.x > 0 ? 1 : -1;
        rigid.AddForce(new Vector2(dirc, 1) * 7, ForceMode2D.Impulse);

        // Animation
        animator.SetTrigger("DoDamaged");
        Invoke("OffDamaged", 0.8f);
    }

    //무적 해제 함수 >> 원래 상태로
    void OffDamaged()
    {
        gameObject.layer = 10;
        spriterenderer.color = new Color(1, 1, 1, 1);
    }
    public void OnDie()
    {
        // Sprite Alpha
        spriterenderer.color = new Color(1, 1, 1, 0.5f);
        // Sprite Flip Y
        spriterenderer.flipY = true;
        // Collider Disable
        capsulePlayer.enabled = false;
        // Die Sound
        PlaySound("DIE");
        // Die Effect Jump
        rigid.AddForce(Vector2.up * 5, ForceMode2D.Impulse);
    }

    public void VelocityZero()
    {
        rigid.velocity = Vector2.zero;
    }
    
    // 사운드 함수 (Sound)
    public void PlaySound(string action)
    {
        switch(action)
        {
            case "JUMP" : 
                audioSource.clip = AudioJump; 
                break;
            case "ATTACK" :
                audioSource.clip = AudioAttack;
                break;
            case "DAMAGED":
                audioSource.clip = AudioDamaged;
                break;
            case "ITEM" :
                audioSource.clip = AudioItem;
                break;
            case "DIE" :
                Debug.Log("죽음");
                audioSource.clip = AudioDie;
                break;
            case "FINISH":
                audioSource.clip = AudioFinish;
                break;
        }
        audioSource.Play();
    }
}
