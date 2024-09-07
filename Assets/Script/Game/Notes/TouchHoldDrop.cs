﻿using MajdataPlay.IO;
using MajdataPlay.Types;
using System;
using UnityEngine;
#nullable enable
namespace MajdataPlay.Game.Notes
{
    public class TouchHoldDrop : NoteLongDrop
    {
        public bool isFirework;
        public GameObject tapEffect;
        public GameObject judgeEffect;

        public Sprite board_On;
        public Sprite Board_Off;
        public SpriteRenderer boarder;

        public GameObject[] fans;
        public SpriteMask mask;
        private readonly SpriteRenderer[] fanRenderers = new SpriteRenderer[6];
        private float displayDuration;

        private GameObject firework;
        private Animator fireworkEffect;
        private float moveDuration;

        private float wholeDuration;

        JudgeTextSkin judgeText;

        // Start is called before the first frame update
        protected override void Start()
        {
            base.Start();
            wholeDuration = 3.209385682f * Mathf.Pow(speed, -0.9549621752f);
            moveDuration = 0.8f * wholeDuration;
            displayDuration = 0.2f * wholeDuration;

            holdEffect = Instantiate(holdEffect, noteManager.transform);
            holdEffect.SetActive(false);

            firework = GameObject.Find("FireworkEffect");
            fireworkEffect = firework.GetComponent<Animator>();

            LoadSkin();

            SetfanColor(new Color(1f, 1f, 1f, 0f));
            mask.enabled = false;

            sensorPos = SensorType.C;
            var customSkin = SkinManager.Instance;
            judgeText = customSkin.GetJudgeTextSkin();
            ioManager.BindSensor(Check, SensorType.C);
        }
        protected override void Check(object sender, InputEventArgs arg)
        {
            if (isJudged || !noteManager.CanJudge(gameObject, sensorPos))
                return;
            else if (arg.IsClick)
            {
                if (!ioManager.IsIdle(arg))
                    return;
                else
                    ioManager.SetBusy(arg);
                Judge();
                ioManager.SetIdle(arg);
                if (isJudged)
                {
                    ioManager.UnbindSensor(Check, SensorType.C);
                    objectCounter.NextTouch(SensorType.C);
                }
            }
        }
        protected override void LoadSkin()
        {
            var skin = SkinManager.Instance.GetTouchHoldSkin();
            for (var i = 0; i < 6; i++)
            {
                fanRenderers[i] = fans[i].GetComponent<SpriteRenderer>();
                fanRenderers[i].sortingOrder += noteSortOrder;
            }

            for (var i = 0; i < 4; i++)
                fanRenderers[i].sprite = skin.Fans[i];
            fanRenderers[5].sprite = skin.Boader; // TouchHold Border
            fanRenderers[4].sprite = skin.Point;
            board_On = skin.Boader;
            Board_Off = skin.Off;
        }
        void Judge()
        {

            const float JUDGE_GOOD_AREA = 316.667f;
            const int JUDGE_GREAT_AREA = 250;
            const int JUDGE_PERFECT_AREA = 200;

            const float JUDGE_SEG_PERFECT = 150f;

            if (isJudged)
                return;

            var timing = GetTimeSpanToJudgeTiming();
            var isFast = timing < 0;
            judgeDiff = timing * 1000;
            var diff = MathF.Abs(timing * 1000);
            JudgeType result;
            if (diff > JUDGE_SEG_PERFECT && isFast)
                return;
            else if (diff < JUDGE_SEG_PERFECT)
                result = JudgeType.Perfect;
            else if (diff < JUDGE_PERFECT_AREA)
                result = JudgeType.LatePerfect2;
            else if (diff < JUDGE_GREAT_AREA)
                result = JudgeType.LateGreat;
            else if (diff < JUDGE_GOOD_AREA)
                result = JudgeType.LateGood;
            else
                result = JudgeType.Miss;
            if (isFast)
                judgeDiff = 0;
            else
                judgeDiff = diff;

            judgeResult = result;
            isJudged = true;
            PlayHoldEffect();
        }
        private void FixedUpdate()
        {
            var remainingTime = GetRemainingTime();
            var timing = GetTimeSpanToJudgeTiming();
            var isTooLate = timing > 0.316667f;

            if (remainingTime == 0 && isJudged)
            {
                Destroy(holdEffect);
                Destroy(gameObject);
            }
            
            if (isJudged)
            {
                if (timing <= 0.25f) // 忽略头部15帧
                    return;
                else if (remainingTime <= 0.2f) // 忽略尾部12帧
                    return;
                else if (!gpManager.isStart) // 忽略暂停
                    return;

                var on = ioManager.CheckSensorStatus(SensorType.C, SensorStatus.On);
                if (on)
                    PlayHoldEffect();
                else
                {
                    playerIdleTime += Time.fixedDeltaTime;
                    StopHoldEffect();
                }
            }
            else if (isTooLate)
            {
                judgeDiff = 316.667f;
                judgeResult = JudgeType.Miss;
                ioManager.UnbindSensor(Check, SensorType.C);
                isJudged = true;
                objectCounter.NextTouch(SensorType.C);
            }
        }
        // Update is called once per frame
        private void Update()
        {
            var timing = GetTimeSpanToArriveTiming();
            var pow = -Mathf.Exp(8 * (timing * 0.4f / moveDuration) - 0.85f) + 0.42f;
            var distance = Mathf.Clamp(pow, 0f, 0.4f);

            if (-timing <= wholeDuration && -timing > moveDuration)
            {
                SetfanColor(new Color(1f, 1f, 1f, Mathf.Clamp((wholeDuration + timing) / displayDuration, 0f, 1f)));
                fans[5].SetActive(false);
                mask.enabled = false;
            }
            else if (-timing < moveDuration)
            {
                fans[5].SetActive(true);
                mask.enabled = true;
                SetfanColor(Color.white);
                mask.alphaCutoff = Mathf.Clamp(0.91f * (1 - (LastFor - timing) / LastFor), 0f, 1f);
            }

            if (float.IsNaN(distance)) distance = 0f;

            for (var i = 0; i < 4; i++)
            {
                var pos = (0.226f + distance) * GetAngle(i);
                fans[i].transform.position = pos;
            }
        }
        void EndJudge(ref JudgeType result)
        {
            if (!isJudged) 
                return;
            var realityHT = LastFor - 0.45f - judgeDiff / 1000f;
            var percent = MathF.Min(1, (realityHT - playerIdleTime) / realityHT);
            result = judgeResult;
            if (realityHT > 0)
            {
                if (percent >= 1f)
                {
                    if (judgeResult == JudgeType.Miss)
                        result = JudgeType.LateGood;
                    else if (MathF.Abs((int)judgeResult - 7) == 6)
                        result = (int)judgeResult < 7 ? JudgeType.LateGreat : JudgeType.FastGreat;
                    else
                        result = judgeResult;
                }
                else if (percent >= 0.67f)
                {
                    if (judgeResult == JudgeType.Miss)
                        result = JudgeType.LateGood;
                    else if (MathF.Abs((int)judgeResult - 7) == 6)
                        result = (int)judgeResult < 7 ? JudgeType.LateGreat : JudgeType.FastGreat;
                    else if (judgeResult == JudgeType.Perfect)
                        result = (int)judgeResult < 7 ? JudgeType.LatePerfect1 : JudgeType.FastPerfect1;
                }
                else if (percent >= 0.33f)
                {
                    if (MathF.Abs((int)judgeResult - 7) >= 6)
                        result = (int)judgeResult < 7 ? JudgeType.LateGood : JudgeType.FastGood;
                    else
                        result = (int)judgeResult < 7 ? JudgeType.LateGreat : JudgeType.FastGreat;
                }
                else if (percent >= 0.05f)
                    result = (int)judgeResult < 7 ? JudgeType.LateGood : JudgeType.FastGood;
                else if (percent >= 0)
                {
                    if (judgeResult == JudgeType.Miss)
                        result = JudgeType.Miss;
                    else
                        result = (int)judgeResult < 7 ? JudgeType.LateGood : JudgeType.FastGood;
                }
            }
            print($"TouchHold: {MathF.Round(percent * 100, 2)}%\nTotal Len : {MathF.Round(realityHT * 1000, 2)}ms");
        }
        private void OnDestroy()
        {
            ioManager.UnbindSensor(Check, SensorType.C);
            EndJudge(ref judgeResult);

            var result = new JudgeResult()
            {
                Result = judgeResult,
                IsBreak = isBreak,
                Diff = judgeDiff
            };
            objectCounter.ReportResult(this, result);
            if (!isJudged)
                objectCounter.NextTouch(SensorType.C);
            var audioEffMana = GameObject.Find("NoteAudioManager").GetComponent<NoteAudioManager>();
            if (isFirework && !result.IsMiss)
            {
                fireworkEffect.SetTrigger("Fire");
                firework.transform.position = transform.position;
                
                audioEffMana.PlayHanabiSound();
            }
            audioEffMana.PlayTapSound(false,false,judgeResult);
            audioEffMana.StopTouchHoldSound();

            PlayJudgeEffect(result);
        }
        void PlayJudgeEffect(in JudgeResult judgeResult)
        {
            var obj = Instantiate(judgeEffect, Vector3.zero, transform.rotation);
            var _obj = Instantiate(judgeEffect, Vector3.zero, transform.rotation);
            var judgeObj = obj.transform.GetChild(0);
            var flObj = _obj.transform.GetChild(0);
            var distance = -0.6f;

            judgeObj.transform.position = new Vector3(0, distance, 0);
            flObj.transform.position = new Vector3(0, distance - 0.48f, 0);
            flObj.GetChild(0).transform.rotation = Quaternion.Euler(Vector3.zero);
            judgeObj.GetChild(0).transform.rotation = Quaternion.Euler(Vector3.zero);
            var anim = obj.GetComponent<Animator>();

            var effects = GameObject.Find("NoteEffects");
            var flAnim = _obj.GetComponent<Animator>();
            GameObject effect;
            switch (judgeResult.Result)
            {
                case JudgeType.LateGood:
                case JudgeType.FastGood:
                    judgeObj.GetChild(0).gameObject.GetComponent<SpriteRenderer>().sprite = judgeText.Good;
                    effect = Instantiate(effects.transform.GetChild(3).GetChild(0), transform.position, transform.rotation).gameObject;
                    effect.SetActive(true);
                    break;
                case JudgeType.LateGreat:
                case JudgeType.LateGreat1:
                case JudgeType.LateGreat2:
                case JudgeType.FastGreat2:
                case JudgeType.FastGreat1:
                case JudgeType.FastGreat:
                    judgeObj.GetChild(0).gameObject.GetComponent<SpriteRenderer>().sprite = judgeText.Great;
                    //transform.Rotate(0, 0f, 30f);
                    effect = Instantiate(effects.transform.GetChild(2).GetChild(0), transform.position, transform.rotation).gameObject;
                    effect.SetActive(true);
                    effect.gameObject.GetComponent<Animator>().SetTrigger("great");
                    break;
                case JudgeType.LatePerfect2:
                case JudgeType.FastPerfect2:
                case JudgeType.LatePerfect1:
                case JudgeType.FastPerfect1:
                    judgeObj.GetChild(0).gameObject.GetComponent<SpriteRenderer>().sprite = judgeText.Perfect;
                    transform.Rotate(0, 180f, 90f);
                    Instantiate(tapEffect, transform.position, transform.rotation);
                    break;
                case JudgeType.Perfect:
                    if (GameManager.Instance.Setting.Display.DisplayCriticalPerfect)
                        judgeObj.GetChild(0).gameObject.GetComponent<SpriteRenderer>().sprite = judgeText.CriticalPerfect;
                    else
                        judgeObj.GetChild(0).gameObject.GetComponent<SpriteRenderer>().sprite = judgeText.Perfect;
                    transform.Rotate(0, 180f, 90f);
                    Instantiate(tapEffect, transform.position, transform.rotation);
                    break;
                case JudgeType.Miss:
                    judgeObj.GetChild(0).gameObject.GetComponent<SpriteRenderer>().sprite = judgeText.Miss;
                    break;
                default:
                    break;
            }

            effectManager.PlayFastLate(_obj, flAnim, judgeResult);
            anim.SetTrigger("touch");
        }
        protected override void PlayHoldEffect()
        {
            base.PlayHoldEffect();
            var audioEffMana = GameObject.Find("NoteAudioManager").GetComponent<NoteAudioManager>();
            audioEffMana.PlayTouchHoldSound();
            boarder.sprite = board_On;
        }
        protected override void StopHoldEffect()
        {
            base.StopHoldEffect();
            var audioEffMana = GameObject.Find("NoteAudioManager").GetComponent<NoteAudioManager>();
            audioEffMana.StopTouchHoldSound();
            boarder.sprite = Board_Off;
        }
        Vector3 GetAngle(int index)
        {
            var angle = Mathf.PI / 4 + index * (Mathf.PI / 2);
            return new Vector3(Mathf.Sin(angle), Mathf.Cos(angle));
        }
        void SetfanColor(Color color)
        {
            foreach (var fan in fanRenderers) fan.color = color;
        }
    }
}