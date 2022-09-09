using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Andallfor.TextEngine;

public class TestController : MonoBehaviour
{
    public TextMeshProUGUI text;
    async void Start()
    {
        TextEngine te = new TextEngine(new List<TextEngineInstance>() {
            new TextEngineInstance("The quick brown fox jumps over the lazy dog!",
            new TextOptions(text, 2000))
        });

        await te.drawNext();

        Debug.Log("finished");
    }
}
