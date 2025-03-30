using Cysharp.Threading.Tasks;
using MajdataPlay.IO;
using MajdataPlay.Utils;
using MajSimai;
using NeoSmart.AsyncLock;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
#nullable enable
namespace MajdataPlay.Types
{

    public class SongDetail : ISongDetail
    {
        public bool IsUnlocked { get; private set; } = true;
        public string Title { get; private set; } = string.Empty;
        public string Artist { get; private set; } = string.Empty;
        public string[] Designers { get; private set; } = new string[7];
        public string Description { get; private set; } = string.Empty;
        public string[] Levels { get; private set; } = new string[7];
        public string Hash { get; init; } = string.Empty;
        public DateTime Timestamp { get; init; }
        public ChartStorageLocation Location => ChartStorageLocation.Local;

        protected readonly string _maidataPath = string.Empty;
        protected readonly string _trackPath = string.Empty;
        protected readonly string _videoPath = string.Empty;
        protected readonly string _coverPath = string.Empty;

        protected AudioSampleWrap? _audioTrack = null;
        protected AudioSampleWrap? _previewAudioTrack = null;
        protected Sprite? _cover = null;
        protected SimaiFile? _maidata = null;

        protected readonly AsyncLock _previewAudioTrackLock = new();
        protected readonly AsyncLock _audioTrackLock = new();
        protected readonly AsyncLock _coverLock = new();
        protected readonly AsyncLock _maidataLock = new();
        protected readonly AsyncLock _preloadLock = new();

        protected readonly Func<Task> _preloadCallback;

        readonly SimaiMetadata _metadata;

        public SongDetail(string chartFolder, SimaiMetadata metadata)
        {
            var files = new DirectoryInfo(chartFolder).GetFiles();

            _maidataPath = Path.Combine(chartFolder, "maidata.txt");
            _trackPath = files.FirstOrDefault(o => o.Name is "track.mp3" or "track.ogg").FullName;
            _videoPath = files.FirstOrDefault(o => o.Name is "bg.mp4" or "pv.mp4" or "mv.mp4")?.FullName ?? string.Empty;
            _coverPath = files.FirstOrDefault(o => o.Name is "bg.png" or "bg.jpg")?.FullName ?? string.Empty;
            _maidata = null;

            if (string.IsNullOrEmpty(_coverPath))
                _cover = MajEnv.EmptySongCover;
            _metadata = metadata;
            Title = metadata.Title;
            Artist = metadata.Artist;
            Designers = metadata.Designers;
            Levels = metadata.Levels;
            Hash = metadata.Hash;
            Timestamp = files.FirstOrDefault(x => x.Name is "maidata.txt")?.LastWriteTime ?? DateTime.UnixEpoch;
            _preloadCallback = async () => { await UniTask.WhenAll(GetMaidataAsync(), GetCoverAsync(true)); };
            MajDebug.Log(Hash);
            if(Hash == "bmUnCQYd7lpyrtM+kiHrkA==")
            {
                IsUnlocked = MajEnv.ChartUnlockingStatus["bmUnCQYd7lpyrtM+kiHrkA=="];
                if(!IsUnlocked)
                {
                    Title = "???";
                    Artist = "???";
                    Designers = new string[7]
                    {
                        "???",
                        "???",
                        "???",
                        "???",
                        "???",
                        "???",
                        "???",
                    };
                    Levels = new string[7]
                    {
                        "?",
                        "?",
                        "?",
                        "?",
                        "?",
                        "?",
                        "?",
                    };
                }
            }
        }
        public static async Task<SongDetail> ParseAsync(string chartFolder)
        {
            var maidataPath = Path.Combine(chartFolder, "maidata.txt");
            var metadata = await SimaiParser.Shared.ParseMetadataAsync(maidataPath);

            return new SongDetail(chartFolder, metadata);
        }
        public virtual async UniTask PreloadAsync(CancellationToken token = default)
        {
            try
            {
                if (!await _preloadLock.TryLockAsync(_preloadCallback, TimeSpan.Zero))
                    return;
            }
            finally
            {
                await UniTask.Yield();
            }
        }
        public virtual async UniTask<string> GetVideoPathAsync(CancellationToken token = default)
        {
            await UniTask.Yield();
            return _videoPath;
        }
        public virtual async UniTask<Sprite> GetCoverAsync(bool isCompressed, CancellationToken token = default)
        {
            try
            {
                using (await _coverLock.LockAsync(token))
                {
                    token.ThrowIfCancellationRequested();
                    if (!IsUnlocked)
                        return MajEnv.LockedSongCover;
                    else if (_cover is not null)
                        return _cover;

                    _cover = await SpriteLoader.LoadAsync(_coverPath, token);
                    return _cover;
                }
            }
            finally
            {
                await UniTask.Yield();
            }
        }
        public virtual async UniTask<AudioSampleWrap> GetAudioTrackAsync(CancellationToken token = default)
        {
            try
            {
                using (await _audioTrackLock.LockAsync(token))
                {
                    token.ThrowIfCancellationRequested();
                    if (_audioTrack is not null)
                        return _audioTrack;

                    _audioTrack = await MajInstances.AudioManager.LoadMusicAsync(_trackPath, true);
                    return _audioTrack;
                }
            }
            finally
            {
                await UniTask.Yield();
            }
        }
        public virtual async UniTask<AudioSampleWrap> GetPreviewAudioTrackAsync(CancellationToken token = default)
        {
            try
            {
                using (await _previewAudioTrackLock.LockAsync(token))
                {
                    token.ThrowIfCancellationRequested();
                    if (_previewAudioTrack is not null)
                        return _previewAudioTrack;
                    else if (!IsUnlocked)
                        return EmptyAudioSample.Shared;

                    _previewAudioTrack = await MajInstances.AudioManager.LoadMusicAsync(_trackPath, false);
                    return _previewAudioTrack;
                }
            }
            finally
            {
                await UniTask.Yield();
            }
        }
        public virtual async UniTask<SimaiFile> GetMaidataAsync(bool ignoreCache = false, CancellationToken token = default)
        {
            try
            {
                using (await _maidataLock.LockAsync(token))
                {
                    token.ThrowIfCancellationRequested();
                    if (!ignoreCache && _maidata is not null)
                        return _maidata;

                    _maidata = await SimaiParser.Shared.ParseAsync(_maidataPath);
                    return _maidata;
                }
            }
            finally
            {
                await UniTask.Yield();
            }
        }
        public bool TryUnlock(string passwd)
        {
        THIS_IS_A_LABEL:
            if (IsUnlocked)
            {
                Title = _metadata.Title;
                Artist = _metadata.Artist;
                Designers = _metadata.Designers;
                Levels = _metadata.Levels;
                return true;
            }
            else if (passwd == StoryManager.Password)
            {
                IsUnlocked = true;
                MajEnv.ChartUnlockingStatus[Hash] = true;
                goto THIS_IS_A_LABEL;
            }
            else
            {
                return false;
            }
        }
    }
}