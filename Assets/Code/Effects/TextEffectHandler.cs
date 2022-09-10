using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace Andallfor.TextEngine {
internal abstract class TextEffectHandler {
    protected TextMeshProUGUI text;
    protected TextEngineInstanceDrawer parent;
    public virtual void init(TextMeshProUGUI text, TextEngineInstanceDrawer parent) {
        this.text = text;
        this.parent = parent;
    }
    public abstract bool update(int i, int offset, bool allSent);
}
}
