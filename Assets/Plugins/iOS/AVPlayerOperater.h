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

@protocol AVPlayerOperaterDelegate <NSObject>
@required
- (void)didReady:(AVPlayerOperater*)op;
- (void)didSeek:(AVPlayerOperater*)op;
@end

@interface AVPlayerOperater : NSObject

@property (assign, nonatomic) NSUInteger index;

@property (strong, nonatomic) id<AVPlayerOperaterDelegate> delegate;

@property (strong, nonatomic) AVPlayerCallback* playerCallback;

- (id)initWithIndex:(NSUInteger)index device:(MTLDeviceRef)device;

- (id<MTLTexture>)getOutputTexture;

- (void)setOutputTexture:(id<MTLTexture>)texture;

- (void)setPlayerItemWithPath:(NSString*)contentPath;

- (void)playWhenReady;

- (void)pauseWhenReady;

- (void)seekWithSeconds:(float)seconds;

- (void)setPlayRate:(float)rate;

- (void)setVolume:(float)volume;

- (void)setLoop:(BOOL)loop;

- (void)closeAll;

- (float)getCurrentSconds;

- (float)getDuration;

- (BOOL)isPlaying;

- (NSUInteger)getVideoWidth;

- (NSUInteger)getVideoHeight;

- (void)updateVideo;

- (void)setVideoSizeCallbackWithHandle:(void*)handle caller:(VideoSizeCallbackCaller)caller;

@end

NS_ASSUME_NONNULL_END
