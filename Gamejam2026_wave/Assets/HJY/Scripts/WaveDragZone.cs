//using UnityEngine;
//using UnityEngine.EventSystems;

//public class WaveDragZone : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
//{
//    [Tooltip("이 패널이 어느 쪽 테두리인지 설정하세요")]
//    public SpawnEdge myEdge;
//    [Tooltip("드래그 매니저 본체를 연결하세요")]
//    public WaveDragManager manager;

//    public void OnPointerDown(PointerEventData eventData) => manager.OnZonePointerDown(myEdge, eventData);
//    public void OnDrag(PointerEventData eventData) => manager.OnZoneDrag(myEdge, eventData);
//    public void OnPointerUp(PointerEventData eventData) => manager.OnZonePointerUp(myEdge, eventData);
//}