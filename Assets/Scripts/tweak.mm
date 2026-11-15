#import <UIKit/UIKit.h>
#import <substrate.h>
#import <mach-o/dyld.h>

// Состояния функций мода
static BOOL g_noMiss = NO;
static BOOL g_infiniteMoney = NO;
static BOOL g_freeIAP = NO;

// Указатели на оригинальные функции
static bool (*orig_IsShotSuccessful)(void *__this);
static void (*orig_SpendCoins)(void *__this, int amount);
static bool (*orig_BuyProduct)(void *__this, void *productId);

// ==========================================
// ХУКИ (IL2CPP)
// ==========================================

bool hk_IsShotSuccessful(void *__this) {
    if (g_noMiss) return true;
    return orig_IsShotSuccessful(__this);
}

void hk_SpendCoins(void *__this, int amount) {
    if (g_infiniteMoney) return;
    orig_SpendCoins(__this, amount);
}

bool hk_BuyProduct(void *__this, void *productId) {
    if (g_freeIAP) return true;
    return orig_BuyProduct(__this, productId);
}

// ==========================================
// МЕНЕДЖЕР ИНТЕРФЕЙСА
// ==========================================

@interface ModMenuManager : NSObject
+ (instancetype)sharedInstance;
- (void)showMenu;
@end

@implementation ModMenuManager

+ (instancetype)sharedInstance {
    static ModMenuManager *sharedInstance = nil;
    static dispatch_once_t onceToken;
    dispatch_once(&onceToken, ^{
        sharedInstance = [[ModMenuManager alloc] init];
    });
    return sharedInstance;
}

- (void)showMenu {
    // Безопасный поиск активного окна (iOS 13+)
    UIWindow *targetWindow = nil;
    for (UIWindowScene *scene in [UIApplication sharedApplication].connectedScenes) {
        if (scene.activationState == UISceneActivationStateForegroundActive) {
            for (UIWindow *win in scene.windows) {
                if (win.isKeyWindow) {
                    targetWindow = win;
                    break;
                }
            }
        }
    }
    if (!targetWindow) targetWindow = [UIApplication sharedApplication].keyWindow;
    if (!targetWindow) return;

    NSString *statusMessage = [NSString stringWithFormat:@"Текущие статусы:\n\n• Без промахов: %@\n• Бесконечные монеты: %@\n• Free IAP: %@",
                               g_noMiss ? @"ON [🟢]" : @"OFF [🔴]",
                               g_infiniteMoney ? @"ON [🟢]" : @"OFF [🔴]",
                               g_freeIAP ? @"ON [🟢]" : @"OFF [🔴]"];

    UIAlertController *alert = [UIAlertController alertControllerWithTitle:@"🏀 Basketball Mod Menu"
                                                                   message:statusMessage
                                                            preferredStyle:UIAlertControllerStyleAlert];

    [alert addAction:[UIAlertAction actionWithTitle:(g_noMiss ? @"🎯 Выключить «Без промахов»" : @"🎯 Включить «Без промахов»") style:UIAlertActionStyleDefault handler:^(UIAlertAction * _Nonnull action) {
        g_noMiss = !g_noMiss;
    }]];

    [alert addAction:[UIAlertAction actionWithTitle:(g_infiniteMoney ? @"💎 Выключить монеты" : @"💎 Включить монеты") style:UIAlertActionStyleDefault handler:^(UIAlertAction * _Nonnull action) {
        g_infiniteMoney = !g_infiniteMoney;
    }]];

    [alert addAction:[UIAlertAction actionWithTitle:(g_freeIAP ? @"🔓 Выключить Free IAP" : @"🔓 Включить Free IAP") style:UIAlertActionStyleDefault handler:^(UIAlertAction * _Nonnull action) {
        g_freeIAP = !g_freeIAP;
    }]];

    [alert addAction:[UIAlertAction actionWithTitle:@"📌 Telegram: @xantapers" style:UIAlertActionStyleDefault handler:^(UIAlertAction * _Nonnull action) {
        [[UIApplication sharedApplication] openURL:[NSURL URLWithString:@"https://t.me/xantapers"] options:@{} completionHandler:nil];
    }]];

    [alert addAction:[UIAlertAction actionWithTitle:@"Закрыть" style:UIAlertActionStyleCancel handler:nil]];

    UIViewController *rootVC = targetWindow.rootViewController;
    while (rootVC.presentedViewController) {
        rootVC = rootVC.presentedViewController;
    }
    [rootVC presentViewController:alert animated:YES completion:nil];
}

