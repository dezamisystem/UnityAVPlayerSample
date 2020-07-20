//
//  AVPlayerOperater.m
//  Copyright (c) 2020 東亜プリン秘密研究所. All rights reserved.
//

#import "AVPlayerOperater.h"

#define TAG @"AVPlayerOperater"

NS_ASSUME_NONNULL_BEGIN
@interface AVPlayerOperater ()
{
    __nullable MTLDeviceRef _metalDevice;
    __nullable MTLCommandQueueRef _commandQueue;
    CVMetalTextureCacheRef _textureCache;
    __nullable id<MTLTexture> _inputTexture;
    
    AVPlayer* _Nonnull _avPlayer;
    AVPlayerItemVideoOutput* _Nonnull _videoOutput;
    AVPlayerItem* _Nullable _avPlayerItem;

    void* _Nullable _videoSizeCallbackHandle;
    __nullable VideoSizeCallbackCaller _videoSizeCallbackCaller;

    NSUInteger _videoSizeWidth;
    NSUInteger _videoSizeHeight;
    
    BOOL _isObserverItemStatusContextActive;
    BOOL _isObserverPresentationSizeContextActive;
    
    float _playSpeed;
}

@end
NS_ASSUME_NONNULL_END

static NSKeyValueObservingOptions s_ObservingOptions = NSKeyValueObservingOptionInitial | NSKeyValueObservingOptionNew;
static void* s_ObserveItemStatusContext = (void*)0x1;
static void* s_ObservePresentationSizeContext = (void*)0x2;

@implementation AVPlayerOperater

#pragma mark - Public

#pragma mark Initialize

- (id)init
{
    NSAssert(NO, @"%@: init must never be called!", TAG);
    return nil;
}

- (id)initWithIndex:(NSUInteger)index device:(MTLDeviceRef)device
{
    NSLog(@"%@: initWithIndex", TAG);    
    if (self = [super init]) {
        self.index = index;
        _avPlayer = [[AVPlayer alloc] init];
        _avPlayer.actionAtItemEnd = AVPlayerActionAtItemEndNone;
        // Metal
        _metalDevice = device;
        if (device != nil) {
            _commandQueue = [device newCommandQueue];
            CVMetalTextureCacheCreate(kCFAllocatorDefault, nil, device, nil, &_textureCache);
        }
        // Video
        NSDictionary<NSString*,id>* attributes = @{
            (NSString*)kCVPixelBufferPixelFormatTypeKey: @(kCVPixelFormatType_32BGRA)
        };
        _videoOutput = [[AVPlayerItemVideoOutput alloc] initWithPixelBufferAttributes:attributes];
        // Callback
        self.playerCallback = [[AVPlayerCallback alloc] init];
        // Values
        _playSpeed = 1.f;
        _videoSizeWidth = 0;
        _videoSizeHeight = 0;
    }
    return self;
}

#pragma mark APIs

- (void)setPlayerItemWithPath:(NSString*)contentPath
{
    NSLog(@"%@: setPlayerItemWithPath", TAG);
    if (contentPath == nil) {
        NSLog(@"%@: contentPath is nil!", TAG);
        return;
    }
    NSURL* contentUrl = [NSURL URLWithString:contentPath];
    if (contentUrl == nil) {
        NSLog(@"%@: contentUrl is nil!", TAG);
        return;
    }
    AVURLAsset* contentAsset = [AVURLAsset URLAssetWithURL:contentUrl options:nil];
    if (contentAsset == nil) {
        NSLog(@"%@: contentAsset is nil!", TAG);
        return;
    }
    NSLog(@"%@: contentPath = %@", TAG, contentPath);
    // Asset prepareing
    [self prepareAsset:contentAsset];
}

- (void)playWhenReady
{
    NSLog(@"AVPlayerOperater: playWhenReady");

    [_avPlayer play];
    _avPlayer.rate = _playSpeed;
}

