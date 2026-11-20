# Modular.MVC.Core

ساختار مخزن از صفحه‌ای که فرستادید به‌طور کامل در دسترس نبود؛ بنابراین درخت زیر یک طرح پیشنهادی برای توسعه‌ی Modular.MVC.Core است، نه فهرست کلاس‌های فعلی مخزن. در این طرح، ماژول‌ها و مجوزها در مرکز قرار دارند و بخش‌های حسابداری یا مدیریت فدراسیون می‌توانند به‌صورت ماژول‌های مستقل اضافه شوند.

## ساختار پیشنهادی

```text
Modular.MVC.Core/
├── src/
│   ├── Modular.Core/
│   │   ├── Modules/
│   │   │   ├── IModule.cs
│   │   │   ├── ModuleDescriptor.cs
│   │   │   ├── ModuleType.cs
│   │   │   ├── ModuleStatus.cs
│   │   │   └── ModuleDependency.cs
│   │   ├── Licensing/
│   │   │   ├── License.cs
│   │   │   ├── LicenseType.cs
│   │   │   ├── LicenseFeature.cs
│   │   │   └── LicenseValidationResult.cs
│   │   ├── AccessControl/
│   │   │   ├── AccessLevel.cs
│   │   │   ├── ModuleAccessRule.cs
│   │   │   ├── Permission.cs
│   │   │   └── Role.cs
│   │   └── Common/
│   │       ├── Entity.cs
│   │       └── Result.cs
│   │
│   ├── Modular.Application/
│   │   ├── Modules/
│   │   │   ├── ModuleService.cs
│   │   │   ├── ModuleRegistry.cs
│   │   │   ├── ModuleAccessService.cs
│   │   │   └── DTOs/
│   │   │       ├── ModuleDto.cs
│   │   │       └── UpdateModuleRequest.cs
│   │   ├── Licensing/
│   │   │   ├── LicenseService.cs
│   │   │   ├── ILicenseValidator.cs
│   │   │   └── LicenseFeatureService.cs
│   │   ├── Identity/
│   │   │   ├── AuthenticationService.cs
│   │   │   ├── UserService.cs
│   │   │   ├── RoleService.cs
│   │   │   └── MenuPermissionService.cs
│   │   ├── Navigation/
│   │   │   ├── MenuService.cs
│   │   │   └── PanelLayoutService.cs
│   │   └── Abstractions/
│   │       ├── IModuleRepository.cs
│   │       ├── ILicenseRepository.cs
│   │       └── ICurrentUser.cs
│   │
│   ├── Modular.Infrastructure/
│   │   ├── Persistence/
│   │   │   ├── ModularDbContext.cs
│   │   │   ├── Configurations/
│   │   │   │   ├── ModuleConfiguration.cs
│   │   │   │   ├── LicenseConfiguration.cs
│   │   │   │   ├── RoleConfiguration.cs
│   │   │   │   └── MenuConfiguration.cs
│   │   │   └── Repositories/
│   │   │       ├── ModuleRepository.cs
│   │   │       └── LicenseRepository.cs
│   │   ├── Identity/
│   │   │   ├── ApplicationUser.cs
│   │   │   ├── ApplicationRole.cs
│   │   │   └── IdentityConfiguration.cs
│   │   ├── Licensing/
│   │   │   ├── LicenseSignatureValidator.cs
│   │   │   └── LicenseStorage.cs
│   │   └── Modules/
│   │       ├── AssemblyModuleLoader.cs
│   │       └── ModuleDiscovery.cs
│   │
│   ├── Modular.Web/
│   │   ├── Areas/
│   │   │   ├── Public/
│   │   │   │   ├── Controllers/
│   │   │   │   └── Views/
│   │   │   ├── Admin/
│   │   │   │   ├── Controllers/
│   │   │   │   └── Views/
│   │   │   ├── Accounting/
│   │   │   │   ├── Controllers/
│   │   │   │   └── Views/
│   │   │   └── Federation/
│   │   │       ├── Controllers/
│   │   │       └── Views/
│   │   ├── Controllers/
│   │   │   ├── HomeController.cs
│   │   │   └── AccountController.cs
│   │   ├── Middleware/
│   │   │   ├── ModuleAccessMiddleware.cs
│   │   │   └── LicenseValidationMiddleware.cs
│   │   ├── Authorization/
│   │   │   ├── ModuleAccessHandler.cs
│   │   │   └── ModuleAccessRequirement.cs
│   │   ├── Navigation/
│   │   │   ├── MenuProvider.cs
│   │   │   └── PanelLayoutProvider.cs
│   │   ├── Views/
│   │   └── Program.cs
│   │
│   └── Modules/
│       ├── ModuleManagement/
│       │   ├── ModuleManagementModule.cs
│       │   ├── Controllers/
│       │   │   └── ModulesController.cs
│       │   ├── Application/
│       │   │   ├── ModuleManagementService.cs
│       │   │   └── ModuleManagementValidator.cs
│       │   └── Views/
│       │       └── Modules/
│       │
│       ├── LicenseManagement/
│       │   ├── LicenseManagementModule.cs
│       │   ├── Controllers/
│       │   ├── Application/
│       │   └── Views/
│       │
│       ├── IdentityManagement/
│       │   ├── IdentityManagementModule.cs
│       │   ├── Controllers/
│       │   ├── Application/
│       │   └── Views/
│       │
│       ├── Accounting/
│       │   ├── AccountingModule.cs
│       │   ├── Controllers/
│       │   ├── Application/
│       │   └── Views/
│       │
│       └── FederationManagement/
│           ├── FederationManagementModule.cs
│           ├── Controllers/
│           ├── Application/
│           └── Views/
│
├── tests/
│   ├── Modular.Core.Tests/
│   ├── Modular.Application.Tests/
│   ├── Modular.Infrastructure.Tests/
│   └── Modular.Web.Tests/
│
└── docs/
    ├── architecture.md
    └── module-development.md
```

