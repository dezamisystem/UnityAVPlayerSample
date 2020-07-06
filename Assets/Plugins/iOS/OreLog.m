//
//  OreLog.m
//  Copyright (c) 2020 東亜プリン秘密研究所. All rights reserved.
//

#import "OreLog.h"

@implementation OreLog

+ (void)redirectLogToDocuments
{
    NSArray *allPaths = NSSearchPathForDirectoriesInDomains(NSDocumentDirectory, NSUserDomainMask, YES);
    NSString *documentsDirectory = [allPaths objectAtIndex:0];
    NSDate* dateSource = [NSDate date];
    NSDateFormatter* formatter = [[NSDateFormatter alloc] init];
    [formatter setDateFormat:@"YYYYMMdd_hhmmss"];
    NSString* dateConverted = [formatter stringFromDate:dateSource];
    NSString* fileForError = [NSString stringWithFormat:@"err_%@.txt", dateConverted];
    NSString *pathForError = [documentsDirectory stringByAppendingPathComponent:fileForError];
    freopen([pathForError cStringUsingEncoding:NSASCIIStringEncoding], "a+", stderr);
    NSString* fileForOutput = [NSString stringWithFormat:@"log_%@.txt", dateConverted];
    NSString *pathForOutput = [documentsDirectory stringByAppendingPathComponent:fileForOutput];
    freopen([pathForOutput cStringUsingEncoding:NSASCIIStringEncoding],"a+",stdout);
}

@end
