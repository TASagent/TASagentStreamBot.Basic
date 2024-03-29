﻿using System.Web;
using Microsoft.AspNetCore.SignalR;

using TASagentTwitchBot.Core;
using TASagentTwitchBot.Core.Database;
using TASagentTwitchBot.Core.Donations;
using TASagentTwitchBot.Core.Notifications;
using TASagentTwitchBot.Core.Web.Hubs;

namespace TASagentTwitchBot.Basic.Notifications;

public class TTSOnlyActivityProvider :
    IActivityHandler,
    ITTSHandler,
    IDisposable
{
    private readonly Core.Audio.IAudioPlayer audioPlayer;
    private readonly Core.Audio.Effects.IAudioEffectSystem audioEffectSystem;
    private readonly IActivityDispatcher activityDispatcher;
    private readonly Core.TTS.ITTSRenderer ttsRenderer;
    private readonly IHubContext<TTSMarqueeHub> ttsMarqueeHubContext;

    private readonly CancellationTokenSource generalTokenSource = new CancellationTokenSource();

    private bool disposedValue = false;

    public TTSOnlyActivityProvider(
        Core.Audio.IAudioPlayer audioPlayer,
        Core.Audio.Effects.IAudioEffectSystem audioEffectSystem,
        IActivityDispatcher activityDispatcher,
        Core.TTS.ITTSRenderer ttsRenderer,
        IHubContext<TTSMarqueeHub> ttsMarqueeHubContext)
    {
        this.audioPlayer = audioPlayer;
        this.audioEffectSystem = audioEffectSystem;
        this.activityDispatcher = activityDispatcher;
        this.ttsRenderer = ttsRenderer;
        this.ttsMarqueeHubContext = ttsMarqueeHubContext;
    }

    #region IActivityHandler

    public Task Execute(ActivityRequest activityRequest)
    {
        List<Task> taskList = new List<Task>();

        if (activityRequest is IAudioActivity audioActivity && audioActivity.AudioRequest is not null)
        {
            taskList.Add(audioPlayer.PlayAudioRequest(audioActivity.AudioRequest));
        }

        if (activityRequest is IMarqueeMessageActivity marqueeMessageActivity && !string.IsNullOrEmpty(marqueeMessageActivity.MarqueeMessage))
        {
            //Don't bother waiting on this one to complete
            taskList.Add(ttsMarqueeHubContext.Clients.All.SendAsync("ReceiveTTSNotification", marqueeMessageActivity.MarqueeMessage));
        }

        return Task.WhenAll(taskList).WithCancellation(generalTokenSource.Token);
    }

    public void RegisterDonationTracker(IDonationTracker donationTracker)
    {
        //Disregard, we don't need this information
    }

    #endregion IActivityHandler
    #region ITTSHandler

    bool ITTSHandler.IsTTSVoiceValid(string voice) => ttsRenderer.IsTTSVoiceValid(voice);
    Core.TTS.TTSVoiceInfo? ITTSHandler.GetTTSVoiceInfo(string voice) => ttsRenderer.GetTTSVoiceInfo(voice);

    Task<bool> ITTSHandler.SetTTSEnabled(bool enabled) => ttsRenderer.SetTTSEnabled(enabled);

    public virtual async void HandleTTS(
        User user,
        string message,
        bool approved)
    {
        activityDispatcher.QueueActivity(
            activity: new TTSActivityRequest(
                activityHandler: this,
                description: $"TTS {user.TwitchUserName} : {message}",
                requesterId: user.TwitchUserId,
                audioRequest: await GetTTSAudioRequest(user, message),
                marqueeMessage: GetStandardMarqueeMessage(user, message)),
            approved: approved);
    }

    private Task<Core.Audio.AudioRequest?> GetTTSAudioRequest(
        User user,
        string message)
    {
        return ttsRenderer.TTSRequest(
            authorizationLevel: user.AuthorizationLevel,
            voicePreference: user.TTSVoicePreference,
            pitchPreference: user.TTSPitchPreference,
            speedPreference: user.TTSSpeedPreference,
            effectsChain: audioEffectSystem.SafeParse(user.TTSEffectsChain),
            ttsText: message);
    }

    #endregion ITTSHandler

    private string? GetStandardMarqueeMessage(User user, string message)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            return null;
        }

        return $"<h1><span style=\"color: {(string.IsNullOrWhiteSpace(user.Color) ? "#0000FF" : user.Color)}\" >{HttpUtility.HtmlEncode(user.TwitchUserName)}</span>: {HttpUtility.HtmlEncode(message)}</h1>";
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                generalTokenSource.Cancel();
                generalTokenSource.Dispose();
            }

            disposedValue = true;
        }
    }

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    public class TTSActivityRequest : ActivityRequest, IAudioActivity, IMarqueeMessageActivity
    {
        public Core.Audio.AudioRequest? AudioRequest { get; }
        public string? MarqueeMessage { get; }

        public TTSActivityRequest(
            IActivityHandler activityHandler,
            string description,
            string requesterId,
            Core.Audio.AudioRequest? audioRequest = null,
            string? marqueeMessage = null)
            : base(activityHandler, description, requesterId)
        {
            AudioRequest = audioRequest;
            MarqueeMessage = marqueeMessage;
        }
    }
}
