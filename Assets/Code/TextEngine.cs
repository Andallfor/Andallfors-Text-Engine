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
    public TextEngineInstance tei;
    public TextMeshProUGUI t;
    public Task updateLoop;
    public int waitTimer, currentReadingIndex = -1, invalidOffset = 0;
    public List<TextEffectHandler> effects = new List<TextEffectHandler>();
    public Queue<TextEngineCharacterData> charData = new Queue<TextEngineCharacterData>();
    public Vector3[] characterVerts;
    public bool isDirty;

    public TextEngineInstanceDrawer(TextEngineInstance tei) {
        this.tei = tei;
        this.t = tei.options.textRepresentation;
        this.waitTimer = Mathf.RoundToInt(1000f * (60f / tei.options.lettersPerMinute));

        t.text = "";

        effects.Add(new TextEffectWaveHandler());
        
        updateLoop = new Task(async () => {
            while (true) {
                if (currentReadingIndex == -1) continue;
                if (effects.Count > 0) {
                    List<TextEffectHandler> _effects = new List<TextEffectHandler>();
                    foreach (var effect in effects) {
                        bool response = effect.update(currentReadingIndex, invalidOffset);
                        if (response) _effects.Add(effect);
                    }

                    effects = _effects;
                }

                if (isDirty) {
                    isDirty = false;
                    UnityMainThreadDispatcher.Instance().Enqueue(() => {
                        t.textInfo.meshInfo[0].mesh.vertices = characterVerts;
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
        int offset = 0;
        for (int i = 0; i < tei.text.Length; i++) {
            if (tei.text[i] == ' ') {offset++; continue;}

            int index = (i - offset) * 4;

            charData.Enqueue(new TextEngineCharacterData(verts[index], verts[index + 1], verts[index + 2], verts[index + 3]));
        }

        t.textInfo.meshInfo[0].mesh.vertices = new Vector3[t.textInfo.meshInfo[0].mesh.vertices.Length];
        t.UpdateGeometry(t.textInfo.meshInfo[0].mesh, 0);

        characterVerts = new Vector3[t.textInfo.meshInfo[0].vertices.Length];
    }

    public async Task<bool> start() {
        foreach (TextEffectHandler teh in effects) teh.init(t, this);
        updateLoop.Start();

        for (int i = 0; i < tei.text.Length; i++) {
            currentReadingIndex = i;
            if (tei.text[i] == ' ') {invalidOffset++; await Task.Delay(waitTimer); continue;}

            setCharacterPosition(i - invalidOffset, charData.Dequeue());
            
            await Task.Delay(waitTimer);
        }

        await updateLoop;

        return true;
    }

    public void setCharacterPosition(int meshIndex, TextEngineCharacterData tecd) {
        characterVerts[meshIndex * 4] = tecd.bl;
        characterVerts[meshIndex * 4 + 1] = tecd.tl;
        characterVerts[meshIndex * 4 + 2] = tecd.tr;
        characterVerts[meshIndex * 4 + 3] = tecd.br;

        isDirty = true;
    }

    public void moveCharacterPosition(int meshIndex, TextEngineCharacterData tecd) {
        characterVerts[meshIndex * 4] += tecd.bl;
        characterVerts[meshIndex * 4 + 1] += tecd.tl;
        characterVerts[meshIndex * 4 + 2] += tecd.tr;
        characterVerts[meshIndex * 4 + 3] += tecd.br;

        isDirty = true;
    }

    public TextEngineCharacterData getCharacterPosition(int meshIndex) => new TextEngineCharacterData(
        characterVerts[meshIndex * 4],
        characterVerts[meshIndex * 4 + 1],
        characterVerts[meshIndex * 4 + 2],
        characterVerts[meshIndex * 4 + 3]);
}

internal struct TextEngineCharacterData {
    public Vector3 tl, tr, bl, br;    

    public TextEngineCharacterData(Vector3 all) {
        tl = all;
        tr = all;
        bl = all;
        br = all;
    }

    public TextEngineCharacterData(Vector3 bl, Vector3 tl, Vector3 tr, Vector3 br) {
        this.bl = bl;
        this.tl = tl;
        this.tr = tr;
        this.br = br;
    }

    public TextEngineCharacterData(TextEngineCharacterData tecd) {
        this.bl = tecd.bl;
        this.br = tecd.br;
        this.tl = tecd.tl;
        this.tr = tecd.tr;
    }
}
}
