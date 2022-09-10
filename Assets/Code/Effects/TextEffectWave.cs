using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Andallfor.TextEngine {
internal class TextEffectWaveHandler : TextEffectHandler {
    private List<TextEffectWaveData> activeEffects = new List<TextEffectWaveData>();
    private int lastSeen = -1;
    private float internalCounter = 0, rateOfChange = 0.25f, speed, height;

    public TextEffectWaveHandler(float height, float speed) {
        this.speed = speed;
        this.height = height;
    }

    public override bool update(int i, int offset, bool allSent) {
        if (text.textInfo.meshInfo[0].vertexCount == 0) return true;

        if (i != lastSeen) {
            if (text.text[i] != ' ') {
                activeEffects.Add(new TextEffectWaveData() {
                    offset = i, textIndex = i, meshIndex = i - offset,
                    initPosition = parent.getCharacterPosition(i - offset)
                });
            }

            lastSeen = i;
        }

        Vector3[] verts = new Vector3[text.textInfo.meshInfo[0].vertices.Length];
        text.textInfo.meshInfo[0].vertices.CopyTo(verts, 0);

        foreach (var data in activeEffects) {
            float change = Mathf.Sin(speed * 0.5f * (internalCounter + data.offset)) * height * 10f;
            TextEngineCharacterData tecd = new TextEngineCharacterData(data.initPosition);
            tecd.bl.y += change;
            tecd.br.y += change;
            tecd.tl.y += change;
            tecd.tr.y += change;

            parent.setCharacterPosition(data.meshIndex, tecd);
        }

        internalCounter += rateOfChange;

        return true;
    }
}

internal struct TextEffectWaveData {
    public float offset;
    public int textIndex, meshIndex;
    public TextEngineCharacterData initPosition;
}
}