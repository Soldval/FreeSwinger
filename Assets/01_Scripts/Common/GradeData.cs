// GradeData.cs

using System;
using UnityEngine;

/// <summary>
/// 成績種類を定義する列挙型
/// </summary>
public enum GradeType
{
    MVPSlugger,
    GoldSlugger,
    SliverSlugger,
    MainSlugger,
    Moudasyo,
    LightHitter,
    None
}

/// <summary>
/// 成績種類に対応する画像データを紐付ける構造体
/// </summary>
[Serializable]
public struct GradeImageData
{
    public GradeType gradeType;  // 成績種類
    public Sprite imageSprite;  // 成績の画像データ
}