@end

// ==========================================
// ПЕРЕТАСКИВАЕМАЯ ПЛАВАЮЩАЯ КНОПКА
// ==========================================

@interface DraggableButton : UIButton
@end

@implementation DraggableButton {
    CGPoint _initialPoint;
}

- (instancetype)initWithFrame:(CGRect)frame {
    self = [super initWithFrame:frame];
    if (self) {
        UIPanGestureRecognizer *pan = [[UIPanGestureRecognizer alloc] initWithTarget:self action:@selector(handlePan:)];
        [self addGestureRecognizer:pan];
    }
    return self;
}

- (void)handlePan:(UIPanGestureRecognizer *)gesture {
    UIWindow *window = self.window;
    if (!window) return;
    
    CGPoint translation = [gesture translationInView:window];
    
    if (gesture.state == UIGestureRecognizerStateBegan) {
        _initialPoint = self.center;
    } else if (gesture.state == UIGestureRecognizerStateChanged) {
        self.center = CGPointMake(_initialPoint.x + translation.x, _initialPoint.y + translation.y);
    } else if (gesture.state == UIGestureRecognizerStateEnded) {
        // Ограничение, чтобы кнопка не улетала за границы экрана
        CGRect bounds = window.bounds;
        CGPoint center = self.center;
        
        if (center.x < 35) center.x = 35;
        if (center.x > bounds.size.width - 35) center.x = bounds.size.width - 35;
        if (center.y < 50) center.y = 50;
        if (center.y > bounds.size.height - 50) center.y = bounds.size.height - 50;
        
        [UIView animateWithDuration:0.2 animations:^{
            self.center = center;
        }];
    }
}

@end

// ==========================================
// ИНИЦИАЛИЗАЦИЯ ПРИ ЗАПУСКЕ ИГРЫ
// ==========================================

__attribute__((constructor)) static void initializeMod() {
    dispatch_after(dispatch_time(DISPATCH_TIME_NOW, (int64_t)(4.0 * NSEC_PER_SEC)), dispatch_get_main_queue(), ^{
        UIWindow *window = [UIApplication sharedApplication].keyWindow;
        if (!window) return;
        
        // Создаем кнопку с иконкой 🏀 и поддержкой перетаскивания
        DraggableButton *floatBtn = [[DraggableButton alloc] initWithFrame:CGRectMake(20, 120, 55, 55)];
        floatBtn.backgroundColor = [UIColor colorWithRed:0.1 green:0.1 blue:0.1 alpha:0.85];
        [floatBtn setTitle:@"🏀" forState:UIControlStateNormal];
        floatBtn.titleLabel.font = [UIFont systemFontOfSize:28];
        floatBtn.layer.cornerRadius = 27.5;
        floatBtn.layer.borderWidth = 2.0;
        floatBtn.layer.borderColor = [UIColor orangeColor].CGColor;
        
        // Привязываем открытие меню по клику на кнопку
        [floatBtn addTarget:[ModMenuManager sharedInstance] action:@selector(showMenu) forControlEvents:UIControlEventTouchUpInside];
        
        [window addSubview:floatBtn];

        // Подключение хуков через офсеты из dump.cs
        uint64_t slide = _dyld_get_image_vmaddr_slide(0);
        /*
        MSHookFunction((void *)(slide + 0xYOUR_OFFSET_FOR_SHOT), (void *)hk_IsShotSuccessful, (void **)&orig_IsShotSuccessful);
        MSHookFunction((void *)(slide + 0xYOUR_OFFSET_FOR_COINS), (void *)hk_SpendCoins, (void **)&orig_SpendCoins);
        MSHookFunction((void *)(slide + 0xYOUR_OFFSET_FOR_IAP), (void *)hk_BuyProduct, (void **)&orig_BuyProduct);
        */
    });
}
