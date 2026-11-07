// EffectController.cs

using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// メッシュやエフェクトを管理するクラス
/// </summary>
public class EffectController : MonoBehaviour
{
    [Header("チャージスイングのメッシュ設定")]
    [SerializeField] private Material chargeMaterial;
    [SerializeField] private float chargeThickness = 1.2f;

    [Header("チャージ中エフェクト設定")]
    [SerializeField] private GameObject normalChargeEffectPrefab;
    [SerializeField] private GameObject maxChargeEffectPrefab;
    [SerializeField] private Transform effectSpawnPoint;

    [Header("バットのメッシュ参照設定")]
    [SerializeField] private Transform topPoint;
    [SerializeField] private Transform underPoint;

    [Header("通常スイングのメッシュ設定")]
    [SerializeField] private Material normalMaterial;
    [SerializeField] private float normalThickness = 0.5f;

    [Header("その他メッシュ設定")]
    [SerializeField] private int maxVertexCount = 20;
    [SerializeField] private float minEmitDistance = 0.05f;

    // 内部変数(メッシュ用)
    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;
    private Mesh trailMesh;
    private List<Vector3> topHistory = new List<Vector3>();
    private List<Vector3> underHistory = new List<Vector3>();
    private bool isEmitting = false;
    private bool isChargedTrail = false;

    // 内部変数(エフェクト用)
    private GameObject activeNormalEffect;
    private GameObject activeMaxEffect;
    private bool wasMaxCharged = false;

    void Start()
    {
        // MeshFilterとMeshRendererを自動追加または取得
        meshFilter = gameObject.GetComponent<MeshFilter>();
        if (meshFilter == null)
        {
            meshFilter = gameObject.AddComponent<MeshFilter>();
        }
        meshRenderer = gameObject.GetComponent<MeshRenderer>();
        if(meshRenderer == null)
        {
            meshRenderer = gameObject.AddComponent<MeshRenderer>();
        }

        if(normalMaterial != null)
        {
            meshRenderer.material = normalMaterial;
        }

        // メッシュの初期化
        trailMesh = new Mesh();
        meshFilter.mesh = trailMesh;
        if(effectSpawnPoint == null)
        {
            effectSpawnPoint = transform;
        }
    }

    void LateUpdate()
    {
        // メッシュの削除(トレイル未設定や古いトレイルが残っている場合)
        if(!isEmitting || topPoint == null || underPoint == null)
        {
            if(trailMesh.vertexCount > 0)
            {
                trailMesh.Clear();
            }
            return;
        }

        Vector3 currentTop = topPoint.position;
        Vector3 currentUnder = underPoint.position;

        // ---- 履歴がまだ0もしくは直近で記録した履歴が一定距離移動しているかを判定
        // 少しの移動で大量データ作成されることを防ぐ
        if(topHistory.Count == 0 || Vector3.Distance(topHistory[0], currentTop) > minEmitDistance)
        {
            topHistory.Insert(0, currentTop);
            underHistory.Insert(0, currentUnder);

            if(topHistory.Count > maxVertexCount)
            {
                topHistory.RemoveAt(topHistory.Count - 1);
                underHistory.RemoveAt(underHistory.Count - 1);
            }
        }

        if(topHistory.Count >= 2)
        {
            BuildMesh();
        }
    }

    #region チャージ関係の処理 ----

    /// <summary>
    /// チャージ開始時のエフェクト処理
    /// </summary>
    public void StartChargeEffect()
    {
        wasMaxCharged = false;
        ClearEffects();
        AudioManager.PlaySECharge();

        // チャージエフェクトがアタッチされている位置情報でエフェクトを生成する
        if(normalChargeEffectPrefab != null)
        {
            activeNormalEffect = Instantiate(normalChargeEffectPrefab, effectSpawnPoint.position, effectSpawnPoint.rotation, effectSpawnPoint);
        }
    }

    /// <summary>
    /// チャージ中のフレーム更新
    /// </summary>
    /// <param name="currentTimer">今のチャージ時間</param>
    /// <param name="threshold">チャージ完了時間</param>
    public void UpdateChargeProgress(float currentTimer, float threshold)
    {
        if(!wasMaxCharged && currentTimer >= threshold)
        {
            wasMaxCharged = true;

            if (activeNormalEffect != null)
            {
                Destroy(activeNormalEffect);
            }
            if(maxChargeEffectPrefab != null)
            {
                AudioManager.PlaySEMaxCharge();
                // マックスチャージエフェクトがアタッチされている位置情報でエフェクトを生成する
                activeMaxEffect = Instantiate(maxChargeEffectPrefab, effectSpawnPoint.position, effectSpawnPoint.rotation, effectSpawnPoint);
            }
        }
    }

    #endregion // チャージ関係の処理 ----

    #region スイング関係の処理 ----

    /// <summary>
    /// スイング開始時の処理
    /// </summary>
    /// <param name="isCharged">チャージ状態</param>
    public void StartSwingEffect(bool isCharged)
    {
        ClearEffects();

        isEmitting = true;
        isChargedTrail = isCharged;
        topHistory.Clear();
        underHistory.Clear();

        if(!isCharged)
        {
            AudioManager.PlaySELeftSwing();
        }
        else
        {
            AudioManager.PlaySEChargeLeftSwing();
        }

        if(meshRenderer != null)
        {
            meshRenderer.material = isCharged ? chargeMaterial : normalMaterial;
        }
    }

