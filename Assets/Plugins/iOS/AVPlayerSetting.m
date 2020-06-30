//
//  AVPlayerSetting.m
//  Copyright (c) 2020 東亜プリン秘密研究所. All rights reserved.
//

#import "AVPlayerSetting.h"

#define TAG @"AVPlayerSetting"

#define KEY_NAME @"NAME"
#define KEY_SEX @"SEX"
#define KEY_AGE @"AGE"
#define KEY_BLOOD @"BLOOD"

@interface AVPlayerSetting ()
{
    NSMutableDictionary<NSString*, NSString*>* _paramDictionary;
}
@end

@implementation AVPlayerSetting

- (id)init
{
    if (self = [super init]) {
        _paramDictionary = [NSMutableDictionary dictionary];
    }
    return self;
}

- (NSString* _Nonnull)getName
{
    NSString* res = _paramDictionary[KEY_NAME];
    return res != nil ? res : @"";
}

- (NSString* _Nonnull)getSex
{
    NSString* res = _paramDictionary[KEY_SEX];
    return res != nil ? res : @"";
}

- (NSString* _Nonnull)getAge
{
    NSString* res = _paramDictionary[KEY_AGE];
    return res != nil ? res : @"";
}

- (NSString* _Nonnull)getBlood
{
    NSString* res = _paramDictionary[KEY_BLOOD];
    return res != nil ? res : @"";
}

- (void)loadFromDocument
{
    // /Documentのパスの取得
    NSArray* paths = NSSearchPathForDirectoriesInDomains(NSDocumentDirectory, NSUserDomainMask, YES);
    NSString* path = paths[0];
    NSLog(@"%@ PATH = %@", TAG, path);
    // ファイル名の作成
    NSString *filename = [path stringByAppendingPathComponent:@"settingsample.txt"];
    NSLog(@"%@ Filename = %@", TAG, filename);
    // ファイルの存在確認
    NSFileManager* fm = [NSFileManager defaultManager];
    if (![fm fileExistsAtPath:filename]) {
        // NSStringオブジェクト
        NSString *sample = [NSString stringWithFormat:@"%@=%@\n", KEY_NAME,@"Futaenokiwami"];
        sample = [sample stringByAppendingFormat:@"%@=%@\n", KEY_SEX,@"Male"];
        sample = [sample stringByAppendingFormat:@"%@=%@\n", KEY_AGE,@"49"];
        sample = [sample stringByAppendingFormat:@"%@=%@\n", KEY_BLOOD,@"A"];
        //@"NAME=Oresama\nSEX=Male\nAGE=49\nBLOOD=A";
        // ファイルへの保存
        if (![sample writeToFile:filename atomically:YES encoding:NSUTF8StringEncoding error:nil]) {
            NSLog(@"%@ Failed to write to file", TAG);
            return;
        }
    }
    // ファルからの読込み
    NSString *content = [NSString stringWithContentsOfFile:filename encoding:NSUTF8StringEncoding error:nil];
    if (content != nil) {
        // 行単位で抽出
        [content enumerateLinesUsingBlock:^(NSString * _Nonnull line, BOOL * _Nonnull stop) {
            // 値の取得
            NSArray<NSString*>* paramArray = [line componentsSeparatedByString:@"="];
            if (paramArray.count >= 2) {
                _paramDictionary[paramArray[0]] = paramArray[1];
                NSLog(@"%@: %@ = %@", TAG, paramArray[0], paramArray[1]);
            }
        }];
    }
}

+ (instancetype)shared
{
    static AVPlayerSetting* instance = nil;
    if (instance == nil) {
        instance = [[AVPlayerSetting alloc] init];
    }
    return instance;
}

@end
