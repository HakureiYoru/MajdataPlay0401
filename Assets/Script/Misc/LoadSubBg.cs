using MajdataPlay;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MajdataPlay.Misc
{
    public class LoadSubBg : MonoBehaviour
    {
        // Start is called before the first frame update
        void Start()
        {
            var img = GetComponent<Image>();
            img.sprite = SkinManager.Instance.SelectedSkin.SubDisplay;
            img.color = Color.white;
        }
    }
}