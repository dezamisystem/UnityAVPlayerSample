//
//  AVPlayerOperater.m
//  Copyright (c) 2020 東亜プリン秘密研究所. All rights reserved.
//

#import "AVPlayerOperater.h"

@interface AVPlayerOperater ()
{
	MTLDeviceRef _metalDevice;
	
	MTLCommandQueueRef _commandQueue;
	CVMetalTextureCacheRef _textureCache;
	
	NSUInteger _videoWidth;
	NSUInteger _videoHeight;
	
	id<MTLTexture> _inputTexture;
	id<MTLTexture> _outputTexture;
	
	AVPlayer* _avPlayer;
	AVPlayerItemVideoOutput* _videoOutput;
	AVPlayerItem* _avPlayerItem;
}

@end

static void* _ObserveItemStatusContext = (void*)0x1;
static void* _ObservePlayerItemContext = (void*)0x2;
static void* _ObservePresentationSizeContext = (void*)0x3;

@implementation AVPlayerOperater

- (id)init
{
	return nil;
}

- (id)initWithIndex:(NSUInteger)index
{
	if (self = [super init]) {
		self.index = index;
		_avPlayer = [[AVPlayer alloc] init];
		NSKeyValueObservingOptions options = NSKeyValueObservingOptionOld | NSKeyValueObservingOptionNew;
		[_avPlayer addObserver:self
					forKeyPath:@"currentItem"
					   options:options
					   context:_ObservePlayerItemContext];
		// Metal
		_metalDevice = MTLCreateSystemDefaultDevice();
		_commandQueue = [_metalDevice newCommandQueue];
		CVMetalTextureCacheCreate(kCFAllocatorDefault, nil, _metalDevice, nil, &_textureCache);
		// Video
		_videoWidth = 0;
		_videoHeight = 0;
		NSDictionary<NSString*,id>* attributes = @{
			(NSString*)kCVPixelBufferPixelFormatTypeKey: @(kCVPixelFormatType_32BGRA)
		};
		_videoOutput = [[AVPlayerItemVideoOutput alloc] initWithPixelBufferAttributes:attributes];
		// Callback
		self.playerCallback = [[AVPlayerCallback alloc] init];
		// Notifications
		[NSNotificationCenter.defaultCenter addObserver:self
											   selector:@selector(didPlayToEndTime:)
												   name:AVPlayerItemDidPlayToEndTimeNotification
												 object:nil];
		[NSNotificationCenter.defaultCenter addObserver:self
											   selector:@selector(didChangeAudioRoute:)
												   name:AVAudioSessionRouteChangeNotification
												 object:nil];
	}
	return self;
}

- (id<MTLTexture>)getOutputTexture
{
	return _outputTexture;
}

- (void)setOutputTexture:(id<MTLTexture>)texture
{
	self.outputTexture = texture;
}

- (void)setPlayerItemWithPath:(NSString*)contentPath
{
	NSLog(@"AVPlayerOperater: setPlayerItemWithPath");
	if (contentPath == nil) {
		NSLog(@"AVPlayerOperater: contentPath is nil!");
		return;
	}
	NSURL* contentUrl = [NSURL URLWithString:contentPath];
	if (contentUrl == nil) {
		NSLog(@"AVPlayerOperater: contentUrl is nil!");
		return;
	}
	AVURLAsset* contentAsset = [AVURLAsset URLAssetWithURL:contentUrl options:nil];
	if (contentAsset == nil) {
		NSLog(@"AVPlayerOperater: contentAsset is nil!");
		return;
	}
	NSLog(@"AVPlayerOperater: contentPath = %@", contentPath);
	// Asset prepareing
	NSArray<NSString*>* requestedKeys = @[@"tracks", @"playable"];
	[contentAsset loadValuesAsynchronouslyForKeys:requestedKeys completionHandler:^{
		dispatch_async(dispatch_get_main_queue(), ^{
			[self prepareAsset:contentAsset keys:requestedKeys];
		});
	}];
}

- (void)playWhenReady
{
	NSLog(@"AVPlayerOperater: playWhenReady");

	[_avPlayer play];
}

- (void)pauseWhenReady
{
	[_avPlayer pause];
}

- (void)seekWithSeconds:(float)seconds
{
	CMTime position = CMTimeMakeWithSeconds(seconds, NSEC_PER_SEC);
	[_avPlayer seekToTime:position completionHandler:^(BOOL finished) {
		// callback
		[self.playerCallback onSeek];
	}];
}

- (void)setPlayRate:(float)rate
{
	_avPlayer.rate = rate;
}

- (void)setVolume:(float)volume
{
	_avPlayer.volume = volume;
}

