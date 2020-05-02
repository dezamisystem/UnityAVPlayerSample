//
//  AVPlayerCallback.h
//  Copyright (c) 2020 東亜プリン秘密研究所. All rights reserved.
//

#import <Foundation/Foundation.h>

NS_ASSUME_NONNULL_BEGIN

@interface AVPlayerCallback : NSObject

@property (strong, nonatomic) NSString* unityObjectName;

@property (strong, nonatomic) NSString* unityMethodNameDidReady;

@property (strong, nonatomic) NSString *unityMethodNameDidSeek;

@property (strong, nonatomic) NSString *unityMethodNameDidEnd;

-(void)onReady;

-(void)onSeek;

-(void)onEndTime;

@end

NS_ASSUME_NONNULL_END
