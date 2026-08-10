using UnityEngine;

public class WaveAnimationSpawn : MonoBehaviour
{
    [Header("UI Parent")]
    [Tooltip("파도가 생성될 UI 캔버스(또는 패널) 부모 객체를 넣어주세요")]
    public RectTransform waveContainer;

    [Header("Wave Prefabs (UI)")]
    public GameObject[] wavePrefabs; // 0: 1칸짜리, 1: 2칸짜리, 2: 3칸짜리 (RectTransform 기반 프리팹)

    [Header("Grid Info (UI RectTransforms)")]
    // 기존 Transform 배열을 RectTransform으로 변경!
    public RectTransform[] columnPositions; // 열(X) 중심 좌표 3개 (Left/Right 발사 시 사용)
    public RectTransform[] rowPositions;    // 행(Y) 중심 좌표 3개 (Top/Bottom 발사 시 사용)

    [Header("Edge Anchored Positions")]
    // UI Canvas 기준의 anchoredPosition 픽셀 좌표값입니다.
    public float bottomEdgeY;
    public float topEdgeY;
    public float leftEdgeX;
    public float rightEdgeX;

    public void SpawnWave(WaveInputData inputData)
    {
        int waveSize = inputData.waveSize;
        int minIdx = inputData.laneIndex;
        int maxIdx = minIdx + waveSize - 1; // 끝 칸 번호 도출

        Vector2 spawnPosition = Vector2.zero;
        Quaternion spawnRotation = Quaternion.identity;
        float centerCoord;

        switch (inputData.edge)
        {
            case SpawnEdge.Bottom:
                spawnRotation = Quaternion.Euler(0, 0, 180);
                centerCoord = (columnPositions[minIdx].anchoredPosition.x + columnPositions[maxIdx].anchoredPosition.x) / 2f;
                spawnPosition = new Vector2(centerCoord, bottomEdgeY);
                break;

            case SpawnEdge.Top:
                spawnRotation = Quaternion.Euler(0, 0, 0);
                centerCoord = (columnPositions[minIdx].anchoredPosition.x + columnPositions[maxIdx].anchoredPosition.x) / 2f;
                spawnPosition = new Vector2(centerCoord, topEdgeY);
                break;

            case SpawnEdge.Left:
                spawnRotation = Quaternion.Euler(0, 0, 90);
                centerCoord = (rowPositions[minIdx].anchoredPosition.y + rowPositions[maxIdx].anchoredPosition.y) / 2f;
                spawnPosition = new Vector2(leftEdgeX, centerCoord);
                break;

            case SpawnEdge.Right:
                spawnRotation = Quaternion.Euler(0, 0, -90);
                centerCoord = (rowPositions[minIdx].anchoredPosition.y + rowPositions[maxIdx].anchoredPosition.y) / 2f;
                spawnPosition = new Vector2(rightEdgeX, centerCoord);
                break;
        }

        // 1. 오브젝트 풀에서 비주얼 파도 애니메이션 생성
        GameObject spawnedWave = WavePoolManager.Instance.SpawnFromPool(waveSize, waveContainer, spawnPosition, spawnRotation);

        // 2. 파도 안에 숨어있는 WaveFront(히트박스)를 찾아 초기화 전달
        if (spawnedWave != null)
        {
            WaveFront front = spawnedWave.GetComponentInChildren<WaveFront>();
            if (front != null)
            {
                front.Initialize(waveSize);
            }
        }
    }
}
