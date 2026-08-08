public enum SpawnEdge { Top, Bottom, Left, Right }

public struct WaveInputData
{
    public SpawnEdge edge;
    public int laneIndex; // 0, 1, 2 (Top/Left가 0 기준)
    public int waveSize;  // 1, 2, 3 (몇 칸짜리 파도인지)
}