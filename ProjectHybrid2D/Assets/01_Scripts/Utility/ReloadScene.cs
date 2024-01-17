using UnityEngine;
using UnityEngine.SceneManagement;

public class ReloadScene : MonoBehaviour
{
    [SerializeField] private int sceneIndex = 0;

    public void ReloadSceneNow()
    {
        SceneManager.LoadScene(sceneIndex);
    }
}
