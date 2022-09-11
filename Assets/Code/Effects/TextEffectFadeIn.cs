using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Andallfor.TextEngine {
internal class TextEffectFadeInHandler : TextEffectHandler
{
    private List<TextEffectFadeInData> activeEffects = new List<TextEffectFadeInData>();
    private Dictionary<int, float> steps = new Dictionary<int, float>(); // ugly, but bypasses csharp1654
    private int lastSeen = -1;
    private float animationTime, offsetY, timeChange;

    public TextEffectFadeInHandler(float animationTime, float offsetY) {
        this.animationTime = animationTime;
        this.offsetY = offsetY;

        this.timeChange = (1f / animationTime) * (1f / 60f);
    }

    public override bool update(int reading, int offset) {
        if (text.textInfo.meshInfo[0].vertexCount == 0) return true;

        if (reading != lastSeen && parent.characterHasMesh(text.text[reading])) {
            TECharacter c = parent.getCharacter(reading - offset);
            activeEffects.Add(new TextEffectFadeInData() {
                character = c, initalPosition = c.originalPosition + new TECPosition(new Vector3(0, -offsetY, 0))});

            steps[c.meshIndex] = 0;
            lastSeen = reading;
        }

        if (activeEffects.Count == 0) return true;
        bool stillMoving = false;

        foreach (var data in activeEffects) {
            float step = steps[data.character.meshIndex];
            if (step >= 1) continue;
            else if (step + timeChange >= 1) data.character.setCharacterPosition(data.character.originalPosition);
            else {
                TECPosition pos = TECPosition.lerp(data.initalPosition, data.character.originalPosition, interpPos(step));
                data.character.setCharacterPosition(pos);
                steps[data.character.meshIndex] = step + timeChange;
                stillMoving = true;
            }
        }

        return stillMoving;
    }

    private float interpPos(float x) => x < 0.5f ? 8f * x * x * x * x : 1f - Mathf.Pow(-2f * x + 2f, 4f) / 2f;
}

internal struct TextEffectFadeInData {
    public TECharacter character;
    public TECPosition initalPosition;
}
}