- (void)pauseWhenReady
{
    NSLog(@"AVPlayerOperater: pauseWhenReady");

    [_avPlayer pause];
}

- (void)seekWithSeconds:(float)seconds
{
    NSLog(@"AVPlayerOperater: seekWithSeconds");

    CMTime position = CMTimeMakeWithSeconds(seconds, NSEC_PER_SEC);
    [_avPlayer seekToTime:position completionHandler:^(BOOL finished) {
        // callback
        [self.playerCallback onSeek];
    }];
}

- (void)setPlayRate:(float)rate
{
    _playSpeed = rate;
    if (_avPlayer.rate > 0) {
        _avPlayer.rate = rate;
    }
}

- (void)setVolume:(float)volume
{
    _avPlayer.volume = volume;
}

- (void)closeAll
{
    NSLog(@"AVPlayerOperater: closeAll");

    [NSNotificationCenter.defaultCenter removeObserver:self];
    [self removeAVPlayerCurrentItemPresentationSizeObserver];
    [self removeAVPlayerItemStatusObserver];
    [_avPlayer replaceCurrentItemWithPlayerItem:nil];
    if (_avPlayerItem != nil) {
        [_avPlayerItem removeOutput:_videoOutput];
    }
    _avPlayerItem = nil;
}

- (float)getCurrentSconds
{
    Float64 currentPosition = _avPlayerItem != nil ? CMTimeGetSeconds(_avPlayerItem.currentTime) : -1.f;
    return (float)currentPosition;
}

- (float)getDuration
{
    Float64 duration = _avPlayerItem != nil ? CMTimeGetSeconds(_avPlayerItem.duration) : 0.f;
    return (float)duration;
}

- (BOOL)isPlaying
{
    return _avPlayer.rate != 0 ? true : false;
}

- (NSUInteger)getVideoWidth
{
    return _videoSizeWidth;
}

- (NSUInteger)getVideoHeight
{
    return _videoSizeHeight;
}

- (NSUInteger)getTrackCountWithMediaType:(AVMediaType)type
{
    if (_avPlayerItem == nil) {
        return 0;
    }
    NSArray<AVAssetTrack *> *tracks = [_avPlayerItem.asset tracksWithMediaType:type];
    return tracks.count;
}

// TODO: track info
//- (NSUInteger)getTrackNameWithMediaType:(AVMediaType)type index:(NSUInteger)index
//{
//    if (_avPlayerItem == nil) {
//        return 0;
//    }
//    NSArray<AVAssetTrack *> *tracks = [_avPlayerItem.asset tracksWithMediaType:type];
//    if (index <= tracks.count) {
//        return 0;
//    }
//    AVAssetTrack* track = tracks[index];
//
//    return 0;
//}

#pragma mark Render

- (void)updateVideo
{
    @synchronized (self) {
        [self readBuffer];
    }
}

#pragma mark - Callbacks

- (void)setVideoSizeCallbackWithHandle:(void*)handle caller:(VideoSizeCallbackCaller)caller
{
    _videoSizeCallbackHandle = handle;
    _videoSizeCallbackCaller = caller;
}

#pragma mark - Observers

