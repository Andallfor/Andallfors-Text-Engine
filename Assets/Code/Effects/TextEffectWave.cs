using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Andallfor.TextEngine {
internal class TextEffectWaveHandler : TextEffectHandler {
    private List<TextEffectWaveData> activeEffects = new List<TextEffectWaveData>();
    private int lastSeen = -1;
    private float internalCounter = 0, rateOfChange = 0.25f;
    private bool running = false;

    public override bool update(int i, int offset) {
        if (running) return true;
        if (text.textInfo.meshInfo[0].vertexCount == 0) return true;

        running = true;

        if (i != lastSeen) {
            if (text.text[i] != ' ') {
                activeEffects.Add(new TextEffectWaveData() {
                    height = 1, speed = 1, offset = i,
                    textIndex = i, meshIndex = i - offset,
                    baseY = text.textInfo.meshInfo[0].vertices[i - offset].y,
                    initPosition = parent.getCharacterPosition(i - offset)
                });
            }

            lastSeen = i;
        }

        Vector3[] verts = new Vector3[text.textInfo.meshInfo[0].vertices.Length];
        text.textInfo.meshInfo[0].vertices.CopyTo(verts, 0);

        foreach (var data in activeEffects) {
            float change = Mathf.Sin(data.speed * 0.5f * (internalCounter + data.offset)) * data.height * 10f;
            TextEngineCharacterData tecd = new TextEngineCharacterData(data.initPosition);
            tecd.bl.y += change;
            tecd.br.y += change;
            tecd.tl.y += change;
            tecd.tr.y += change;

            parent.setCharacterPosition(data.meshIndex, tecd);
        }

        internalCounter += rateOfChange;
        running = false;

        return true;
    }
}

internal struct TextEffectWaveData {
    public float height, speed, offset, baseY;
    public int textIndex, meshIndex;
    public TextEngineCharacterData initPosition;
}
}