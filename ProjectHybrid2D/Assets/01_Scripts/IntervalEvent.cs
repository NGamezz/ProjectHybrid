using UnityEngine;
using System.Timers;
using UnityEngine.Events;
using System.Collections;

public class IntervalEvent : MonoBehaviour
{
    [SerializeField] private float Duration = 5.0f;
    [SerializeField] private UnityEvent OnEnd;
    [SerializeField] private bool startOnAwake = true;
    [SerializeField] private bool repeat = true;

    private void Start ()
    {
        if ( startOnAwake )
        {
            StartTimer();
        }
    }

    public void ByPassTimer ()
    {
        OnEnd?.Invoke();
        StopAllCoroutines();
    }

    public void StartTimer ()
    {
        StopAllCoroutines();
        StartCoroutine(StartTimerCount());
    }

    private IEnumerator StartTimerCount ()
    {
        yield return new WaitForSeconds(Duration);

        OnEnd?.Invoke();

        if ( repeat )
        {
            StartTimer();
        }
    }
}