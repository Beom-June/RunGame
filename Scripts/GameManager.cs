using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    // 점수와 스테이지 관리
    public int TotalPoint;
    public int StagePoint;
    public int StageIndex;
    public int Health;

    
    public PlayerMove playermove;
    public GameObject[] Stages;

    // UI 관련
    public Image[] UIHealth;
    public Text UIPoint;
    public Text UIStage;
    public GameObject UIRestartBtn;

    void Update()
    {
        UIPoint.text = (TotalPoint + StagePoint).ToString();
    }
    public void NextStage()
    {
        //스테이지 변경 (Change Stage)
        if(StageIndex < Stages.Length -1 )
        {
            Stages[StageIndex].SetActive(false);
            StageIndex++;
            Stages[StageIndex].SetActive(true);
            PlayerReposition();

            UIStage.text = "STAGE " + (StageIndex + 1);
        }
        // 게임 클리어 (Game Clear)
        else
        {
            // 플레이어 컨트롤 락 (Player Control Lock)
            Time.timeScale = 0;
            // 결과 UI (Result UI)
            Debug.Log("게임 클리어");
            // 재시작 버튼 UI (Restart Button UI) >> 게임이 끝났을 때 활성화
            // 버튼 텍스트는 자식오브젝트이므로 InChildren을 덧 붙여야 함.
            Text BtnText = UIRestartBtn.GetComponentInChildren<Text>();
            BtnText.text = "☆ Clear ☆";
            UIRestartBtn.SetActive(true);
        }
        // 포인트 계산 (Calculate Point)
        TotalPoint += StagePoint;
        StagePoint = 0;
    }
    public void HealthDown()
    {
        if (Health > 1)
        {
            Health--;
            // 색상 어둡게 변경
            UIHealth[Health].color = new Color(1, 0, 0, 0.3f);
        }
        else
        {
            // 모든 체력 UI 끄기 (All Health UI Off)
            UIHealth[0].color = new Color(1, 0, 0, 0.3f);
            // 플레이어 죽는 효과 (Player Die Effect)
            playermove.OnDie();
            playermove.PlaySound("DIE");
            // 결과 UI (Result UI)
            Debug.Log("죽었습니다.");
            // 재시작 버튼 UI(Retry Button UI)
            UIRestartBtn.SetActive(true);
        }
    }
    void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.tag == "Player")
        {
            // 플레이어 원위치 (Player Reposition).... 마지막 체력에서 낭떨어지일 땐, 원위치 하지 않기
            if (Health > 1)
            {
                PlayerReposition();
                Debug.Log("플레이어 떨어짐");
            }
            //체력 감소
            HealthDown();
        }
    }
    void PlayerReposition()
    {
        playermove.transform.position = new Vector3(0, 0, -1);
        playermove.VelocityZero();
    }
    public void Restart()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene(0);
    }
}
