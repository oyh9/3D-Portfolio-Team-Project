using UnityEngine;
using TMPro;

public class LiquidDisplay3D : MonoBehaviour
{
    public TextMeshPro liquidName;
    public TextMeshPro liquidColor;
    public TextMeshPro liquidDanger;
    public TextMeshPro liquidph;

    public void SetLiquidInfo(LiquidData liquidData)
    {
        liquidName.text = "Liquid Name : " + liquidData.Name.Trim();
        liquidColor.text = "Liquid Color : " + liquidData.Color.Trim();
        liquidDanger.text = "Liquid Danger : " + liquidData.Danger.ToString();
        liquidph.text = "Liquid ph : " + liquidData.ph.ToString();
    }
}
