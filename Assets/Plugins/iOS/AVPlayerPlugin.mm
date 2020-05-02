#import <Foundation/Foundation.h>
#import <UIKit/UIKit.h>

#include "IUnityInterface.h"
#include "IUnityGraphics.h"

#include "AVPlayerOperater.h"
    
static IUnityInterfaces* s_UnityInterfaces = NULL;
static IUnityGraphics* s_Graphics = NULL;
static UnityGfxRenderer s_RendererType = kUnityGfxRendererNull;

static void UNITY_INTERFACE_API OnGraphicsDeviceEvent(UnityGfxDeviceEventType eventType)
{
    switch (eventType)
    {
        case kUnityGfxDeviceEventInitialize:
        {
            s_RendererType = s_Graphics->GetRenderer();
            //TODO: ユーザー初期化コード
            break;
        }
        case kUnityGfxDeviceEventShutdown:
        {
            s_RendererType = kUnityGfxRendererNull;
            //TODO: ユーザーシャットダウンコード
            break;
        }
		default:
			break;
    };
}

// Unity プラグインロードイベント
extern "C" void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API UnityPluginLoad(IUnityInterfaces* unityInterfaces)
{
    s_UnityInterfaces = unityInterfaces;
    s_Graphics = unityInterfaces->Get<IUnityGraphics>();
        
    s_Graphics->RegisterDeviceEventCallback(OnGraphicsDeviceEvent);
        
    // OnGraphicsDeviceEvent(initialize) をプラグインのロードに手動で実行して
    // グラフィックスデバイスがすでに初期化されている場合でもイベントを逃さないようにします
    OnGraphicsDeviceEvent(kUnityGfxDeviceEventInitialize);
}

// Unity プラグインアンロードイベント
extern "C" void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API UnityPluginUnload()
{
    s_Graphics->UnregisterDeviceEventCallback(OnGraphicsDeviceEvent);
}

#pragma mark - Wrapper

// Wrapper members
static NSMutableDictionary<NSString*,AVPlayerOperater*>* s_avPlayerOperaterMap = [NSMutableDictionary dictionary];
static int s_avPlayerOperaterNumber = 0;

static bool isExistOperater(AVPlayerOperater* op)
{
	NSArray<AVPlayerOperater*>* list = [s_avPlayerOperaterMap allValues];
	if ([list containsObject:op]) {
		return true;
	}
	return false;
}

// Wrapper methods
extern "C" AVPlayerOperater* AVPlayerCreate()
{
	NSLog(@"AVPlayerPlugin: AVPlayerCreate");

	AVPlayerOperater* op = [[AVPlayerOperater alloc] initWithMetal];
	s_avPlayerOperaterNumber ++;
	NSString* key = [NSString stringWithFormat:@"%d", s_avPlayerOperaterNumber];
	[s_avPlayerOperaterMap setValue:op forKey:key];
	return op;
}

extern "C" void AVPlayerSetContent(AVPlayerOperater* op, const char* contentPath)
{
	NSLog(@"AVPlayerPlugin: AVPlayerSetContent");
	
	if (!isExistOperater(op)) {
		return;
	}
	if (contentPath == nil) {
		NSLog(@"AVPlayerPlugin: contentPath is nil!");
		return;
	}
	NSString* contentString = [NSString stringWithUTF8String:contentPath];
	[op setPlayerItemWithPath:contentString];
}

extern "C" void AVPlayerPlay(AVPlayerOperater* op)
{
	NSLog(@"AVPlayerPlugin: AVPlayerPlay");

	if (!isExistOperater(op)) {
		return;
	}
	[op playWhenReady];
}

extern "C" void AVPlayerPause(AVPlayerOperater* op)
{
	if (!isExistOperater(op)) {
		return;
	}
	[op pauseWhenReady];
}

extern "C" void AVPlayerSeek(AVPlayerOperater* op, float seconds)
{
	if (!isExistOperater(op)) {
		return;
	}
	[op seekWithSeconds:seconds];
}

extern "C" void AVPlayerSetRate(AVPlayerOperater* op, float rate)
{
	if (!isExistOperater(op)) {
		return;
	}
	[op setPlayRate:rate];
}

extern "C" void AVPlayerSetVolume(AVPlayerOperater* op, float volume)
{
	if (!isExistOperater(op)) {
		return;
	}
	[op setVolume:volume];
}

extern "C" void AVPlayerClose(AVPlayerOperater* op)
{
	if (!isExistOperater(op)) {
		return;
	}
	[op closeAll];
	for (NSString* key in [s_avPlayerOperaterMap keyEnumerator]) {
		if (op == [s_avPlayerOperaterMap valueForKey:key]) {
			[s_avPlayerOperaterMap removeObjectForKey:key];
			break;
		}
	}
}

extern "C" float AVPlayerGetCurrentPosition(AVPlayerOperater* op)
{
	if (!isExistOperater(op)) {
		return -1.f;
	}
	return [op getCurrentSconds];
}

extern "C" float AVPlayerGetDuration(AVPlayerOperater* op)
{
	if (!isExistOperater(op)) {
		return 0.f;
	}
	return [op getDuration];
}

extern "C" bool AVPlayerIsPlaying(AVPlayerOperater* op)
{
	if (!isExistOperater(op)) {
		return 0.f;
	}
	return [op isPlaying];
}

extern "C" void AVPlayerSetOnReady(AVPlayerOperater* op, const char* objectName, const char* methodName)
{
	if (!isExistOperater(op)) {
		return;
	}
	if (objectName == nil) {
		return;
	}
	if (methodName == nil) {
		return;
	}
	op.playerCallback.unityObjectName = [NSString stringWithUTF8String:objectName];
	op.playerCallback.unityMethodNameDidReady = [NSString stringWithUTF8String:methodName];
}

extern "C" void AVPlayerSetOnSeek(AVPlayerOperater* op, const char* objectName, const char* methodName)
{
	if (!isExistOperater(op)) {
		return;
	}
	if (objectName == nil) {
		return;
	}
	if (methodName == nil) {
		return;
	}
	op.playerCallback.unityObjectName = [NSString stringWithUTF8String:objectName];
	op.playerCallback.unityMethodNameDidSeek = [NSString stringWithUTF8String:methodName];
}

extern "C" void AVPlayerSetOnEndTime(AVPlayerOperater* op, const char* objectName, const char* methodName)
{
	if (!isExistOperater(op)) {
		return;
	}
	if (objectName == nil) {
		return;
	}
	if (methodName == nil) {
		return;
	}
	op.playerCallback.unityObjectName = [NSString stringWithUTF8String:objectName];
	op.playerCallback.unityMethodNameDidEnd = [NSString stringWithUTF8String:methodName];
}


#pragma mark - Render

// 特定のレンダリングイベントを処理するプラグイン関数
static void UNITY_INTERFACE_API OnRenderEvent(int eventID)
{
    //TODO: ユーザーレンダリングコード
	
}
    
// プラグイン特有のスクリプトにコールバックを渡すための自由に定義した関数
extern "C" UnityRenderingEvent UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API AVPlayerGetRenderEventFunc()
{
    return OnRenderEvent;
}

// EOF
