using UnityEngine;

public class PathDot : MonoBehaviour
{
    private RectTransform person;
    private RectTransform myRect;

    private void Awake()
    {
        myRect = GetComponent<RectTransform>();
    }

    public void SetPerson(RectTransform targetPerson)
    {
        person = targetPerson;
    }

    void Update()
    {
        if (person == null) return;

        // UI ÁÂÇ¥°è °Å¸® °è»ê
        float distance = Vector2.Distance(myRect.anchoredPosition, person.anchoredPosition);

        // »ç¶÷ÀÌ ´å ±ÙÃ³¸¦ Áö³ª°¡¸é »èÁ¦ (UI ÇÈ¼¿ÀÌ¹Ç·Î °Å¸®¸¦ Á» ´õ ³Ë³ËÇÏ°Ô ÁÜ)
        if (distance <= 20f)
        {
            Destroy(gameObject);
        }
    }
}