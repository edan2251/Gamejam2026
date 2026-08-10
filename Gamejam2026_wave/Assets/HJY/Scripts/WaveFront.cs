using System.Collections.Generic;
using UnityEngine;
using static Person;

public class WaveFront : MonoBehaviour
{
    private List<Person> sweptPersons = new List<Person>();

    public void Initialize(int size)
    {
        sweptPersons.Clear();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Wall")) return;

        Person person = collision.GetComponent<Person>();
        // 중복 추가 방지를 위해 !sweptPersons.Contains(person) 추가
        if (person != null && person.currentState == PersonState.Moving && !sweptPersons.Contains(person))
        {
            person.HitByWave();
            person.transform.SetParent(this.transform);
            sweptPersons.Add(person);

            // 디버그: 누가 파도에 탔는지 이름 확인
            Debug.Log($"[{gameObject.name}] 파도가 {person.gameObject.name}을(를) 휩쓸었습니다! 현재 탑승 인원: {sweptPersons.Count}");
        }
    }

    public void ProcessWaveEndScoreAndCleanup()
    {
        int scoreGained = sweptPersons.Count;

        if (scoreGained > 0)
        {
            Debug.Log($"파도 연출 종료! 득점: +{scoreGained}명 밀어냄");
            if (UIManager.Instance != null)
            {
                UIManager.Instance.AddScore(scoreGained);
            }
        }

        foreach (var person in sweptPersons)
        {
            if (person != null) Destroy(person.gameObject);
        }
        sweptPersons.Clear();
    }
}