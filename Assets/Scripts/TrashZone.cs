using UnityEngine;
using UnityEngine.EventSystems;
using PrimeTween;

public class TrashZone : MonoBehaviour, IDropHandler
{
    public void OnDrop(PointerEventData eventData)
    {
        // Kiểm tra xem có thả lá bài vào không
        Card card = eventData.pointerDrag?.GetComponent<Card>();
        if (card != null)
        {
            //AudioManager.instance.PlayTrashZoneSFX();
            // Gọi animation xoá lá bài
            //card.DestroyWithAnimation();
        }
    }
}
