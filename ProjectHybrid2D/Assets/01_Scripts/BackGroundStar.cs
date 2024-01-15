using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Burst;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

public enum StarStatus
{
    None = 0,
    Running = 1,
    Stopped = 2,
}

public class BackGroundStar : MonoBehaviour
{
    [Tooltip("Different kind of star Prefabs, if there's only one kind, only insert 1.")]
    [SerializeField] private GameObject[] stars;
    [Tooltip("X min, Y max")]
    [SerializeField] float2 starSpawnRange;
    [Range(0, 150)]
    [SerializeField] private int amountOfStars = 10;
    [SerializeField] private float moveSpeed = 5.0f;
    [SerializeField] private float maxNewPosRetries = 5;
    [SerializeField] private float maxDistanceToCenter = 10.0f;
    private readonly List<StarObject> currentStars = new();
    private Vector3 centerPosition;

    private void Start ()
    {
        SetupStars();
        centerPosition = transform.position;
    }

    private void SetupStars ()
    {
        for ( int i = 0; i < amountOfStars; i++ )
        {
            var newStar = Instantiate(stars[Random.Range(0, stars.Length)], transform);
            newStar.transform.position = Random.insideUnitCircle.normalized * Random.Range(starSpawnRange.x, starSpawnRange.y);
            currentStars.Add(new(newStar.transform));
        }
    }

    private void FixedUpdate ()
    {
        UpdateStars();
    }

    private void UpdateStars ()
    {
        if ( currentStars.Count < amountOfStars )
        { return; }

        for ( int i = 0; i < currentStars.Count; i++ )
        {
            var currentStar = currentStars[i];
            
            if ( currentStar.status == StarStatus.Running )
            {
                Vector3 direction = currentStar.GetTargetPos() - currentStar.transform.position;
                currentStar.transform.Translate(moveSpeed * Time.fixedDeltaTime * direction.normalized);

                if ( direction.magnitude < 1.0f || OutSideOfRange(currentStar.transform.position) )
                {
                    currentStar.status = StarStatus.None;
                }
            }
            if ( currentStar.status == StarStatus.None )
            {
                currentStar.status = StarStatus.Running;
                Vector3 newPosition = CalculateNewPosition(currentStar);
                currentStar.SetTargetPos(newPosition);
            }
        }
    }

    private bool OutSideOfRange ( Vector3 currentPosition )
    {
        return Vector3.Distance(currentPosition, centerPosition) > maxDistanceToCenter;
    }

    private Vector3 CalculateNewPosition ( StarObject currentStar )
    {
        Vector3 newPosition = Random.insideUnitCircle.normalized * Random.Range(0.01f, maxDistanceToCenter);

        if ( Vector3.Distance(newPosition, centerPosition) > maxDistanceToCenter && currentStar.retries < maxNewPosRetries )
        {
            currentStar.retries++;
            return CalculateNewPosition(currentStar);
        }
        else if ( currentStar.retries >= maxNewPosRetries )
        {
            currentStar.transform.position = Random.insideUnitCircle.normalized * Random.Range(0.01f, maxDistanceToCenter);
            currentStar.retries = 0;
            return newPosition;
        }

        currentStar.retries = 0;
        return newPosition;
    }

    [BurstCompile]
    private async Task<bool> AnyStarsMoving ()
    {
        await Awaitable.BackgroundThreadAsync();

        foreach ( var star in currentStars )
        {
            if ( star.status == StarStatus.Running )
            {
                return true;
            }
        }
        return false;
    }
}

public class StarObject
{
    public Transform transform;
    public StarStatus status;
    public float retries = 0;
    private Vector3 targetPosition;

    public void SetTargetPos ( Vector3 targetPos )
    {
        targetPos.z = 0;
        targetPosition = targetPos;
    }

    public Vector3 GetTargetPos ()
    {
        return targetPosition;
    }

    public StarObject ( Transform transform )
    {
        this.transform = transform;
        this.status = StarStatus.None;
        targetPosition = Vector3.zero;
    }
}