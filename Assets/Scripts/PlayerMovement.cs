using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5f; // 移動速度（タイル間移動のスピード）
    private Vector2 targetPosition; // 次の移動先
    private bool isMoving = false; // 移動中
    private bool canMove = false; // 操作許可
    private bool isFootstepPlaying = false;
    private HashSet<Vector3> correctPath; // 正しい経路HashSet
    private List<Vector3> goalPositions; // ゴール位置
    private Vector3 startPosition; // スタート位置

    void Start()
    {
        targetPosition = transform.position; // 初期位置を現在位置に設定
        startPosition = transform.position; // スタート位置を保存
    }

    public void EnableControl(List<Vector3> path, List<Vector3> goals)
    {
        canMove = true; 
        correctPath = new HashSet<Vector3>(path); // 経路情報をHashSetに変換
        goalPositions = goals; // ゴール位置
    }

    void Update()
    {
        // ゲームオーバー中またはステージ間の場合は動作しない
        if (GameManager.Instance.isGameOver || GameManager.Instance.isStageTransitioning)
            return;
        
        if (!canMove || isMoving) return; 

        // キー入力処理
        Vector2 inputDirection = Vector2.zero;
        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow)) inputDirection = Vector2.up;
        if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow)) inputDirection = Vector2.down;
        if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow)) inputDirection = Vector2.left;
        if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow)) inputDirection = Vector2.right;

        // 移動先を計算
        if (inputDirection != Vector2.zero)
        {
            Vector2 newPosition = (Vector2)transform.position + inputDirection;

            // 移動先が経路に含まれているか確認
            if (correctPath.Contains(newPosition))
            {
                if (!isFootstepPlaying)
                {
                    SoundManager.Instance.PlaySE(SESoundData.SE.Asioto);
                    isFootstepPlaying = true; 
                }
                targetPosition = newPosition; // 新しい移動先を設定
                StartCoroutine(MoveToTarget()); // 移動を開始

                // タイルを明るくする
                Collider2D tile = Physics2D.OverlapPoint(newPosition);
                if (tile != null)
                {
                    LightController.Instance.ActivateTile(tile.gameObject);
                }

                // ゴール到達判定
                CheckGoals(newPosition);
            }
            else
            {
                OnWrongPath(); // 道がない場合の処理
            }
        }
    }

    // 一マス移動
    private IEnumerator MoveToTarget()
    {
        isMoving = true; 
        
        while ((Vector2)transform.position != targetPosition)
        {
            transform.position = Vector2.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
            yield return null;
        }

        isMoving = false; 
        isFootstepPlaying = false;
    }

    private void CheckGoals(Vector3 position)
    {
        // ゴール到達判定
        for (int i = goalPositions.Count - 1; i >= 0; i--)
        {
            if (Vector3.Distance(position, goalPositions[i]) < 0.1f)
            {
                Debug.Log("ゴールに到達しました！");
                goalPositions.RemoveAt(i); // 到達したゴールをリストから削除
                GameManager.Instance.OnGoalReached(position); // ゴール到達を通知
            }
        }
    }

    private void OnWrongPath()
    {
        Debug.Log("間違った経路を選択しました。スタート地点に戻ります。");
        canMove = false; // 一時的に操作を停止
        GameManager.Instance.OnWrongPath(); // GameManagerに通知
        StartCoroutine(ReturnToStart());
    }

    public IEnumerator ReturnToStart()
    {
        // プレイヤーをスタート地点に戻す処理
        while ((Vector2)transform.position != (Vector2)startPosition)
        {
            transform.position = Vector2.MoveTowards(transform.position, startPosition, moveSpeed * Time.deltaTime);
            yield return null;
        } 

        canMove = true; // 再び操作を許可
    }
}