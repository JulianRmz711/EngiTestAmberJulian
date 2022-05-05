using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class GameMessage : MonoBehaviour
{
    [SerializeField] GameObject messageObject;
    [SerializeField] TextMeshProUGUI text;

    public void SetMessage(bool isWin)
    {
        messageObject.SetActive(true);
        text.text = string.Empty;
        text.text = isWin ? "Win!" : "Lose!";
        //
        Time.timeScale = 0;
    }
}