    /// <summary>
    /// スイング時にTopとUnderの軌跡を繋ぐMeshを生成する処理
    /// </summary>
    private void BuildMesh()
    {
        // ---- メッシュ生成のためのメモリ確保
        int count = topHistory.Count;  // 履歴の数
        Vector3[] vertices = new Vector3[count * 4];  // 3Dの頂点配列(上側、下側、上側厚み、下側厚み)
        Vector2[] uv = new Vector2[count * 4];  // UV座標配列(頂点数と一致させる)
        int[] triangles = new int[(count - 1) * 24];  // 1つの頂点につき6個のインデックスが必要

        // ---- チャージ状態に応じてトレイル太さを切替える
        float currentThickness = isChargedTrail ? chargeThickness : normalThickness;
        Vector3 thicknessOffset = new Vector3(currentThickness, currentThickness, currentThickness);

        // ---- 履歴1つずつに4つの頂点とUV座標を作成する
        for (int i = 0; i < count; i++)
        {
            Vector3 top = topHistory[i];
            Vector3 under = underHistory[i];

            // ローカル座標系に変換しつつ4つの頂点を作成
            vertices[i * 4] = transform.InverseTransformPoint(top);
            vertices[i * 4 + 1] = transform.InverseTransformPoint(under);
            vertices[i * 4 + 2] = transform.InverseTransformPoint(top + thicknessOffset);
            vertices[i * 4 + 3] = transform.InverseTransformPoint(under + thicknessOffset);

            // メッシュの長さに沿ってUV座標を作成
            float v = (float)i / (count - 1);
            uv[i * 4] = new Vector2(0, v);
            uv[i * 4 + 1] = new Vector2(1, v);
            uv[i * 4 + 2] = new Vector2(0, v);
            uv[i * 4 + 3] = new Vector2(1, v);
        }

        // ---- 4つの頂点を繋ぎ合わせ3Dポリゴンを作成する
        int triIndex = 0;
        for (int i = 0; i < count - 1; i++)
        {
            int baseIndex = i * 4;
            int nextBaseIndex = (i + 1) * 4;

            // 面1(前面)
            triangles[triIndex++] = baseIndex;
            triangles[triIndex++] = nextBaseIndex;
            triangles[triIndex++] = baseIndex + 1;

            triangles[triIndex++] = baseIndex + 1;
            triangles[triIndex++] = nextBaseIndex;
            triangles[triIndex++] = nextBaseIndex + 1;

            // 面2(後面)
            triangles[triIndex++] = baseIndex + 2;
            triangles[triIndex++] = baseIndex + 3;
            triangles[triIndex++] = nextBaseIndex + 2;

            triangles[triIndex++] = baseIndex + 3;
            triangles[triIndex++] = nextBaseIndex + 3;
            triangles[triIndex++] = nextBaseIndex + 2;

            // 面3(上部厚み)
            triangles[triIndex++] = baseIndex;
            triangles[triIndex++] = baseIndex + 2;
            triangles[triIndex++] = nextBaseIndex;

            triangles[triIndex++] = nextBaseIndex;
            triangles[triIndex++] = baseIndex + 2;
            triangles[triIndex++] = nextBaseIndex + 2;

            // 面4(下部厚み)
            triangles[triIndex++] = baseIndex + 1;
            triangles[triIndex++] = nextBaseIndex + 1;
            triangles[triIndex++] = baseIndex + 3;

            triangles[triIndex++] = baseIndex + 3;
            triangles[triIndex++] = nextBaseIndex + 1;
            triangles[triIndex++] = nextBaseIndex + 3;
        }

        // ---- メッシュをクリアしてから新メッシュを描画する
        trailMesh.Clear();
        trailMesh.vertices = vertices;  // 「4つの頂点」を登録
        trailMesh.uv = uv;  // 「UV座標」を登録
        trailMesh.triangles = triangles;  // 「三角形の面」を登録
        trailMesh.RecalculateNormals();  // 光や影を自動計算
    }

    #endregion // スイング関係の処理 ----

    #region エフェクト・メッシュのクリア処理 ----

    /// <summary>
    /// 全てのメッシュ・エフェクトを停止・消去する
    /// </summary>
    public void StopAllEffects()
    {
        isEmitting = false;
        isChargedTrail = false;

        // 履歴とメッシュをクリアする
        topHistory.Clear();
        underHistory.Clear();
        if(trailMesh != null)
        {
            trailMesh.Clear();
        }

        ClearEffects();
    }

    /// <summary>
    /// 全てのエフェクトをクリアする処理
    /// </summary>
    private void ClearEffects()
    {
        // 通常チャージのエフェクトをクリアする
        if(activeNormalEffect != null)
        {
            Destroy(activeNormalEffect);
            activeNormalEffect = null;
        }

        // マックスチャージのエフェクトをクリアする
        if(activeMaxEffect != null)
        {
            Destroy(activeMaxEffect);
            activeMaxEffect= null;
        }
    }

    #endregion // エフェクト・メッシュのクリア処理 ----
}
