//
//  AVPlayerSetting.h
//  Copyright (c) 2020 東亜プリン秘密研究所. All rights reserved.
//

#import <Foundation/Foundation.h>

NS_ASSUME_NONNULL_BEGIN

@interface AVPlayerSetting : NSObject

- (NSString* _Nonnull)getName;

- (NSString* _Nonnull)getSex;

- (NSString* _Nonnull)getAge;

- (NSString* _Nonnull)getBlood;

- (void)loadFromDocument;

+ (instancetype)shared;

@end

NS_ASSUME_NONNULL_END
