using UnityEngine;

public enum Axis
{
    X = 0,
    Y = 1,
    Z = 2,
}

public class BackgroundShip : MonoBehaviour
{
    [SerializeField] private GameObject shipObject;
    [SerializeField] private float moveSpeed = 10.0f;
    [SerializeField] private float spinIncrement = 1.0f;
    [SerializeField] private bool spin = false;

    [SerializeField] private Axis rotationAxis = Axis.Z;

    private Vector3 targetPosition = Vector3.zero;

    private void Start ()
    {
        SetRandomPosition();
    }

    private void SetRandomPosition ()
    {
        Vector3 newPosition = Random.insideUnitCircle.normalized * Random.Range(20, 30);
        newPosition.z = shipObject.transform.position.z;
        shipObject.transform.position = newPosition;
    }

    public void SpawnShip ()
    {
        SetRandomPosition();

        targetPosition = transform.position - shipObject.transform.position;
        targetPosition.z = 0;

        targetPosition = targetPosition.normalized * (targetPosition.magnitude * 2) - targetPosition;
    }

    private void HandleMovement()
    {
        var direction = targetPosition - shipObject.transform.position;
        direction.z = 0;
        if ( direction.magnitude > 5.0f )
        {
            shipObject.transform.Translate(moveSpeed * Time.fixedDeltaTime * direction.normalized, Space.World);
        }
        else
        {
            Debug.Log("Reached Destination.");
            targetPosition = Vector3.zero;
        }

        if(spin)
        {
            Vector3 rotation = Vector3.zero;
            rotation[(int)rotationAxis] = spinIncrement;
            shipObject.transform.Rotate(rotation);
        }
    }

    private void FixedUpdate ()
    {
        if ( targetPosition == Vector3.zero )
        { return; }

        HandleMovement();
    }
}