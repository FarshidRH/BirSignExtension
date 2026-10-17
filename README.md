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

## 🔔 وب‌هوک‌های بیرساین

ارسال‌کنندهٔ بیرساین رویدادهای چارت سازمانی را با `POST` و بدنهٔ JSON ارسال می‌کند. در پروتکل فعلی **نیازی به Secret یا امضای HMAC نیست**؛ مکانیزم آزمایشی امضا پیش از انتشار از کد Extension حذف شده است.

- APIهای قبلی `AddBirSignAuthentication` و `UseBirSignAuthentication` و تنظیمات احراز هویت OIDC **بدون تغییر باقی مانده‌اند**.
- `WebhookMiddleware`، `WebhookStartupFilter` و `WebhookOwinMiddleware` همان امضاهای سازندهٔ نسخهٔ اصلی را دارند و نیازی به به‌روزرسانی کد سامانه‌های متصل نیست.
- هدر `X-BirSign-Event-Id` و فیلد `EventId` در JSON برای شناسایی رویداد حفظ شده‌اند. تحویل به روش *at-least-once* است؛ دریافت‌کننده باید پردازش رویداد تکراری را مدیریت کند.
- به دلیل نبود احراز اصالت Webhook در سطح برنامه، دسترسی مسیر دریافت را در Reverse Proxy یا فایروال به فرستنده‌های مجاز محدود کنید.
- `BirSign:ClientSecret` مربوط به OIDC و API مدیریت است و **باید همچنان پیکربندی شود**؛ ارتباطی با Webhook ندارد.

برای جزئیات به [راهنمای Webhooks در Wiki](https://github.com/FarshidRH/BirSignExtension/wiki/Webhooks) مراجعه کنید.

## 🚀 راهنمای نصب و استفاده
برای راه‌اندازی سریع و استفاده از اکستنشن‌های بیرساین، به مستندات موجود در [ویکی پروژه (Wiki)](https://github.com/FarshidRH/BirSignExtension/wiki) مراجعه کنید. این مستندات شامل راهنمای گام‌به‌گام نصب، پیکربندی و نمونه‌های کد برای راه‌اندازی سریع است.

## 📄 لایسنس

این پروژه تحت لایسنس **MIT** منتشر شده است. برای مطالعه جزئیات، فایل LICENSE را مشاهده کنید.

</div>

