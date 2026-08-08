using UnityEngine;

public class PathDot : MonoBehaviour
{

    private Transform person;

    public void SetPerson(Transform targetperson)
    {
        person = targetperson;
    }


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (person == null)
            return;

        float distance = Vector2.Distance(transform.position, person.position);

        if(distance <= 0.2f)
        {
            Destroy(gameObject);
            
        }




    }
}
