using Cysharp.Threading.Tasks;
using MajdataPlay.IO;
using MajdataPlay.Utils;
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
        public static string Password { get; private set; } = "11112222";
        public VideoPlayer StoryVideo;
        public VideoPlayer NukeVideo;
        public SpriteRenderer VideoRender;
        public TextMeshProUGUI DialogText;
        public TextMeshProUGUI NameText;
        public Image Character;
        public Image DialogWindow;
        public Image NameWindow;

        public Image[] SelectionBackground;
        public TextMeshProUGUI[] SelectionText;

        public Image[] PassButtonBackground;
        public Text[] PassButtonText;

        public Sprite[] XxlbSprites;

        public bool isAfterStory;

        string[] program;
        void Start()
        {
            NameText.text = "";
            DialogText.text = "";
            Character.color = new Color(1, 1, 1, 0);
            DialogWindow.color = new Color(1, 1, 1, 0);
            NameWindow.color = new Color(1, 1, 1, 0);
            if (isAfterStory)
            {
                program = File.ReadAllLines(Application.streamingAssetsPath + "/after_story.txt");
            }
            else
            {
                program = File.ReadAllLines(Application.streamingAssetsPath + "/story.txt");
            }      
            NukeVideo.gameObject.SetActive(false);
            HideSelection();
            start().Forget();
        }

        private void HideSelection()
        {
            for (int i = 0; i < SelectionBackground.Length; i++)
            {
                SelectionBackground[i].color = new Color(1, 1, 1, 0);
                SelectionText[i].text = "";
            }
            for(int i=0; i< PassButtonBackground.Length; i++)
            {
                PassButtonBackground[i].color = new Color(1, 1, 1, 0);
                PassButtonText[i].text = "";
            }
        }

        async UniTaskVoid start()
        {
            //play cg video
            await UniTask.Delay(100);
            
            MajInstances.SceneSwitcher.FadeOut();
            if (!isAfterStory)
            {
                var videosound = MajInstances.AudioManager.GetSFX("story.mp3");
                videosound.Play();
                StoryVideo.Play();
                if (MajInstances.Setting.Misc.AllowStorySkip)
                {
                    PassButtonBackground[4].color = new Color(1, 1, 1, 1);
                    PassButtonText[4].text = "跳过";
                }
                
                while (videosound.CurrentSec < videosound.Length.TotalSeconds - 0.1)
                {
                    if (InputManager.CheckButtonStatus(Types.SensorArea.A5, Types.SensorStatus.On)
                        &&MajInstances.Setting.Misc.AllowStorySkip)
                    {
                        break;
                    }
                    await UniTask.Yield();
                }
                StoryVideo.Pause();
                videosound.Stop();

                //we do it old style way so it has that effect
                for (float i = 1; i > 0; i = i - 0.0625f)
                {
                    VideoRender.color = new Color(1, 1, 1, i);
                    await UniTask.Delay(120);
                }
                Destroy(StoryVideo.gameObject);
                MajDebug.Log("VideoOver");
                videosound.Stop();
                PassButtonBackground[4].color = new Color(1, 1, 1, 0);
                PassButtonText[4].text = "";
                MajInstances.Setting.Misc.AllowStorySkip = true;
            }
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



            for (int pc = 0; pc < program.Length; pc++)
            {
                PassButtonBackground[4].color = new Color(1, 1, 1, 1);
                PassButtonText[4].text = ">>>";

                var line = program[pc];
                var parts = line.Split("|");

                if (line == "") continue;
                if (line.StartsWith("选项"))
                {
                    HideSelection();
                    var select1 = parts[1].Split("=");
                    var select2 = parts[2].Split("=");
                    for (float i = 1; i > 0; i = i - 0.0625f)
                    {
                        SelectionBackground[0].color = new Color(1, 1, 1, 1 - i);
                        SelectionBackground[1].color = new Color(1, 1, 1, 1 - i);
                        await UniTask.Delay(120);
                    }
                    SelectionText[0].text = select1[0];
                    SelectionText[1].text = select2[0];
                    bool anykey = false;
                    MajInstances.LightManager.SetButtonLight(Color.green, 1);
                    MajInstances.LightManager.SetButtonLight(Color.green, 2);
                    MajInstances.LightManager.SetButtonLight(Color.green, 5);
                    MajInstances.LightManager.SetButtonLight(Color.green, 6);
                    while (!anykey)
                    {
                        if (
                            InputManager.CheckAreaStatus(Types.SensorArea.A2, Types.SensorStatus.On)||
                            InputManager.CheckAreaStatus(Types.SensorArea.A7, Types.SensorStatus.On) ||
                            InputManager.CheckSensorStatus(Types.SensorArea.B2, Types.SensorStatus.On) ||
                            InputManager.CheckSensorStatus(Types.SensorArea.B7, Types.SensorStatus.On) ||
                            InputManager.CheckSensorStatus(Types.SensorArea.B8, Types.SensorStatus.On) ||
                            InputManager.CheckSensorStatus(Types.SensorArea.B1, Types.SensorStatus.On))
                        {
                            pc = int.Parse(select1[1]) - 1;
                            anykey = true;
                        }
                        if (
                            InputManager.CheckAreaStatus(Types.SensorArea.A3, Types.SensorStatus.On) ||
                            InputManager.CheckAreaStatus(Types.SensorArea.A6, Types.SensorStatus.On) ||
                            InputManager.CheckSensorStatus(Types.SensorArea.B3, Types.SensorStatus.On) ||
                            InputManager.CheckSensorStatus(Types.SensorArea.B6, Types.SensorStatus.On) ||
                            InputManager.CheckSensorStatus(Types.SensorArea.B4, Types.SensorStatus.On) ||
                            InputManager.CheckSensorStatus(Types.SensorArea.B5, Types.SensorStatus.On))
                        {
                            pc = int.Parse(select2[1]) - 1;
                            anykey = true;
                        }
                        await UniTask.Yield();
                    }
                    MajInstances.LightManager.SetAllLight(Color.white);
                    HideSelection();
                    continue;
                }
                if (line.StartsWith("NUKE"))
                {
                    await Nuke();
                    Application.Quit();
                    return;
                }
                if (line.StartsWith("TOUCHRGB"))
                {
                    Character.gameObject.GetComponent<Animator>().SetTrigger("rgb");
                    continue;
                }

                if (parts[1] != "null")
                {
                    Character.color = new Color(1, 1, 1, 1);
                    Character.sprite = XxlbSprites[int.Parse(parts[1])];
                }

                string name = parts[0];
                string text = parts[2];
                await PrintText(name, text);

                //waitkey
                PassButtonBackground[3].color = new Color(1, 1, 1, 1);
                PassButtonText[3].text = ">..";
                MajInstances.LightManager.SetButtonLight(Color.green, 3);
                while (!(
                    InputManager.CheckButtonStatus(Types.SensorArea.A4, Types.SensorStatus.On)|| 
                    InputManager.CheckButtonStatus(Types.SensorArea.A5, Types.SensorStatus.On)))
                {
                    await UniTask.Yield();
                }
                MajInstances.LightManager.SetButtonLight(Color.white, 3);
                PassButtonBackground[3].color = new Color(1, 1, 1, 0);
                PassButtonText[3].text = "";
            }
            HideSelection();
            if (!isAfterStory)
            {
                var Password = "";
                for (int j = 0; j < 8; j++)
                {
                    var thiskey = Random.Range(0, 8);
                    Password += thiskey + 1;
                    await PrintText("小小蓝白", "一定要通关哦？", 30);
                    for (float i = 1; i > 0; i = i - 0.0625f)
                    {
                        for (int k = 0; k < PassButtonBackground.Length; k++)
                        {
                            PassButtonBackground[k].color = new Color(1, 1, 1, 1 - i);
                            PassButtonText[k].color = new Color(PassButtonText[k].color.r, PassButtonText[k].color.g, PassButtonText[k].color.b, 1 - i);
                            PassButtonText[k].text = Random.Range(0, 2) == 1 ? "不好" : "好";
                        }
                        await UniTask.Delay(120);
                    }
                    for (int k = 0; k < PassButtonBackground.Length; k++)
                    {
                        PassButtonText[k].text = "不好";
                    }
                    PassButtonText[thiskey].text = "好";
                    var pressedkey = -1;
                    while (pressedkey == -1)
                    {
                        for (int i = 0; i < 8; i++)
                        {
                            if (InputManager.CheckAreaStatus((Types.SensorArea)i, Types.SensorStatus.On))
                            {
                                pressedkey = i;
                            }
                        }
                        await UniTask.Yield();
                    }
                    if (pressedkey != thiskey)
                    {
                        await Nuke();
                        Application.Quit();
                    }
                    await UniTask.Delay(200);
                }
                StoryManager.Password = Password;
            }
            //print(Password);
            bgm.Stop();
            SongStorage.CollectionIndex = SongStorage.Collections.Length - 1;
            MajInstances.SceneSwitcher.SwitchScene("List", false);
        }

        async UniTask PrintText(string name, string text,int delaytime =50)
        {
            NameText.text = name;
            DialogText.text = "";
            for (int i = 0; i < text.Length; i++)
            {
                if (text[i] == '颰')
                {
                    await Nuke();
                    continue;
                }

                DialogText.text += text[i];
                if (!InputManager.CheckButtonStatus(Types.SensorArea.A5, Types.SensorStatus.On))
                {
                    if (name == "小小蓝白")
                    {
                        await XxlbSays(text[i]);
                    }
                    else
                    {
                        MajInstances.AudioManager.PlaySFX("pop.mp3");
                        await UniTask.Delay(delaytime);
                    }
                }
                else
                {
                    await UniTask.Yield();
                }
                
            }
            if (!InputManager.CheckButtonStatus(Types.SensorArea.A5, Types.SensorStatus.On))
            {
                await UniTask.Delay(200);
            }
        }

        async UniTask XxlbSays(char text)
        {
            var voice = MajInstances.AudioManager.GetSFX("random.mp3");
            voice.Play();
            var byt = System.BitConverter.GetBytes(text).FirstOrDefault();
            var pos = ((double)byt / (double)byte.MaxValue);
            var start = pos * voice.Length.TotalSeconds;
            voice.CurrentSec = start;
            await UniTask.Delay(50);
            voice.Stop();
        }

        async UniTask Nuke()
        {
            NukeVideo.gameObject.SetActive(true);
            MajInstances.AudioManager.PlaySFX("爆.mp3");
            NukeVideo.Play();
            await UniTask.WaitForSeconds(2);
            NukeVideo.Stop();
            NukeVideo.time = 0;
            NukeVideo.gameObject.SetActive(false);
        }
    }
}
