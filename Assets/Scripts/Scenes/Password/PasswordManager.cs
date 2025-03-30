using MajdataPlay.Extensions;
using MajdataPlay.IO;
using MajdataPlay.Types;
using MajdataPlay.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

namespace MajdataPlay.Password
{
    internal class PasswordManager: MajComponent
    {
        [SerializeField]
        TextMeshPro _1;
        [SerializeField]
        TextMeshPro _2;
        [SerializeField]
        TextMeshPro _3;
        [SerializeField]
        TextMeshPro _4;
        [SerializeField]
        TextMeshPro _5;
        [SerializeField]
        TextMeshPro _6;
        [SerializeField]
        TextMeshPro _7;
        [SerializeField]
        TextMeshPro _8;
        [SerializeField]
        GameObject _cursor;

        readonly int[] _ = new int[8];
        readonly TextMeshPro[] __ = new TextMeshPro[8];
        SongDetail ___;
        int ____ = 0;

        protected override void Awake()
        {
            base.Awake();
            __[0] = _1;
            __[1] = _2;
            __[2] = _3;
            __[3] = _4;
            __[4] = _5;
            __[5] = _6;
            __[6] = _7;
            __[7] = _8;
            ___ = Majdata<SongDetail>.Instance!;
        }
        private void Start()
        {
            MajInstances.SceneSwitcher.FadeOut();
        }
        private void OnDestroy()
        {
            Majdata<SongDetail>.Free();
        }
        void Update()
        {
            for (var i = 0; i < 8; i++)
            {
                __[i].text = _[i].ToString();
            }
            _cursor.transform.position = new Vector3()
            {
                x = __[____].transform.position.x,
                y = -0.95f,
                z = 0
            };
            if (____ >= 8)
                return;
            Span<SensorArea> _____ = stackalloc SensorArea[8]
            {
                SensorArea.A1,
                SensorArea.A2,
                SensorArea.A3,
                SensorArea.A4,
                SensorArea.A5,
                SensorArea.A6,
                SensorArea.A7,
                SensorArea.A8,
            };
            var ______ = 0;
            for (var i = 0; i < 8; i++)
            {
                var _______ = _____[i];
                if(InputManager.IsButtonClickedInThisFrame(_______))
                {
                    ______ = i + 1;
                    break;
                }
            }
            if(______ != 0)
            {
                _[____++] = ______;
            }
            if(____ >= 8)
            {
                if (!___.TryUnlock(string.Join("", _)))
                {
                    for (var i = 0; i < 8; i++)
                    {
                        _[i] = 0;
                    }
                    ____ = 0;
                }
                else
                {
                    MajInstances.SceneSwitcher.SwitchScene("List");
                    return;
                }
            }
        }
    }
}
