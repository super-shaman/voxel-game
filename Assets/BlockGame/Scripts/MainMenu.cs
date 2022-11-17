
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class MainMenu : MonoBehaviour
{

    public Slider viewDistanceSlider;
    public TextMeshProUGUI loadSizeText;
    public TMP_Dropdown dropDown;
    public Slider frameRate;
    public TextMeshProUGUI frameRateText;
    public TMP_Dropdown qualityDropDown;

    public void StartGame()
    {
        SceneManager.LoadScene(1);
    }
    public static void LoadMenu()
    {
        SceneManager.LoadScene(0);
    }

    // Start is called before the first frame update
    Resolution r;
    void Start()
    {
        r = Screen.currentResolution;
        Screen.SetResolution(r.width, r.height, FullScreenMode.ExclusiveFullScreen, 60);
        QualitySettings.SetQualityLevel(5);
    }

    public void OnApplicationQuit()
    {
        if (!Application.isEditor)
        {
            System.Diagnostics.Process.GetCurrentProcess().Close();
            System.Diagnostics.Process.GetCurrentProcess().Kill();

        }
    }

    public void Quit()
    {
        if (!Application.isEditor)
        {
            System.Diagnostics.Process.GetCurrentProcess().Close();
            System.Diagnostics.Process.GetCurrentProcess().Kill();

        }
    }

    public static int loadSize = 4;

    public void SetViewDistance()
    {
        loadSize = (int)viewDistanceSlider.value;
        Debug.Log(loadSize);
        loadSizeText.text = "Load Size " + loadSize;
    }

    public void SetResolution()
    {
        if (dropDown.value == 1)
        {
            Screen.SetResolution((int)(r.width*0.75), (int)(r.height*0.75), FullScreenMode.ExclusiveFullScreen, 60);
        }else
        {
            Screen.SetResolution(r.width, r.height, FullScreenMode.ExclusiveFullScreen, 60);
        }
    }

    public void SetFrameRate()
    {
        int rate = (int)frameRate.value*30;
        Application.targetFrameRate = rate;
        frameRateText.text = "Frame Rate " + rate;
    }

    public void SetQualitySettings()
    {
        QualitySettings.SetQualityLevel(5-qualityDropDown.value);
    }

    void Update()
    {

    }
}
