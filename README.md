<div dir="rtl">


# اکستنشن‌های بیرساین (BirSign Extensions)

اکستنشن‌های بیرساین مجموعه‌ای از کتابخانه‌های تخصصی در بستر ‪.NET‬ هستند که فرآیند یکپارچه‌سازی سامانه‌های مختلف با سرویس Single Sign-On بیرساین را ساده و استاندارد می‌سازند.  
این ابزارها با هدف کاهش پیچیدگی‌های پیکربندی OIDC و افزایش سرعت توسعه طراحی شده‌اند تا تیم‌های فنی بتوانند بدون درگیری با جزئیات پروتکل، احراز هویت امن و متمرکز را در نرم‌افزارهای خود فعال کنند.

این مجموعه برای انواع پروژه‌ها شامل برنامه‌های وب، سرویس‌ها، نرم‌افزارهای سازمانی و میکروسرویس‌ها قابل استفاده است و امکان توسعه‌پذیری کامل را نیز فراهم می‌نماید.

## 📦 پکیج‌های منتشر شده در NuGet

نسخه‌های مختلف اکستنشن برای فریم‌ورک‌های ‪.NET‬ به صورت پکیج‌های جداگانه ارائه شده‌اند:

- **‪.NET Framework Extension:‬**  
  [https://www.nuget.org/packages/MapIdeaHub.BirSign.NetFrameworkExtension](https://www.nuget.org/packages/MapIdeaHub.BirSign.NetFrameworkExtension)

- **‪.NET Core Extension: ‬**  
  [https://www.nuget.org/packages/MapIdeaHub.BirSign.NetCoreExtension](https://www.nuget.org/packages/MapIdeaHub.BirSign.NetCoreExtension)

## 🧪 پروژه‌های نمونه (Sample Projects)

برای مشاهده‌ی نحوه‌ی پیاده‌سازی عملی این اکستنشن‌ها، پروژه‌های نمونه زیر در مخزن قرار گرفته‌اند:

| نام پروژه                     | توضیحات                                               | مسیر در مخزن |
|-------------------------------|--------------------------------------------------------|---------------|
| ASP.NET MVC (.Net Framework) | نمونه‌ای از اتصال برنامه MVC کلاسیک به بیرساین | [samples/MvcNetFramework](https://github.com/FarshidRH/BirSignExtension/tree/main/samples/MvcNetFramework) |
| ASP.NET MVC (.Net Core)      | نمونه‌ای از اتصال پروژه MVC Core به بیرساین          | [samples/MvcNetCore](https://github.com/FarshidRH/BirSignExtension/tree/main/samples/MvcNetCore) |

## 🔔 وب‌هوک‌ها و سازگاری نسخه‌ها

ارسال‌کنندهٔ جدید BirSign رویدادها را **بدون امضای HMAC** ارسال می‌کند و هدر `X-BirSign-Event-Id` و فیلد `EventId` در JSON را حفظ می‌کند.

- گیرندهٔ ASP.NET Core و گیرندهٔ .NET Framework در این شاخه نیازی به `BirSign:WebhookSigningSecret` یا `BirSign.WebhookSigningSecret` ندارند. اگر کلیدهای قدیمی در پیکربندی باقی بمانند، نادیده گرفته می‌شوند.
- امضاهای عمومی `AddBirSignAuthentication` و `UseBirSignAuthentication` تغییری نکرده‌اند. سازنده‌های قدیمی Middleware و کلاس `WebhookSignature` صرفاً برای سازگاری API باقی مانده‌اند و **به معنای فعال بودن تأیید امضا نیستند**.
- **تغییر رفتاری مهم:** دریافت‌کنندگانی که هنوز نسخهٔ دارای اعتبارسنجی HMAC را با Secret تنظیم‌شده اجرا می‌کنند، پیام بدون امضای سرور جدید را با `401` رد می‌کنند. لازم است بسته/گیرندهٔ آن‌ها به نسخهٔ سازگار ارتقا یابد.
- مسیر Webhook را به ورودی‌های قابل اعتماد محدود کنید (Reverse Proxy / فایروال). تحویل رویدادها *at-least-once* است؛ `EventId` را برای جلوگیری از پردازش دوباره، در پایگاه داده ثبت کنید.
- `BirSign:ClientSecret` مربوط به OIDC و API مدیریت است و **نباید حذف شود**؛ تغییر فوق فقط دربارهٔ امضای Webhook است.

برای جزئیات به [راهنمای Webhooks در Wiki](https://github.com/FarshidRH/BirSignExtension/wiki/Webhooks) مراجعه کنید.

## 🚀 راهنمای نصب و استفاده
برای راه‌اندازی سریع و استفاده از اکستنشن‌های بیرساین، به مستندات موجود در [ویکی پروژه (Wiki)](https://github.com/FarshidRH/BirSignExtension/wiki) مراجعه کنید. این مستندات شامل راهنمای گام‌به‌گام نصب، پیکربندی و نمونه‌های کد برای راه‌اندازی سریع است.

## 📄 لایسنس

این پروژه تحت لایسنس **MIT** منتشر شده است. برای مطالعه جزئیات، فایل LICENSE را مشاهده کنید.

</div>