## توضیح کلاس‌ها و اجزای اصلی

### ماژول‌ها

- **IModule**: قرارداد پایه‌ای که هر ماژول پیاده‌سازی می‌کند؛ برای معرفی نام، شناسه و تنظیمات ماژول.
- **ModuleDescriptor**: فراداده‌ی ماژول؛ مثل نام، نسخه، نوع لایسنس موردنیاز و سطح‌های دسترسی مجاز.
- **ModuleType**: نوع ماژول؛ مثلاً عمومی یا وابسته به یک نوع نرم‌افزار.
- **ModuleStatus**: وضعیت عملیاتی ماژول، مانند فعال، غیرفعال یا در دسترس نبودن.
- **ModuleDependency**: وابستگی‌های لازم برای اجرای ماژول.
### لایسنس

- **License**: اطلاعات لایسنس نصب‌شده، از جمله نوع محصول، مشتری، تاریخ اعتبار و وضعیت.
- **LicenseType**: نوع محصولی که لایسنس برای آن صادر شده است؛ مانند حسابداری یا مدیریت فدراسیون.
- **LicenseFeature**: قابلیت‌هایی که لایسنس اجازه‌ی استفاده از آن‌ها را می‌دهد.
- **LicenseValidationResult**: نتیجه‌ی بررسی لایسنس، همراه با دلیل معتبر یا نامعتبر بودن.
### دسترسی و نقش‌ها

- **AccessLevel**: سطح ورود و دسترسی؛ برای نمونه عمومی، کاربر عادی، مدیر واحد و مدیر سیستم.
- **ModuleAccessRule**: مشخص می‌کند هر ماژول برای کدام سطح‌ها قابل استفاده باشد.
- **Permission**: یک مجوز مشخص، مانند مشاهده، ایجاد، ویرایش یا حذف.
- **Role**: مجموعه‌ای از مجوزها که به کاربران تخصیص داده می‌شود.
### سرویس‌های برنامه