- (void)closeAll
{
	[NSNotificationCenter.defaultCenter removeObserver:self];
	[_avPlayer removeObserver:self forKeyPath:@"status"];
	_videoWidth = 0;
	_videoHeight = 0;
	[_avPlayer replaceCurrentItemWithPlayerItem:nil];
	[_avPlayerItem removeOutput:_videoOutput];
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

- (void)updateVideo
{
	@synchronized (self) {
		[self readBuffer];
	}
}

#pragma mark - Observers

- (void)observeValueForKeyPath:(NSString *)keyPath
					  ofObject:(id)object
						change:(NSDictionary<NSKeyValueChangeKey,id> *)change
					   context:(void *)context
{
	NSLog(@"AVPlayerOperater: observeValueForKeyPath = %@", keyPath);

	NSKeyValueObservingOptions options = NSKeyValueObservingOptionOld | NSKeyValueObservingOptionNew;
	if (context == _ObservePlayerItemContext) {
		AVPlayerItem* playerItem = (AVPlayerItem*)object;
		[playerItem addObserver:self
					 forKeyPath:@"status"
						options:options
						context:_ObserveItemStatusContext];
			NSLog(@"AVPlayerOperater: New playerItem");
	}
	else if (context == _ObserveItemStatusContext) {
		AVPlayerStatus status = [[change objectForKey:NSKeyValueChangeNewKey] integerValue];
		if (status == AVPlayerItemStatusReadyToPlay) {
			AVPlayerItem* playerItem = (AVPlayerItem*)object;
			[playerItem addObserver:self
						 forKeyPath:@"presentationSize"
							options:options
							context:_ObservePresentationSizeContext];
			// callback
			[self.playerCallback onReady];
		}
		NSLog(@"AVPlayerOperater: New status");
	}
	else if (context == _ObservePresentationSizeContext) {
		AVPlayerItem* playerItem = (AVPlayerItem*)object;
		NSLog(@"AVPlayerOperater: New presentationSize (%f, %f)", playerItem.presentationSize.width, playerItem.presentationSize.height);
	}
	else {
		[super observeValueForKeyPath:keyPath ofObject:object change:change context:context];
	}
}

#pragma mark - Notifications

- (void)didChangeAudioRoute:(NSNotification*)notification
{
	NSLog(@"AVPlayerOperater: didChangeAudioRoute");
}

- (void)didPlayToEndTime:(NSNotification*)notification
{
	[self.playerCallback onEndTime];
}

#pragma mark - Private

-(void)prepareAsset:(AVAsset*)asset keys:(NSArray*)requestedKeys
{
	NSLog(@"AVPlayerOperater: prepareAsset");

	// check succesful loading
    for(NSString* key in requestedKeys) {
        NSError* error = nil;
        AVKeyValueStatus keyStatus = [asset statusOfValueForKey:key error:&error];
        if(keyStatus == AVKeyValueStatusFailed) {
			NSLog(@"AVPlayerOperater: %@ keyStatus is nil!", key);
            return;
        }
    }

    if(!asset.playable) {
		NSLog(@"AVPlayerOperater: asset.playable is NO!");
        return;
    }
	
	_avPlayerItem = [AVPlayerItem playerItemWithAsset:asset];
	[_avPlayerItem addOutput:_videoOutput];
	[_avPlayer replaceCurrentItemWithPlayerItem: _avPlayerItem];
}

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
				_videoWidth = width;
				_videoHeight = height;
				_inputTexture = CVMetalTextureGetTexture(cvTextureOut);
				CFRelease(cvTextureOut);
			}
			CFRelease(pixelBuffer);
		}
	} // autoreleasepool
}

- (void)copyTexture
{
	if (_commandQueue == nil) {
		return;
	}
	if (_inputTexture == nil) {
		return;
	}
	if (_outputTexture == nil) {
		return;
	}
	if (_videoWidth == 0) {
		return;
	}
	if (_videoHeight == 0) {
		return;
	}
	
	MTLCommandBufferRef cmdBuffer = [_commandQueue commandBuffer];
	id<MTLBlitCommandEncoder> encoder = [cmdBuffer blitCommandEncoder];
	[encoder copyFromTexture:_inputTexture
				 sourceSlice:0
				 sourceLevel:0
				sourceOrigin:MTLOriginMake(0, 0, 0)
				  sourceSize:MTLSizeMake(_videoWidth, _videoHeight, _inputTexture.depth)
				   toTexture:_outputTexture
			destinationSlice:0
			destinationLevel:0
		   destinationOrigin:MTLOriginMake(0, 0, 0)];
	[encoder endEncoding];
	_inputTexture = nil;
}

- (NSArray*)getTracksWithMediaType:(AVMediaType)type
{
	if (_avPlayerItem == nil) {
		return nil;
	}
	return [_avPlayerItem.asset tracksWithMediaType:type];
}

@end
