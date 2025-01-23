using Cysharp.Threading.Tasks;
using MajdataPlay.IO;
using MajdataPlay.Net;
using MajdataPlay.Types;
using MajdataPlay.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using UnityEngine;
using UnityEngine.UI;

namespace MajdataPlay.Result
{
    public class OnlineInteractionSender : MonoBehaviour
    {
        public Text infotext;
        public Text uploadtext;
        public Image thumb;

        SongDetail SongDetail;

        public bool Init(SongDetail song)
        {
            if (song.ApiEndpoint == null)
            {
                infotext.text = "";
                thumb.gameObject.SetActive(false);
                return false;
            }
            SongDetail = song;
            MajInstances.InputManager.BindAnyArea(OnAreaDown);
            //MajInstances.LightManager.SetButtonLight(Color.yellow, 2);
            return true;
        }

        private void OnAreaDown(object sender, InputEventArgs e)
        {
            if (e.IsDown && (e.Type == SensorType.E3 || e.Type == SensorType.B3))
            {
                MajInstances.InputManager.UnbindAnyArea(OnAreaDown);
                SendInteraction(SongDetail);
            }
        }

        private void OnDestroy()
        {
            MajInstances.InputManager.UnbindAnyArea(OnAreaDown);
        }

        public void SendInteraction(SongDetail song)
        {
            SendLike(song).Forget();
        }

        async UniTask SendLike(SongDetail song)
        {
            infotext.text = "�Ե�...";
            //MajInstances.LightManager.SetButtonLight(Color.blue, 4);
            try
            {
                await MajInstances.OnlineManager.SendLike(song);
                infotext.text = "���޳ɹ�";
            }
            catch (Exception ex)
            {
                infotext.text = ex.Message;
                MajDebug.LogError(ex);
                //MajInstances.LightManager.SetButtonLight(Color.red, 4);
                return;
            }
        }

        public async UniTask SendScore(MaiScore score)
        {
            uploadtext.text = "�����ϴ��ɼ�";
            try
            {
                await MajInstances.OnlineManager.SendScore(SongDetail, score);
                uploadtext.text = "�ϴ��ɼ��ɹ�";
            }
            catch (Exception ex)
            {
                uploadtext.text = ex.Message;
                MajDebug.LogError(ex);
                return;
            }
        }
    }
}