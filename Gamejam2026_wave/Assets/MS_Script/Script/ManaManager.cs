using UnityEngine;

public class ManaManager : MonoBehaviour
{
    [Header("Mana")]
    [SerializeField] private int mana = 10;

    public int CurrentMana => mana;


    private void Start()
    {
        Debug.Log(
            "현재 Mana : " +
            mana
        );
    }


    // =========================================================
    // Mana 사용
    // =========================================================

    public bool UseMana(int amount)
    {
        // 잘못된 값 방지
        if (amount <= 0)
        {
            return false;
        }


        // Mana 부족
        if (mana < amount)
        {
            Debug.Log(
                "Mana 부족! 현재 Mana : " +
                mana +
                " / 필요한 Mana : " +
                amount
            );

            return false;
        }


        // Mana 감소
        mana -= amount;


        Debug.Log(
            "Mana " +
            amount +
            " 소모! 남은 Mana : " +
            mana
        );


        return true;
    }
}