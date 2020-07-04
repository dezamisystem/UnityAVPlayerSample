//
//  AVPlayerPlugin.mm
//  Copyright (c) 2020 東亜プリン秘密研究所. All rights reserved.
//

#import <UIKit/UIKit.h>
#import "UnityAppController.h"

#include <map>

#include "Unity/IUnityInterface.h"
#include "Unity/IUnityGraphics.h"
#include "Unity/IUnityGraphicsMetal.h"

#import "AVPlayerOperater.h"

#define TAG @"AVPlayerPlugin"

static IUnityInterfaces* s_UnityInterfaces = NULL;
static IUnityGraphics* s_Graphics = NULL;
static IUnityGraphicsMetal* s_MetalGraphics = NULL;
static bool initialized = false;

static void UNITY_INTERFACE_API OnGraphicsDeviceEvent(UnityGfxDeviceEventType eventType)
{
    NSLog(@"%@ OnGraphicsDeviceEvent", TAG);
    
    switch (eventType)
    {
        case kUnityGfxDeviceEventInitialize:
        {
            initialized = false;
            break;
        }
        case kUnityGfxDeviceEventShutdown:
        {
            initialized = false;
            break;
        }
        default:
            break;
    };
}

// Unity プラグインロードイベント
extern "C" void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API UnityPluginLoad(IUnityInterfaces* unityInterfaces)
{
    NSLog(@"%@ UnityPluginLoad", TAG);
    
    s_UnityInterfaces = unityInterfaces;
    s_Graphics = unityInterfaces->Get<IUnityGraphics>();
    s_MetalGraphics   = s_UnityInterfaces->Get<IUnityGraphicsMetal>();
        
    s_Graphics->RegisterDeviceEventCallback(OnGraphicsDeviceEvent);
        
    // OnGraphicsDeviceEvent(initialize) をプラグインのロードに手動で実行して
    // グラフィックスデバイスがすでに初期化されている場合でもイベントを逃さないようにします
    OnGraphicsDeviceEvent(kUnityGfxDeviceEventInitialize);
}

// Unity プラグインアンロードイベント
extern "C" void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API UnityPluginUnload()
{
    NSLog(@"%@ UnityPluginUnload", TAG);

    s_Graphics->UnregisterDeviceEventCallback(OnGraphicsDeviceEvent);
}

#pragma mark - AppController

@interface MyAppController : UnityAppController
- (void)shouldAttachRenderDelegate;
@end

@implementation MyAppController
- (void)shouldAttachRenderDelegate
{
    NSLog(@"%@ shouldAttachRenderDelegate", TAG);

    UnityRegisterRenderingPluginV5(&UnityPluginLoad, &UnityPluginUnload);
}
@end
IMPL_APP_CONTROLLER_SUBCLASS(MyAppController);

#pragma mark - Wrapper

// Wrapper members
static std::map<int, AVPlayerOperater*> s_avPlayerOperaterMap;
static int s_avPlayerOperaterNumber = 0;

static bool isExistOperater(AVPlayerOperater* op)
{
    if (op == NULL) {
        return false;
    }
    for (auto keyValue : s_avPlayerOperaterMap) {
        if (keyValue.second == op) {
            return true;
        }
    }
    return false;
}

extern "C" int AVPlayerGetEventID(AVPlayerOperater* op)
{
    if (!isExistOperater(op)) {
        return -1;
    }
    return (int)op.index;
}

extern "C" id<MTLTexture> AVPlayerGetTexturePtr(AVPlayerOperater* op)
{
    if (!isExistOperater(op)) {
        return NULL;
    }
    return op.outputTexture;
}

extern "C" void AVPlayerSetTexturePtr(AVPlayerOperater* op, id<MTLTexture> texture)
{
    if (!isExistOperater(op)) {
        return;
    }
    op.outputTexture = texture;
}

extern "C" AVPlayerOperater* AVPlayerCreate()
{
    NSLog(@"AVPlayerPlugin: AVPlayerCreate");
    
    s_avPlayerOperaterNumber ++;
    AVPlayerOperater* op = [[AVPlayerOperater alloc] initWithIndex:s_avPlayerOperaterNumber device:s_MetalGraphics->MetalDevice()];
    s_avPlayerOperaterMap[s_avPlayerOperaterNumber] = op;
    return op;
}

extern "C" void AVPlayerSetContent(AVPlayerOperater* op, const char* contentPath)
{
    NSLog(@"AVPlayerPlugin: AVPlayerSetContent");
    
    if (!isExistOperater(op)) {
        return;
    }
    if (contentPath == NULL) {
        NSLog(@"AVPlayerPlugin: contentPath is NULL!");
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

extern "C" void AVPlayerSetLoop(AVPlayerOperater* op, bool loop)
{
    if (!isExistOperater(op)) {
        return;
    }
    [op setIsLoopPlay:loop];
}

extern "C" void AVPlayerClose(AVPlayerOperater* op)
{
    if (!isExistOperater(op)) {
        return;
    }
    [op closeAll];
    for (auto keyValue : s_avPlayerOperaterMap) {
        if (keyValue.second == op) {
            s_avPlayerOperaterMap[keyValue.first] = NULL;
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
        return false;
    }
    return [op isPlaying];
}

extern "C" int AVPlayerGetVideoWidth(AVPlayerOperater* op)
{
    if (!isExistOperater(op)) {
        return 0.f;
    }
    return (int)[op getVideoWidth];
}

extern "C" int AVPlayerGetVideoHeight(AVPlayerOperater* op)
{
    if (!isExistOperater(op)) {
        return 0.f;
    }
    return (int)[op getVideoHeight];
}

extern "C" bool AVPlayerIsLoop(AVPlayerOperater* op)
{
    if (!isExistOperater(op)) {
        return false;
    }
    return op.isLoopPlay;
}

#pragma mark - Callbacks

extern "C" void AVPlayerSetOnReady(AVPlayerOperater* op, const char* objectName, const char* methodName)
{
    if (!isExistOperater(op)) {
        return;
    }
    if (objectName == NULL) {
        return;
    }
    if (methodName == NULL) {
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
    if (objectName == NULL) {
        return;
    }
    if (methodName == NULL) {
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
    if (objectName == NULL) {
        return;
    }
    if (methodName == NULL) {
        return;
    }
    op.playerCallback.unityObjectName = [NSString stringWithUTF8String:objectName];
    op.playerCallback.unityMethodNameDidEnd = [NSString stringWithUTF8String:methodName];
}

extern "C" void AVPlayerCallbackOnVideoSize(AVPlayerOperater* op, void* methodHandle, VideoSizeCallbackCaller caller)
{
    if (!isExistOperater(op)) {
        return;
    }
    [op setVideoSizeCallbackWithHandle:methodHandle caller:caller];
}

#pragma mark - Render

// 特定のレンダリングイベントを処理するプラグイン関数
static void UNITY_INTERFACE_API OnRenderEvent(int eventID)
{
    if (eventID < 0) {
        return;
    }
    AVPlayerOperater* op = s_avPlayerOperaterMap[eventID];
    if (op != NULL) {
        [op updateVideo];
    }
}
    
// プラグイン特有のスクリプトにコールバックを渡すための自由に定義した関数
extern "C" UnityRenderingEvent UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API AVPlayerGetRenderEventFunc()
{
    return OnRenderEvent;
}

// EOF
