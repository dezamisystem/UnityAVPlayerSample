//
//  AVPlayerCallback.m
//  Copyright (c) 2020 東亜プリン秘密研究所. All rights reserved.
//

#import "AVPlayerCallback.h"

@interface AVPlayerCallback ()
@end

@implementation AVPlayerCallback

-(void)onReady
{
	NSLog(@"AVPlayerCallback: onReady");

	if (self.unityObjectName == nil || self.unityMethodNameDidReady == nil) {
		return;
	}
	NSLog(@"object = %@, method = %@", self.unityObjectName, self.unityMethodNameDidReady);
	const char* objectName = [self.unityObjectName UTF8String];
	if (objectName == nil) {
		return;
	}
	const char* methodName = [self.unityMethodNameDidReady UTF8String];
	if (methodName == nil) {
		return;
	}
	UnitySendMessage(objectName, methodName, "onReady");
}

-(void)onSeek
{
	NSLog(@"AVPlayerCallback: onSeek");

	if (self.unityObjectName == nil || self.unityMethodNameDidSeek == nil) {
		return;
	}
	NSLog(@"object = %@, method = %@", self.unityObjectName, self.unityMethodNameDidSeek);
	const char* objectName = [self.unityObjectName UTF8String];
	if (objectName == nil) {
		return;
	}
	const char* methodName = [self.unityMethodNameDidSeek UTF8String];
	if (methodName == nil) {
		return;
	}
	UnitySendMessage(objectName, methodName, "didSeek");
}

-(void)onEndTime
{
	NSLog(@"AVPlayerCallback: onEndTime");

	if (self.unityObjectName == nil || self.unityMethodNameDidEnd == nil) {
		return;
	}
	NSLog(@"object = %@, method = %@", self.unityObjectName, self.unityMethodNameDidEnd);
	const char* objectName = [self.unityObjectName UTF8String];
	if (objectName == nil) {
		return;
	}
	const char* methodName = [self.unityMethodNameDidEnd UTF8String];
	if (methodName == nil) {
		return;
	}
	UnitySendMessage(objectName, methodName, "onEndTime");
}

@end
