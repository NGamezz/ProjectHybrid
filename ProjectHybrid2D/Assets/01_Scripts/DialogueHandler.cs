using System.Threading.Tasks;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.U2D;

public class DialogueHandler : MonoBehaviour
{
    [Tooltip("Amount of seconds, delay.")]
    [SerializeField] private float writeSpeed = 0.05f;

    [SerializeField] private string[] possibleCorrectDialogue;
    [SerializeField] private string[] possibleInCorrectDialogue;

    [SerializeField] private GameObject textBubbleGameObject;
    [SerializeField] private SpriteShapeController spriteShapeController;

    [SerializeField] private TMP_Text dialogueText;
    private bool continueDialogue;

    private Custom.Utility utility = new();
    private bool cancel = false;

    private void Awake ()
    {
        utility.Setup();
        textBubbleGameObject.SetActive(false);
    }

    public async void SetBubble ( bool state )
    {
        await Awaitable.MainThreadAsync();

        textBubbleGameObject.SetActive(state);
    }

    public async void ContinueDialogue ( Customer customer, bool correct )
    {
        var text = correct ? utility.GetRandomElementFromArray(possibleCorrectDialogue) :
            utility.GetRandomElementFromArray(possibleInCorrectDialogue);

        await TypeWriterPlay(text);

        if ( !correct )
        {
            cancel = true;
        }
        else
        {
            continueDialogue = true;
        }
    }

    public void EndDialogue ()
    {
        cancel = true;
    }

    private void SetTextBubble ( Vector3 customerPosition )
    {
        var spline = spriteShapeController.spline;
        spline.SetTangentMode(8, ShapeTangentMode.Continuous);
        spline.SetPosition(8, spriteShapeController.transform.InverseTransformPoint(customerPosition));
        spline.SetHeight(8, 0.1f);
        spriteShapeController.BakeCollider();
    }

    public async void PlayDialogue ( Customer customer )
    {
        cancel = false;
        SetBubble(true);
        //SetTextBubble(customer.meshObject.transform.position);
        await TypeWriterPlay(customer.CurrentDialogue());
        customer.NextDialogue();
        await TypeWriterPlay(customer.CurrentDialogue());
        customer.NextDialogue();

        var waitingCount = 0;
        await Awaitable.BackgroundThreadAsync();
        foreach ( var _ in customer.DesiredPotion )
        {
            while ( !continueDialogue && !cancel )
            {
                waitingCount++;
            }

            waitingCount = 0;
            continueDialogue = false;

            await TypeWriterPlay(customer.CurrentDialogue());
            customer.NextDialogue();

            if ( cancel )
            {
                SetBubble(false);
                return;
            }
        }

        SetBubble(false);
    }

    private async Task TypeWriterPlay ( string text )
    {
        if ( text == "" || cancel )
        { return; }

        await Awaitable.MainThreadAsync();

        dialogueText.text = "";

        foreach ( char c in text )
        {
            if ( cancel )
            { return; }

            dialogueText.text += c;
            await Awaitable.WaitForSecondsAsync(writeSpeed);
        }
    }
}