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

    public override bool update(int reading, int offset) {
        if (text.textInfo.meshInfo[0].vertexCount == 0) return true;

        if (reading != lastSeen && parent.characterHasMesh(text.text[reading])) {
            TECharacter c = parent.getCharacter(reading - offset);
            activeEffects.Add(new TextEffectWaveData() {
                character = c, offset = reading});

            lastSeen = reading;
        }

        Vector3[] verts = new Vector3[text.textInfo.meshInfo[0].vertices.Length];
        text.textInfo.meshInfo[0].vertices.CopyTo(verts, 0);

        foreach (var data in activeEffects) {
            float change = Mathf.Sin(speed * 0.5f * (internalCounter + data.offset)) * height * 10f;
            TECPosition tecd = new TECPosition(data.character.originalPosition);
            tecd.bl.y += change;
            tecd.br.y += change;
            tecd.tl.y += change;
            tecd.tr.y += change;

            data.character.setCharacterPosition(tecd);
        }

        internalCounter += rateOfChange;

        return true;
    }
}

internal struct TextEffectWaveData {
    public float offset;
    public TECharacter character;
}
}
