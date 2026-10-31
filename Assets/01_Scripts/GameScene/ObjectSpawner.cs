// ObjectSpawner.cs

using System.Collections;
using UnityEngine;

/// <summary>
/// オブジェクトのランダム生成用クラス
/// </summary>
public class ObjectSpawner : MonoBehaviour
{
    [Header("スポーンさせるオブジェクト(プレハブ)")]
    [SerializeField] private GameObject[] spawnPrefabs;

    [Header("スポーンさせる時間間隔")]
    [SerializeField] private float spawnInterval = 3.0f;

    [Header("スポーンさせる範囲")]
    [SerializeField] private float minX = -167f;
    [SerializeField] private float maxX = 167f;
    [SerializeField] private float minZ = -142.5f;
    [SerializeField] private float maxZ = 142.5f;

    [Header("スポーンさせる空中の高さ")]
    [SerializeField] private float spawnY = 30.0f;

    void Start()
    {
        // ゲーム開始から一定時間ごとにオブジェクトをランダム生成するコルーチンを開始
        StartCoroutine(SpawnRoutine());
    }

    /// <summary>
    /// 一定時間ごとにランダム生成処理を呼び出す処理(コルーチン)
    /// </summary>
    /// <returns>待機時間 IEnumerator</returns>
    private IEnumerator SpawnRoutine()
    {
        while(true)
        {
            // 一定時間待機
            yield return new WaitForSeconds(spawnInterval);

            // ランダム生成を実行
            SpawnObject();
        }
    }

    /// <summary>
    /// ランダムに1つ選択したオブジェクトをランダム位置に生成する処理
    /// </summary>
    private void SpawnObject()
    {
        if (spawnPrefabs == null || spawnPrefabs.Length == 0) return;

        // プレハブ配列からランダムに1つ選択
        int randomIndex = Random.Range(0, spawnPrefabs.Length);
        GameObject targetPrefab = spawnPrefabs[randomIndex];

        // ランダムな位置を設定
        float randomX = Random.Range(minX, maxX);
        float randomZ = Random.Range(minZ, maxZ);
        Vector3 spawnPosition = new Vector3(randomX, spawnY, randomZ);

        // 空中にオブジェクトを生成
        Instantiate(targetPrefab, spawnPosition, Quaternion.identity);

        Debug.Log($"オブジェクトを生成しました！{spawnPosition}");
    }
}
