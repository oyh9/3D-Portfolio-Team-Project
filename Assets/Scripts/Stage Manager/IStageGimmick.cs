using UnityEngine;

public interface IStageGimmick
{
    GimmickType GetGimmickType();
    string GetGimmickID();
    void Activate();                // 기믹들 실행 함수명 통일 필요
}
