using UnityEngine;

public class ObjectRotator : MonoBehaviour
{
    public bool canRotate = true;
    float speed = 500.0f; //how fast the object should rotate

    void Update()
    {
        if(canRotate)
        {
            if (Input.GetMouseButton(1))
            {
                float xInput = Input.GetAxis("Mouse X");
                float yInput = Input.GetAxis("Mouse Y");

                transform.Rotate(new Vector3(yInput, -xInput, 0.0f) * Time.deltaTime * speed, Space.World);
            }
        }
        
    }
}

