using System;
using System.Collections.Generic;

[Serializable]
public class LiquidData
{
    public string Name;
    public string Color;
    public int Danger;
    public float ph;
}

[Serializable]
public class LiquidDataList
{
    public List<LiquidData> Liquids;
}