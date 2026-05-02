# AI Training Studio

AI Training Studio 是一個 Windows 桌面工具，目標是讓新手用圖形化介面把需求、資料、參數與整合方式整理成 AI 訓練包。

執行檔：

```text
dist\AITrainingStudio.exe
```

目前專案沒有 `.bat` 啟動器。請直接用檔案總管雙擊 `dist\AITrainingStudio.exe`。

## 可以產生什麼

- `.aipack` AI 行為包
- `training_project.json` 專案設定
- `requirements_prompt.txt` 提示詞與需求整理
- `model_card_zh-TW.md` 模型卡
- `data_manifest.csv` 本機資料清單
- `operation_mapping.csv` AI 輸出到實際操作的對應表
- `resource_discovery_queue.csv` 自動找資料計畫
- `action_inference_from_video_zh-TW.md` 只有畫面時推測操作的教學
- `training_backend_plan_zh-TW.md` 訓練方式計畫
- Unity / Godot / Unreal / 自訂引擎 / Mod SDK 接入範本
- 開發板 / 手把橋接範本
- 文字 AI App HTML 範本
- 本地 GPU 直接訓練資料夾 `local_gpu_train`

## 基本使用方式

1. 開啟 `dist\AITrainingStudio.exe`。
2. 在「1. 專案」選擇要做 NPC、遊戲操作、文字 AI 或自訂流程。
3. 在「2. 需求與資料」輸入需求、人格、資料來源。
4. 在「3. 參數」調整創意、穩定、記憶、反應速度、安全限制與訓練方式。
5. 在「4. 建立檔案」建立訓練包、整合包或文字 AI App。

## 資料來源

你可以加入：

- 本機檔案
- 本機資料夾
- 影片網址
- 網站網址
- 文件網址
- GitHub 或其他公開資料來源網址
- 自動找資料關鍵字

如果沒有提供本機檔案，也可以勾選「沒有資料時，幫我產生自動找資料計畫」。

這會產生 `resource_discovery_queue.csv`，列出：

- 要搜尋的關鍵字
- 建議來源類型
- 為什麼需要這份資料
- 如何標註資料
- 授權與網站條款注意事項

注意：程式不會未經授權自動下載或爬取網站內容。真正下載、爬取或訓練前，仍要確認你有權使用該資料。

## 只有畫面、沒有搖桿操作資料

如果你只有遊玩影片或遊戲畫面，沒有實際搖桿按鍵紀錄，可以勾選：

```text
只有畫面時，幫我推測可能操作
```

程式會產生 `action_inference_from_video_zh-TW.md`，教你如何把畫面狀態推測成操作標籤，例如：

- 直線、前方無障礙：可能是 `ACCELERATE`
- 左彎：可能是 `STEER_LEFT`
- 右彎：可能是 `STEER_RIGHT`
- 前方障礙或牆壁接近：可能是 `BRAKE` 或閃避
- 道具出現或被追擊：可能是 `USE_ITEM`

這是「推測標註」，不是保證正確。正式訓練前仍建議人工抽查，最好補上實際控制紀錄或遊戲狀態資料。

## 模型等級 vs 訓練方式

這兩個選項用途不同。

### 模型等級

「模型等級」只決定輸出檔案完整度，不決定要不要 GPU。

- 入門：需求整理與行為包
- 進階：可接 API / 本機模型
- 完整：輸出訓練資料格式與整合檔

### 訓練方式

「訓練方式」才決定要不要使用 GPU。

- 行為包 / 提示詞：不用 GPU，最快開始
- 大模型 API：使用雲端 LLM 或外部模型
- 本地 GPU 直接訓練：使用自己的顯卡開始訓練
- 混合：先用大模型整理，再用本地 GPU 訓練

## 本地 GPU 直接訓練

當「訓練方式」選擇本地 GPU 或混合模式時，輸出包會新增：

```text
local_gpu_train
```

裡面包含：

- `README_zh-TW.md`
- `train_config.json`
- `dataset_template.csv`
- `start_local_gpu_training.ps1`
- `check_gpu.py`
- `train_stub.py`

使用方式：

```powershell
cd local_gpu_train
.\start_local_gpu_training.ps1
```

這個啟動腳本會檢查：

- 是否有 Python
- 是否安裝 PyTorch
- 是否偵測到常見 GPU / 加速後端

GPU 不限定 NVIDIA。可以是 NVIDIA、AMD、Intel、Apple Silicon 或其他 AI 加速器。只是不同硬體需要不同訓練框架或後端，例如 CUDA、ROCm、DirectML、OpenVINO、MPS，或廠商自己的 SDK。內建檢查腳本只能檢查常見 PyTorch 後端，若你的硬體使用其他後端，可以替換 `train_stub.py` 或改寫 `check_gpu.py`。

如果環境不足，腳本會提示缺少什麼。

`train_stub.py` 是本地 GPU 訓練入口範本。它會讀取 `train_config.json` 和 `dataset_template.csv`，你可以之後把它替換成真正的 PyTorch、影像模型、LLM 微調或強化學習訓練程式。

## 參數勾選項說明

- 產生新手教學：輸出一步一步的中文操作流程，適合完全沒有程式背景的人。
- 產生遊戲引擎整合範本：輸出 Unity、Godot、Unreal、自訂引擎或 Mod SDK 的接入範本。
- 產生開發板 / 手把橋接範本：輸出序列埠、控制策略、影片標註與硬體橋接範本，可對應 Switch、PlayStation、Xbox、PC、Android、模擬器、自訂 HID 或其他合法控制環境。
- 產生找資料清單：輸出自動找資料計畫與標註建議。
- 產生文字 AI App 範本：輸出可雙擊開啟的 HTML 文字 AI App 範本。
- 產生風險與限制說明：列出授權、安全、遊戲條款與真實設備風險。

## 重新編譯

在 PowerShell 執行：

```powershell
.\build.ps1
```

輸出會在：

```text
dist\AITrainingStudio.exe
```

如果編譯失敗並提示檔案被使用，請先關閉正在執行的 `AITrainingStudio.exe`。

## 重要限制

AI Training Studio 不會破解遊戲，也不能把 AI 憑空塞進任意封閉遊戲。

封閉遊戲必須有官方 Mod、SDK、腳本接口、原始碼或合法外部控制方式，才可能把 AI 行為包接入。

接 Switch、PlayStation、Xbox、PC、Android、模擬器、自訂 HID、開發板或其他真實設備時，只能使用合法硬體與合法方式，不應繞過安全機制或違反服務條款。

入門模式產生的是可讀、可整合的 AI 行為包，不是大型神經網路權重檔。真正訓練 LLM、視覺模型或強化學習模型仍需要資料集、訓練框架、評測流程，以及相應的雲端或本地 GPU 環境。