- (void)observeValueForKeyPath:(NSString *)keyPath
                      ofObject:(id)object
                        change:(NSDictionary<NSKeyValueChangeKey,id> *)change
                       context:(void *)context
{
    NSLog(@"AVPlayerOperater: observeValueForKeyPath = %@", keyPath);

    if (context == s_ObserveItemStatusContext) {
        AVPlayerStatus status = [[change objectForKey:NSKeyValueChangeNewKey] integerValue];
        if (status == AVPlayerItemStatusReadyToPlay) {
            [self removeAVPlayerItemStatusObserver];
            [self addAVPlayerCurrentItemPresentationSizeObserver];
            // Notification
            [self addAVPlayerItemDidPlayToEndTimeNotification];
            // callback
            [self.playerCallback onReady];

            NSLog(@"AVPlayerOperater: status == AVPlayerItemStatusReadyToPlay");
        }
    }
    else if (context == s_ObservePresentationSizeContext) {
        AVPlayerItem* playerItem = _avPlayer.currentItem;
        NSUInteger width = playerItem.presentationSize.width;
        NSUInteger height = playerItem.presentationSize.height;
        if (!(width == _videoSizeWidth && height == _videoSizeHeight)) {
            NSLog(@"%@: New presentationSize (%lu, %lu)", TAG, width, height);
            _videoSizeWidth = width;
            _videoSizeHeight = height;
            if (self.outputTexture == nil) {
                // Create output texture
                CGSize videoSize = CGSizeMake(width, height);
                self.outputTexture = [self createOutputTextureWithSize:videoSize];
            }
            // Video Size Callback
            if (_videoSizeCallbackHandle != nil && _videoSizeCallbackCaller != nil) {
                (_videoSizeCallbackCaller)(self, (int)_videoSizeWidth, (int)_videoSizeHeight, _videoSizeCallbackHandle);
            }
        }
    }
    else {
        [super observeValueForKeyPath:keyPath ofObject:object change:change context:context];
    }
}

#define OBSERVER_KEY_PATH_STATUS @"status"

- (void)addAVPlayerItemStatusObserver
{
    if (_isObserverItemStatusContextActive) {
        return;
    }
    if (_avPlayerItem == nil) {
        return;
    }
    [_avPlayerItem addObserver:self
                    forKeyPath:OBSERVER_KEY_PATH_STATUS
                       options:s_ObservingOptions
                       context:s_ObserveItemStatusContext];
    _isObserverItemStatusContextActive = YES;
}

- (void)removeAVPlayerItemStatusObserver
{
    if (!_isObserverItemStatusContextActive) {
        return;
    }
    if (_avPlayerItem == nil) {
        return;
    }
    [_avPlayerItem removeObserver:self forKeyPath:OBSERVER_KEY_PATH_STATUS];
    _isObserverItemStatusContextActive = NO;
}

#define OBSERVER_KEY_PATH_PRESENTATION_SIZE @"currentItem.presentationSize"

- (void)addAVPlayerCurrentItemPresentationSizeObserver
{
    if (_isObserverPresentationSizeContextActive) {
        return;
    }
    [_avPlayer addObserver:self
                forKeyPath:OBSERVER_KEY_PATH_PRESENTATION_SIZE
                   options:s_ObservingOptions
                   context:s_ObservePresentationSizeContext];
    _isObserverPresentationSizeContextActive = YES;
}

- (void)removeAVPlayerCurrentItemPresentationSizeObserver
{
    if (!_isObserverPresentationSizeContextActive) {
        return;
    }
    [_avPlayer removeObserver:self forKeyPath:OBSERVER_KEY_PATH_PRESENTATION_SIZE];
    _isObserverPresentationSizeContextActive = NO;
}

#pragma mark - Notifications

- (void)didPlayToEndTime:(NSNotification*)notification
{
    if (self.isLoopPlay) {
        [_avPlayer seekToTime:kCMTimeZero completionHandler:^(BOOL finished) {
            [_avPlayer play];
        }];
    }
    else {
        [self.playerCallback onEndTime];
    }
}

- (void)addAVPlayerItemDidPlayToEndTimeNotification
{
    [NSNotificationCenter.defaultCenter addObserver:self
                                           selector:@selector(didPlayToEndTime:)
                                               name:AVPlayerItemDidPlayToEndTimeNotification
                                             object:nil];
}

- (void)removeAVPlayerItemDidPlayToEndTimeNotification
{
    [NSNotificationCenter.defaultCenter removeObserver:self
                                                  name:AVPlayerItemDidPlayToEndTimeNotification
                                                object:nil];
}

#pragma mark - Private

