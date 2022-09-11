using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using TMPro;
using System;
using System.Threading.Tasks;

namespace Andallfor.TextEngine {
public class TextEngine
{
    private Queue<TextEngineInstance> internalTextQueue = new Queue<TextEngineInstance>();

    public TextEngine(List<TextEngineInstance> instances) {
        internalTextQueue = new Queue<TextEngineInstance>(instances);
    }

    public async Task<bool> drawNext() {
        if (internalTextQueue.Count == 0) return false;

        await (new TextEngineInstanceDrawer(internalTextQueue.Dequeue())).start();

        return true;
    }
}

internal class TextEngineInstanceDrawer {
    private TextEngineInstance tei;
    private TextMeshProUGUI t;
    private Task updateLoop;
    private int waitTimer, currentReadingIndex = -1, invalidOffset = 0;
    private List<TextEffectHandler> effects = new List<TextEffectHandler>();
    private Vector3[] characterVerts;
    private Color32[] characterColors;
    private TECharacter[] characters;
    public bool isDirty;

    public TextEngineInstanceDrawer(TextEngineInstance tei) {
        this.tei = tei;
        this.t = tei.options.textRepresentation;
        this.waitTimer = Mathf.RoundToInt(1000f * (60f / tei.options.lettersPerMinute));

        t.text = "";

        //effects.Add(new TextEffectSpreadHandler(0.3f, new Vector2(0.85f, 1.25f), new Vector2(50, -60)));
        effects.Add(new TextEffectFadeInHandler(0.5f, new Vector2(25, -100), new Color32(255, 255, 255, 0)));
        //effects.Add(new TextEffectWaveHandler(1, 1));
        
        
        updateLoop = new Task(async () => {
            while (true) {
                if (currentReadingIndex == -1) continue;
                if (effects.Count > 0) {
                    List<TextEffectHandler> _effects = new List<TextEffectHandler>();
                    bool allSent = currentReadingIndex == tei.text.Length - 1;
                    foreach (var effect in effects) {
                        bool response = effect.update(currentReadingIndex, invalidOffset);
                        if (response || !allSent) _effects.Add(effect);
                    }

                    effects = _effects;
                }

                if (isDirty) {
                    isDirty = false;
                    UnityMainThreadDispatcher.Instance().Enqueue(() => {
                        t.textInfo.meshInfo[0].mesh.vertices = characterVerts;
                        t.textInfo.meshInfo[0].mesh.colors32 = characterColors;
                        t.UpdateGeometry(t.textInfo.meshInfo[0].mesh, 0);
                    });
                }
                
                await Task.Delay(17); // 1/60th of a second
            }
        });

        // clear/save textint
        t.text = tei.text;
        t.ForceMeshUpdate(true, true);
        Vector3[] verts = t.textInfo.meshInfo[0].vertices;
        Color32 color = t.color;
        List<TECharacter> c = new List<TECharacter>();
        int offset = 0;
        for (int i = 0; i < tei.text.Length; i++) {
            if (!characterHasMesh(tei.text[i])) {offset++; continue;}

            int index = (i - offset) * 4;
            TECharacter tec = new TECharacter(
                i,
                i - offset,
                tei.text[i],
                new TECPosition(verts[index], verts[index + 1], verts[index + 2], verts[index + 3]),
                new TECColor32(color, color, color, color),
                this);
            c.Add(tec);
        }

        characters = c.ToArray();

        t.textInfo.meshInfo[0].mesh.vertices = new Vector3[t.textInfo.meshInfo[0].mesh.vertices.Length];
        t.UpdateGeometry(t.textInfo.meshInfo[0].mesh, 0);

        characterVerts = new Vector3[t.textInfo.meshInfo[0].vertices.Length];
        characterColors = new Color32[t.textInfo.meshInfo[0].colors32.Length];
    }

    public async Task<bool> start() {
        foreach (TextEffectHandler teh in effects) teh.init(t, this);
        updateLoop.Start();
        currentReadingIndex = -1;

        for (int i = 0; i < tei.text.Length; i++) {
            currentReadingIndex = i;
            if (!characterHasMesh(tei.text[i])) {
                invalidOffset++;
                await Task.Delay(waitTimer);
                continue;
            }

            // TODO: if we have custom effects running, defer this to them (so we wont have instances where the normal letters appear for a split second due to desync)
            TECharacter c = getCharacter(i - invalidOffset);
            c.setCharacterPosition(c.originalPosition);
            
            await Task.Delay(waitTimer);
        }

        await updateLoop;

        return true;
    }

    public void markDirty() {isDirty = true;}
    public void markDirty(TECharacter tec) {
        characterVerts[tec.meshIndex * 4] = tec.position.bl;
        characterVerts[tec.meshIndex * 4 + 1] = tec.position.tl;
        characterVerts[tec.meshIndex * 4 + 2] = tec.position.tr;
        characterVerts[tec.meshIndex * 4 + 3] = tec.position.br;

        characterColors[tec.meshIndex * 4] = tec.color.bl;
        characterColors[tec.meshIndex * 4 + 1] = tec.color.tl;
        characterColors[tec.meshIndex * 4 + 2] = tec.color.tr;
        characterColors[tec.meshIndex * 4 + 3] = tec.color.br;

        isDirty = true;
    }
    
    public TECharacter getCharacter(int meshIndex) => characters[meshIndex];

    public bool characterHasMesh(char c) => (c == ' ' || c == '\n') ? false : true;
}
}
