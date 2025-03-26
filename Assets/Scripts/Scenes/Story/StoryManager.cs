using Cysharp.Threading.Tasks;
using MajdataPlay.IO;
using MajdataPlay.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
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
        string[] program;
        void Start()
        {
            NameText.text = "";
            DialogText.text = "";
            Character.color = new Color(1, 1, 1, 0);
            DialogWindow.color = new Color(1, 1, 1, 0);
            NameWindow.color = new Color(1, 1, 1, 0);
            program = File.ReadAllLines(Application.streamingAssetsPath + "/story.txt");
            
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
            bgm.Volume = 0.7f;

            //UI fade in
            for (float i = 1; i > 0; i = i - 0.0625f)
            {
                Character.color = new Color(1, 1, 1, 1 - i);
                await UniTask.Delay(120);
            }
            DialogWindow.color = new Color(1, 1, 1, 0.8f);
            NameWindow.color = new Color(1, 1, 1, 0.8f);


            var voice = MajInstances.AudioManager.GetSFX("random.mp3");
            for (int pc = 0; pc < program.Length; pc++)
            {
                var line = program[pc];
                if (line == "") continue;
                if (line.StartsWith("选项")) continue;
                if (line.StartsWith("NUKE")) { 
                    await Nuke();
                    Application.Quit();
                    return;
                }
                var parts = line.Split("|");
                if (parts[1] !="null")
                {
                    Character.color = new Color(1, 1, 1, 1);
                    //TODO: switch sprite here
                }

                string name = parts[0];
                string text = parts[2];

                NameText.text = name;
                DialogText.text = "";
                voice.Play();
                for (int i = 0; i < text.Length; i++)
                {
                    DialogText.text += text[i];

                    if (text[i] == '颰')
                    {
                        await Nuke();
                        continue;
                    }

                    if (name == "小小蓝白")
                    {
                        voice.Play();
                        var byt = BitConverter.GetBytes(text[i]).FirstOrDefault();
                        var pos = ((double)byt / (double)byte.MaxValue);
                        var start = pos * voice.Length.TotalSeconds;
                        voice.CurrentSec = start;
                        await UniTask.Delay(50);
                        voice.Stop();
                    }
                    else
                    {
                        MajInstances.AudioManager.PlaySFX("pop.mp3");
                        await UniTask.Delay(50);
                    }

                    
                }
                //waitkey
                MajInstances.LightManager.SetButtonLight(Color.green, 3);
                while (!InputManager.CheckButtonStatus(Types.SensorArea.A4, Types.SensorStatus.On))
                {
                    await UniTask.Yield();
                }
                MajInstances.LightManager.SetButtonLight(Color.white, 3);
            }
        }

        async UniTask Nuke()
        {
            MajInstances.AudioManager.PlaySFX("bgm_explosion.mp3");
            //TODO: Play video effect here
            await UniTask.WaitForSeconds(2);
        }
    }
}
