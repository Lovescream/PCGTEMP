using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Log {
    public string key;
    public string log;

    private Log() { }
    public Log(string key, string content) { this.key = key; this.log = content; }
}