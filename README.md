# [程式說明](https://blog.darkthread.net/blog/min-api-run-with-tray-icon)
 
### 2023-02-04 更新
 
這個範例專案的初衷是提供一個最基本的可運作 PoC，展示如何整合 NotifyIcon 並加入選單功能，故程式碼會力求直覺簡單，以便於理解及避免失焦，若要實際應用需自行依需求重構改良。

Kevin Cheng 提供了一個重構後版本，對架構進行了不少優化，比原始範例更有容易擴充應用，十分有參考價值，需要的朋友可參考 [refactory 分支](/darkthread/MinApiTrayIcon/tree/refactory)，或原 [Fork 版本](/dcvsling/MinApiTrayIcon/tree/dev/kevin)。
 
1. 將 App.ico 放入 Assets
2. 將 html 作為內嵌移動到 Assets/index.html
3. 增加 Resources 的統一讀取方法
4. 將 Popup 功能改寫成 HostedService
5. MapFallback 返回 index.html
6. 偵測關閉改為使用 CancallationTokenSource
