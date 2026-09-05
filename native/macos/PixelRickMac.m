#import <AppKit/AppKit.h>
#import <ServiceManagement/ServiceManagement.h>
#import <ApplicationServices/ApplicationServices.h>

// All exports are called on Avalonia's main/UI thread. Geometry is in Cocoa points,
// converted at this boundary to a primary-display top-left origin, never backing pixels.
typedef struct { double left, top, right, bottom, scale; } PRScreen;
static double primaryTop(void) { return NSMaxY(NSScreen.screens.firstObject.frame); }
static NSWindow *windowFor(void *handle) {
    id obj = (__bridge id)handle;
    return [obj isKindOfClass:NSWindow.class] ? obj : ([obj isKindOfClass:NSView.class] ? [obj window] : nil);
}
static NSWindow *pet;
static id showObserver;
static BOOL showRequested;
int pr_screens(PRScreen *output, int capacity) {
    NSArray<NSScreen *> *screens = NSScreen.screens; double top = primaryTop();
    int count = MIN((int)screens.count, capacity);
    for (int i = 0; i < count; i++) {
        NSRect r = screens[i].visibleFrame;
        output[i] = (PRScreen){r.origin.x, top - NSMaxY(r), NSMaxX(r), top - r.origin.y, screens[i].backingScaleFactor};
    }
    return count;
}
void pr_cursor(double *x, double *y) { NSPoint p = NSEvent.mouseLocation; *x = p.x; *y = primaryTop() - p.y; }
double pr_double_click(void) { return NSEvent.doubleClickInterval; }
void pr_configure(void *handle) {
    pet = windowFor(handle); pet.opaque = NO; pet.backgroundColor = NSColor.clearColor;
    pet.hasShadow = NO; pet.hidesOnDeactivate = NO; pet.animationBehavior = NSWindowAnimationBehaviorNone;
    pet.collectionBehavior = NSWindowCollectionBehaviorCanJoinAllSpaces | NSWindowCollectionBehaviorFullScreenAuxiliary;
    if (!showObserver) showObserver = [NSDistributedNotificationCenter.defaultCenter addObserverForName:@"org.pixelrick.show" object:nil queue:NSOperationQueue.mainQueue usingBlock:^(NSNotification *n) { showRequested = YES; }];
}
void pr_frame(void *handle, double left, double top, double width, double height, int topmost) {
    NSWindow *w = windowFor(handle); NSRect r = NSMakeRect(left, primaryTop() - top - height, width, height);
    if (!NSEqualRects(w.frame, r)) [w setFrame:r display:YES];
    NSInteger level = topmost ? NSFloatingWindowLevel : NSNormalWindowLevel;
    if (w.level != level) w.level = level;
}
void pr_ignore(void *handle, int ignore) { windowFor(handle).ignoresMouseEvents = ignore != 0; }
int pr_ignoring(void *handle) { return windowFor(handle).ignoresMouseEvents; }
double pr_backing_scale(void *handle) { return windowFor(handle).backingScaleFactor; }
int pr_is_opaque(void *handle) { return windowFor(handle).opaque; }
int pr_is_accessory(void) { return NSApp.activationPolicy == NSApplicationActivationPolicyAccessory; }
void pr_show_existing(void) { [NSDistributedNotificationCenter.defaultCenter postNotificationName:@"org.pixelrick.show" object:nil userInfo:nil deliverImmediately:YES]; }
int pr_take_show_requested(void) { BOOL requested = showRequested; showRequested = NO; return requested; }
void pr_show(void *handle) { [windowFor(handle) orderFrontRegardless]; }
void pr_sound(void) { NSBeep(); }
// SMAppService stores login configuration in macOS itself. No shell scripts or LaunchAgents.
int pr_login_status(void) {
    if (@available(macOS 13.0, *)) return (int)SMAppService.mainAppService.status;
    return -1;
}
int pr_login_set(int enabled) {
    if (@available(macOS 13.0, *)) {
        NSError *error = nil;
        BOOL ok = enabled ? [SMAppService.mainAppService registerAndReturnError:&error] : [SMAppService.mainAppService unregisterAndReturnError:&error];
        if (!ok) { NSLog(@"PixelRick login item: %@", error); return 0; }
        return 1;
    }
    return 0;
}
// Local-window snapshots don't capture unrelated applications or require screen recording permission.
int pr_snapshot(void *handle, const char *path) {
    NSView *view = windowFor(handle).contentView;
    NSBitmapImageRep *rep = [view bitmapImageRepForCachingDisplayInRect:view.bounds];
    [view cacheDisplayInRect:view.bounds toBitmapImageRep:rep];
    return [[rep representationUsingType:NSBitmapImageFileTypePNG properties:@{}] writeToFile:[NSString stringWithUTF8String:path] atomically:YES];
}
// Test harness uses real AppKit events through Avalonia's normal input pipeline.
void pr_test_mouse(void *handle, int kind, double x, double y, int clicks) {
    NSWindow *w = windowFor(handle);
    NSEventType types[] = {NSEventTypeLeftMouseDown, NSEventTypeLeftMouseDragged, NSEventTypeLeftMouseUp};
    NSPoint local = NSMakePoint(x, w.contentView.bounds.size.height - y);
    NSPoint global = [w convertPointToScreen:local];
    CGWarpMouseCursorPosition(CGPointMake(global.x, primaryTop() - global.y));
    w.ignoresMouseEvents = NO;
    NSEvent *event = [NSEvent mouseEventWithType:types[kind] location:local modifierFlags:0 timestamp:NSProcessInfo.processInfo.systemUptime windowNumber:w.windowNumber context:nil eventNumber:1 clickCount:clicks pressure:1];
    [NSApp sendEvent:event];
}
