using UnityEngine;
using System;
using Unity.Mathematics;
using System.Collections.Generic;
using System.Threading.Tasks;

[Serializable]
public class Potion
{
    public string Name;
    public Ingredients Ingredients;
    public int AmountOfIngredients;
}

public class CustomerCreater : MonoBehaviour
{
    [SerializeField] private SkinnedMeshRenderer skinnedMeshRenderer;
    [SerializeField] private Mesh[] meshArray;

    [SerializeField] private Potion[] possiblePotions;

    [SerializeField] private string[] possibleNames;

    [SerializeField] private string[] possibleOpeningDialogue;
    [SerializeField] private string[] possibleRequestDialogue;
    [SerializeField] private string[] possibleNextPotionDialogue;

    [SerializeField] private int2 minAndMaxAmountOfPotions;
    [Tooltip("How many times it may try to get a new ingredient, when it gets a duplicate. " +
        "Used during the randomization of the potion ingredients."), Range(1, 10)]
    [SerializeField] private int maxAmountOfIngredientRetries = 5;

    private int amountOfRandomPotionTries = 0;
    private int numberOfPossiblePotions;

    private void Start ()
    {
        numberOfPossiblePotions = possiblePotions.Length;

        minAndMaxAmountOfPotions.y += 1;

        if ( minAndMaxAmountOfPotions.y > numberOfPossiblePotions || minAndMaxAmountOfPotions.y == 0 )
        {
            minAndMaxAmountOfPotions.y = numberOfPossiblePotions;
        }
        if ( minAndMaxAmountOfPotions.x >= numberOfPossiblePotions || minAndMaxAmountOfPotions.x == 0 )
        {
            minAndMaxAmountOfPotions.x = numberOfPossiblePotions - 1;
        }
    }

    private GameObject GenerateRandomMesh ()
    {
        GameObject mesh = new();
        return mesh;
    }

    private async Task<string[]> SetDialogue ( Customer customer )
    {
        await Awaitable.BackgroundThreadAsync();

        string[] dialogue = new string[customer.DesiredPotion.Count * 2 + 1];

        dialogue[0] = Utility.GetRandomElementFromArray(possibleOpeningDialogue);

        dialogue[1] = Utility.GetRandomElementFromArray(possibleRequestDialogue);

        string currentString = "";
        int index = 2;
        foreach ( var potion in customer.DesiredPotion )
        {
            dialogue[1] += $", {potion.Name}";
            var current = Utility.GetRandomElementFromArray(possibleNextPotionDialogue, currentString, 0, maxAmountOfIngredientRetries);

            if ( current == null )
            { continue; }

            currentString = current;
            dialogue[index] = currentString;
            index++;
        }

        await Awaitable.MainThreadAsync();

        return dialogue;
    }

    public void ReuseCustomer ( ref Customer customer )
    {
        int amountOfPotions = UnityEngine.Random.Range(minAndMaxAmountOfPotions.x, minAndMaxAmountOfPotions.y);

        for ( int i = 0; i < amountOfPotions; i++ )
        {
            var potion = Utility.GetRandomElementFromList(customer.DesiredPotion, possiblePotions, 0, maxAmountOfIngredientRetries);
            if ( potion != null )
            {
                customer.DesiredPotion.Add(potion);
            }
        }
        UnityEngine.Debug.Log("Created Potions.");
        customer.AmountOfPotions = customer.DesiredPotion.Count;

        customer.Name = GenerateName();
        UnityEngine.Debug.Log("Created Name.");

        //customer.Dialogue = SetDialogue(customer).Result;
        //UnityEngine.Debug.Log("Created Dialogue.");

        customer.meshObject = GenerateRandomMesh();
        UnityEngine.Debug.Log("Created Mesh.");
    }

    public Customer CreateCustomer ()
    {
        var customer = new Customer();

        ReuseCustomer(ref customer);

        return customer;
    }

    private string GenerateName ()
    {
        if ( possibleNames.Length < 1 )
        { return ""; }

        string name = Utility.GetRandomElementFromArray(possibleNames);
        return name;
    }
}

[Serializable]
public class Customer
{
    public GameObject meshObject;
    public string Name;
    //public Ingredients DesiredPotionIngredients;
    public string[] Dialogue;
    public List<Potion> DesiredPotion = new();
    public int AmountOfPotions;
    private int currentPotionIndex = 0;
    private int currentDialogueIndex = 0;

    private Action outOfPotions;

    public void NextDialogue ()
    {
        currentDialogueIndex = (currentDialogueIndex + 1) % Dialogue.Length;
    }

    public void SetOutOfPotionsAction ( Action action )
    {
        outOfPotions = action;
    }

    public string CurrentDialogue ()
    {
        return Dialogue[currentDialogueIndex];
    }

    public bool AreIngredientsCorrect ( Ingredients insertedIngredients, Action<Customer, bool> callBack )
    {
        bool isCorrect = (DesiredPotion[currentPotionIndex].Ingredients == insertedIngredients);

        if ( isCorrect )
        {
            if ( currentPotionIndex >= DesiredPotion.Count )
            {
                outOfPotions?.Invoke();
            }
            currentPotionIndex = (currentPotionIndex + 1) % DesiredPotion.Count;
        }

        callBack?.Invoke(this, isCorrect);

        return isCorrect;
    }
}

[Flags]
public enum Ingredients
{
    None = 0,
    Frog = 1 << 0,
    Carrot = 1 << 1,
    Potato = 1 << 2,
    StrawBerry = 1 << 3,
    Mustache = 1 << 4,
}