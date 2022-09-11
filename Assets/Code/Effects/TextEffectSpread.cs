using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

namespace Andallfor.TextEngine {
internal class TextEffectSpreadHandler : TextEffectHandler {
    private List<TextEffectSpreadData> activeEffects = new List<TextEffectSpreadData>();
    private Dictionary<int, float> steps = new Dictionary<int, float>(); // ugly, but bypasses csharp1654
    private int lastSeen = -1;
    private float animationTime, timeChange;
    private Vector2 spreadRange, scaleRange;

    public TextEffectSpreadHandler(float animationTime, Vector2 scaleRange, Vector2 spreadRange) {
        this.animationTime = animationTime;
        this.scaleRange = scaleRange;
        this.spreadRange = spreadRange;

        this.timeChange = (1f / animationTime) * (1f / 60f);
    }

    public override bool update(int reading, int offset) {
        if (text.textInfo.meshInfo[0].vertexCount == 0) return true;

        if (reading != lastSeen && parent.characterHasMesh(text.text[reading])) {
                System.Random r = new System.Random();
                float x = (float) r.NextDouble() * spreadRange.x * 2f - spreadRange.x;
                float y = (float) r.NextDouble() * spreadRange.y * 2f - spreadRange.y;
                float s = scaleRange.x + (scaleRange.y - scaleRange.x) * (float) r.NextDouble();

                var off = new TECPosition(new Vector3(x, y, 0));

                TECharacter c = parent.getCharacter(reading - offset);
                activeEffects.Add(new TextEffectSpreadData() {
                    character = c,
                    initalPosition = c.originalPosition + off,
                    scale = s
                });

                steps[c.meshIndex] = 0;

                c.setCharacterPosition(c.originalPosition + off);

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
                pos += TECPosition.scale(data.character.originalPosition, data.scale - (data.scale - 1f) * step * step * step);
                data.character.setCharacterPosition(pos);
                steps[data.character.meshIndex] = step + timeChange;
                stillMoving = true;
            }
            
        }

        return stillMoving;
    }

    private float interpPos(float x) {
        float n1 = 7.5625f;
        float d1 = 2.75f;

        if (x < 1f / d1) return n1 * x * x;
        else if (x < 2f / d1) return n1 * (x -= 1.5f / d1) * x + 0.75f;
        else if (x < 2.5f / d1) return n1 * (x -= 2.25f / d1) * x + 0.9375f;    
        else return n1 * (x -= 2.625f / d1) * x + 0.984375f;
    }
}

internal struct TextEffectSpreadData {
    public TECharacter character;
    public float scale;
    public TECPosition initalPosition;
}
}
