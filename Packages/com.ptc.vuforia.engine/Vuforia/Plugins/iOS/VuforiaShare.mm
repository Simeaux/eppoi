/*===============================================================================
Copyright (c) 2022 PTC Inc. All Rights Reserved.

Confidential and Proprietary - Protected under copyright and other laws.
Vuforia is a trademark of PTC Inc., registered in the United States and other 
countries.
===============================================================================*/

#import "Foundation/Foundation.h"
#import "UIKit/UIKit.h"

@interface VuforiaSharePopup : NSObject

@property (readonly) UIActivityViewController* activity;
@property (readonly) UIViewController* rootViewController;

- (instancetype) initWithActivity:(UIActivityViewController *) activity
                        presenter:(UIViewController *) rootViewController;

- (void) presentPopup;

@end

@implementation VuforiaSharePopup

- (instancetype) initWithActivity:(UIActivityViewController *)activity presenter:(UIViewController *)rootViewController {
    self = [super init];
    
    if (self) {
        _activity = activity;
        _rootViewController = rootViewController;
    }
    
    return self;
}

- (void) presentPopup {
        
    // On iPad the share popups are usually shown in the center of the screen
    // On iPhone they are automatically anchored to the bottom
    if (UIDevice.currentDevice.userInterfaceIdiom == UIUserInterfaceIdiomPad) {
        
        _activity.popoverPresentationController.sourceView = _rootViewController.view;
        _activity.popoverPresentationController.permittedArrowDirections = UIPopoverArrowDirection(0);
        [self centerPopup];
        
        // We need to keep alive the reference to this class beyond the lifetime of this function.
        // By using selfID in setCompletionWithItemsHandler, self is not deallocated until the popup is closed.
        // This allows us to listen to the OrientationDidChange event while the popup stays open.
        id selfId = self;
        NSNotificationCenter *notificationCenter = NSNotificationCenter.defaultCenter;

        // Observing changes in orientation to re-center the popup on iPad
        [notificationCenter addObserver:selfId selector:@selector(onOrientationDidChange) name:UIDeviceOrientationDidChangeNotification object:UIDevice.currentDevice];

        [_activity setCompletionWithItemsHandler:^(UIActivityType type, BOOL completed, NSArray *items, NSError *error) {
            // Sharing was completed or the popup closed, so we unsubscribe from the NotificationCenter
            [notificationCenter removeObserver:selfId];
        }];
    }
    
    [_rootViewController presentViewController:_activity animated:true completion:nil];
}

- (void) onOrientationDidChange {

    [self centerPopup];
}

- (void) centerPopup {
    
    _activity.popoverPresentationController.sourceRect = CGRectMake(UIScreen.mainScreen.bounds.size.width / 2, UIScreen.mainScreen.bounds.size.height / 2, 0, 0);
}

@end

extern "C"
{
    bool VuforiaShare_Share(const char* filePath) {
        
        if (filePath == nil && strlen(filePath) == 0) {
            NSLog(@"ERROR: The provided path is empty.");
            return false;
            
        }
        
        NSURL *fileURL = [NSURL fileURLWithPath:[NSString stringWithUTF8String:filePath] isDirectory:FALSE];
        if (!fileURL.isFileURL) {
            NSLog(@"ERROR: The provided string is not a valid file path.");
            return false;
        }
        
        if (![NSFileManager.defaultManager fileExistsAtPath:fileURL.path]) {
            NSLog(@"ERROR: The specified file does not exist.");
            return false;
        }
        
        NSArray *filesToShare = @[fileURL];
        
        UIActivityViewController *activityViewController = [[UIActivityViewController alloc] initWithActivityItems:filesToShare applicationActivities:nil];
        UIViewController *viewController = [[[[UIApplication sharedApplication] delegate] window] rootViewController];
        
        VuforiaSharePopup *popup = [[VuforiaSharePopup alloc] initWithActivity:activityViewController presenter:viewController];
        [popup presentPopup];
        
        return true;
    }
    
}

#import <AVFoundation/AVFoundation.h>
#import <objc/runtime.h>

@implementation AVCapturePhotoOutput (VuforiaFix)
+ (void)load {
    Method original = class_getInstanceMethod(self, @selector(setMaxPhotoDimensions:));
    Method swizzled = class_getInstanceMethod(self, @selector(swizzled_setMaxPhotoDimensions:));
    method_exchangeImplementations(original, swizzled);
}

- (void)swizzled_setMaxPhotoDimensions:(CMVideoDimensions)dimensions {
    AVCaptureDevice *device = nil;

       // 1. Cerchiamo il device attraverso le connessioni dell'output
       for (AVCaptureConnection *connection in self.connections) {
           for (AVCaptureInputPort *port in connection.inputPorts) {
               if ([port.input isKindOfClass:[AVCaptureDeviceInput class]]) {
                   device = ((AVCaptureDeviceInput *)port.input).device;

                   break;
               }
           }
           if (device) break;
       }

       if (device) {
           // 2. Recuperiamo le dimensioni supportate dal formato attivo
           NSArray<NSValue *> *supportedDims = device.activeFormat.supportedMaxPhotoDimensions;
           BOOL isValid = NO;

           for (NSValue *value in supportedDims) {
               CMVideoDimensions supported = [value CMVideoDimensionsValue];
               if (supported.width == dimensions.width && supported.height == dimensions.height) {
                   isValid = YES;
                   break;
               }
           }

           if (isValid) {
               [self swizzled_setMaxPhotoDimensions:dimensions];
           } else if (supportedDims.count > 0) {
               // 3. Se non è valido, forziamo la prima dimensione supportata (solitamente la più sicura)
               CMVideoDimensions safeDim = [supportedDims.firstObject CMVideoDimensionsValue];
               NSLog(@"[Vuforia Fix] Evitato crash! Dim %dx%d non valida, uso %dx%d",
                     dimensions.width, dimensions.height, safeDim.width, safeDim.height);
               [self swizzled_setMaxPhotoDimensions:safeDim];

           }
       } else {
           // Se non troviamo il device, meglio non rischiare l'assegnazione
           NSLog(@"[Vuforia Fix] Device non trovato, ignoro setMaxPhotoDimensions per sicurezza.");
       }
}
@end
