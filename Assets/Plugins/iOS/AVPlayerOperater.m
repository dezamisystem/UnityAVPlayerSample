//
//  AVPlayerOperater.m
//  Copyright (c) 2020 東亜プリン秘密研究所. All rights reserved.
//

#import "AVPlayerOperater.h"

@interface AVPlayerOperater ()

@property (nonnull, nonatomic) AVPlayer* avPlayer;

@property (nonnull, nonatomic) AVPlayerItemVideoOutput* videoOutput;

@property (strong, nonatomic) AVPlayerItem *avPlayerItem;

@property (assign, nonatomic) CGSize videoSize;

@property (strong, nonatomic) MTLDeviceRef metalDevice;

@property (strong, nonatomic) id<MTLTexture> outputTexture;

@end

static void* _ObserveItemStatusContext = (void*)0x1;
static void* _ObservePlayerItemContext = (void*)0x2;

@implementation AVPlayerOperater

- (id)init
{
	return nil;
}

- (id)initWithMetal
{
	if (self = [super init]) {
						
		self.avPlayer = [[AVPlayer alloc] init];
		[self.avPlayer addObserver:self
						forKeyPath:@"currentItem"
						   options:NSKeyValueObservingOptionNew | NSKeyValueObservingOptionInitial
						   context:_ObservePlayerItemContext];
		// Metal
		self.metalDevice = MTLCreateSystemDefaultDevice();
		// Video
		self.videoSize = CGSizeZero;
		NSDictionary<NSString*,id>* attributes = @{
			(NSString*)kCVPixelBufferPixelFormatTypeKey: @(kCVPixelFormatType_32BGRA)
		};
		self.videoOutput = [[AVPlayerItemVideoOutput alloc] initWithPixelBufferAttributes:attributes];
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

	[self.avPlayer play];
}

- (void)pauseWhenReady
{
	[self.avPlayer pause];
}

- (void)seekWithSeconds:(float)seconds
{
	CMTime position = CMTimeMakeWithSeconds(seconds, NSEC_PER_SEC);
	[self.avPlayer seekToTime:position completionHandler:^(BOOL finished) {
		// callback
		[self.playerCallback onSeek];
	}];
}

- (void)setPlayRate:(float)rate
{
	self.avPlayer.rate = rate;
}

- (void)setVolume:(float)volume
{
	self.avPlayer.volume = volume;
}

- (void)closeAll
{
	[NSNotificationCenter.defaultCenter removeObserver:self];
	[self.avPlayer removeObserver:self forKeyPath:@"status"];
	self.videoSize = CGSizeZero;
	[self.avPlayer replaceCurrentItemWithPlayerItem:nil];
	[self.avPlayerItem removeOutput:self.videoOutput];
	self.avPlayerItem = nil;
}

- (float)getCurrentSconds
{
	Float64 currentPosition = self.avPlayerItem != nil ? CMTimeGetSeconds(self.avPlayerItem.currentTime) : -1.f;
	return (float)currentPosition;
}

- (float)getDuration
{
	Float64 duration = self.avPlayerItem != nil ? CMTimeGetSeconds(self.avPlayerItem.duration) : 0.f;
	return (float)duration;
}

- (BOOL)isPlaying
{
	return self.avPlayer.rate != 0 ? true : false;
}

#pragma mark - Observers

- (void)observeValueForKeyPath:(NSString *)keyPath
					  ofObject:(id)object
						change:(NSDictionary<NSKeyValueChangeKey,id> *)change
					   context:(void *)context
{
	NSLog(@"AVPlayerOperater: observeValueForKeyPath = %@", keyPath);

	if (context == _ObserveItemStatusContext) {
		AVPlayerStatus status = [[change objectForKey:NSKeyValueChangeNewKey] integerValue];
		switch (status) {
			case AVPlayerStatusReadyToPlay:
				// callback
				[self.playerCallback onReady];
				break;
			default:
				break;
		}
	}
	else if (context == _ObservePlayerItemContext) {
		if ([change objectForKey:NSKeyValueChangeNewKey] != (id)[NSNull null]) {
			AVPlayerItem* playerItem = (AVPlayerItem*)object;
			[playerItem addObserver:self
								forKeyPath:@"status"
								   options:NSKeyValueObservingOptionNew | NSKeyValueObservingOptionInitial
								   context:_ObserveItemStatusContext];
			NSLog(@"AVPlayerOperater: New playerItem");
		}
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
	
	self.avPlayerItem = [AVPlayerItem playerItemWithAsset:asset];
	[self.avPlayerItem addOutput:self.videoOutput];
	[self.avPlayer replaceCurrentItemWithPlayerItem: self.avPlayerItem];
	// Audio setting
//	[[AVAudioSession sharedInstance] setActive:YES error:nil];
}

- (void)readBuffer
{
	CMTime currentTime = self.avPlayer.currentTime;
	if ([self.videoOutput hasNewPixelBufferForItemTime:currentTime]) {
		CVPixelBufferRef pixelBuffer = [self.videoOutput copyPixelBufferForItemTime:currentTime itemTimeForDisplay:nil];
		if (pixelBuffer != nil) {
			self.videoSize = CGSizeMake(CVPixelBufferGetWidth(pixelBuffer), CVPixelBufferGetHeight(pixelBuffer));
		}
	}
}

- (NSArray*)getTracksWithMediaType:(AVMediaType)type
{
	if (self.avPlayerItem == nil) {
		return nil;
	}
	return [self.avPlayerItem.asset tracksWithMediaType:type];
}

@end
