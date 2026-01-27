using UnityEngine;

public class CardRotate : MonoBehaviour
{
    [SerializeField] private float rotateSpeed = 45f;   

    void Update()
    {
        transform.Rotate(0, rotateSpeed * Time.deltaTime, 0);
    }
}