/// Prepare to play
/// @param asset AVAsset
-(void)prepareAsset:(AVAsset*)asset
{
    NSLog(@"AVPlayerOperater: prepareAsset");
    
    _avPlayerItem = [AVPlayerItem playerItemWithAsset:asset];
    [_avPlayerItem addOutput:_videoOutput];
    
    [self addAVPlayerItemStatusObserver];
    
    [_avPlayer replaceCurrentItemWithPlayerItem: _avPlayerItem];
}

/// Create a video output texture
/// @param videoSize video size
/// @return MTLTextureRef
- (MTLTextureRef)createOutputTextureWithSize:(CGSize)videoSize
{
    if (_metalDevice == nil) {
        return nil;
    }
    if (videoSize.width == 0) {
        return nil;
    }
    if (videoSize.height == 0) {
        return nil;
    }
    NSLog(@"%@: createOutputTextureWithSize", TAG);
    MTLTextureDescriptor* descriptor =
    [MTLTextureDescriptor texture2DDescriptorWithPixelFormat:MTLPixelFormatBGRA8Unorm
                                                       width:videoSize.width
                                                      height:videoSize.height
                                                   mipmapped:NO];
    return [_metalDevice newTextureWithDescriptor:descriptor];
}

/// Read texture buffer
- (void)readBuffer
{
    if (_metalDevice == nil) {
        return;
    }

    CMTime currentTime = _avPlayer.currentTime;
    if (![_videoOutput hasNewPixelBufferForItemTime:currentTime]) {
        return;
    }

    @autoreleasepool
    {
        CVPixelBufferRef pixelBuffer = [_videoOutput copyPixelBufferForItemTime:currentTime
                                                             itemTimeForDisplay:nil];
        if (pixelBuffer != nil) {
            size_t width = CVPixelBufferGetWidth(pixelBuffer);
            size_t height = CVPixelBufferGetHeight(pixelBuffer);
            
            CVMetalTextureRef cvTextureOut = nil;
            CVReturn status = CVMetalTextureCacheCreateTextureFromImage(kCFAllocatorDefault,
                                                                        _textureCache,
                                                                        pixelBuffer,
                                                                        nil,
                                                                        MTLPixelFormatBGRA8Unorm,
                                                                        width,
                                                                        height,
                                                                        0,
                                                                        &cvTextureOut);
            if(status == kCVReturnSuccess) {
                _inputTexture = CVMetalTextureGetTexture(cvTextureOut);
                CFRelease(cvTextureOut);
            }
            CFRelease(pixelBuffer);
            
            [self copyTextureWithWidth:width height:height];
        }
    } // autoreleasepool
}

/// Copy from input texture to output texture
/// @param width Width
/// @param height Height
- (void)copyTextureWithWidth:(NSUInteger)width height:(NSUInteger)height
{
    if (_commandQueue == nil) {
        return;
    }
    if (_inputTexture == nil) {
        return;
    }
    if (self.outputTexture == nil) {
        return;
    }
    
    // Size check of difference
    if (!(width == self.outputTexture.width && height == self.outputTexture.height)) {
        self.outputTexture = nil;
        // Create output texture
        CGSize videoSize = CGSizeMake(width, height);
        self.outputTexture = [self createOutputTextureWithSize:videoSize];
    }
    if (self.outputTexture == nil) {
        return;
    }

    MTLCommandBufferRef commandBuffer = [_commandQueue commandBuffer];
    id<MTLBlitCommandEncoder> encoder = [commandBuffer blitCommandEncoder];
    [encoder copyFromTexture:_inputTexture
                 sourceSlice:0
                 sourceLevel:0
                sourceOrigin:MTLOriginMake(0, 0, 0)
                  sourceSize:MTLSizeMake(width, height, _inputTexture.depth)
                   toTexture:self.outputTexture
            destinationSlice:0
            destinationLevel:0
           destinationOrigin:MTLOriginMake(0, 0, 0)];
    [encoder endEncoding];
    [commandBuffer commit];
    [commandBuffer waitUntilCompleted];
    _inputTexture = nil;
}

@end
