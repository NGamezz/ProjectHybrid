using UnityEngine;
using System;
using Unity.Mathematics;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

[Serializable]
public class Potion
{
    public string Name;
    public Ingredients Ingredients;
    public int AmountOfIngredients;
}

public class CustomerCreater : MonoBehaviour
{
    [SerializeField] private Potion[] possiblePotions;

    [SerializeField] private string[] possibleNames;

    [SerializeField] private string[] possibleOpeningDialogue;
    [SerializeField] private string[] possibleRequestDialogue;
    [SerializeField] private string[] possibleNextPotionDialogue;

    [Tooltip("Prefab of possible meshes.")]
    [SerializeField] private GameObject[] possibleMeshes;

    [SerializeField] private int2 minAndMaxAmountOfPotions;
    [Tooltip("How many times it may try to get a new ingredient, when it gets a duplicate. " +
        "Used during the randomization of the potion ingredients."), Range(1, 10)]
    [SerializeField] private int maxAmountOfIngredientRetries = 5;

    private Custom.Utility utility = new();
    private int numberOfPossiblePotions;
    private Unity.Mathematics.Random random;

    private void Awake()
    {
        random = new((uint)UnityEngine.Random.Range(0, 10000));
        utility.Setup();
    }

    private void Start()
    {
        numberOfPossiblePotions = possiblePotions.Length;

        minAndMaxAmountOfPotions.y += 1;

        if (minAndMaxAmountOfPotions.y > numberOfPossiblePotions || minAndMaxAmountOfPotions.y == 0)
        {
            minAndMaxAmountOfPotions.y = numberOfPossiblePotions;
        }
        if (minAndMaxAmountOfPotions.x >= numberOfPossiblePotions || minAndMaxAmountOfPotions.x == 0)
        {
            minAndMaxAmountOfPotions.x = numberOfPossiblePotions - 1;
        }
    }

    private string[] SetDialogue(Customer customer)
    {
        string[] dialogue = new string[customer.DesiredPotion.Count + 1];

        dialogue[0] = "";
        dialogue[0] = utility.GetRandomElementFromArray(possibleOpeningDialogue);

        dialogue[1] = "";
        dialogue[1] = utility.GetRandomElementFromArray(possibleRequestDialogue) + " : \n";

        string currentString = "";
        int index = 2;

        for (int i = 0; i < customer.DesiredPotion.Count; i++)
        {
            var potion = customer.DesiredPotion[i];

            dialogue[1] += $"- {potion.Name}\n";

            if (customer.DesiredPotion.Count == 1 || i == customer.DesiredPotion.Count - 1)
            {
                return dialogue;
            }

            var current = utility.GetRandomElementFromArray(possibleNextPotionDialogue, currentString, 0, maxAmountOfIngredientRetries);
            if (current == null)
            {
                continue;
            }

            currentString = current;
            dialogue[index] = currentString;
            index++;
        }

        return dialogue;
    }

    public void ReuseCustomer(ref Customer customer)
    {
        Debug.Log("Reuse Customer.");

        int amountOfPotions = random.NextInt(minAndMaxAmountOfPotions.x, minAndMaxAmountOfPotions.y);

        Debug.Log($"Creating {amountOfPotions} potions.");

        for (int i = 0; i < amountOfPotions; i++)
        {
            var potion = utility.GetRandomElementFromList(customer.DesiredPotion, possiblePotions.ToList(), 0, maxAmountOfIngredientRetries);
            if (potion != null)
            {
                customer.DesiredPotion.Add(potion);
            }
        }
        UnityEngine.Debug.Log("Created Potions.");
        customer.AmountOfPotions = customer.DesiredPotion.Count;

        customer.Name = GenerateName();
        UnityEngine.Debug.Log("Created Name.");

        customer.Dialogue = SetDialogue(customer);
        UnityEngine.Debug.Log("Created Dialogue.");

        customer.meshObject = GetGameObject().Result;
        UnityEngine.Debug.Log("Set Mesh.");
    }

    private GameObject previousGameObject;
    private async Task<GameObject> GetGameObject()
    {
        if (possibleMeshes.Length < 1)
        { return default; }

        await Awaitable.MainThreadAsync();

        var prefab = utility.GetRandomElementFromArray(possibleMeshes, previousGameObject, 0, 5);
        previousGameObject = prefab;

        var gameObject = Instantiate(prefab, transform);
        gameObject.transform.position = UnityEngine.Random.insideUnitCircle.normalized * UnityEngine.Random.Range(10, 15);

        return gameObject;
    }

    public Customer CreateCustomer()
    {
        var customer = new Customer();

        ReuseCustomer(ref customer);

        return customer;
    }

    private string GenerateName()
    {
        if (possibleNames.Length < 1)
        { return ""; }

        string name = utility.GetRandomElementFromArray(possibleNames);
        return name;
    }
}

[Serializable]
public class Customer
{
    public GameObject meshObject;
    public string Name;
    public string[] Dialogue;
    public List<Potion> DesiredPotion = new();
    public int AmountOfPotions;
    public int CurrentPotionIndex { get; private set; } = 0;
    private int currentDialogueIndex = 0;

    private Action outOfPotions;

    public void NextDialogue()
    {
        currentDialogueIndex = (currentDialogueIndex + 1) % Dialogue.Length;
    }

    public void SetOutOfPotionsAction(Action action)
    {
        outOfPotions = action;
    }

    public string CurrentDialogue()
    {
        return Dialogue[currentDialogueIndex];
    }

    public bool AreIngredientsCorrect(Ingredients insertedIngredients, Action<Customer, bool> callBack)
    {
        bool isCorrect = (DesiredPotion[CurrentPotionIndex].Ingredients == insertedIngredients);

        if (isCorrect)
        {
            CurrentPotionIndex++;
            if (CurrentPotionIndex >= DesiredPotion.Count)
            {
                outOfPotions?.Invoke();
                CurrentPotionIndex = 0;
            }
        }

        callBack?.Invoke(this, isCorrect);

        return isCorrect;
    }
}

[Flags]
public enum Ingredients
{
    None = 0,
    Rood = 1 << 0,
    Groen = 1 << 1,
    Roze = 1 << 2,
    Paars = 1 << 3,
    Rose = 1 << 6,
    Hair = 1 << 7,
    GeckoHeado = 1 << 5,
    Feather = 1 << 4,
}