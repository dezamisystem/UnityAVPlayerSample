//
//  DebugLog.m
//  Copyright (c) 2020 東亜プリン秘密研究所. All rights reserved.
//

#import "DebugLog.h"

void DebugLog(NSString *format, ...)
{
    va_list args;
    va_start(args, format);
    NSLogv(format, args);
    va_end(args);
}
