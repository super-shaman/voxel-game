
using UnityEngine;
using UnityEngine.SceneManagement;
public class MainMenu : MonoBehaviour
{
    

    public void StartGame()
    {
        SceneManager.LoadScene(1);
    }
    public static void LoadMenu()
    {
        SceneManager.LoadScene(0);
    }

    // Start is called before the first frame update
    void Start()
    {
    }

    public void OnApplicationQuit()
    {
        if (!Application.isEditor)
        {
            System.Diagnostics.Process.GetCurrentProcess().Close();
            System.Diagnostics.Process.GetCurrentProcess().Kill();

        }
    }
    void Update()
    {

    }
}
