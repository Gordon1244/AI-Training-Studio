# AI Training Studio

這是一個 Windows 桌面工具。雙擊 `dist\AITrainingStudio.exe` 後即可使用。

它會把你的需求、人格設定、訓練資料、參數與整合目標整理成 AI 行為包，並可產生：

- `.aipack` 訓練包
- `training_project.json`
- 資料清單 `data_manifest.csv`
- 操作對應表 `operation_mapping.csv`
- 新手教學
- Unity / Godot / Unreal 接入範本
- 開發板 / 手把橋接範本
- 文字 AI App HTML 範本

## 使用方式

1. 打開 `dist\AITrainingStudio.exe`。
2. 在「1. 專案」選擇要做 NPC、遊戲操作、文字 AI 或自訂流程。
3. 在「2. 需求與資料」輸入白話需求，加入任意數量的資料檔或資料夾。
4. 在「3. 參數」調整創意、穩定、記憶、反應速度、安全限制。
5. 在「4. 建立檔案」按「建立訓練包」或「建立整合包」。

## 重新編譯

在 PowerShell 執行：

```powershell
.\build.ps1
```

輸出會在 `dist\AITrainingStudio.exe`。

## 重要限制

這個工具不會破解遊戲，也不能把 AI 憑空塞進任意封閉遊戲。封閉遊戲必須有官方 Mod、SDK、腳本接口、原始碼或合法外部控制方式。

入門模式產生的是可讀、可整合的 AI 行為包，不是大型神經網路權重檔。真正微調 LLM、視覺模型或強化學習模型仍需要 GPU、雲端服務或專門訓練框架。
