// EffectController.cs

using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// トレイルやエフェクトを管理するクラス
/// </summary>
public class EffectController : MonoBehaviour
{
    [Header("トレイル参照設定")]
    [SerializeField] private Transform topPoint;
    [SerializeField] private Transform underPoint;

    [Header("通常スイングのトレイル設定")]
    [SerializeField] private Material normalMaterial;
    [SerializeField] private float normalThickness = 0.5f;

    [Header("チャージスイングのトレイル設定")]
    [SerializeField] private Material chargeMaterial;
    [SerializeField] private float chargeThickness = 1.2f;

    [Header("チャージ中エフェクト設定")]
    [SerializeField] private GameObject normalChargeEffectPrefab;
    [SerializeField] private GameObject maxChargeEffectPrefab;
    [SerializeField] private Transform effectSpawnPoint;

    [Header("その他トレイル設定")]
    [SerializeField] private int maxVertexCount = 20;
    [SerializeField] private float minEmitDistance = 0.05f;

    // 内部変数(トレイル用)
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

        // 初期マテリアル設定
        if(normalMaterial != null)
        {
            meshRenderer.material = normalMaterial;
        }

        trailMesh = new Mesh();
        meshFilter.mesh = trailMesh;

        if(effectSpawnPoint == null)
        {
            effectSpawnPoint = transform;
        }
    }

    void LateUpdate()
    {
        if(!isEmitting || topPoint == null || underPoint == null)
        {
            if(trailMesh.vertexCount > 0)
            {
                trailMesh.Clear();
            }
            return;
        }

        // 一定距離動いたら新しい頂点を記憶する
        Vector3 currentTop = topPoint.position;
        Vector3 currentUnder = underPoint.position;

        if(topHistory.Count == 0 || Vector3.Distance(topHistory[0], currentTop) > minEmitDistance)
        {
            topHistory.Insert(0, currentTop);
            underHistory.Insert(0, currentUnder);

            // 最大数超えたら古いものを削除する
            if(topHistory.Count > maxVertexCount)
            {
                topHistory.RemoveAt(topHistory.Count - 1);
                underHistory.RemoveAt(underHistory.Count - 1);
            }
        }

        // 履歴が2つ以上あればメッシュを構築する
        if(topHistory.Count >= 2)
        {
            BuildMesh();
        }
    }

    /// <summary>
    /// チャージ開始時のエフェクト処理
    /// </summary>
    public void StartChargeEffect()
    {
        wasMaxCharged = false;
        ClearEffects();

        if(normalChargeEffectPrefab != null)
        {
            activeNormalEffect = Instantiate(normalChargeEffectPrefab, effectSpawnPoint.position, effectSpawnPoint.rotation, effectSpawnPoint);
        }
    }

    /// <summary>
    /// チャージ中のフレーム更新
    /// </summary>
    /// <param name="currentTimer"></param>
    /// <param name="threshold"></param>
    public void UpdateChargeProgress(float currentTimer, float threshold)
    {
        if(!wasMaxCharged && currentTimer >= threshold)
        {
            wasMaxCharged = true;

            // 通常チャージを消してマックスチャージ用エフェクトを生成
            if (activeNormalEffect != null)
            {
                Destroy(activeNormalEffect);
            }
             
            if(maxChargeEffectPrefab != null)
            {
                activeMaxEffect = Instantiate(maxChargeEffectPrefab, effectSpawnPoint.position, effectSpawnPoint.rotation, effectSpawnPoint);
            }
        }
    }

    /// <summary>
    /// スイング開始時の処理
    /// </summary>
    /// <param name="isCharged"></param>
    public void StartSwingEffect(bool isCharged)
    {
        ClearEffects();

        isEmitting = true;
        isChargedTrail = isCharged;
        topHistory.Clear();
        underHistory.Clear();

        if(meshRenderer != null)
        {
            meshRenderer.material = isCharged ? chargeMaterial : normalMaterial;
        }
    }

    /// <summary>
    /// すべてのエフェクトを停止・消去する
    /// </summary>
    public void StopAllEffects()
    {
        isEmitting = false;
        isChargedTrail = false;
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
        if(activeNormalEffect != null)
        {
            Destroy(activeNormalEffect);
            activeNormalEffect = null;
        }

        if(activeMaxEffect != null)
        {
            Destroy(activeMaxEffect);
            activeMaxEffect= null;
        }
    }

    /// <summary>
    /// TopとUnderの軌跡を繋ぐMeshを生成する処理
    /// </summary>
    private void BuildMesh()
    {
        int count = topHistory.Count;
        Vector3[] vertices = new Vector3[count * 4];
        Vector2[] uv = new Vector2[count * 4];
        int[] triangles = new int[(count - 1) * 24];

        // チャージ状態に応じてトレイル太さを切替える
        float currentThickness = isChargedTrail ? chargeThickness : normalThickness;
        Vector3 thicknessOffset = new Vector3(currentThickness, currentThickness, currentThickness);

        for(int i = 0; i < count; i++)
        {
            Vector3 top = topHistory[i];
            Vector3 under = underHistory[i];

            // ローカル座標系に変換しつつ厚みを持たせた4つの頂点を作成
            vertices[i * 4] = transform.InverseTransformPoint(top);
            vertices[i * 4 + 1] = transform.InverseTransformPoint(under);
            vertices[i * 4 + 2] = transform.InverseTransformPoint(top + thicknessOffset);
            vertices[i * 4 + 3] = transform.InverseTransformPoint(under + thicknessOffset);

            float v = (float)i / (count - 1);
            uv[i * 4] = new Vector2(0, v);
            uv[i * 4 + 1] = new Vector2(1, v);
            uv[i * 4 + 2] = new Vector2(0, v);
            uv[i * 4 + 3] = new Vector2(1, v);
        }

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

            // 面3(上面)
            triangles[triIndex++] = baseIndex;
            triangles[triIndex++] = baseIndex + 2;
            triangles[triIndex++] = nextBaseIndex;

            triangles[triIndex++] = nextBaseIndex;
            triangles[triIndex++] = baseIndex + 2;
            triangles[triIndex++] = nextBaseIndex + 2;

            // 面4(下面)
            triangles[triIndex++] = baseIndex + 1;
            triangles[triIndex++] = nextBaseIndex + 1;
            triangles[triIndex++] = baseIndex + 3;

            triangles[triIndex++] = baseIndex + 3;
            triangles[triIndex++] = nextBaseIndex + 1;
            triangles[triIndex++] = nextBaseIndex + 3;
        }

        trailMesh.Clear();
        trailMesh.vertices = vertices;
        trailMesh.uv = uv;
        trailMesh.triangles = triangles;
        trailMesh.RecalculateNormals();
    }
}
