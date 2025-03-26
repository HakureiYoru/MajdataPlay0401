using Cysharp.Threading.Tasks;
using MajdataPlay.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Video;

namespace MajdataPlay
{
    public class StoryManager : MonoBehaviour
    {
        public VideoPlayer StoryVideo;
        public SpriteRenderer VideoRender;
        public TextMeshProUGUI DialogText;
        public TextMeshProUGUI NameText;
        public Image Character;
        public Image DialogWindow;
        public Image NameWindow;

        void Start()
        {
            NameText.text = "";
            DialogText.text = "";
            Character.color = new Color(1, 1, 1, 0);
            DialogWindow.color = new Color(1, 1, 1, 0);
            NameWindow.color = new Color(1, 1, 1, 0);
            start().Forget();
        }

        async UniTaskVoid start()
        {
            //play cg video
            await UniTask.Delay(100);
            StoryVideo.Play();
            MajInstances.SceneSwitcher.FadeOut();
            var videosound = MajInstances.AudioManager.GetSFX("story.mp3");
            videosound.Play();
            await UniTask.WaitForSeconds((float)videosound.Length.TotalSeconds);

            //we do it old style way so it has that effect
            for(float i = 1; i > 0; i = i - 0.0625f)
            {
                VideoRender.color = new Color(1, 1, 1, i);
                await UniTask.Delay(120);
            }
            Destroy(StoryVideo.gameObject);
            MajDebug.Log("VideoOver");
            videosound.Stop();

            var bgm = MajInstances.AudioManager.GetSFX("bgm_story.mp3");
            bgm.IsLoop = true;
            bgm.Play();

            //UI fade in
            for (float i = 1; i > 0; i = i - 0.0625f)
            {
                Character.color = new Color(1, 1, 1, 1 - i);
                await UniTask.Delay(120);
            }
            DialogWindow.color = new Color(1, 1, 1, 1);
            NameWindow.color = new Color(1, 1, 1, 1);


            var voice = MajInstances.AudioManager.GetSFX("random.mp3");
            while (true)
            {
                //TODO: Deserialize here
                string name = "dydy";
                string text = "大家好啊 我是说的道理 今天来点大家想看的东西啊 啊啊啊米浴说的道 ↓ 理↑";

                NameText.text = name;
                DialogText.text = "";
                voice.Play();
                for (int i = 0; i < text.Length; i++)
                {
                    DialogText.text += text[i];
                    var byt = BitConverter.GetBytes(text[i]).FirstOrDefault();
                    var pos = ((double)byt / (double)byte.MaxValue);
                    var start = pos * voice.Length.TotalSeconds;
                    voice.CurrentSec = start;
                    await UniTask.Delay(50);
                }
                voice.Stop();
                //TODO: Waitkey
                await UniTask.Delay(2000);
            }
        }

        async UniTask Nuke()
        {
            Application.Quit();
        }
    }
}
