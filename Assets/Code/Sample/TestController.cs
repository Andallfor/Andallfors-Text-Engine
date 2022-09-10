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
            new TextEngineInstance(@"Lorem ipsum dolor sit amet, consectetur adipiscing elit. Ut tempor lacinia lectus id malesuada. Duis dapibus mi et ipsum luctus posuere. Phasellus commodo at nibh eu ullamcorper. Nullam vestibulum ut mauris at aliquet. Vivamus luctus luctus elit. Curabitur vestibulum nulla dolor, sed vehicula ex bibendum vitae. Maecenas sagittis molestie vestibulum. Nam nisl felis, hendrerit a accumsan eget, pharetra sed tellus. Mauris rutrum blandit ipsum ac commodo. Nulla fringilla felis eu consectetur ullamcorper.",
            new TextOptions(text, 3000))
        });

        await te.drawNext();

        Debug.Log("finished");
    }
}
