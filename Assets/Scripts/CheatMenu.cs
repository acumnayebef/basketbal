#import <UIKit/UIKit.h>

// Состояния функций мода
static BOOL g_noMiss = NO;
static BOOL g_autoDribble = NO;
static BOOL g_infiniteMoney = NO;
static BOOL g_freeIAP = NO;

// Менеджер графического интерфейса меню
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
    UIWindow *keyWindow = [UIApplication sharedApplication].keyWindow;
    if (!keyWindow) return;

    // Создаем нативное окно меню на базе UIAlertController (оптимально для tweak.mm без тяжелых движков вроде ImGui)
    UIAlertController *alert = [UIAlertController alertControllerWithTitle:@"🏀 Basketball Mod Menu"
                                                                   message:@"Управление игроком и магазином\nСоздатель: @xantapers"
                                                            preferredStyle:UIAlertControllerStyleAlert];

    // Вкладка / Функции Player
    [alert addAction:[UIAlertAction actionWithTitle:[NSString stringWithFormat:@"🎯 Без промахов: %@", g_noMiss ? @"[ON]" : @"[OFF]"] style:UIAlertActionStyleDefault handler:^(UIAlertAction * _Nonnull action) {
        g_noMiss = !g_noMiss;
        [self showMenu];
    }]];

    [alert addAction:[UIAlertAction actionWithTitle:[NSString stringWithFormat:@"💨 Обвод игрока: %@", g_autoDribble ? @"[ON]" : @"[OFF]"] style:UIAlertActionStyleDefault handler:^(UIAlertAction * _Nonnull action) {
        g_autoDribble = !g_autoDribble;
        [self showMenu];
    }]];

    // Вкладка / Функции Money & IAP
    [alert addAction:[UIAlertAction actionWithTitle:[NSString stringWithFormat:@"💎 Бесконечные монеты: %@", g_infiniteMoney ? @"[ON]" : @"[OFF]"] style:UIAlertActionStyleDefault handler:^(UIAlertAction * _Nonnull action) {
        g_infiniteMoney = !g_infiniteMoney;
        [self showMenu];
    }]];

    [alert addAction:[UIAlertAction actionWithTitle:[NSString stringWithFormat:@"🔓 Встроенные покупки (Free IAP): %@", g_freeIAP ? @"[ON]" : @"[OFF]"] style:UIAlertActionStyleDefault handler:^(UIAlertAction * _Nonnull action) {
        g_freeIAP = !g_freeIAP;
        [self showMenu];
    }]];

    // Вкладка Creators
    [alert addAction:[UIAlertAction actionWithTitle:@"📌 Creators: @xantapers" style:UIAlertActionStyleDefault handler:^(UIAlertAction * _Nonnull action) {
        [[UIApplication sharedApplication] openURL:[NSURL URLWithString:@"https://t.me/xantapers"] options:@{} completionHandler:nil];
        [self showMenu];
    }]];

    [alert addAction:[UIAlertAction actionWithTitle:@"Закрыть меню" style:UIAlertActionStyleCancel handler:nil]];

    // Показываем поверх игры
    UIViewController *rootVC = keyWindow.rootViewController;
    while (rootVC.presentedViewController) {
        rootVC = rootVC.presentedViewController;
    }
    [rootVC presentViewController:alert animated:YES completion:nil];
}

@end

// Плавающая кнопка запуска меню поверх игры
@interface FloatingButtonLoader : NSObject
@end

@implementation FloatingButtonLoader
+ (void)load {
    dispatch_after(dispatch_time(DISPATCH_TIME_NOW, (int64_t)(4.0 * NSEC_PER_SEC)), dispatch_get_main_queue(), ^{
        UIWindow *window = [UIApplication sharedApplication].keyWindow;
        if (!window) return;
        
        UIButton *floatBtn = [UIButton buttonWithType:UIButtonTypeCustom];
        floatBtn.frame = CGRectMake(20, 120, 55, 55);
        floatBtn.backgroundColor = [UIColor colorWithRed:0.1 green:0.1 blue:0.1 alpha:0.85];
        [floatBtn setTitle:@"🏀" forState:UIControlStateNormal];
        floatBtn.titleLabel.font = [UIFont systemFontOfSize:28];
        floatBtn.layer.cornerRadius = 27.5;
        floatBtn.layer.borderWidth = 2.0;
        floatBtn.layer.borderColor = [UIColor orangeColor].CGColor;
        
        [floatBtn addTarget:[ModMenuManager sharedInstance] action:@selector(showMenu) forControlEvents:UIControlEventTouchUpInside];
        [window addSubview:floatBtn];
    });
}
end

// ==========================================
// IL2CPP ХУКИ (Замени имена классов/методов под свой dump.txt)
// ==========================================

%hook ScoreManager

- (bool)IsShotSuccessful {
    if (g_noMiss) {
        return true; // Все броски летят точно в цель
    }
    return %orig;
}

%end

%hook PlayerInventory

- (void)SpendCoins:(int)amount {
    if (g_infiniteMoney) {
        return; // Отменяем списание монет при покупках
    }
    %orig(amount);
}

%end

%hook IAPManager

- (bool)BuyProduct:(NSString *)productId {
    if (g_freeIAP) {
        // Перехватываем покупку и выдаем товар бесплатно
        return true; 
    }
    return %orig(productId);
}

%end
