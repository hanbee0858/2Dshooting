using UnityEngine;
public class Test : MonoBehaviour
{
    private int speed = 3;
    void Update()
    {
        transform.position += movePosition();
        Debug.Log("dfdaf" + speed);
    }
    private Vector3 movePosition()
    {
        return Vector3.up * Time.deltaTime * speed;
    }
}
