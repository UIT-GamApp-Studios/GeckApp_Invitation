using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneController : MonoBehaviour
{
    public static SceneController Instance;

    [SerializeField] private string gameplayScene = "Gameplay";

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void ChangeScene(string sceneName)
    {   
        SceneTransition.Instance.PlayTransition(() => {Time.timeScale = 1f; SceneManager.LoadScene(sceneName); });
    }

    public void PlayGame()
    {
        SceneTransition.Instance.PlayTransition(() => {SceneManager.LoadScene(gameplayScene);});
    }

    public void QuitGame()
    {
        Debug.Log("Quit game");
        SceneTransition.Instance.PlayTransition(() => {Application.Quit();});
    } 

}