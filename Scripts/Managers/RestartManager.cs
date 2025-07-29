using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using System.IO;
public class RestartManager : MonoBehaviour
{
    private Animator ui_Animator;
    [SerializeField] private GameObject UI_Canvas;
    [SerializeField] private InputActionProperty bButtonAction;



    [Header("Player Data")]
    [SerializeField] private PlayerDataScript Player_Data;
    private void Start()
    {
        if (!bButtonAction.action.enabled)
            bButtonAction.action.Enable();


        ui_Animator = UI_Canvas.GetComponentInChildren<Animator>();
    }


    private void Update()
    {
        if (Input.GetKeyUp(KeyCode.Escape))
        {
            UI_Canvas.SetActive(!UI_Canvas.activeSelf);
            Cursor.lockState = Cursor.lockState == CursorLockMode.Locked ? CursorLockMode.None : CursorLockMode.Locked;
            Cursor.visible = Cursor.lockState != CursorLockMode.Locked; // Show cursor when unlocked
        
         }
        if (bButtonAction.action.WasPressedThisFrame())
        {
            UI_Canvas.SetActive(!UI_Canvas.activeSelf);
        }
    }



    public void OnRestartButton()
    {
        Debug.Log("Restarting");
        switch (GameManagerScript.instance.currentState)
        {
            case FlightState.BoardingState:
                if(ui_Animator != null)
                  ui_Animator.Play("FadeOut");

                StartCoroutine(RestartBoardingState());
                break;
            case FlightState.TakeOffState:
                if (ui_Animator != null)
                    ui_Animator.Play("FadeOut");

                  StartCoroutine(RestartTakeOffState());
                break;
            case FlightState.InFlightState:
                RestartInFlightState();

                break;
            case FlightState.LandingState:
                RestartLandingState();
                break;
        }
    }


    private void UpdateData()
    {
        Player_Data.Level_Diffculty = GameManagerScript.instance.LevelDiff; // Take The Level Diff
        float timeInSeconds = 0f;

        if(TimerScript_VR.instance != null)
           timeInSeconds = TimerScript_VR.instance.time_spent;// changed here to TimerScript_VR


        int minutes = Mathf.FloorToInt(timeInSeconds / 60f);
        int seconds = Mathf.FloorToInt(timeInSeconds % 60f);
        Player_Data.Time_Spent = string.Format("{0:00}:{1:00}", minutes, seconds);

    }

    public void OnQuitButton()
    {
        UpdateData(); // Update The Data Of The Player

        SavePlayerDataToFile();

        if(ui_Animator != null)
         ui_Animator.Play("FadeOut");


        SceneManager.LoadScene("MainMenuScene");
    }

    private IEnumerator RestartBoardingState()
    {
        yield return new WaitForSeconds(1.35f);
        SceneManager.LoadScene(SceneManager.GetActiveScene().name); // Reseting The Whole Scene
    }

    private IEnumerator RestartTakeOffState()
    {
        yield return new WaitForSeconds(1.35f);

        SoundManagerScript.instance.StopAllSounds();

        GameManagerScript.instance.ChangeCurrentState(FlightState.TakeOffState);
    }

    private void RestartInFlightState()
    {
        if (ui_Animator != null)
        {
            RestartFadeAnim();
            ui_Animator.Play("FadeOut");
        }

      
        TimerScript.instance.timerTime = GameManagerScript.instance.currentTimerTime;
    }

    private void RestartLandingState()
    {
        if(ui_Animator != null)
          ui_Animator.Play("FadeOut");

    }



    private void RestartFadeAnim()
    {
        ui_Animator.Rebind();
        ui_Animator.Update(0f);
    }


    #region Player Data Save

    public void SavePlayerDataToFile()
    {
        string path = Path.Combine(Application.persistentDataPath, "Flight_Player_Data.txt");

        string content =
            "----------------\n" +
            "Player Name: " + Player_Data.Player_Name + "\n" +
            "Player Age: " + Player_Data.Player_Age + "\n" +
            "Time Spent: " + Player_Data.Time_Spent + "\n" +
            "Level Difficulty: " + Player_Data.Level_Diffculty + "\n" +
            "Timestamp: " + System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "\n";

        File.AppendAllText(path, content + "\n");
        Debug.Log("Player data appended to: " + path);

        StartCoroutine(UploadFile());/// upload the file after saving.

    }

    private IEnumerator UploadFile()
    {
        string filePath = Path.Combine(Application.persistentDataPath, "Flight_Player_Data.txt");
        byte[] fileData = File.ReadAllBytes(filePath);

        WWWForm form = new WWWForm();
        form.AddBinaryData("file", fileData, "Flight_Player_Data.txt", "text/plain");

        using (UnityEngine.Networking.UnityWebRequest www = UnityEngine.Networking.UnityWebRequest.Post("http://91.99.139.98:5000/upload", form))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityEngine.Networking.UnityWebRequest.Result.Success)
            {
                Debug.Log("Upload complete");
            }
            else
            {
                Debug.LogError("Upload failed: " + www.error);
            }
        }
    }


    #endregion
}
