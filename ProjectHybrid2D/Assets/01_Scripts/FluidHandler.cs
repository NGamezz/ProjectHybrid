using System.Threading.Tasks;
using UnityEngine;

public class FluidHandler : MonoBehaviour
{
    [SerializeField] private Animation[] animations;
    [SerializeField] private Ingredients fluids;
    [SerializeField] private float amountOfFluids;

    private void Start ()
    {
        foreach ( var animation in animations )
        {
            animation.playAutomatically = false;
            animation.enabled = false;
        }
    }

    private async Task PlayAnimation ( int index )
    {
        await Awaitable.MainThreadAsync();

        var currentAnimation = animations[index];

        currentAnimation.enabled = true;
        currentAnimation.Play();

        while ( currentAnimation.isPlaying )
        {
            await Awaitable.WaitForSecondsAsync(0.1f);
        }

        currentAnimation.enabled = false;
    }

    public bool IsFluid(Ingredients ingredient)
    {
        for ( int i = 0; i < amountOfFluids; i++ )
        {
            if ( (ingredient & fluids) != 0 )
            {
                return true;
            }
        }
        return false;
    }

    public async Task StartAnimation ( int index )
    {
        if ( index >= animations.Length || animations.Length < 1 )
        {
            return;
        }

        await PlayAnimation(index);
    }
}
