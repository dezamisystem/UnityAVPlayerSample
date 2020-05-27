//
//  AVPlayerCallback.h
//  Copyright (c) 2020 東亜プリン秘密研究所. All rights reserved.
//

#import <Foundation/Foundation.h>

NS_ASSUME_NONNULL_BEGIN

@interface AVPlayerCallback : NSObject

@property (nonatomic, copy) NSString* unityObjectName;

@property (nonatomic, copy) NSString* unityMethodNameDidReady;

@property (nonatomic, copy) NSString *unityMethodNameDidSeek;

@property (nonatomic, copy) NSString *unityMethodNameDidEnd;

-(void)onReady;

-(void)onSeek;

-(void)onEndTime;

@end

NS_ASSUME_NONNULL_END
