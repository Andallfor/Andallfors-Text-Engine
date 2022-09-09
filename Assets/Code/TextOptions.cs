using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public struct TextOptions {
    public TextMeshProUGUI textRepresentation;
    public float lettersPerMinute;
    public TextOptions(TextMeshProUGUI representation, float lettersPerMinute) {
        this.textRepresentation = representation;
        this.lettersPerMinute = lettersPerMinute;
    }
}