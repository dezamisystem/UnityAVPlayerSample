//
//  AVPlayerOperater.h
//  Copyright (c) 2020 東亜プリン秘密研究所. All rights reserved.
//

#import <Foundation/Foundation.h>
#import <AVFoundation/AVFoundation.h>
#import <MetalKit/MetalKit.h>
#import "AVPlayerCallback.h"

NS_ASSUME_NONNULL_BEGIN

@class AVPlayerOperater;

typedef void (*VideoSizeCallbackCaller)(AVPlayerOperater* sender, int width, int height, void* methodHandle);

@interface AVPlayerOperater : NSObject

@property (nonatomic) NSUInteger index;

@property (nonatomic, strong, nullable) id<MTLTexture> outputTexture;

@property (nonatomic) BOOL isLoopPlay;

@property (nonatomic, strong) AVPlayerCallback* playerCallback;

- (id)initWithIndex:(NSUInteger)index device:(MTLDeviceRef)device;

- (void)setPlayerItemWithPath:(NSString*)contentPath;

- (void)playWhenReady;

- (void)pauseWhenReady;

- (void)seekWithSeconds:(float)seconds;

- (void)setPlayRate:(float)rate;

- (void)setVolume:(float)volume;

- (void)closeAll;

- (float)getCurrentSconds;

- (float)getDuration;

- (BOOL)isPlaying;

- (NSUInteger)getVideoWidth;

- (NSUInteger)getVideoHeight;

- (NSUInteger)getTrackCountWithMediaType:(AVMediaType)type;

- (void)updateVideo;

- (void)setVideoSizeCallbackWithHandle:(void*)handle caller:(VideoSizeCallbackCaller)caller;

@end

NS_ASSUME_NONNULL_END
