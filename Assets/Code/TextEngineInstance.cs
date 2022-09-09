using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TextEngineInstance {
    public string text {get; private set;}
    public TextOptions options {get; private set;}
    public TextEngineInstance(string text, TextOptions options) {
        this.text = text;
        this.options = options;
    }
}