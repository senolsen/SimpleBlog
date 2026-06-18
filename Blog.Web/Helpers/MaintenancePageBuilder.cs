using System.Net;

namespace Blog.Web.Helpers;

public static class MaintenancePageBuilder
{
    public static string Build(string siteTitle, string? message)
    {
        var title = WebUtility.HtmlEncode(string.IsNullOrWhiteSpace(siteTitle) ? "SimpleBlog" : siteTitle);
        var bodyMessage = WebUtility.HtmlEncode(
            string.IsNullOrWhiteSpace(message)
                ? "Sitemiz şu anda planlı bakım çalışması nedeniyle geçici olarak hizmet dışıdır. Kısa süre içinde tekrar yayında olacağız."
                : message);

        return $$"""
<!DOCTYPE html>
<html lang="tr">
<head>
    <meta charset="utf-8">
    <meta name="viewport" content="width=device-width, initial-scale=1">
    <meta name="robots" content="noindex, nofollow">
    <title>{{title}} — Bakım Modu</title>
    <link rel="preconnect" href="https://fonts.googleapis.com">
    <link rel="preconnect" href="https://fonts.gstatic.com" crossorigin>
    <link href="https://fonts.googleapis.com/css2?family=Inter:wght@400;500;600;700&display=swap" rel="stylesheet">
    <style>
        *, *::before, *::after { box-sizing: border-box; margin: 0; padding: 0; }

        body {
            font-family: 'Inter', system-ui, -apple-system, sans-serif;
            min-height: 100vh;
            display: flex;
            align-items: center;
            justify-content: center;
            background: #0f172a;
            color: #e2e8f0;
            overflow: hidden;
            position: relative;
        }

        .bg-orbs {
            position: fixed;
            inset: 0;
            overflow: hidden;
            z-index: 0;
        }

        .orb {
            position: absolute;
            border-radius: 50%;
            filter: blur(80px);
            opacity: 0.45;
            animation: float 12s ease-in-out infinite;
        }

        .orb-1 {
            width: 420px; height: 420px;
            background: #6366f1;
            top: -120px; left: -80px;
        }

        .orb-2 {
            width: 360px; height: 360px;
            background: #8b5cf6;
            bottom: -100px; right: -60px;
            animation-delay: -4s;
        }

        .orb-3 {
            width: 240px; height: 240px;
            background: #06b6d4;
            top: 50%; left: 50%;
            transform: translate(-50%, -50%);
            animation-delay: -8s;
            opacity: 0.25;
        }

        @keyframes float {
            0%, 100% { transform: translate(0, 0) scale(1); }
            33% { transform: translate(30px, -20px) scale(1.05); }
            66% { transform: translate(-20px, 15px) scale(0.95); }
        }

        .card {
            position: relative;
            z-index: 1;
            width: min(520px, calc(100vw - 2rem));
            padding: 3rem 2.5rem;
            background: rgba(255, 255, 255, 0.06);
            backdrop-filter: blur(24px);
            -webkit-backdrop-filter: blur(24px);
            border: 1px solid rgba(255, 255, 255, 0.12);
            border-radius: 24px;
            text-align: center;
            box-shadow: 0 25px 60px rgba(0, 0, 0, 0.4);
            animation: fadeUp 0.7s ease-out;
        }

        @keyframes fadeUp {
            from { opacity: 0; transform: translateY(24px); }
            to { opacity: 1; transform: translateY(0); }
        }

        .icon-wrap {
            width: 88px;
            height: 88px;
            margin: 0 auto 1.75rem;
            background: linear-gradient(135deg, #6366f1, #8b5cf6);
            border-radius: 22px;
            display: flex;
            align-items: center;
            justify-content: center;
            box-shadow: 0 12px 32px rgba(99, 102, 241, 0.4);
            animation: pulse 3s ease-in-out infinite;
        }

        @keyframes pulse {
            0%, 100% { box-shadow: 0 12px 32px rgba(99, 102, 241, 0.4); }
            50% { box-shadow: 0 12px 48px rgba(99, 102, 241, 0.65); }
        }

        .icon-wrap svg {
            width: 44px;
            height: 44px;
            color: #fff;
            animation: spin 8s linear infinite;
        }

        @keyframes spin {
            from { transform: rotate(0deg); }
            to { transform: rotate(360deg); }
        }

        .site-name {
            font-size: 0.8rem;
            font-weight: 600;
            letter-spacing: 0.12em;
            text-transform: uppercase;
            color: #a5b4fc;
            margin-bottom: 0.75rem;
        }

        h1 {
            font-size: 1.75rem;
            font-weight: 700;
            color: #f8fafc;
            margin-bottom: 1rem;
            line-height: 1.3;
        }

        .message {
            font-size: 1rem;
            line-height: 1.7;
            color: #94a3b8;
            margin-bottom: 2rem;
        }

        .status-bar {
            display: inline-flex;
            align-items: center;
            gap: 0.5rem;
            padding: 0.5rem 1.25rem;
            background: rgba(99, 102, 241, 0.15);
            border: 1px solid rgba(99, 102, 241, 0.3);
            border-radius: 999px;
            font-size: 0.85rem;
            font-weight: 500;
            color: #c7d2fe;
        }

        .status-dot {
            width: 8px;
            height: 8px;
            background: #818cf8;
            border-radius: 50%;
            animation: blink 1.5s ease-in-out infinite;
        }

        @keyframes blink {
            0%, 100% { opacity: 1; }
            50% { opacity: 0.3; }
        }

        .progress-track {
            margin-top: 2rem;
            height: 4px;
            background: rgba(255, 255, 255, 0.08);
            border-radius: 999px;
            overflow: hidden;
        }

        .progress-fill {
            height: 100%;
            width: 40%;
            background: linear-gradient(90deg, #6366f1, #8b5cf6, #06b6d4);
            border-radius: 999px;
            animation: slide 2.5s ease-in-out infinite;
        }

        @keyframes slide {
            0% { transform: translateX(-100%); width: 40%; }
            50% { width: 60%; }
            100% { transform: translateX(350%); width: 40%; }
        }
    </style>
</head>
<body>
    <div class="bg-orbs">
        <div class="orb orb-1"></div>
        <div class="orb orb-2"></div>
        <div class="orb orb-3"></div>
    </div>

    <main class="card" role="main">
        <div class="icon-wrap" aria-hidden="true">
            <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="1.5">
                <path stroke-linecap="round" stroke-linejoin="round" d="M11.42 15.17l-5.1 3.03a1 1 0 01-1.52-.87V6.67a1 1 0 011.52-.87l5.1 3.03m0 0l5.58-3.32a1 1 0 011.52.87v10.66a1 1 0 01-1.52.87l-5.58-3.32M11.42 15.17V8.83" />
            </svg>
        </div>

        <p class="site-name">{{title}}</p>
        <h1>Bakım Çalışması Devam Ediyor</h1>
        <p class="message">{{bodyMessage}}</p>

        <div class="status-bar">
            <span class="status-dot"></span>
            Kısa süre içinde döneceğiz
        </div>

        <div class="progress-track" aria-hidden="true">
            <div class="progress-fill"></div>
        </div>
    </main>
</body>
</html>
""";
    }
}