- **ModuleService**: عملیات مدیریتی ماژول‌ها، از جمله فعال‌سازی و غیرفعال‌سازی.
- **ModuleRegistry**: فهرست ماژول‌های کشف‌شده و ثبت‌شده در برنامه.
- **ModuleAccessService**: بررسی دسترسی کاربر به ماژول بر اساس نقش و سطح.
- **LicenseService**: دریافت و مدیریت لایسنس جاری.
- **ILicenseValidator**: قرارداد بررسی اعتبار لایسنس و قابلیت‌های مجاز.
- **AuthenticationService**: ورود، خروج و مدیریت فرایند احراز هویت.
- **MenuPermissionService**: فیلتر و کنترل نمایش منوها بر اساس نقش و مجوز.
- **PanelLayoutService**: مدیریت چیدمان پنل‌ها و منوهای هر پنل.
### زیرساخت

- **ModularDbContext**: مدل پایگاه داده و نقطه‌ی دسترسی EF Core.
- **ModuleConfiguration و سایر کلاس‌های Configuration**: نگاشت موجودیت‌ها و قواعد ذخیره‌سازی.
- **ModuleRepository و LicenseRepository**: دسترسی داده‌ای به ماژول‌ها و لایسنس‌ها.
- **ApplicationUser و ApplicationRole**: مدل‌های کاربر و نقش مبتنی بر ASP.NET Core Identity.
- **LicenseSignatureValidator**: بررسی امضای دیجیتال لایسنس برای جلوگیری از تغییر غیرمجاز.
- **AssemblyModuleLoader**: بارگذاری اسمبلی‌های ماژول‌ها.
- **ModuleDiscovery**: پیدا کردن ماژول‌های موجود و آماده‌سازی آن‌ها برای ثبت.
### وب و پنل‌ها

- **Public**: صفحات عمومی سایت که بدون ورود قابل مشاهده‌اند.
- **Admin**: پنل اصلی برای مدیریت لایسنس، ماژول‌ها، کاربران، نقش‌ها و چیدمان پنل‌ها.
- **Accounting و Federation**: پنل‌های اختصاصی که فقط با لایسنس و دسترسی مناسب فعال می‌شوند.
- **ModuleAccessMiddleware**: کنترل اولیه‌ی دسترسی به مسیرهای ماژول‌ها.
- **LicenseValidationMiddleware**: بررسی وضعیت لایسنس در درخواست‌های مرتبط.
- **ModuleAccessHandler و ModuleAccessRequirement**: پیاده‌سازی کنترل دسترسی استاندارد ASP.NET Core Authorization.
- **MenuProvider و PanelLayoutProvider**: تولید منو و چیدمان متناسب با کاربر و پنل.
### ماژول‌های مستقل

- **ModuleManagementModule**: ماژول مدیریتی برای فهرست، ایجاد، ویرایش، فعال‌سازی و غیرفعال‌سازی ماژول‌ها.
- **LicenseManagementModule**: مدیریت صدور، ثبت و بررسی لایسنس.
- **IdentityManagementModule**: مدیریت کاربران، نقش‌ها و سطوح دسترسی.
- **AccountingModule**: نمونه‌ی ماژول اختصاصی محصول حسابداری.
- **FederationManagementModule**: نمونه‌ی ماژول اختصاصی محصول مدیریت فدراسیون.
## نکته‌ی معماری

یک نکته‌ی معماری: عمومی یا اختصاصی بودن ماژول بهتر است از طریق ModuleDescriptor و سیاست لایسنس تعیین شود، نه صرفاً با نام پوشه. به همین شکل، مخفی‌کردن منو به‌تنهایی امنیت ایجاد نمی‌کند؛ بررسی مجوز باید در مسیرهای اجرایی و سرویس‌ها هم انجام شود.
