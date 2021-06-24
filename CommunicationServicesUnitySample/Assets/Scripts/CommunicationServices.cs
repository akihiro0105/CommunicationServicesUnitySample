#if WINDOWS_UWP
using Azure.Communication;
using Azure.Communication.Calling;
using System;
using System.Collections.Generic;
#endif
using System.Threading.Tasks;

public class CommunicationServices
{
#if WINDOWS_UWP
    private CallClient callClient;
    private CallAgent callAgent;
    private Call call;
    private DeviceManager deviceManager;
    private LocalVideoStream[] localVideoStream;
#endif

    public async Task Init(string token, string user)
    {
#if WINDOWS_UWP
        await InitCallAgent(token, user);
#endif
    }

#if WINDOWS_UWP
    // アクセストークンから接続用ユーザーを作成
    private async Task InitCallAgent(string user_token_, string user_name)
    {
        var token_credential = new CommunicationTokenCredential(user_token_);

        callClient = new CallClient();
        deviceManager = await callClient.GetDeviceManager();
        localVideoStream = new LocalVideoStream[1];

        var callAgentOptions = new CallAgentOptions()
        {
            DisplayName = user_name
        };
        callAgent = await callClient.CreateCallAgent(token_credential, callAgentOptions);
        callAgent.OnCallsUpdated += CallAgent_OnCallsUpdated;
        callAgent.OnIncomingCall += CallAgent_OnIncomingCall;
    }

    // 通話着信時のイベント
    private async void CallAgent_OnIncomingCall(object sender, IncomingCall incomingCall)
    {
        GetCameraDevice();

        AcceptCallOptions acceptCallOptions = new AcceptCallOptions();
        acceptCallOptions.VideoOptions = new VideoOptions(localVideoStream);
        call = await incomingCall.AcceptAsync(acceptCallOptions);
    }

    // 相手ビデオ受信時のイベント
    private async void CallAgent_OnCallsUpdated(object sender, CallsUpdatedEventArgs args)
    {
        foreach (var call in args.AddedCalls)
        {
            foreach (var remoteParticipant in call.RemoteParticipants)
            {
                await AddVideoStreams(remoteParticipant.VideoStreams);
                remoteParticipant.OnVideoStreamsUpdated += async (s, a) => await AddVideoStreams(a.AddedRemoteVideoStreams);
            }
            call.OnRemoteParticipantsUpdated += Call_OnRemoteParticipantsUpdated; ;
            call.OnStateChanged += Call_OnStateChanged;
        }
    }

    // 通話状態変更イベント
    private async void Call_OnStateChanged(object sender, PropertyChangedEventArgs args)
    {
        switch (((Call)sender).State)
        {
            // 通話終了
            case CallState.Disconnected:
                break;
            default:
                break;
        }
    }

    private async void Call_OnRemoteParticipantsUpdated(object sender, ParticipantsUpdatedEventArgs args)
    {
        foreach (var remoteParticipant in args.AddedParticipants)
        {
            await AddVideoStreams(remoteParticipant.VideoStreams);
            remoteParticipant.OnVideoStreamsUpdated += async (s, a) => await AddVideoStreams(a.AddedRemoteVideoStreams);
        }
    }

    // 相手ビデオ表示
    private async Task AddVideoStreams(IReadOnlyList<RemoteVideoStream> streams)
    {

        foreach (var remoteVideoStream in streams)
        {
            var remoteUri = await remoteVideoStream.CreateBindingAsync();
            remoteVideoStream.Start();
        }
    }

    // 利用するカメラの取得
    private async void GetCameraDevice()
    {
        if (deviceManager.Cameras.Count > 0)
        {
            var videoDeviceInfo = deviceManager.Cameras[0];
            localVideoStream[0] = new LocalVideoStream(videoDeviceInfo);
            var localUri = await localVideoStream[0].CreateBindingAsync();
        }
    }
#endif

    // 通話開始ボタン押下イベント
    public async Task CallButton_ClickAsync(string call_text)
    {
#if WINDOWS_UWP
        GetCameraDevice();

        var startCallOptions = new StartCallOptions();
        startCallOptions.VideoOptions = new VideoOptions(localVideoStream);
        var callees = new ICommunicationIdentifier[1]
        {
                new CommunicationUserIdentifier(call_text)
        };
        call = await callAgent.StartCallAsync(callees, startCallOptions);
#endif
    }

    // 通話終了ボタン押下イベント
    public async Task HangupButton_Click()
    {
#if WINDOWS_UWP
        await call.HangUpAsync(new HangUpOptions());
#endif
    }

    // Teams参加ボタン押下イベント
    public async Task TeamsButton_Click(string call_text)
    {
#if WINDOWS_UWP
        GetCameraDevice();

        var joinCallOptions = new JoinCallOptions();
        joinCallOptions.VideoOptions = new VideoOptions(localVideoStream);
        var teamsMeetingLinkLocator = new TeamsMeetingLinkLocator(call_text);
        call = await callAgent.JoinAsync(teamsMeetingLinkLocator, joinCallOptions);
#endif
    }
}
