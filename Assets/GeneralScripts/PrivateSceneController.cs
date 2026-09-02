using UnityEngine;
using UnityEngine.SceneManagement;

public class PrivateSceneController : MonoBehaviour
{
    [SerializeField] private string gameplayScene = "Gameplay";

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