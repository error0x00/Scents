using UnityEngine;

// 공통 열거형 정의
public enum ScentTagType
{
    Material,  // 재료용
    Perfume    // 향수용
}

public enum EventType
{
    Main,
    Sub
}

public enum NotePosition
{
    Top,
    Middle,
    Base
}

public enum ScentFamily
{
    Floral,
    Fruity,
    Woody,
    Animalic,
    Citrus,
    Spicy,
    Aquatic,
    Earthy,
    Powdery
}

public enum ScentGender
{
    Feminine,
    Masculine,
    Unisex
}

// 압착 미니게임 다듬기 조작 방식 분류
public enum MaterialType
{
    Root,       // 뿌리류 - 칼질 연타
    Wood,       // 목질류 - 원형 드래그 빻기
    RindResin   // 수지/껍질류 - 직선 드래그 긁어내기
}

// 재료 및 향수 품질 등급
public enum Quality
{
    Low,        // 낮은 품질
    Normal,     // 평범한 품질
    Good,       // 좋은 품질
    Excellent   // 뛰어난 품질 (스킬 해금 후 획득 가능)
}