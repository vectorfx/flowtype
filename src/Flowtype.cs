using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using System.Windows.Forms;
using Microsoft.Win32;

[assembly: System.Reflection.AssemblyVersion("1.3.21.0")]
[assembly: System.Reflection.AssemblyFileVersion("1.3.21.0")]

namespace Flowtype
{
    public sealed class AppSettings
    {
        public string Engine;
        public string CleanupProvider;
        public string Hotkey;
        public string Style;
        public bool CleanupEnabled;
        public bool ContextEnabled;
        public bool AutoPaste;
        public bool SaveHistory;
        public bool KeepFailedAudio;
        public bool StartWithWindows;
        public string ApiBaseUrl;
        public string TranscriptionModel;
        public string CleanupModel;
        public string OpenRouterUrl;
        public string OpenRouterModel;
        public string WhisperExePath;
        public string WhisperServerPath;
        public string WhisperModelPath;
        public string LocalModelQuality;
        public string OllamaUrl;
        public string OllamaModel;
        public string GroqApiUrl;
        public string GroqTranscriptionModel;
        public float MicGain;
        public bool TurboTranscription;
        public bool SuppressNonSpeech;
        public bool CompletionSound;
        public bool ShowInsertNotification;
        public string OverlayTheme;
        public List<string> Dictionary;
        public Dictionary<string, string> Snippets;

        public static AppSettings Defaults()
        {
            AppSettings value = new AppSettings();
            value.Engine = "Local";
            value.CleanupProvider = "BuiltIn";
            value.Hotkey = "Win + Ctrl";
            value.Style = "Natural";
            value.CleanupEnabled = true;
            value.ContextEnabled = true;
            value.AutoPaste = true;
            value.SaveHistory = false;
            value.KeepFailedAudio = true;
            value.StartWithWindows = true;
            value.ApiBaseUrl = "https://api.openai.com/v1";
            value.TranscriptionModel = "gpt-4o-mini-transcribe";
            value.CleanupModel = "gpt-4o-mini";
            value.OpenRouterUrl = "https://openrouter.ai/api/v1";
            value.OpenRouterModel = "openrouter/free";
            value.WhisperExePath = "";
            value.WhisperServerPath = "";
            value.WhisperModelPath = "";
            value.LocalModelQuality = "Instant";
            value.OllamaUrl = "http://localhost:11434";
            value.OllamaModel = "";
            value.GroqApiUrl = "https://api.groq.com/openai/v1";
            value.GroqTranscriptionModel = "whisper-large-v3-turbo";
            value.MicGain = 1.2f;
            value.TurboTranscription = true;
            value.SuppressNonSpeech = false;
            value.CompletionSound = true;
            value.ShowInsertNotification = false;
            value.OverlayTheme = "Dark";
            value.Dictionary = new List<string>();
            value.Snippets = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            return value;
        }

        public void Repair()
        {
            if (String.IsNullOrWhiteSpace(Engine)) Engine = "Local";
            if (String.Equals(Engine, "Smart", StringComparison.OrdinalIgnoreCase)) Engine = "OpenAI";
            if (String.Equals(Engine, "Windows", StringComparison.OrdinalIgnoreCase)) Engine = "Local";
            if (String.IsNullOrWhiteSpace(CleanupProvider)) CleanupProvider = Engine == "OpenAI" ? "OpenAI" : "BuiltIn";
            if (String.IsNullOrWhiteSpace(Hotkey)) Hotkey = "Win + Ctrl";
            if (String.IsNullOrWhiteSpace(Style)) Style = "Natural";
            if (String.IsNullOrWhiteSpace(ApiBaseUrl)) ApiBaseUrl = "https://api.openai.com/v1";
            if (String.IsNullOrWhiteSpace(TranscriptionModel)) TranscriptionModel = "gpt-4o-mini-transcribe";
            if (String.IsNullOrWhiteSpace(CleanupModel)) CleanupModel = "gpt-4o-mini";
            if (String.IsNullOrWhiteSpace(OpenRouterUrl)) OpenRouterUrl = "https://openrouter.ai/api/v1";
            if (String.IsNullOrWhiteSpace(OpenRouterModel) ||
                String.Equals(OpenRouterModel, "google/gemma-4-26b-a4b-it:free", StringComparison.OrdinalIgnoreCase))
                OpenRouterModel = "openrouter/free";
            // The free router can queue for tens of seconds. Never put that
            // nondeterministic wait in the dictation insertion path.
            if (String.Equals(CleanupProvider, "OpenRouter", StringComparison.OrdinalIgnoreCase) &&
                String.Equals(OpenRouterModel, "openrouter/free", StringComparison.OrdinalIgnoreCase))
                CleanupProvider = "BuiltIn";
            if (String.IsNullOrWhiteSpace(LocalModelQuality)) LocalModelQuality = "Instant";
            if (String.Equals(LocalModelQuality, "Fast", StringComparison.OrdinalIgnoreCase)) LocalModelQuality = "Instant";
            if (String.Equals(LocalModelQuality, "Flow Quality", StringComparison.OrdinalIgnoreCase)) LocalModelQuality = "Instant";
            if (String.IsNullOrWhiteSpace(OllamaUrl)) OllamaUrl = "http://localhost:11434";
            if (String.IsNullOrWhiteSpace(GroqApiUrl)) GroqApiUrl = "https://api.groq.com/openai/v1";
            if (String.IsNullOrWhiteSpace(GroqTranscriptionModel)) GroqTranscriptionModel = "whisper-large-v3-turbo";
            if (MicGain < 0.5f || MicGain > 3f) MicGain = 1.2f;
            if (String.Equals(Engine, "Groq", StringComparison.OrdinalIgnoreCase)) CleanupProvider = "BuiltIn";
            if (String.IsNullOrWhiteSpace(OverlayTheme)) OverlayTheme = "Dark";
            if (!String.Equals(OverlayTheme, "Glass", StringComparison.OrdinalIgnoreCase) &&
                !String.Equals(OverlayTheme, "Dark", StringComparison.OrdinalIgnoreCase) &&
                !String.Equals(OverlayTheme, "Purple", StringComparison.OrdinalIgnoreCase) &&
                !String.Equals(OverlayTheme, "Light", StringComparison.OrdinalIgnoreCase) &&
                !String.Equals(OverlayTheme, "Mono", StringComparison.OrdinalIgnoreCase))
                OverlayTheme = "Dark";
            if (Dictionary == null) Dictionary = new List<string>();
            if (Snippets == null) Snippets = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        }
    }

    public static class UiTheme
    {
        public static readonly Color Window = Color.FromArgb(244, 244, 245);
        public static readonly Color Surface = Color.White;
        public static readonly Color Header = Color.FromArgb(250, 250, 250);
        public static readonly Color Border = Color.FromArgb(212, 212, 216);
        public static readonly Color BorderSoft = Color.FromArgb(228, 228, 231);
        public static readonly Color Text = Color.FromArgb(24, 24, 27);
        public static readonly Color TextMuted = Color.FromArgb(113, 113, 122);
        public static readonly Color Accent = Color.FromArgb(39, 39, 42);
        public static readonly Color AccentHover = Color.FromArgb(63, 63, 70);
    }

    public static class AppFonts
    {
        private static readonly PrivateFontCollection Collection = new PrivateFontCollection();
        private static Font ui;
        private static Font uiLarge;
        private static Font uiBold;
        private static bool initialized;

        public static Font Ui(float size, FontStyle style)
        {
            Ensure();
            if (style == FontStyle.Bold) return new Font(uiBold.FontFamily, size, style);
            return new Font(ui.FontFamily, size, style);
        }

        public static Font UiLarge(float size)
        {
            Ensure();
            return new Font(uiLarge.FontFamily, size);
        }

        private static void Ensure()
        {
            if (initialized) return;
            initialized = true;
            string root = Path.Combine(FlowtypeApp.AppDirectory ?? "", "assets", "fonts");
            if (TryLoadRobotoMonoPair(root)) return;
            if (TryLoadSingleFont(Path.Combine(root, "RobotoMono-Regular.ttf"))) return;
            try
            {
                FontFamily mono = new FontFamily("Roboto Mono");
                ui = new Font(mono, 9.25f);
                uiLarge = new Font(mono, 10.25f);
                uiBold = new Font(mono, 9.75f, FontStyle.Bold);
                return;
            }
            catch { }
            ui = new Font("Segoe UI", 9.25f);
            uiLarge = new Font("Segoe UI", 10.25f);
            uiBold = new Font("Segoe UI", 9.75f, FontStyle.Bold);
        }

        private static bool TryLoadRobotoMonoPair(string root)
        {
            string regularPath = Path.Combine(root, "RobotoMono-Regular.ttf");
            string boldPath = Path.Combine(root, "RobotoMono-Bold.ttf");
            if (!File.Exists(regularPath)) return false;
            try
            {
                Collection.AddFontFile(regularPath);
                FontFamily regular = FindFamily(Collection, "Roboto Mono");
                FontFamily bold = regular;
                if (File.Exists(boldPath))
                {
                    Collection.AddFontFile(boldPath);
                    bold = FindFamily(Collection, "Roboto Mono") ?? regular;
                }
                if (regular == null) return false;
                ui = new Font(regular, 9.25f);
                uiLarge = new Font(regular, 10.25f);
                uiBold = new Font(bold, 9.75f, FontStyle.Bold);
                return true;
            }
            catch { return false; }
        }

        private static bool TryLoadSingleFont(string fontPath)
        {
            try
            {
                if (!File.Exists(fontPath)) return false;
                Collection.AddFontFile(fontPath);
                FontFamily family = Collection.Families[Collection.Families.Length - 1];
                ui = new Font(family, 9.25f);
                uiLarge = new Font(family, 10.25f);
                uiBold = new Font(family, 9.75f, FontStyle.Bold);
                return true;
            }
            catch { return false; }
        }

        private static FontFamily FindFamily(PrivateFontCollection collection, string preferredName)
        {
            for (int index = collection.Families.Length - 1; index >= 0; index--)
            {
                if (String.Equals(collection.Families[index].Name, preferredName, StringComparison.OrdinalIgnoreCase))
                    return collection.Families[index];
            }
            return collection.Families.Length > 0 ? collection.Families[collection.Families.Length - 1] : null;
        }
    }

    public static class LatencyStats
    {
        public static long LastRecordMs;
        public static long LastTranscribeMs;
        public static long LastCleanMs;
        public static long LastTotalMs;
        public static event Action StatsUpdated;

        public static string Summary
        {
            get
            {
                if (LastTotalMs <= 0) return "No dictation yet.";
                return "Last: record " + LastRecordMs + " ms · transcribe " + LastTranscribeMs +
                    " ms · clean " + LastCleanMs + " ms · total " + LastTotalMs + " ms";
            }
        }

        private static void NotifyChanged()
        {
            Action handler = StatsUpdated;
            if (handler != null) handler();
        }

        public static void Update(long recordMs, long transcribeMs, long cleanMs, long totalMs)
        {
            LastRecordMs = recordMs;
            LastTranscribeMs = transcribeMs;
            LastCleanMs = cleanMs;
            LastTotalMs = totalMs;
            NotifyChanged();
        }
    }

    public static class RecordingCue
    {
        private static byte[] startBytes;
        private static byte[] completeBytes;

        public static void Preload()
        {
            startBytes = LoadCue("recording-start.wav", EmbeddedAudio.RecordingStart);
            completeBytes = LoadCue("recording-complete.wav", EmbeddedAudio.RecordingComplete);
        }

        public static void PlayStart()
        {
            Play(startBytes);
        }

        public static void PlayComplete()
        {
            Play(completeBytes);
        }

        private static byte[] LoadCue(string fileName, byte[] embedded)
        {
            if (embedded != null && embedded.Length > 44) return embedded;
            string path = Path.Combine(FlowtypeApp.AppDirectory ?? "", "assets", "audio", fileName);
            if (File.Exists(path))
            {
                try { return File.ReadAllBytes(path); }
                catch { }
            }
            return null;
        }

        private static void Play(byte[] wavBytes)
        {
            if (wavBytes == null || wavBytes.Length < 44) return;
            byte[] payload = (byte[])wavBytes.Clone();
            ThreadPool.QueueUserWorkItem(delegate
            {
                try
                {
                    using (MemoryStream stream = new MemoryStream(payload, false))
                    using (System.Media.SoundPlayer player = new System.Media.SoundPlayer(stream))
                    {
                        player.Load();
                        player.PlaySync();
                    }
                }
                catch { }
            });
        }
    }

    public static class Hotkeys
    {
        private static readonly Dictionary<string, int> Values = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
        {
            { "Win + Ctrl", 0 },
            { "Right Ctrl", 0xA3 },
            { "Right Alt", 0xA5 },
            { "Caps Lock", 0x14 },
            { "F8", 0x77 },
            { "F9", 0x78 },
            { "F10", 0x79 },
            { "F12", 0x7B }
        };

        public static string[] Names
        {
            get { return Values.Keys.ToArray(); }
        }

        public static int Code(string name)
        {
            int value;
            return Values.TryGetValue(name ?? "", out value) && value != 0 ? value : 0xA3;
        }

        public static bool IsChord(string name)
        {
            return String.Equals(name, "Win + Ctrl", StringComparison.OrdinalIgnoreCase);
        }
    }

    public sealed class HotkeyChordTracker
    {
        private bool winDown;
        private bool controlDown;
        public bool Active { get; private set; }

        public bool Update(uint key, bool down, out bool active)
        {
            bool before = Active;
            if (key == 0x5B || key == 0x5C) winDown = down;
            if (key == 0x11 || key == 0xA2 || key == 0xA3) controlDown = down;
            Active = winDown && controlDown;
            active = Active;
            return before != Active;
        }

        public void Reset()
        {
            winDown = false;
            controlDown = false;
            Active = false;
        }

        public static bool Handles(uint key)
        {
            return key == 0x5B || key == 0x5C || key == 0x11 || key == 0xA2 || key == 0xA3;
        }
    }

    public static class NativeKeyState
    {
        [DllImport("user32.dll")]
        private static extern short GetAsyncKeyState(int virtualKey);

        private static bool Down(int virtualKey)
        {
            return (GetAsyncKeyState(virtualKey) & 0x8000) != 0;
        }

        public static bool IsWinCtrlDown()
        {
            return (Down(0x5B) || Down(0x5C)) && (Down(0x11) || Down(0xA2) || Down(0xA3));
        }
    }

    public sealed class ConfigStore
    {
        private readonly JavaScriptSerializer serializer = new JavaScriptSerializer();
        public string Root { get; private set; }
        public string SettingsPath { get { return Path.Combine(Root, "settings.json"); } }
        public string HistoryPath { get { return Path.Combine(Root, "history.jsonl"); } }
        public string RecoveryPath { get { return Path.Combine(Root, "Recovery"); } }
        private string SecretPath { get { return Path.Combine(Root, "smart-key.bin"); } }
        private string OpenRouterSecretPath { get { return Path.Combine(Root, "openrouter-key.bin"); } }
        private string GroqSecretPath { get { return Path.Combine(Root, "groq-key.bin"); } }

        public ConfigStore()
        {
            Root = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Flowtype");
            Directory.CreateDirectory(Root);
            Directory.CreateDirectory(RecoveryPath);
        }

        public bool IsFirstRun
        {
            get { return !File.Exists(SettingsPath); }
        }

        public AppSettings Load()
        {
            try
            {
                if (!File.Exists(SettingsPath)) return AppSettings.Defaults();
                string raw = File.ReadAllText(SettingsPath, Encoding.UTF8);
                AppSettings value = serializer.Deserialize<AppSettings>(raw);
                if (value == null) value = AppSettings.Defaults();
                if (raw.IndexOf("CompletionSound", StringComparison.OrdinalIgnoreCase) < 0) value.CompletionSound = true;
                value.Repair();
                return value;
            }
            catch
            {
                return AppSettings.Defaults();
            }
        }

        public void Save(AppSettings settings)
        {
            settings.Repair();
            string json = serializer.Serialize(settings);
            string temporary = SettingsPath + ".new";
            File.WriteAllText(temporary, json, new UTF8Encoding(false));
            if (File.Exists(SettingsPath)) File.Delete(SettingsPath);
            File.Move(temporary, SettingsPath);
            ApplyAutostart(settings.StartWithWindows);
        }

        public string LoadApiKey()
        {
            try
            {
                if (!File.Exists(SecretPath)) return "";
                byte[] encrypted = File.ReadAllBytes(SecretPath);
                byte[] plain = ProtectedData.Unprotect(encrypted, null, DataProtectionScope.CurrentUser);
                return Encoding.UTF8.GetString(plain);
            }
            catch { return ""; }
        }

        public void SaveApiKey(string apiKey)
        {
            if (String.IsNullOrWhiteSpace(apiKey))
            {
                if (File.Exists(SecretPath)) File.Delete(SecretPath);
                return;
            }
            byte[] plain = Encoding.UTF8.GetBytes(apiKey.Trim());
            byte[] encrypted = ProtectedData.Protect(plain, null, DataProtectionScope.CurrentUser);
            File.WriteAllBytes(SecretPath, encrypted);
        }

        public string LoadOpenRouterKey()
        {
            try
            {
                if (!File.Exists(OpenRouterSecretPath)) return "";
                byte[] encrypted = File.ReadAllBytes(OpenRouterSecretPath);
                return Encoding.UTF8.GetString(ProtectedData.Unprotect(encrypted, null, DataProtectionScope.CurrentUser));
            }
            catch { return ""; }
        }

        public void SaveOpenRouterKey(string apiKey)
        {
            if (String.IsNullOrWhiteSpace(apiKey))
            {
                if (File.Exists(OpenRouterSecretPath)) File.Delete(OpenRouterSecretPath);
                return;
            }
            byte[] encrypted = ProtectedData.Protect(Encoding.UTF8.GetBytes(apiKey.Trim()), null, DataProtectionScope.CurrentUser);
            File.WriteAllBytes(OpenRouterSecretPath, encrypted);
        }

        public string LoadGroqKey()
        {
            try
            {
                if (!File.Exists(GroqSecretPath)) return "";
                byte[] encrypted = File.ReadAllBytes(GroqSecretPath);
                return Encoding.UTF8.GetString(ProtectedData.Unprotect(encrypted, null, DataProtectionScope.CurrentUser));
            }
            catch { return ""; }
        }

        public void SaveGroqKey(string apiKey)
        {
            if (String.IsNullOrWhiteSpace(apiKey))
            {
                if (File.Exists(GroqSecretPath)) File.Delete(GroqSecretPath);
                return;
            }
            byte[] encrypted = ProtectedData.Protect(Encoding.UTF8.GetBytes(apiKey.Trim()), null, DataProtectionScope.CurrentUser);
            File.WriteAllBytes(GroqSecretPath, encrypted);
        }

        public void ApplyAutostart(bool enabled)
        {
            using (RegistryKey key = Registry.CurrentUser.CreateSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run"))
            {
                if (enabled)
                {
                    string executable = Path.Combine(FlowtypeApp.AppDirectory, "Flowtype.exe");
                    string command;
                    if (File.Exists(executable)) command = "\"" + executable + "\"";
                    else
                    {
                        string script = Path.Combine(FlowtypeApp.AppDirectory, "Flowtype.ps1");
                        command = String.Format("\"{0}\" -NoProfile -ExecutionPolicy Bypass -STA -WindowStyle Hidden -File \"{1}\"",
                            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), @"WindowsPowerShell\v1.0\powershell.exe"), script);
                    }
                    key.SetValue("Flowtype", command, RegistryValueKind.String);
                }
                else
                {
                    key.DeleteValue("Flowtype", false);
                }
            }
        }

        public void LogError(Exception exception)
        {
            try
            {
                File.AppendAllText(Path.Combine(Root, "errors.log"),
                    DateTime.Now.ToString("o") + Environment.NewLine + exception + Environment.NewLine + Environment.NewLine,
                    Encoding.UTF8);
            }
            catch { }
        }
    }

    public sealed class HistoryEntry
    {
        public DateTime CreatedUtc;
        public string Application;
        public string RawText;
        public string FinalText;
        public string Engine;
    }

    public sealed class HistoryStore
    {
        private readonly string path;
        private readonly JavaScriptSerializer serializer = new JavaScriptSerializer();
        private readonly object gate = new object();

        public HistoryStore(string path) { this.path = path; }

        public void Add(HistoryEntry entry)
        {
            lock (gate)
            {
                File.AppendAllText(path, serializer.Serialize(entry) + Environment.NewLine, new UTF8Encoding(false));
            }
        }

        public List<HistoryEntry> Load()
        {
            List<HistoryEntry> entries = new List<HistoryEntry>();
            if (!File.Exists(path)) return entries;
            foreach (string line in File.ReadLines(path, Encoding.UTF8))
            {
                try
                {
                    if (!String.IsNullOrWhiteSpace(line)) entries.Add(serializer.Deserialize<HistoryEntry>(line));
                }
                catch { }
            }
            entries.Reverse();
            return entries.Take(500).ToList();
        }

        public void Clear()
        {
            lock (gate)
            {
                if (File.Exists(path)) File.Delete(path);
            }
        }
    }

    public sealed class ForegroundInfo
    {
        public IntPtr Handle;
        public IntPtr FocusHandle;
        public string Title;
        public string ProcessName;

        public string AppLabel
        {
            get
            {
                if (!String.IsNullOrWhiteSpace(ProcessName)) return ProcessName;
                if (!String.IsNullOrWhiteSpace(Title)) return Title;
                return "Unknown app";
            }
        }
    }

    public static class ForegroundContext
    {
        [StructLayout(LayoutKind.Sequential)]
        private struct Rect
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct GuiThreadInfo
        {
            public int Size;
            public uint Flags;
            public IntPtr ActiveWindow;
            public IntPtr FocusWindow;
            public IntPtr CaptureWindow;
            public IntPtr MenuOwnerWindow;
            public IntPtr MoveSizeWindow;
            public IntPtr CaretWindow;
            public Rect CaretRectangle;
        }

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);

        [DllImport("user32.dll")]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint processId);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool GetGUIThreadInfo(uint threadId, ref GuiThreadInfo info);

        [DllImport("user32.dll")]
        private static extern void keybd_event(byte key, byte scan, uint flags, UIntPtr extraInfo);

        private const uint KEYEVENTF_KEYUP = 0x0002;

        public static ForegroundInfo Capture(bool includeContext)
        {
            ForegroundInfo result = new ForegroundInfo();
            result.Handle = GetForegroundWindow();
            StringBuilder title = new StringBuilder(1024);
            GetWindowText(result.Handle, title, title.Capacity);
            result.Title = title.ToString();
            uint processId;
            uint threadId = GetWindowThreadProcessId(result.Handle, out processId);
            try { result.ProcessName = Process.GetProcessById((int)processId).ProcessName; }
            catch { result.ProcessName = ""; }
            result.FocusHandle = FocusedWindow(threadId);
            return result;
        }

        private static IntPtr FocusedWindow(uint threadId)
        {
            try
            {
                GuiThreadInfo info = new GuiThreadInfo();
                info.Size = Marshal.SizeOf(typeof(GuiThreadInfo));
                return GetGUIThreadInfo(threadId, ref info) ? info.FocusWindow : IntPtr.Zero;
            }
            catch { return IntPtr.Zero; }
        }

        public static bool IsSameTarget(ForegroundInfo original)
        {
            if (original == null || GetForegroundWindow() != original.Handle) return false;
            if (original.FocusHandle == IntPtr.Zero) return true;
            uint processId;
            uint threadId = GetWindowThreadProcessId(original.Handle, out processId);
            IntPtr current = FocusedWindow(threadId);
            return current == IntPtr.Zero || current == original.FocusHandle;
        }

        public static bool CopyAndPaste(string text, ForegroundInfo original, bool autoPaste)
        {
            Exception last = null;
            for (int attempt = 0; attempt < 6; attempt++)
            {
                try
                {
                    Clipboard.SetText(text ?? "");
                    last = null;
                    break;
                }
                catch (Exception exception)
                {
                    last = exception;
                    Thread.Sleep(40 * (attempt + 1));
                }
            }
            if (last != null) throw last;
            if (!autoPaste || !IsSameTarget(original)) return false;

            keybd_event(0x11, 0, 0, UIntPtr.Zero);
            keybd_event(0x56, 0, 0, UIntPtr.Zero);
            keybd_event(0x56, 0, KEYEVENTF_KEYUP, UIntPtr.Zero);
            keybd_event(0x11, 0, KEYEVENTF_KEYUP, UIntPtr.Zero);
            return true;
        }

        public static void PressEnter()
        {
            keybd_event(0x0D, 0, 0, UIntPtr.Zero);
            keybd_event(0x0D, 0, KEYEVENTF_KEYUP, UIntPtr.Zero);
        }
    }

    public sealed class GlobalKeyHook : IDisposable
    {
        private delegate IntPtr HookProc(int code, IntPtr wParam, IntPtr lParam);

        [StructLayout(LayoutKind.Sequential)]
        private struct KeyboardData
        {
            public uint vkCode;
            public uint scanCode;
            public uint flags;
            public uint time;
            public UIntPtr extraInfo;
        }

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int hookId, HookProc callback, IntPtr module, uint threadId);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool UnhookWindowsHookEx(IntPtr hook);

        [DllImport("user32.dll")]
        private static extern IntPtr CallNextHookEx(IntPtr hook, int code, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string moduleName);

        private readonly HookProc callback;
        private IntPtr handle;
        private string hotkeyName;
        private int primaryKey;
        private readonly HotkeyChordTracker chordTracker = new HotkeyChordTracker();
        public string HotkeyName
        {
            get { return hotkeyName; }
            set
            {
                hotkeyName = String.IsNullOrWhiteSpace(value) ? "Right Ctrl" : value;
                primaryKey = Hotkeys.Code(hotkeyName);
                chordTracker.Reset();
            }
        }
        public bool CaptureEscape { get; set; }
        public event Action<bool> HotkeyChanged;
        public event Action CancelPressed;

        public GlobalKeyHook(string hotkey)
        {
            HotkeyName = hotkey;
            callback = Callback;
            handle = SetWindowsHookEx(13, callback, GetModuleHandle(null), 0);
            if (handle == IntPtr.Zero) throw new System.ComponentModel.Win32Exception(Marshal.GetLastWin32Error());
        }

        private IntPtr Callback(int code, IntPtr wParam, IntPtr lParam)
        {
            if (code >= 0)
            {
                KeyboardData data = (KeyboardData)Marshal.PtrToStructure(lParam, typeof(KeyboardData));
                bool injected = (data.flags & 0x10) != 0;
                bool down = wParam == (IntPtr)0x0100 || wParam == (IntPtr)0x0104;
                bool up = wParam == (IntPtr)0x0101 || wParam == (IntPtr)0x0105;
                if (!injected && Hotkeys.IsChord(hotkeyName) && (down || up) && HotkeyChordTracker.Handles(data.vkCode))
                {
                    bool active;
                    if (chordTracker.Update(data.vkCode, down, out active))
                    {
                        Action<bool> handler = HotkeyChanged;
                        if (handler != null) handler(active);
                    }
                    // Let Windows see modifier events so neither modifier can become stuck.
                    return CallNextHookEx(handle, code, wParam, lParam);
                }
                if (!injected && data.vkCode == (uint)primaryKey && (down || up))
                {
                    Action<bool> handler = HotkeyChanged;
                    if (handler != null) handler(down);
                    return (IntPtr)1;
                }
                if (!injected && CaptureEscape && data.vkCode == 0x1B && down)
                {
                    Action handler = CancelPressed;
                    if (handler != null) handler();
                    return (IntPtr)1;
                }
            }
            return CallNextHookEx(handle, code, wParam, lParam);
        }

        public void Dispose()
        {
            if (handle != IntPtr.Zero)
            {
                UnhookWindowsHookEx(handle);
                handle = IntPtr.Zero;
            }
        }
    }

    public sealed class WaveRecorder : IDisposable
    {
        [StructLayout(LayoutKind.Sequential)]
        private struct WaveFormat
        {
            public ushort formatTag;
            public ushort channels;
            public uint samplesPerSecond;
            public uint averageBytesPerSecond;
            public ushort blockAlign;
            public ushort bitsPerSample;
            public ushort extraSize;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct WaveHeader
        {
            public IntPtr data;
            public uint bufferLength;
            public uint bytesRecorded;
            public IntPtr user;
            public uint flags;
            public uint loops;
            public IntPtr next;
            public IntPtr reserved;
        }

        private delegate void WaveCallback(IntPtr input, uint message, IntPtr instance, IntPtr parameter1, IntPtr parameter2);

        [DllImport("winmm.dll")]
        private static extern int waveInOpen(out IntPtr input, uint deviceId, ref WaveFormat format, WaveCallback callback, IntPtr instance, uint flags);
        [DllImport("winmm.dll")]
        private static extern int waveInPrepareHeader(IntPtr input, IntPtr header, uint size);
        [DllImport("winmm.dll")]
        private static extern int waveInUnprepareHeader(IntPtr input, IntPtr header, uint size);
        [DllImport("winmm.dll")]
        private static extern int waveInAddBuffer(IntPtr input, IntPtr header, uint size);
        [DllImport("winmm.dll")]
        private static extern int waveInStart(IntPtr input);
        [DllImport("winmm.dll")]
        private static extern int waveInStop(IntPtr input);
        [DllImport("winmm.dll")]
        private static extern int waveInReset(IntPtr input);
        [DllImport("winmm.dll")]
        private static extern int waveInClose(IntPtr input);
        [DllImport("winmm.dll", CharSet = CharSet.Unicode)]
        private static extern int waveInGetErrorText(int error, StringBuilder text, int length);

        private sealed class Buffer
        {
            public IntPtr Data;
            public IntPtr Header;
        }

        private const uint CallbackFunction = 0x00030000;
        private const uint DataMessage = 0x03C0;
        private const int SampleRate = 16000;
        // 64 ms buffers keep the visual meter attached to the voice instead of
        // updating in quarter-second jumps.
        private const int BufferSize = 2048;
        private readonly object gate = new object();
        private readonly List<Buffer> buffers = new List<Buffer>();
        private WaveCallback callback;
        private IntPtr input;
        private FileStream rawStream;
        private string rawPath;
        private string wavePath;
        private readonly object micLock = new object();
        private volatile bool recording;
        public float MicGain { get; set; }
        public event Action<float> LevelChanged;

        public WaveRecorder()
        {
            MicGain = 1.2f;
        }

        public bool IsRecording { get { return recording; } }

        public void Start(string outputWavePath)
        {
            lock (micLock)
            {
                if (recording) throw new InvalidOperationException("The recorder is already running.");
                wavePath = outputWavePath;
                rawPath = outputWavePath + ".pcm";
                Directory.CreateDirectory(Path.GetDirectoryName(outputWavePath));
                rawStream = new FileStream(rawPath, FileMode.Create, FileAccess.Write, FileShare.Read);

                WaveFormat format = new WaveFormat();
                format.formatTag = 1;
                format.channels = 1;
                format.samplesPerSecond = SampleRate;
                format.bitsPerSample = 16;
                format.blockAlign = 2;
                format.averageBytesPerSecond = SampleRate * 2;
                format.extraSize = 0;
                callback = OnWaveMessage;
                int error = waveInOpen(out input, unchecked((uint)-1), ref format, callback, IntPtr.Zero, CallbackFunction);
                if (error != 0)
                {
                    rawStream.Dispose();
                    rawStream = null;
                    throw new InvalidOperationException("Microphone error: " + ErrorText(error));
                }

                int headerSize = Marshal.SizeOf(typeof(WaveHeader));
                for (int index = 0; index < 6; index++)
                {
                    Buffer buffer = new Buffer();
                    buffer.Data = Marshal.AllocHGlobal(BufferSize);
                    buffer.Header = Marshal.AllocHGlobal(headerSize);
                    WaveHeader header = new WaveHeader();
                    header.data = buffer.Data;
                    header.bufferLength = BufferSize;
                    Marshal.StructureToPtr(header, buffer.Header, false);
                    Check(waveInPrepareHeader(input, buffer.Header, (uint)headerSize));
                    Check(waveInAddBuffer(input, buffer.Header, (uint)headerSize));
                    buffers.Add(buffer);
                }
                recording = true;
                Check(waveInStart(input));
            }
        }

        private void OnWaveMessage(IntPtr source, uint message, IntPtr instance, IntPtr parameter1, IntPtr parameter2)
        {
            if (message != DataMessage || parameter1 == IntPtr.Zero) return;
            WaveHeader header = (WaveHeader)Marshal.PtrToStructure(parameter1, typeof(WaveHeader));
            if (recording && header.bytesRecorded > 0)
            {
                byte[] data = new byte[header.bytesRecorded];
                Marshal.Copy(header.data, data, 0, data.Length);
                ApplyGainInPlace(data);
                lock (gate)
                {
                    if (recording && rawStream != null) rawStream.Write(data, 0, data.Length);
                }
                float peak = 0;
                double power = 0;
                int sampleCount = 0;
                for (int index = 0; index + 1 < data.Length; index += 2)
                {
                    short sample = (short)(data[index] | (data[index + 1] << 8));
                    float normalized = sample / 32768f;
                    peak = Math.Max(peak, Math.Abs(normalized));
                    power += normalized * normalized;
                    sampleCount++;
                }
                float rms = sampleCount == 0 ? 0 : (float)Math.Sqrt(power / sampleCount);
                float meter = Math.Min(1f, rms * 4.2f + peak * 0.42f);
                Action<float> levelHandler = LevelChanged;
                if (levelHandler != null) levelHandler(meter);
            }
            if (recording) waveInAddBuffer(input, parameter1, (uint)Marshal.SizeOf(typeof(WaveHeader)));
        }

        public string Stop()
        {
            lock (micLock)
            {
                if (!recording) return wavePath;
                recording = false;
                waveInStop(input);
                waveInReset(input);
                Thread.Sleep(20);
                int headerSize = Marshal.SizeOf(typeof(WaveHeader));
                foreach (Buffer buffer in buffers)
                {
                    waveInUnprepareHeader(input, buffer.Header, (uint)headerSize);
                    Marshal.FreeHGlobal(buffer.Header);
                    Marshal.FreeHGlobal(buffer.Data);
                }
                buffers.Clear();
                waveInClose(input);
                input = IntPtr.Zero;
                lock (gate)
                {
                    if (rawStream != null)
                    {
                        rawStream.Flush();
                        rawStream.Dispose();
                        rawStream = null;
                    }
                }
                WriteWave(rawPath, wavePath, MicGain);
                try { File.Delete(rawPath); } catch { }
                return wavePath;
            }
        }

        public void Cancel()
        {
            string path = Stop();
            try { if (!String.IsNullOrWhiteSpace(path) && File.Exists(path)) File.Delete(path); } catch { }
        }

        private static int SoftLimitSample(int sample)
        {
            const int threshold = 26000;
            const int ceiling = 32767;
            if (sample > threshold)
            {
                int excess = sample - threshold;
                return Math.Min(ceiling, threshold + excess / 3);
            }
            if (sample < -threshold)
            {
                int excess = sample + threshold;
                return Math.Max(-ceiling, -threshold + excess / 3);
            }
            return sample;
        }

        private void ApplyGainInPlace(byte[] data)
        {
            float gain = MicGain <= 0 ? 1f : MicGain;
            if (Math.Abs(gain - 1f) < 0.01f) return;
            for (int index = 0; index + 1 < data.Length; index += 2)
            {
                int sample = (short)(data[index] | (data[index + 1] << 8));
                int amplified = SoftLimitSample((int)Math.Round(sample * gain));
                data[index] = (byte)(amplified & 0xFF);
                data[index + 1] = (byte)((amplified >> 8) & 0xFF);
            }
        }

        private static int AbsPcmSample(short sample)
        {
            int value = sample;
            return value == short.MinValue ? 32768 : Math.Abs(value);
        }

        private static void WriteWave(string pcmPath, string outputPath, float micGain)
        {
            byte[] pcm = File.ReadAllBytes(pcmPath);
            if (pcm.Length >= 2)
            {
                int peak = 1;
                for (int index = 0; index + 1 < pcm.Length; index += 2)
                {
                    int sample = AbsPcmSample((short)(pcm[index] | (pcm[index + 1] << 8)));
                    if (sample > peak) peak = sample;
                }
                float target = 24000f;
                float normalize = peak < 1200 ? Math.Min(3.5f, target / peak) : Math.Min(2.2f, target / peak);
                // pcm already includes MicGain from ApplyGainInPlace — do not apply it again here.
                // When the input is already loud, never boost; soft-limit instead of hard-clipping.
                if (peak >= 28000) normalize = Math.Min(1f, target / peak);
                else if (peak >= 20000) normalize = Math.Min(1f, normalize);
                float totalGain = normalize;
                if (totalGain > 4.5f) totalGain = 4.5f;
                if (Math.Abs(totalGain - 1f) > 0.02f)
                {
                    for (int index = 0; index + 1 < pcm.Length; index += 2)
                    {
                        int sample = (short)(pcm[index] | (pcm[index + 1] << 8));
                        int amplified = SoftLimitSample((int)Math.Round(sample * totalGain));
                        pcm[index] = (byte)(amplified & 0xFF);
                        pcm[index + 1] = (byte)((amplified >> 8) & 0xFF);
                    }
                }
            }
            using (FileStream output = new FileStream(outputPath, FileMode.Create, FileAccess.Write, FileShare.Read))
            using (BinaryWriter writer = new BinaryWriter(output, Encoding.ASCII))
            {
                writer.Write(Encoding.ASCII.GetBytes("RIFF"));
                writer.Write((int)(36 + pcm.Length));
                writer.Write(Encoding.ASCII.GetBytes("WAVEfmt "));
                writer.Write(16);
                writer.Write((short)1);
                writer.Write((short)1);
                writer.Write(SampleRate);
                writer.Write(SampleRate * 2);
                writer.Write((short)2);
                writer.Write((short)16);
                writer.Write(Encoding.ASCII.GetBytes("data"));
                writer.Write(pcm.Length);
                writer.Write(pcm);
            }
        }

        private static void Check(int error)
        {
            if (error != 0) throw new InvalidOperationException("Microphone error: " + ErrorText(error));
        }

        private static string ErrorText(int error)
        {
            StringBuilder text = new StringBuilder(256);
            waveInGetErrorText(error, text, text.Capacity);
            return text.Length == 0 ? error.ToString(CultureInfo.InvariantCulture) : text.ToString();
        }

        public void Dispose()
        {
            try { if (recording) Cancel(); } catch { }
        }
    }

    public static class TranscriptionQuality
    {
        private static readonly HashSet<string> AllowedShortOutputs = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "a", "i", "ok", "no", "go", "hi", "hey", "yes", "yeah", "yep", "nope", "oh", "ah", "um"
        };

        public static bool ShouldReject(string raw, long recordMs, long audioBytes)
        {
            if (String.IsNullOrWhiteSpace(raw)) return true;
            string text = raw.Trim();
            if (text.Length == 0) return true;
            if (AllowedShortOutputs.Contains(text)) return false;

            // Whisper often hallucinates a lone letter when the clip was cut short.
            if (text.Length == 1 && recordMs >= 250) return true;

            // A multi-syllable clip that only produced 1-2 characters is almost always garbage.
            if (text.Length <= 2 && recordMs >= 350 && audioBytes >= 8000) return true;

            return false;
        }
    }

    public static class TextProcessor
    {
        public static string Clean(SpeechTranscript transcript, AppSettings settings, ForegroundInfo context)
        {
            if (transcript == null) return "";
            string cleaned = Clean(transcript.Text, settings, context);
            bool hasPhraseTiming = transcript.Segments != null && transcript.Segments.Count >= 2;
            bool hasWordTiming = transcript.Words != null && transcript.Words.Count >= 2;
            if (cleaned.Contains("\n") || (!hasPhraseTiming && !hasWordTiming)) return cleaned;
            string structured = FormatProsodicList(transcript, settings, context);
            return String.IsNullOrWhiteSpace(structured) ? cleaned : structured;
        }

        public static string Clean(string input, AppSettings settings)
        {
            return Clean(input, settings, null);
        }

        public static bool ExtractPressEnter(ref string text)
        {
            string value = text ?? "";
            Match command = Regex.Match(value, @"\bpress enter[.!?]?\s*$", RegexOptions.IgnoreCase);
            if (!command.Success) return false;
            text = value.Remove(command.Index, command.Length).TrimEnd();
            return true;
        }

        public static string Clean(string input, AppSettings settings, ForegroundInfo context)
        {
            if (String.IsNullOrWhiteSpace(input)) return "";
            string text = StripPromptHallucinations(input.Trim(), settings, context);

            foreach (KeyValuePair<string, string> snippet in settings.Snippets)
            {
                if (!String.IsNullOrWhiteSpace(snippet.Key))
                    text = Regex.Replace(text, @"(?<!\w)" + Regex.Escape(snippet.Key) + @"(?!\w)",
                        delegate { return snippet.Value ?? ""; }, RegexOptions.IgnoreCase);
            }
            foreach (string entry in settings.Dictionary)
            {
                string[] map = Regex.Split(entry ?? "", @"\s*(?:=>|=)\s*", RegexOptions.None);
                if (map.Length == 2 && map[0].Length > 0)
                    text = Regex.Replace(text, @"\b" + Regex.Escape(map[0]) + @"\b", delegate { return map[1]; }, RegexOptions.IgnoreCase);
            }

            text = ApplyFuzzyDictionary(text, settings, context);

            text = ApplyBacktrack(text);

            if (!String.Equals(settings.Style, "Verbatim", StringComparison.OrdinalIgnoreCase))
            {
                text = Regex.Replace(text, @"\b(?:um+|uh+|erm+|hmm+)\b[,.]?\s*", "", RegexOptions.IgnoreCase);
                text = Regex.Replace(text, @"(^|[,.!?]\s+)\s*(?:you know|I mean)\s*[,—-]?\s*", "$1", RegexOptions.IgnoreCase);
                if (String.Equals(settings.Style, "Concise", StringComparison.OrdinalIgnoreCase))
                    text = Regex.Replace(text, @"\b(?:basically|literally|kind of|sort of)\b[,.]?\s*", "", RegexOptions.IgnoreCase);
            }
            text = Regex.Replace(text, @"\b(\w+)\s+\1\b", "$1", RegexOptions.IgnoreCase);
            text = RemoveRepeatedPhrases(text);
            text = RemoveWhisperRepetitions(text);
            text = Regex.Replace(text,
                @"\b(\d{1,2}(?::\d{2})?\s*(?:a\.?m\.?|p\.?m\.?)?)\s*,?\s*(?:no|sorry|actually|I mean)\s*,?\s*(\d{1,2}(?::\d{2})?\s*(?:a\.?m\.?|p\.?m\.?)?)\b",
                "$2", RegexOptions.IgnoreCase);
            text = Regex.Replace(text, @"\b(?:new paragraph|start (?:a )?new paragraph|skip (?:a )?line)\b", "\n\n", RegexOptions.IgnoreCase);
            text = Regex.Replace(text, @"\b(?:new line|next line|line break)\b", "\n", RegexOptions.IgnoreCase);
            text = Regex.Replace(text, @"\s+(?:em dash|long dash)\s+", " — ", RegexOptions.IgnoreCase);
            text = Regex.Replace(text, @"\s+ellipsis\b", "…", RegexOptions.IgnoreCase);
            text = Regex.Replace(text, @"\bopen quote\s*", "\"", RegexOptions.IgnoreCase);
            text = Regex.Replace(text, @"\s*close quote\b", "\"", RegexOptions.IgnoreCase);
            text = text.Replace(" -- ", " — ");

            text = FormatBullets(text);
            text = FormatNumbered(text);
            text = FormatInferredList(text);

            text = Regex.Replace(text, @"\s+comma\b", ",", RegexOptions.IgnoreCase);
            text = Regex.Replace(text, @"\s+(?:period|full stop)\b", ".", RegexOptions.IgnoreCase);
            text = Regex.Replace(text, @"\s+question mark\b", "?", RegexOptions.IgnoreCase);
            text = Regex.Replace(text, @"\s+exclamation (?:mark|point)\b", "!", RegexOptions.IgnoreCase);
            text = Regex.Replace(text, @"\s+colon\b", ":", RegexOptions.IgnoreCase);
            text = Regex.Replace(text, @"\s+semicolon\b", ";", RegexOptions.IgnoreCase);
            text = Regex.Replace(text, @"\s+(?:ampersand|and sign)\b", " &", RegexOptions.IgnoreCase);
            text = Regex.Replace(text, @"\s+(?:percent sign|percentage symbol)\b", "%", RegexOptions.IgnoreCase);
            text = Regex.Replace(text, @"\s+(?:asterisk|star symbol)\b", " *", RegexOptions.IgnoreCase);
            text = Regex.Replace(text, @"\s+(?:forward slash|slash symbol)\b", "/", RegexOptions.IgnoreCase);
            text = Regex.Replace(text, @"\s+backslash\b", @"\", RegexOptions.IgnoreCase);
            text = Regex.Replace(text, @"\s+underscore\b", "_", RegexOptions.IgnoreCase);
            text = Regex.Replace(text, @"\s+(?:at sign|at symbol)\b", "@", RegexOptions.IgnoreCase);
            text = Regex.Replace(text, @"\s+(?:hashtag|hash symbol)\b", " #", RegexOptions.IgnoreCase);
            text = Regex.Replace(text, @"\s+plus sign\b", "+", RegexOptions.IgnoreCase);
            text = Regex.Replace(text, @"\s+(?:equals sign|equal sign)\b", "=", RegexOptions.IgnoreCase);
            text = Regex.Replace(text, @"\s+open (?:parenthesis|paren)\s*", " (", RegexOptions.IgnoreCase);
            text = Regex.Replace(text, @"\s+close (?:parenthesis|paren)\b", ")", RegexOptions.IgnoreCase);
            text = Regex.Replace(text, @"[ \t]+([,.;:!?])", "$1");
            text = Regex.Replace(text, @"([,.;:!?])(?=[A-Za-z])", "$1 ");
            text = Regex.Replace(text, @"[ \t]{2,}", " ");
            text = Regex.Replace(text, @" *\n *", "\n");
            text = Regex.Replace(text, @"\n{3,}", "\n\n");
            text = Paragraphize(text, context).Trim();
            if (IsTerminalContext(context)) return text.TrimEnd('.', ' ');
            text = Capitalize(text);
            if (!Regex.IsMatch(text, @"[.!?…,:;\)\]\""']$", RegexOptions.None) && !text.Contains("\n")) text += ".";
            return ApplyApplicationStyle(text, settings, context);
        }

        private static string FormatBullets(string text)
        {
            Regex marker = new Regex(@"(?:^|[\s,;:])(?:bullet point|next bullet|new bullet|next point|another point|final point)\s*[:,]?\s+", RegexOptions.IgnoreCase);
            MatchCollection matches = marker.Matches(text);
            if (matches.Count < 2) return text;
            string prefix = text.Substring(0, matches[0].Index).Trim();
            List<string> items = new List<string>();
            for (int index = 0; index < matches.Count; index++)
            {
                int start = matches[index].Index + matches[index].Length;
                int end = index + 1 < matches.Count ? matches[index + 1].Index : text.Length;
                string item = text.Substring(start, end - start).Trim(' ', ',', '.', ';');
                if (item.Length > 0) items.Add(item);
            }
            StringBuilder output = new StringBuilder();
            if (prefix.Length > 0) output.Append(prefix.TrimEnd(':') + ":\n");
            foreach (string item in items) output.Append("- " + item + "\n");
            return output.ToString().TrimEnd();
        }

        private static string FormatNumbered(string text)
        {
            Regex marker = new Regex(@"(?:^|[\s,;:])(?:first(?:ly)?|second(?:ly)?|third(?:ly)?|fourth(?:ly)?|fifth(?:ly)?|finally|number\s+(?:one|two|three|four|five))\s*[:,]?\s+", RegexOptions.IgnoreCase);
            MatchCollection matches = marker.Matches(text);
            if (matches.Count < 2) return FormatCardinalList(text);
            string prefix = text.Substring(0, matches[0].Index).Trim();
            List<string> items = new List<string>();
            for (int index = 0; index < matches.Count; index++)
            {
                int start = matches[index].Index + matches[index].Length;
                int end = index + 1 < matches.Count ? matches[index + 1].Index : text.Length;
                string item = text.Substring(start, end - start).Trim(' ', ',', '.', ';');
                if (item.Length > 0) items.Add(item);
            }
            StringBuilder output = new StringBuilder();
            if (prefix.Length > 0) output.Append(prefix.TrimEnd(':') + ":\n");
            for (int index = 0; index < items.Count; index++) output.Append((index + 1).ToString(CultureInfo.InvariantCulture) + ". " + items[index] + "\n");
            return output.ToString().TrimEnd();
        }

        private static string FormatCardinalList(string text)
        {
            Regex marker = new Regex(@"(?:^|[\s,;:])(?<number>one|two|three|four|five|six|seven|eight|nine|ten)\s*[:.)-]?\s+", RegexOptions.IgnoreCase);
            MatchCollection matches = marker.Matches(text);
            if (matches.Count < 2) return text;
            string prefix = text.Substring(0, matches[0].Index).Trim();
            if (!LooksLikeListIntro(prefix)) return text;
            string[] order = { "one", "two", "three", "four", "five", "six", "seven", "eight", "nine", "ten" };
            int expected = 0;
            foreach (Match match in matches)
            {
                string number = match.Groups["number"].Value.ToLowerInvariant();
                int position = Array.IndexOf(order, number);
                if (position != expected) return text;
                expected++;
            }
            List<string> items = new List<string>();
            for (int index = 0; index < matches.Count; index++)
            {
                int start = matches[index].Index + matches[index].Length;
                int end = index + 1 < matches.Count ? matches[index + 1].Index : text.Length;
                string item = text.Substring(start, end - start).Trim(' ', ',', '.', ';');
                if (item.Length > 0) items.Add(item);
            }
            if (items.Count < 2) return text;
            StringBuilder output = new StringBuilder();
            if (prefix.Length > 0) output.Append(prefix.TrimEnd(':') + ":\n");
            for (int index = 0; index < items.Count; index++)
                output.Append((index + 1).ToString(CultureInfo.InvariantCulture) + ". " + items[index] + "\n");
            return output.ToString().TrimEnd();
        }

        private static string FormatInferredList(string text)
        {
            if (text.Contains("\n")) return text;
            Match list = Regex.Match(text,
                @"^(?<head>.*?\b(?:things|points|steps|reasons|goals|priorities|options|items|tasks|changes)\b.*?(?:\bare\b|\binclude(?:s)?\b|:))\s*(?<body>.+)$",
                RegexOptions.IgnoreCase);
            if (!list.Success) return text;
            string body = list.Groups["body"].Value.Trim();
            string[] parts = Regex.Split(body, @"\s*(?:;|\.(?=\s)|,(?:\s+and)?|\b(?:and then|then|next|also|finally)\b)\s*", RegexOptions.IgnoreCase)
                .Select(value => value.Trim(' ', ',', '.', ';')).Where(value => value.Length > 0).ToArray();
            if (parts.Length < 3 || parts.Length > 9 || parts.Any(value => value.Length > 120)) return text;
            StringBuilder output = new StringBuilder(list.Groups["head"].Value.Trim().TrimEnd(':') + ":\n");
            foreach (string item in parts) output.Append("- " + item + "\n");
            return output.ToString().TrimEnd();
        }

        private static string FormatProsodicList(SpeechTranscript transcript, AppSettings settings, ForegroundInfo context)
        {
            List<SpeechSegment> phrases = transcript.Segments.Where(value => value != null && !String.IsNullOrWhiteSpace(value.Text)).ToList();
            List<SpeechSegment> wordPhrases = BuildWordPhrases(transcript.Words);
            if (wordPhrases.Count > phrases.Count) phrases = wordPhrases;
            if (phrases.Count < 2 || phrases.Count > 9) return "";
            string first = phrases[0].Text.Trim();
            if (!LooksLikeListIntro(first)) return "";
            int realPauses = 0;
            for (int index = 1; index < phrases.Count; index++)
                if (phrases[index].Start - phrases[index - 1].End >= 0.28) realPauses++;
            if (phrases.Count < 3 && realPauses == 0) return "";

            Match split = Regex.Match(first, @"^(?<head>.*?(?:\bare\b|\binclude(?:s)?\b|:))\s*(?<tail>.+)$", RegexOptions.IgnoreCase);
            string heading = split.Success ? split.Groups["head"].Value : first;
            List<string> items = new List<string>();
            if (split.Success && !String.IsNullOrWhiteSpace(split.Groups["tail"].Value)) items.Add(split.Groups["tail"].Value);
            for (int index = 1; index < phrases.Count; index++) items.Add(phrases[index].Text);
            items = items.Select(value => Clean(value, settings, context).Trim().TrimEnd('.')).Where(value => value.Length > 0 && value.Length <= 140).ToList();
            if (items.Count < 2) return "";
            string cleanHeading = Clean(heading, settings, context).Trim().TrimEnd('.', ':');
            StringBuilder output = new StringBuilder(cleanHeading + ":\n");
            foreach (string item in items) output.Append("- " + Capitalize(item) + "\n");
            return output.ToString().TrimEnd();
        }

        private static List<SpeechSegment> BuildWordPhrases(List<SpeechWord> words)
        {
            List<SpeechSegment> phrases = new List<SpeechSegment>();
            if (words == null || words.Count < 2) return phrases;
            StringBuilder text = new StringBuilder();
            double start = words[0].Start;
            double end = words[0].End;
            for (int index = 0; index < words.Count; index++)
            {
                SpeechWord word = words[index];
                if (word == null || String.IsNullOrWhiteSpace(word.Text)) continue;
                if (text.Length > 0 && word.Start - end >= 0.32)
                {
                    phrases.Add(new SpeechSegment { Text = text.ToString().Trim(), Start = start, End = end });
                    text.Clear();
                    start = word.Start;
                }
                string token = word.Text;
                if (text.Length > 0 && !Char.IsWhiteSpace(token[0]) && !Regex.IsMatch(token, @"^[,.;:!?)]")) text.Append(' ');
                text.Append(token);
                end = word.End;
            }
            if (text.Length > 0) phrases.Add(new SpeechSegment { Text = text.ToString().Trim(), Start = start, End = end });
            return phrases;
        }

        private static bool LooksLikeListIntro(string value)
        {
            return Regex.IsMatch(value ?? "",
                @"\b(?:top|following|list|things|points|steps|reasons|goals|priorities|options|items|tasks|changes|we need|I need|I want|include|are)\b",
                RegexOptions.IgnoreCase);
        }

        private static string ApplyFuzzyDictionary(string text, AppSettings settings, ForegroundInfo context)
        {
            List<string> terms = CollectCanonicalTerms(settings, context);
            if (terms.Count == 0) return text;
            return Regex.Replace(text, @"\b[A-Za-z][A-Za-z'-]{2,}\b", delegate(Match match)
            {
                string word = match.Value;
                if (terms.Exists(value => String.Equals(value, word, StringComparison.OrdinalIgnoreCase))) return word;
                string best = null;
                int bestDistance = int.MaxValue;
                int tieCount = 0;
                foreach (string term in terms)
                {
                    if (Math.Abs(term.Length - word.Length) > 2) continue;
                    int distance = LevenshteinDistance(word, term);
                    int maxDistance = term.Length <= 5 ? 1 : 2;
                    if (distance <= 0 || distance > maxDistance) continue;
                    if (distance < bestDistance)
                    {
                        bestDistance = distance;
                        best = term;
                        tieCount = 1;
                    }
                    else if (distance == bestDistance)
                    {
                        tieCount++;
                    }
                }
                return best != null && tieCount == 1 ? PreserveCase(word, best) : word;
            });
        }

        private static List<string> CollectCanonicalTerms(AppSettings settings, ForegroundInfo context)
        {
            List<string> terms = new List<string>();
            HashSet<string> seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            Action<string> add = delegate(string value)
            {
                if (String.IsNullOrWhiteSpace(value)) return;
                value = value.Trim();
                if (value.Length < 4 || seen.Contains(value)) return;
                seen.Add(value);
                terms.Add(value);
            };
            if (settings != null)
            {
                foreach (string entry in settings.Dictionary)
                {
                    string[] map = Regex.Split(entry ?? "", @"\s*(?:=>|=)\s*", RegexOptions.None);
                    if (map.Length == 2)
                    {
                        add(map[1]);
                        add(map[0]);
                    }
                    else add(entry);
                }
                foreach (KeyValuePair<string, string> snippet in settings.Snippets)
                {
                    add(snippet.Key);
                    add(snippet.Value);
                }
            }
            if (context != null && !String.IsNullOrWhiteSpace(context.Title))
            {
                foreach (Match token in Regex.Matches(context.Title, @"\b[A-Za-z][A-Za-z'-]{3,}\b"))
                    add(token.Value);
            }
            return terms;
        }

        private static string PreserveCase(string original, string replacement)
        {
            if (String.IsNullOrEmpty(original) || String.IsNullOrEmpty(replacement)) return replacement;
            if (Char.IsUpper(original[0]))
            {
                if (original.Length > 1 && Char.IsUpper(original[1])) return replacement.ToUpperInvariant();
                return Char.ToUpperInvariant(replacement[0]) + (replacement.Length > 1 ? replacement.Substring(1) : "");
            }
            return replacement;
        }

        private static int LevenshteinDistance(string left, string right)
        {
            if (String.Equals(left, right, StringComparison.OrdinalIgnoreCase)) return 0;
            int rows = left.Length + 1;
            int cols = right.Length + 1;
            int[] previous = new int[cols];
            int[] current = new int[cols];
            for (int col = 0; col < cols; col++) previous[col] = col;
            for (int row = 1; row < rows; row++)
            {
                current[0] = row;
                for (int col = 1; col < cols; col++)
                {
                    int cost = Char.ToLowerInvariant(left[row - 1]) == Char.ToLowerInvariant(right[col - 1]) ? 0 : 1;
                    current[col] = Math.Min(Math.Min(current[col - 1] + 1, previous[col] + 1), previous[col - 1] + cost);
                }
                int[] swap = previous;
                previous = current;
                current = swap;
            }
            return previous[cols - 1];
        }

        private static string ApplyBacktrack(string text)
        {
            string valuePattern = @"(?:\d{1,4}(?::\d{2})?(?:\s*(?:a\.?m\.?|p\.?m\.?))?|Monday|Tuesday|Wednesday|Thursday|Friday|Saturday|Sunday|today|tomorrow|morning|afternoon|evening)";
            text = Regex.Replace(text,
                @"\b(?<old>" + valuePattern + @")\s*(?:[,….—-]+\s*)?(?:no|sorry|actually|I mean|scratch that)\s*[,.:—-]?\s*(?<new>" + valuePattern + @")\b",
                "${new}", RegexOptions.IgnoreCase);
            text = Regex.Replace(text,
                @"\b(?<old>[A-Za-z][A-Za-z'-]*)\s*[,….—-]+\s*(?:no|sorry|I mean|scratch that)\s*[,.:—-]?\s*(?<new>[A-Za-z][A-Za-z'-]*)\b",
                "${new}", RegexOptions.IgnoreCase);
            text = Regex.Replace(text,
                @"^.{1,160}\b(?:start over|never mind)\b\s*[,.:—-]?\s*", "", RegexOptions.IgnoreCase);
            text = Regex.Replace(text,
                @"\b(as\s+(?:a|an|the)\s+)([A-Za-z'-]+(?:\s+[A-Za-z'-]+){0,2})\s*(?:[,….]+\s*)?\1([A-Za-z'-]+(?:\s+[A-Za-z'-]+){0,2})\b",
                "$1$3", RegexOptions.IgnoreCase);
            return text;
        }

        public static string StripPromptHallucinations(string text, AppSettings settings, ForegroundInfo context)
        {
            if (String.IsNullOrWhiteSpace(text)) return "";
            text = text.Trim();

            // Whisper often echoes the STT prompt at the tail of longer clips.
            Match targetWindow = Regex.Match(text, @"\bTarget window\b", RegexOptions.IgnoreCase);
            if (targetWindow.Success && targetWindow.Index >= 20)
                text = text.Substring(0, targetWindow.Index).TrimEnd(' ', '\t', '-', '–', '—', ',');

            Match preferredTerms = Regex.Match(text, @"\bPreferred names and spellings\b", RegexOptions.IgnoreCase);
            if (preferredTerms.Success && preferredTerms.Index >= 20)
                text = text.Substring(0, preferredTerms.Index).TrimEnd(' ', '\t', '-', '–', '—', ',');

            text = Regex.Replace(text, @"[\s\-–—,]*\b(?:Target window|Outro to)\b.*$", "", RegexOptions.IgnoreCase);
            text = Regex.Replace(text, @"[\s,]*(?:Camp\.\d|P\.\$[%&$#@]*).*$", "", RegexOptions.IgnoreCase);
            text = Regex.Replace(text, @"[\s,]*[A-Za-z0-9.\s]*[%&$#@]{2,}[A-Za-z0-9.%&$#@\s]*$", "");
            return text.Trim();
        }

        private static string RemoveRepeatedPhrases(string text)
        {
            for (int pass = 0; pass < 3; pass++)
            {
                string next = Regex.Replace(text,
                    @"\b(?<phrase>[A-Za-z0-9']+(?:\s+[A-Za-z0-9']+){0,3})\s*[,]?\s+\k<phrase>\b",
                    "${phrase}", RegexOptions.IgnoreCase);
                if (String.Equals(next, text, StringComparison.Ordinal)) break;
                text = next;
            }
            return text;
        }

        private static string RemoveWhisperRepetitions(string text)
        {
            if (String.IsNullOrWhiteSpace(text) || text.Length < 40) return text;
            for (int pass = 0; pass < 2; pass++)
            {
                string next = Regex.Replace(text,
                    @"([^.!?\n""']{12,}[.!?]+)\s+\1+",
                    "$1", RegexOptions.IgnoreCase);
                if (String.Equals(next, text, StringComparison.Ordinal)) break;
                text = next;
            }
            for (int pass = 0; pass < 2; pass++)
            {
                string next = Regex.Replace(text,
                    @"(\b[A-Za-z0-9']+(?:\s+[A-Za-z0-9']+){2,8})\s*(?:\1\s*)+$",
                    "$1", RegexOptions.IgnoreCase);
                if (String.Equals(next, text, StringComparison.Ordinal)) break;
                text = next;
            }
            return text;
        }

        private static string Paragraphize(string text, ForegroundInfo context)
        {
            if (text.Contains("\n") || text.Length < 420 || context == null) return text;
            string app = (context.ProcessName ?? "").ToLowerInvariant();
            bool prose = app.Contains("outlook") || app.Contains("winword") || app.Contains("onenote") ||
                app.Contains("notepad") || app.Contains("wordpad") || app.Contains("chrome") || app.Contains("msedge") || app.Contains("firefox");
            if (!prose) return text;
            MatchCollection sentences = Regex.Matches(text, @"[^.!?]+[.!?]+(?:[""']|$)?|[^.!?]+$");
            if (sentences.Count < 4) return text;
            StringBuilder output = new StringBuilder();
            int paragraphLength = 0;
            foreach (Match sentence in sentences)
            {
                string value = sentence.Value.Trim();
                if (value.Length == 0) continue;
                if (paragraphLength > 0 && paragraphLength + value.Length > 300)
                {
                    output.Append("\n\n");
                    paragraphLength = 0;
                }
                else if (paragraphLength > 0) output.Append(' ');
                output.Append(value);
                paragraphLength += value.Length + 1;
            }
            return output.Length == 0 ? text : output.ToString();
        }

        private static string ApplyApplicationStyle(string text, AppSettings settings, ForegroundInfo context)
        {
            string signature = ContextSignature(context);
            bool messaging = Regex.IsMatch(signature,
                @"\b(slack|discord|whatsapp|telegram|signal|teams|messenger|wechat|beeper|viber|reddit|twitter|\bx\b|instagram)\b",
                RegexOptions.IgnoreCase);
            int sentenceCount = Regex.Matches(text, @"[.!?](?:\s|$)").Count;
            bool casual = String.Equals(settings.Style, "Casual", StringComparison.OrdinalIgnoreCase);
            bool formal = String.Equals(settings.Style, "Formal", StringComparison.OrdinalIgnoreCase);
            if (!formal && text.EndsWith(".", StringComparison.Ordinal) && !text.Contains("\n") &&
                ((messaging && sentenceCount <= 2) || (casual && sentenceCount <= 10)))
                text = text.Substring(0, text.Length - 1);
            return text;
        }

        private static bool IsTerminalContext(ForegroundInfo context)
        {
            string signature = ContextSignature(context);
            return Regex.IsMatch(signature, @"\b(powershell|pwsh|command prompt|cmd\.exe|terminal|windows terminal|windowsterminal|wsl|bash|zsh)\b", RegexOptions.IgnoreCase);
        }

        private static string ContextSignature(ForegroundInfo context)
        {
            if (context == null) return "";
            return (context.ProcessName ?? "") + " " + (context.Title ?? "");
        }

        private static string Capitalize(string text)
        {
            bool upper = true;
            StringBuilder output = new StringBuilder(text.Length);
            for (int index = 0; index < text.Length; index++)
            {
                char value = text[index];
                if (upper && Char.IsLetter(value))
                {
                    value = Char.ToUpper(value, CultureInfo.CurrentCulture);
                    upper = false;
                }
                output.Append(value);
                if (value == '.' || value == '?' || value == '!' || value == '\n') upper = true;
            }
            return output.ToString();
        }
    }

    public static class ApiHelpers
    {
        public static string ErrorMessage(string body, HttpStatusCode status)
        {
            try
            {
                object root = new JavaScriptSerializer().DeserializeObject(body);
                Dictionary<string, object> dictionary = root as Dictionary<string, object>;
                if (dictionary != null && dictionary.ContainsKey("error"))
                {
                    Dictionary<string, object> error = dictionary["error"] as Dictionary<string, object>;
                    if (error != null && error.ContainsKey("message")) return Convert.ToString(error["message"], CultureInfo.InvariantCulture);
                }
            }
            catch { }
            string compact = Regex.Replace(body ?? "", @"\s+", " ").Trim();
            if (compact.Length > 300) compact = compact.Substring(0, 300) + "…";
            return String.Format("API request failed ({0}). {1}", (int)status, compact);
        }

        public static string ExtractText(object node)
        {
            Dictionary<string, object> dictionary = node as Dictionary<string, object>;
            if (dictionary != null)
            {
                object type;
                object text;
                if (dictionary.TryGetValue("type", out type) && Convert.ToString(type) == "output_text" && dictionary.TryGetValue("text", out text))
                    return Convert.ToString(text);
                foreach (object value in dictionary.Values)
                {
                    string found = ExtractText(value);
                    if (!String.IsNullOrWhiteSpace(found)) return found;
                }
            }
            IEnumerable sequence = node as IEnumerable;
            if (sequence != null && !(node is string))
            {
                foreach (object value in sequence)
                {
                    string found = ExtractText(value);
                    if (!String.IsNullOrWhiteSpace(found)) return found;
                }
            }
            return "";
        }
    }

    public sealed class OpenAiEngine
    {
        private readonly JavaScriptSerializer serializer = new JavaScriptSerializer();

        private HttpClient Client(string apiKey)
        {
            HttpClient client = new HttpClient();
            client.Timeout = TimeSpan.FromMinutes(5);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
            client.DefaultRequestHeaders.UserAgent.ParseAdd("Flowtype-Desktop/1.3.21");
            return client;
        }

        public async Task<string> TranscribeAsync(string wavePath, AppSettings settings, string apiKey)
        {
            if (String.IsNullOrWhiteSpace(apiKey)) throw new InvalidOperationException("OpenAI speech mode needs an API key. Open Flowtype Settings from the tray icon.");
            string url = settings.ApiBaseUrl.TrimEnd('/') + "/audio/transcriptions";
            using (HttpClient client = Client(apiKey))
            using (MultipartFormDataContent form = new MultipartFormDataContent())
            using (FileStream stream = File.OpenRead(wavePath))
            using (StreamContent audio = new StreamContent(stream))
            {
                audio.Headers.ContentType = new MediaTypeHeaderValue("audio/wav");
                form.Add(audio, "file", Path.GetFileName(wavePath));
                form.Add(new StringContent(settings.TranscriptionModel), "model");
                form.Add(new StringContent("json"), "response_format");
                if (settings.Dictionary.Count > 0)
                {
                    string prompt = "Preferred spellings and terms: " + String.Join(", ", settings.Dictionary.Take(80).ToArray());
                    form.Add(new StringContent(prompt), "prompt");
                }
                HttpResponseMessage response = await client.PostAsync(url, form);
                string body = await response.Content.ReadAsStringAsync();
                if (!response.IsSuccessStatusCode) throw new InvalidOperationException(ApiHelpers.ErrorMessage(body, response.StatusCode));
                Dictionary<string, object> value = serializer.DeserializeObject(body) as Dictionary<string, object>;
                if (value == null || !value.ContainsKey("text")) throw new InvalidOperationException("The transcription response did not contain text.");
                return Convert.ToString(value["text"], CultureInfo.InvariantCulture).Trim();
            }
        }

        public async Task<string> CleanupAsync(string raw, ForegroundInfo context, AppSettings settings, string apiKey)
        {
            string instructions =
                "You are the final cleanup stage for push-to-talk dictation. Return only the text to insert—no quotes, preface, or commentary. " +
                "Preserve the speaker's meaning, facts, names, tone, and level of certainty. Remove filler words and abandoned false starts. " +
                "Apply spoken self-corrections using the final intended wording. Add punctuation and paragraph breaks. " +
                "Infer structure from speech patterns: when ideas are enumerated or delivered as distinct points, format them as Markdown bullets or numbers even if the speaker did not literally say 'bullet point'. " +
                "Use natural em dashes for genuine asides or sharp pivots, but do not overuse them. Match the target app: short conversational text in chat, polished prose in documents/email, and exact tokens in developer tools. " +
                "Expand configured snippets and use preferred spellings. Do not invent information. Do not answer the dictated text. " +
                "For code, commands, URLs, identifiers, or quoted wording, preserve exact tokens. Style: " + settings.Style + ".";

            StringBuilder input = new StringBuilder();
            input.AppendLine("DICTATION:");
            input.AppendLine(raw);
            if (settings.ContextEnabled && context != null)
            {
                input.AppendLine();
                input.AppendLine("TARGET APPLICATION: " + context.AppLabel);
                if (!String.IsNullOrWhiteSpace(context.Title)) input.AppendLine("WINDOW TITLE: " + context.Title);
            }
            if (settings.Dictionary.Count > 0) input.AppendLine("PREFERRED TERMS: " + String.Join("; ", settings.Dictionary.Take(100).ToArray()));
            if (settings.Snippets.Count > 0)
            {
                input.AppendLine("SNIPPETS:");
                foreach (KeyValuePair<string, string> pair in settings.Snippets.Take(50))
                    input.AppendLine(pair.Key + " => " + pair.Value);
            }

            Dictionary<string, object> payload = new Dictionary<string, object>();
            payload["model"] = settings.CleanupModel;
            payload["instructions"] = instructions;
            payload["input"] = input.ToString();
            payload["max_output_tokens"] = 3000;
            payload["store"] = false;
            string json = serializer.Serialize(payload);
            using (HttpClient client = Client(apiKey))
            using (StringContent content = new StringContent(json, Encoding.UTF8, "application/json"))
            {
                HttpResponseMessage response = await client.PostAsync(settings.ApiBaseUrl.TrimEnd('/') + "/responses", content);
                string body = await response.Content.ReadAsStringAsync();
                if (!response.IsSuccessStatusCode) throw new InvalidOperationException(ApiHelpers.ErrorMessage(body, response.StatusCode));
                string text = ApiHelpers.ExtractText(serializer.DeserializeObject(body));
                if (String.IsNullOrWhiteSpace(text)) throw new InvalidOperationException("The cleanup response did not contain text.");
                return text.Trim();
            }
        }

        public async Task TestAsync(AppSettings settings, string apiKey)
        {
            if (String.IsNullOrWhiteSpace(apiKey)) throw new InvalidOperationException("Enter an API key first.");
            using (HttpClient client = Client(apiKey))
            {
                HttpResponseMessage response = await client.GetAsync(settings.ApiBaseUrl.TrimEnd('/') + "/models");
                string body = await response.Content.ReadAsStringAsync();
                if (!response.IsSuccessStatusCode) throw new InvalidOperationException(ApiHelpers.ErrorMessage(body, response.StatusCode));
            }
        }
    }

    public sealed class OllamaEngine
    {
        private readonly JavaScriptSerializer serializer = new JavaScriptSerializer();

        public async Task<string> CleanupAsync(string raw, ForegroundInfo context, AppSettings settings)
        {
            if (String.IsNullOrWhiteSpace(settings.OllamaModel)) return TextProcessor.Clean(raw, settings, context);
            string prompt =
                "Clean the following voice dictation for insertion into " + (context == null ? "an app" : context.AppLabel) + ". " +
                "Return only the cleaned text. Preserve meaning and tone; remove fillers and false starts; honor corrections; add punctuation; " +
                "infer lists from the way points are spoken and format them as bullets or numbers; use em dashes for natural asides; adapt to the target app; " +
                "never answer or comment on the dictation. Style: " + settings.Style + ".\n\n" + raw;
            Dictionary<string, object> payload = new Dictionary<string, object>();
            payload["model"] = settings.OllamaModel;
            payload["prompt"] = prompt;
            payload["stream"] = false;
            using (HttpClient client = new HttpClient())
            using (StringContent content = new StringContent(serializer.Serialize(payload), Encoding.UTF8, "application/json"))
            {
                client.Timeout = TimeSpan.FromMinutes(5);
                HttpResponseMessage response = await client.PostAsync(settings.OllamaUrl.TrimEnd('/') + "/api/generate", content);
                string body = await response.Content.ReadAsStringAsync();
                if (!response.IsSuccessStatusCode) throw new InvalidOperationException(ApiHelpers.ErrorMessage(body, response.StatusCode));
                Dictionary<string, object> value = serializer.DeserializeObject(body) as Dictionary<string, object>;
                if (value == null || !value.ContainsKey("response")) throw new InvalidOperationException("Ollama returned no text.");
                return Convert.ToString(value["response"], CultureInfo.InvariantCulture).Trim();
            }
        }
    }

    public sealed class OpenRouterEngine
    {
        private readonly JavaScriptSerializer serializer = new JavaScriptSerializer();

        private HttpClient Client(string apiKey)
        {
            HttpClient client = new HttpClient();
            client.Timeout = TimeSpan.FromMinutes(5);
            string key = (apiKey ?? "").Trim();
            if (key.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase)) key = key.Substring(7).Trim();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", key);
            client.DefaultRequestHeaders.UserAgent.ParseAdd("Flowtype-Desktop/1.3.21");
            client.DefaultRequestHeaders.Add("X-OpenRouter-Title", "Flowtype Desktop");
            return client;
        }

        private static string EffectiveModel(AppSettings settings)
        {
            string model = settings == null ? "" : settings.OpenRouterModel;
            if (String.IsNullOrWhiteSpace(model) || String.Equals(model, "google/gemma-4-26b-a4b-it:free", StringComparison.OrdinalIgnoreCase))
                return "openrouter/free";
            return model.Trim();
        }

        private static string ChatUrl(AppSettings settings)
        {
            string value = settings == null ? "" : (settings.OpenRouterUrl ?? "").Trim();
            if (String.IsNullOrWhiteSpace(value)) value = "https://openrouter.ai/api/v1";
            value = value.TrimEnd('/');
            if (value.EndsWith("/chat/completions", StringComparison.OrdinalIgnoreCase)) return value;
            if (String.Equals(value, "https://openrouter.ai", StringComparison.OrdinalIgnoreCase)) value += "/api/v1";
            return value + "/chat/completions";
        }

        public async Task<string> CleanupAsync(string raw, ForegroundInfo context, AppSettings settings, string apiKey)
        {
            if (String.IsNullOrWhiteSpace(apiKey)) throw new InvalidOperationException("OpenRouter cleanup needs an API key.");
            string system =
                "You clean push-to-talk dictation. Return only text to insert, with no preface. Preserve meaning, names, tone, facts, and uncertainty. " +
                "Remove fillers and abandoned starts, honor the speaker's final self-correction, add punctuation and paragraphs, and format spoken enumerations as bullets or numbers. " +
                "Infer lists from rhythm and enumerated ideas even when the speaker does not literally say 'bullet point'. Use natural em dashes for real asides or pivots without overusing them. " +
                "Adapt to the target app: concise conversational text in chat, polished prose in email/documents, exact tokens in developer tools. " +
                "Never answer the dictation or invent information. Preserve exact code, URLs, commands, and identifiers. Style: " + settings.Style + ".";
            StringBuilder user = new StringBuilder(raw);
            if (settings.ContextEnabled && context != null)
            {
                user.AppendLine();
                user.AppendLine();
                user.AppendLine("Target app: " + context.AppLabel);
                if (!String.IsNullOrWhiteSpace(context.Title)) user.AppendLine("Window: " + context.Title);
            }
            if (settings.Dictionary.Count > 0) user.AppendLine("Preferred terms: " + String.Join("; ", settings.Dictionary.Take(100).ToArray()));
            if (settings.Snippets.Count > 0)
            {
                user.AppendLine("Voice snippets:");
                foreach (KeyValuePair<string, string> pair in settings.Snippets.Take(50)) user.AppendLine(pair.Key + " => " + pair.Value);
            }

            object[] messages = new object[]
            {
                new Dictionary<string, object> { { "role", "system" }, { "content", system } },
                new Dictionary<string, object> { { "role", "user" }, { "content", user.ToString() } }
            };
            Dictionary<string, object> payload = new Dictionary<string, object>();
            payload["model"] = EffectiveModel(settings);
            payload["messages"] = messages;
            payload["max_tokens"] = 3000;
            payload["temperature"] = 0.1;
            using (HttpClient client = Client(apiKey))
            using (StringContent content = new StringContent(serializer.Serialize(payload), Encoding.UTF8, "application/json"))
            {
                // Smart polish is optional. It must never hold dictation hostage.
                client.Timeout = TimeSpan.FromMilliseconds(1800);
                HttpResponseMessage response = await client.PostAsync(ChatUrl(settings), content);
                string body = await response.Content.ReadAsStringAsync();
                if (!response.IsSuccessStatusCode) throw new InvalidOperationException(ApiHelpers.ErrorMessage(body, response.StatusCode));
                Dictionary<string, object> root = serializer.DeserializeObject(body) as Dictionary<string, object>;
                object choicesValue;
                if (root != null && root.TryGetValue("choices", out choicesValue))
                {
                    IEnumerable choices = choicesValue as IEnumerable;
                    if (choices != null)
                    {
                        foreach (object choiceValue in choices)
                        {
                            Dictionary<string, object> choice = choiceValue as Dictionary<string, object>;
                            object messageValue;
                            if (choice == null || !choice.TryGetValue("message", out messageValue)) continue;
                            Dictionary<string, object> message = messageValue as Dictionary<string, object>;
                            object textValue;
                            if (message != null && message.TryGetValue("content", out textValue))
                            {
                                string text = Convert.ToString(textValue, CultureInfo.InvariantCulture).Trim();
                                if (text.Length > 0) return text;
                            }
                        }
                    }
                }
                throw new InvalidOperationException("OpenRouter returned no cleanup text.");
            }
        }

        public async Task<string> TestAsync(AppSettings settings, string apiKey)
        {
            if (String.IsNullOrWhiteSpace(apiKey)) throw new InvalidOperationException("Enter an OpenRouter API key first.");
            object[] messages = new object[]
            {
                new Dictionary<string, object> { { "role", "user" }, { "content", "Reply with OK." } }
            };
            Dictionary<string, object> payload = new Dictionary<string, object>();
            payload["model"] = EffectiveModel(settings);
            payload["messages"] = messages;
            payload["max_tokens"] = 8;
            payload["temperature"] = 0;
            using (HttpClient client = Client(apiKey))
            using (StringContent content = new StringContent(serializer.Serialize(payload), Encoding.UTF8, "application/json"))
            {
                HttpResponseMessage response = await client.PostAsync(ChatUrl(settings), content);
                string body = await response.Content.ReadAsStringAsync();
                if (!response.IsSuccessStatusCode) throw new InvalidOperationException(ApiHelpers.ErrorMessage(body, response.StatusCode));
                Dictionary<string, object> root = serializer.DeserializeObject(body) as Dictionary<string, object>;
                string resolvedModel = root != null && root.ContainsKey("model") ? Convert.ToString(root["model"], CultureInfo.InvariantCulture) : "";
                return String.IsNullOrWhiteSpace(resolvedModel) ? EffectiveModel(settings) : resolvedModel;
            }
        }
    }

    public sealed class GroqEngine : IDisposable
    {
        private readonly JavaScriptSerializer serializer = new JavaScriptSerializer();
        private HttpClient client;
        private string boundKey = "";

        private HttpClient GetClient(string apiKey)
        {
            string key = apiKey ?? "";
            if (client != null && String.Equals(boundKey, key, StringComparison.Ordinal)) return client;
            if (client != null) client.Dispose();
            client = new HttpClient();
            client.Timeout = TimeSpan.FromSeconds(90);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", key);
            client.DefaultRequestHeaders.UserAgent.ParseAdd("Flowtype-Desktop/1.3.21");
            boundKey = key;
            return client;
        }

        public async Task WarmAsync(AppSettings settings, string apiKey)
        {
            if (String.IsNullOrWhiteSpace(apiKey)) return;
            string url = settings.GroqApiUrl.TrimEnd('/') + "/models";
            using (HttpResponseMessage response = await GetClient(apiKey).GetAsync(url))
            {
                if (!response.IsSuccessStatusCode)
                {
                    string body = await response.Content.ReadAsStringAsync();
                    throw new InvalidOperationException(ApiHelpers.ErrorMessage(body, response.StatusCode));
                }
            }
        }

        public async Task<SpeechTranscript> TranscribeAsync(string wavePath, AppSettings settings, string apiKey, ForegroundInfo context)
        {
            if (String.IsNullOrWhiteSpace(apiKey))
                throw new InvalidOperationException("Groq mode needs an API key. Get a free key at console.groq.com, then paste it in Settings.");
            string url = settings.GroqApiUrl.TrimEnd('/') + "/audio/transcriptions";
            string model = String.IsNullOrWhiteSpace(settings.GroqTranscriptionModel) ? "whisper-large-v3-turbo" : settings.GroqTranscriptionModel.Trim();
            HttpClient http = GetClient(apiKey);
            using (MultipartFormDataContent form = new MultipartFormDataContent())
            using (FileStream stream = File.OpenRead(wavePath))
            using (StreamContent audio = new StreamContent(stream))
            {
                audio.Headers.ContentType = new MediaTypeHeaderValue("audio/wav");
                form.Add(audio, "file", Path.GetFileName(wavePath));
                form.Add(new StringContent(model), "model");
                form.Add(new StringContent("json"), "response_format");
                form.Add(new StringContent("en"), "language");
                form.Add(new StringContent("0"), "temperature");
                string prompt = WhisperEngine.BuildPrompt(settings, context);
                if (prompt.Length > 0) form.Add(new StringContent(prompt), "prompt");
                HttpResponseMessage response = await http.PostAsync(url, form);
                string body = await response.Content.ReadAsStringAsync();
                if (!response.IsSuccessStatusCode) throw new InvalidOperationException(ApiHelpers.ErrorMessage(body, response.StatusCode));
                Dictionary<string, object> value = serializer.DeserializeObject(body) as Dictionary<string, object>;
                if (value == null || !value.ContainsKey("text"))
                    throw new InvalidOperationException("Groq returned no transcription text.");
                SpeechTranscript transcript = new SpeechTranscript();
                transcript.Text = Convert.ToString(value["text"], CultureInfo.InvariantCulture).Trim();
                if (String.IsNullOrWhiteSpace(transcript.Text)) throw new InvalidOperationException("No speech was detected.");
                return transcript;
            }
        }

        public async Task TestAsync(AppSettings settings, string apiKey)
        {
            if (String.IsNullOrWhiteSpace(apiKey)) throw new InvalidOperationException("Enter a Groq API key first.");
            await WarmAsync(settings, apiKey);
        }

        public void Dispose()
        {
            if (client != null)
            {
                client.Dispose();
                client = null;
                boundKey = "";
            }
        }
    }

    public sealed class SpeechSegment
    {
        public string Text = "";
        public double Start;
        public double End;
    }

    public sealed class SpeechWord
    {
        public string Text = "";
        public double Start;
        public double End;
    }

    public sealed class SpeechTranscript
    {
        public string Text = "";
        public List<SpeechSegment> Segments = new List<SpeechSegment>();
        public List<SpeechWord> Words = new List<SpeechWord>();
    }

    public sealed class WhisperEngine : IDisposable
    {
        private readonly JavaScriptSerializer serializer = new JavaScriptSerializer();
        private readonly SemaphoreSlim serverGate = new SemaphoreSlim(1, 1);
        private Process server;
        private int serverPort;
        private string loadedModel = "";
        private bool disposed;

        public async Task WarmAsync(AppSettings settings)
        {
            if (!CanUseServer(settings)) return;
            await EnsureServerAsync(settings);
        }

        public void Unload()
        {
            StopServer();
        }

        public async Task<SpeechTranscript> TranscribeAsync(string wavePath, AppSettings settings, ForegroundInfo context)
        {
            Validate(settings);
            if (CanUseServer(settings))
            {
                try
                {
                    await EnsureServerAsync(settings);
                    return await TranscribeWithServerAsync(wavePath, settings, context);
                }
                catch
                {
                    StopServer();
                }
            }
            return await RunCliAsync(wavePath, settings, context);
        }

        private static void Validate(AppSettings settings)
        {
            if (String.IsNullOrWhiteSpace(settings.WhisperExePath) || !File.Exists(settings.WhisperExePath) ||
                String.IsNullOrWhiteSpace(settings.WhisperModelPath) || !File.Exists(settings.WhisperModelPath))
                throw new InvalidOperationException("The local engine is not installed. Open Flowtype Settings, choose Local, and click Install local engine.");
        }

        private static bool CanUseServer(AppSettings settings)
        {
            return settings != null && !String.IsNullOrWhiteSpace(settings.WhisperServerPath) && File.Exists(settings.WhisperServerPath) &&
                !String.IsNullOrWhiteSpace(settings.WhisperModelPath) && File.Exists(settings.WhisperModelPath);
        }

        private async Task EnsureServerAsync(AppSettings settings)
        {
            await serverGate.WaitAsync();
            try
            {
                if (disposed) throw new ObjectDisposedException("WhisperEngine");
                if (server != null && !server.HasExited && String.Equals(loadedModel, settings.WhisperModelPath, StringComparison.OrdinalIgnoreCase)) return;
                StopServer();
                serverPort = FindFreePort();
                int threads = Math.Max(2, Math.Min(16, Environment.ProcessorCount - 1));
                ProcessStartInfo start = new ProcessStartInfo();
                start.FileName = settings.WhisperServerPath;
                start.Arguments = "-m " + Quote(settings.WhisperModelPath) + " --host 127.0.0.1 --port " +
                    serverPort.ToString(CultureInfo.InvariantCulture) + " -t " + threads.ToString(CultureInfo.InvariantCulture) + " -nt -nc -l en -fa";
                start.WorkingDirectory = Path.GetDirectoryName(settings.WhisperServerPath);
                start.CreateNoWindow = true;
                start.UseShellExecute = false;
                server = Process.Start(start);
                try { server.PriorityClass = ProcessPriorityClass.AboveNormal; } catch { }
                loadedModel = settings.WhisperModelPath;

                DateTime deadline = DateTime.UtcNow.AddMinutes(2);
                Exception last = null;
                while (DateTime.UtcNow < deadline)
                {
                    if (server == null || server.HasExited) throw new InvalidOperationException("The local speech service stopped while loading the model.");
                    try
                    {
                        using (TcpClient probe = new TcpClient())
                        {
                            Task connect = probe.ConnectAsync(IPAddress.Loopback, serverPort);
                            Task finished = await Task.WhenAny(connect, Task.Delay(250));
                            if (finished == connect && probe.Connected) return;
                        }
                    }
                    catch (Exception exception) { last = exception; }
                    await Task.Delay(120);
                }
                throw new TimeoutException("The local speech model took too long to load." + (last == null ? "" : " " + last.Message));
            }
            finally { serverGate.Release(); }
        }

        private async Task<SpeechTranscript> TranscribeWithServerAsync(string wavePath, AppSettings settings, ForegroundInfo context)
        {
            bool turbo = settings.TurboTranscription;
            using (HttpClient client = new HttpClient())
            using (MultipartFormDataContent form = new MultipartFormDataContent())
            using (FileStream stream = File.OpenRead(wavePath))
            using (StreamContent audio = new StreamContent(stream))
            {
                client.Timeout = TimeSpan.FromSeconds(turbo ? 45 : 90);
                audio.Headers.ContentType = new MediaTypeHeaderValue("audio/wav");
                form.Add(audio, "file", Path.GetFileName(wavePath));
                form.Add(new StringContent("0.0"), "temperature");
                form.Add(new StringContent("0.0"), "temperature_inc");
                form.Add(new StringContent("1"), "best_of");
                form.Add(new StringContent("1"), "beam_size");
                if (turbo)
                {
                    form.Add(new StringContent("json"), "response_format");
                }
                else
                {
                    form.Add(new StringContent("false"), "no_timestamps");
                    form.Add(new StringContent("true"), "token_timestamps");
                    form.Add(new StringContent("true"), "no_language_probabilities");
                    form.Add(new StringContent("verbose_json"), "response_format");
                }
                form.Add(new StringContent(settings.SuppressNonSpeech ? "true" : "false"), "suppress_non_speech");
                string prompt = BuildPrompt(settings, context);
                if (prompt.Length > 0)
                {
                    form.Add(new StringContent(prompt), "prompt");
                }
                HttpResponseMessage response = await client.PostAsync("http://127.0.0.1:" + serverPort.ToString(CultureInfo.InvariantCulture) + "/inference", form);
                string body = await response.Content.ReadAsStringAsync();
                if (!response.IsSuccessStatusCode) throw new InvalidOperationException("Local transcription service failed: " + response.StatusCode + ".");
                Dictionary<string, object> value = serializer.DeserializeObject(body) as Dictionary<string, object>;
                string text = value != null && value.ContainsKey("text") ? Convert.ToString(value["text"], CultureInfo.InvariantCulture).Trim() : "";
                if (String.IsNullOrWhiteSpace(text)) throw new InvalidOperationException("No speech was detected.");
                SpeechTranscript transcript = new SpeechTranscript();
                transcript.Text = text;
                if (turbo) return transcript;
                object segmentValue;
                IEnumerable segments = value != null && value.TryGetValue("segments", out segmentValue) ? segmentValue as IEnumerable : null;
                if (segments != null)
                {
                    foreach (object item in segments)
                    {
                        Dictionary<string, object> segment = item as Dictionary<string, object>;
                        if (segment == null || !segment.ContainsKey("text")) continue;
                        SpeechSegment phrase = new SpeechSegment();
                        phrase.Text = Convert.ToString(segment["text"], CultureInfo.InvariantCulture).Trim();
                        phrase.Start = segment.ContainsKey("start") ? Convert.ToDouble(segment["start"], CultureInfo.InvariantCulture) : 0;
                        phrase.End = segment.ContainsKey("end") ? Convert.ToDouble(segment["end"], CultureInfo.InvariantCulture) : phrase.Start;
                        if (phrase.Text.Length > 0) transcript.Segments.Add(phrase);
                        object wordValue;
                        IEnumerable words = segment.TryGetValue("words", out wordValue) ? wordValue as IEnumerable : null;
                        if (words == null) continue;
                        foreach (object wordItem in words)
                        {
                            Dictionary<string, object> word = wordItem as Dictionary<string, object>;
                            if (word == null || !word.ContainsKey("word") || !word.ContainsKey("start") || !word.ContainsKey("end")) continue;
                            SpeechWord spokenWord = new SpeechWord();
                            spokenWord.Text = Convert.ToString(word["word"], CultureInfo.InvariantCulture);
                            spokenWord.Start = Convert.ToDouble(word["start"], CultureInfo.InvariantCulture);
                            spokenWord.End = Convert.ToDouble(word["end"], CultureInfo.InvariantCulture);
                            if (!String.IsNullOrWhiteSpace(spokenWord.Text)) transcript.Words.Add(spokenWord);
                        }
                    }
                }
                return transcript;
            }
        }

        private static Task<SpeechTranscript> RunCliAsync(string wavePath, AppSettings settings, ForegroundInfo context)
        {
            return Task.Run(delegate
            {
                string outputBase = Path.Combine(Path.GetTempPath(), "flowtype-transcript-" + Guid.NewGuid().ToString("N"));
                int threads = Math.Max(2, Math.Min(16, Environment.ProcessorCount - 1));
                string prompt = BuildPrompt(settings, context);
                ProcessStartInfo start = new ProcessStartInfo();
                start.FileName = settings.WhisperExePath;
                start.Arguments = "-m " + Quote(settings.WhisperModelPath) + " -f " + Quote(wavePath) + " -otxt -of " + Quote(outputBase) +
                    " -nt -l en -fa -t " + threads.ToString(CultureInfo.InvariantCulture) + (prompt.Length == 0 ? "" : " --prompt " + Quote(prompt));
                start.WorkingDirectory = Path.GetDirectoryName(settings.WhisperExePath);
                start.CreateNoWindow = true;
                start.UseShellExecute = false;
                start.RedirectStandardError = true;
                start.RedirectStandardOutput = true;
                using (Process process = Process.Start(start))
                {
                    Task<string> errorRead = process.StandardError.ReadToEndAsync();
                    Task<string> outputRead = process.StandardOutput.ReadToEndAsync();
                    if (!process.WaitForExit(10 * 60 * 1000))
                    {
                        try { process.Kill(); } catch { }
                        throw new TimeoutException("Local transcription took longer than ten minutes.");
                    }
                    Task.WaitAll(new Task[] { errorRead, outputRead }, 5000);
                    string standardError = errorRead.IsCompleted ? errorRead.Result : "";
                    string outputPath = outputBase + ".txt";
                    if (process.ExitCode != 0 || !File.Exists(outputPath))
                        throw new InvalidOperationException("Local transcription failed. " + Tail(standardError, 500));
                    string text = File.ReadAllText(outputPath, Encoding.UTF8).Trim();
                    try { File.Delete(outputPath); } catch { }
                    if (String.IsNullOrWhiteSpace(text)) throw new InvalidOperationException("No speech was detected.");
                    SpeechTranscript transcript = new SpeechTranscript();
                    transcript.Text = text;
                    return transcript;
                }
            });
        }

        public static string BuildPrompt(AppSettings settings, ForegroundInfo context)
        {
            List<string> terms = new List<string>();
            foreach (string entry in settings.Dictionary.Take(80))
            {
                string value = entry ?? "";
                int split = value.IndexOf("=>", StringComparison.Ordinal);
                if (split >= 0) value = value.Substring(split + 2).Trim();
                else
                {
                    split = value.IndexOf('=');
                    if (split >= 0) value = value.Substring(split + 1).Trim();
                }
                if (value.Length > 0) terms.Add(value);
            }
            StringBuilder prompt = new StringBuilder();
            if (terms.Count > 0) prompt.Append(String.Join(", ", terms.Distinct(StringComparer.OrdinalIgnoreCase).ToArray()) + ".");
            // Window title is passed to LLM cleanup only — including "Target window:" here
            // makes Whisper echo it into the transcript on longer clips.
            return prompt.ToString().Replace("\"", "'").Trim();
        }

        private static int FindFreePort()
        {
            TcpListener listener = new TcpListener(IPAddress.Loopback, 0);
            listener.Start();
            int port = ((IPEndPoint)listener.LocalEndpoint).Port;
            listener.Stop();
            return port;
        }

        private void StopServer()
        {
            if (server != null)
            {
                try { if (!server.HasExited) server.Kill(); } catch { }
                try { server.Dispose(); } catch { }
            }
            server = null;
            loadedModel = "";
            serverPort = 0;
        }

        public void Dispose()
        {
            disposed = true;
            StopServer();
            serverGate.Dispose();
        }

        private static string Quote(string value) { return "\"" + value.Replace("\"", "\\\"") + "\""; }
        private static string Tail(string value, int length)
        {
            if (String.IsNullOrWhiteSpace(value)) return "";
            return value.Length <= length ? value : value.Substring(value.Length - length);
        }
    }

    public sealed class LocalInstallResult
    {
        public string Executable;
        public string ServerExecutable;
        public string Model;
    }

    public sealed class LocalEngineInstaller
    {
        private readonly string appDirectory;

        public LocalEngineInstaller(string appDirectory) { this.appDirectory = appDirectory; }

        public async Task<LocalInstallResult> InstallAsync(Action<int, string> progress)
        {
            return await InstallAsync("Instant", progress);
        }

        public async Task<LocalInstallResult> InstallAsync(string quality, Action<int, string> progress)
        {
            string root = Path.Combine(appDirectory, "tools", "whisper");
            string modelRoot = Path.Combine(root, "models");
            Directory.CreateDirectory(root);
            Directory.CreateDirectory(modelRoot);
            string zipPath = Path.Combine(root, "whisper-win-x64.zip");
            string modelName = "ggml-base.en-q5_1.bin";
            string modelPath = Path.Combine(modelRoot, modelName);
            long minimumModelBytes = 50000000L;

            string bundledRoot = Path.Combine(root, "bundled-bin");
            string executable = Directory.Exists(bundledRoot)
                ? Directory.GetFiles(bundledRoot, "whisper-cli.exe", SearchOption.AllDirectories).FirstOrDefault() : "";
            string serverExecutable = Directory.Exists(bundledRoot)
                ? Directory.GetFiles(bundledRoot, "whisper-server.exe", SearchOption.AllDirectories).FirstOrDefault() : "";
            if (String.IsNullOrWhiteSpace(executable))
            {
                progress(1, "Finding the current official Windows build…");
                List<string> binaryUrls = new List<string>
                {
                    "https://github.com/ggml-org/whisper.cpp/releases/latest/download/whisper-bin-x64.zip",
                    "https://github.com/ggml-org/whisper.cpp/releases/download/v1.9.1/whisper-bin-x64.zip"
                };
                await DownloadFromOfficialSources(binaryUrls, zipPath, 1000000L, 2, 20, "Downloading whisper.cpp…", progress);
                string binRoot = Path.Combine(root, "bin-" + DateTime.UtcNow.ToString("yyyyMMddHHmmss", CultureInfo.InvariantCulture) + "-" + Guid.NewGuid().ToString("N").Substring(0, 6));
                Directory.CreateDirectory(binRoot);
                ZipFile.ExtractToDirectory(zipPath, binRoot);
                executable = Directory.GetFiles(binRoot, "whisper-cli.exe", SearchOption.AllDirectories).FirstOrDefault();
                if (String.IsNullOrWhiteSpace(executable)) throw new InvalidOperationException("The official archive did not contain whisper-cli.exe.");
                serverExecutable = Directory.GetFiles(binRoot, "whisper-server.exe", SearchOption.AllDirectories).FirstOrDefault();
            }
            else progress(20, "Bundled whisper.cpp engine is ready.");

            if (!File.Exists(modelPath) || new FileInfo(modelPath).Length < minimumModelBytes)
            {
                List<string> modelUrls = new List<string>
                {
                    "https://huggingface.co/ggerganov/whisper.cpp/resolve/main/" + modelName + "?download=true",
                    "https://huggingface.co/ggerganov/whisper.cpp/resolve/main/" + modelName,
                    "https://hf.co/ggerganov/whisper.cpp/resolve/main/" + modelName + "?download=true"
                };
                await DownloadFromOfficialSources(modelUrls, modelPath, minimumModelBytes, 20, 100,
                    "Downloading Instant speech model…", progress);
            }
            if (!File.Exists(modelPath) || new FileInfo(modelPath).Length < minimumModelBytes)
                throw new InvalidOperationException("The speech-model download was incomplete. Retry to resume it, or choose an existing .bin model in Settings.");
            progress(100, "Local engine installed.");
            try { File.Delete(zipPath); } catch { }
            LocalInstallResult result = new LocalInstallResult();
            result.Executable = executable;
            result.ServerExecutable = serverExecutable ?? "";
            result.Model = modelPath;
            return result;
        }

        private static async Task DownloadFromOfficialSources(IEnumerable<string> urls, string path, long minimumBytes,
            int from, int to, string label, Action<int, string> progress)
        {
            List<string> errors = new List<string>();
            foreach (string url in urls.Where(value => !String.IsNullOrWhiteSpace(value)).Distinct(StringComparer.OrdinalIgnoreCase))
            {
                try
                {
                    await DownloadResumable(url, path, minimumBytes, from, to, label, progress);
                    return;
                }
                catch (Exception exception)
                {
                    Uri source;
                    string host = Uri.TryCreate(url, UriKind.Absolute, out source) ? source.Host : "download source";
                    errors.Add(host + ": " + exception.Message);
                    try { File.Delete(path + ".part"); } catch { }
                }
            }
            throw new InvalidOperationException("All official download sources failed. " + String.Join(" | ", errors.ToArray()) +
                " You can retry, or use the manual file buttons in Local settings.");
        }

        private static async Task DownloadResumable(string url, string path, long minimumBytes,
            int from, int to, string label, Action<int, string> progress)
        {
            string partialPath = path + ".part";
            if (File.Exists(path) && new FileInfo(path).Length < minimumBytes && !File.Exists(partialPath))
                File.Move(path, partialPath);

            Exception last = null;
            for (int attempt = 1; attempt <= 3; attempt++)
            {
                try
                {
                    long existing = File.Exists(partialPath) ? new FileInfo(partialPath).Length : 0;
                    using (HttpClient client = new HttpClient(new HttpClientHandler { AllowAutoRedirect = true }))
                    using (HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, url))
                    {
                        client.Timeout = TimeSpan.FromMinutes(60);
                        client.DefaultRequestHeaders.UserAgent.ParseAdd("Flowtype-Desktop/1.3.21");
                        if (existing > 0) request.Headers.Range = new RangeHeaderValue(existing, null);
                        using (HttpResponseMessage response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead))
                        {
                            if (!response.IsSuccessStatusCode) throw new InvalidOperationException("HTTP " + (int)response.StatusCode + " " + response.ReasonPhrase);
                            bool append = existing > 0 && response.StatusCode == HttpStatusCode.PartialContent;
                            if (!append) existing = 0;
                            long contentBytes = response.Content.Headers.ContentLength ?? -1;
                            long expectedTotal = response.Content.Headers.ContentRange != null && response.Content.Headers.ContentRange.Length.HasValue
                                ? response.Content.Headers.ContentRange.Length.Value
                                : (contentBytes > 0 ? existing + contentBytes : -1);
                            progress(from, label + (existing > 0 ? " resuming…" : " attempt " + attempt.ToString(CultureInfo.InvariantCulture) + "…"));
                            using (Stream input = await response.Content.ReadAsStreamAsync())
                            using (FileStream output = new FileStream(partialPath, append ? FileMode.Append : FileMode.Create,
                                FileAccess.Write, FileShare.None, 131072, true))
                            {
                                byte[] buffer = new byte[131072];
                                long received = existing;
                                int read;
                                while ((read = await input.ReadAsync(buffer, 0, buffer.Length)) > 0)
                                {
                                    await output.WriteAsync(buffer, 0, read);
                                    received += read;
                                    int percent = expectedTotal > 0 ? from + (int)((to - from) * received / expectedTotal) : from;
                                    progress(Math.Max(from, Math.Min(to, percent)), label);
                                }
                            }
                            long finalBytes = new FileInfo(partialPath).Length;
                            if (expectedTotal > 0 && finalBytes < expectedTotal)
                                throw new EndOfStreamException("Connection ended at " + finalBytes.ToString("N0", CultureInfo.InvariantCulture) +
                                    " of " + expectedTotal.ToString("N0", CultureInfo.InvariantCulture) + " bytes.");
                            if (finalBytes < minimumBytes)
                                throw new InvalidDataException("Downloaded only " + finalBytes.ToString("N0", CultureInfo.InvariantCulture) + " bytes.");
                            if (File.Exists(path)) File.Delete(path);
                            File.Move(partialPath, path);
                            progress(to, label);
                            return;
                        }
                    }
                }
                catch (Exception exception)
                {
                    last = exception;
                }
                if (attempt < 3)
                {
                    progress(from, label + " retrying…");
                    await Task.Delay(700 * attempt);
                }
            }
            throw last ?? new InvalidOperationException("Download failed.");
        }
    }

    public sealed class RecordingOverlay : Form
    {
        [StructLayout(LayoutKind.Sequential)]
        private struct NativePoint
        {
            public int X;
            public int Y;
            public NativePoint(int x, int y) { X = x; Y = y; }
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct NativeSize
        {
            public int Width;
            public int Height;
            public NativeSize(int width, int height) { Width = width; Height = height; }
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        private struct BlendFunction
        {
            public byte BlendOp;
            public byte BlendFlags;
            public byte SourceConstantAlpha;
            public byte AlphaFormat;
        }

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool UpdateLayeredWindow(IntPtr window, IntPtr destinationDc, ref NativePoint destination,
            ref NativeSize size, IntPtr sourceDc, ref NativePoint source, int colorKey, ref BlendFunction blend, int flags);
        [DllImport("user32.dll")]
        private static extern IntPtr GetDC(IntPtr window);
        [DllImport("user32.dll")]
        private static extern int ReleaseDC(IntPtr window, IntPtr dc);
        [DllImport("gdi32.dll")]
        private static extern IntPtr CreateCompatibleDC(IntPtr dc);
        [DllImport("gdi32.dll")]
        private static extern bool DeleteDC(IntPtr dc);
        [DllImport("gdi32.dll")]
        private static extern IntPtr SelectObject(IntPtr dc, IntPtr value);
        [DllImport("gdi32.dll")]
        private static extern bool DeleteObject(IntPtr value);

        private const int LayeredAlpha = 0x00000002;
        private const byte SourceAlpha = 0x01;
        private readonly System.Windows.Forms.Timer timer;
        private readonly Stopwatch elapsed = new Stopwatch();
        private float level;
        private readonly float[] bands = new float[13];
        private int animationTick;
        private bool maxRaised;
        private string theme = "Dark";
        private Bitmap glassBackdrop;
        private Point glassBackdropOffset;
        private float revealProgress = 1f;
        private bool exiting;
        private int overlaySession;
        private int pendingHideSession;
        private readonly Stopwatch revealClock = new Stopwatch();
        private const int RevealInMs = 130;
        private const int RevealOutMs = 100;
        public event Action MaximumDurationReached;

        public void SetTheme(string value)
        {
            theme = String.IsNullOrWhiteSpace(value) ? "Dark" : value.Trim();
        }

        public RecordingOverlay()
        {
            FormBorderStyle = FormBorderStyle.None;
            ShowInTaskbar = false;
            TopMost = true;
            Width = 108;
            Height = 36;
            timer = new System.Windows.Forms.Timer();
            timer.Interval = 32;
            timer.Tick += delegate
            {
                UpdateRevealAnimation();
                level *= 0.84f;
                for (int index = 0; index < bands.Length; index++) bands[index] *= 0.82f;
                animationTick++;
                if (elapsed.IsRunning && elapsed.Elapsed >= TimeSpan.FromMinutes(10) && !maxRaised)
                {
                    maxRaised = true;
                    Action handler = MaximumDurationReached;
                    if (handler != null) handler();
                }
                RenderLayered();
            };
        }

        protected override bool ShowWithoutActivation { get { return true; } }

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams value = base.CreateParams;
                // No-activate, tool window, click-through and per-pixel alpha.
                value.ExStyle |= 0x08000000 | 0x00000080 | 0x00000020 | 0x00080000;
                return value;
            }
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            PositionOverlay();
            RenderLayered();
        }

        private void PositionOverlay()
        {
            Rectangle area = Screen.FromPoint(Cursor.Position).WorkingArea;
            Location = new Point(area.Left + (area.Width - Width) / 2, area.Bottom - Height - 6);
        }

        private static GraphicsPath RoundedRectangle(RectangleF bounds, float radius)
        {
            float diameter = radius * 2;
            GraphicsPath path = new GraphicsPath();
            path.AddArc(bounds.Left, bounds.Top, diameter, diameter, 180, 90);
            path.AddArc(bounds.Right - diameter, bounds.Top, diameter, diameter, 270, 90);
            path.AddArc(bounds.Right - diameter, bounds.Bottom - diameter, diameter, diameter, 0, 90);
            path.AddArc(bounds.Left, bounds.Bottom - diameter, diameter, diameter, 90, 90);
            path.CloseFigure();
            return path;
        }

        public void ShowRecording(string hotkey, string overlayTheme)
        {
            overlaySession++;
            SetTheme(overlayTheme);
            level = 0;
            Array.Clear(bands, 0, bands.Length);
            animationTick = 0;
            elapsed.Restart();
            maxRaised = false;
            exiting = false;
            revealProgress = 0f;
            revealClock.Restart();
            timer.Start();
            PositionOverlay();
            if (IsGlassTheme()) CaptureGlassBackdrop();
            if (!Visible) Show();
            RenderLayered();
        }

        public void ShowProcessing() { HideNow(); }

        public void ShowResult(bool pasted) { HideNow(); }

        public void ShowFailure(string message) { HideNow(); }

        public void HideNow()
        {
            elapsed.Reset();
            maxRaised = false;
            pendingHideSession = overlaySession;
            exiting = true;
            revealClock.Restart();
            timer.Start();
            if (!Visible) Visible = true;
            RenderLayered();
        }

        public void EnsureHidden()
        {
            if (exiting) return;
            if (!Visible) return;
            overlaySession++;
            FinishHideImmediate();
        }

        private static float EaseOutCubic(float value)
        {
            float inverse = 1f - value;
            return 1f - inverse * inverse * inverse;
        }

        private static float EaseInQuad(float value)
        {
            return value * value;
        }

        private void UpdateRevealAnimation()
        {
            if (exiting)
            {
                if (pendingHideSession != overlaySession)
                {
                    exiting = false;
                    return;
                }
                float step = Math.Min(1f, revealClock.ElapsedMilliseconds / (float)RevealOutMs);
                revealProgress = 1f - EaseInQuad(step);
                if (step >= 1f || revealClock.ElapsedMilliseconds > RevealOutMs + 150)
                    FinishHide();
                return;
            }
            if (revealProgress >= 1f) return;
            float enterStep = Math.Min(1f, revealClock.ElapsedMilliseconds / (float)RevealInMs);
            revealProgress = EaseOutCubic(enterStep);
        }

        private void FinishHide()
        {
            if (!exiting || pendingHideSession != overlaySession) return;
            FinishHideImmediate();
        }

        private void FinishHideImmediate()
        {
            timer.Stop();
            ReleaseGlassBackdrop();
            exiting = false;
            revealProgress = 1f;
            Hide();
        }

        private RectangleF GetCapsuleBounds()
        {
            const float capsuleWidth = 94f;
            const float capsuleHeight = 26f;
            return new RectangleF(
                (Width - capsuleWidth) / 2f,
                (Height - capsuleHeight) / 2f,
                capsuleWidth,
                capsuleHeight);
        }

        private void ReleaseGlassBackdrop()
        {
            if (glassBackdrop == null) return;
            glassBackdrop.Dispose();
            glassBackdrop = null;
        }

        private void CaptureGlassBackdrop()
        {
            ReleaseGlassBackdrop();
            if (!IsHandleCreated) CreateHandle();

            RectangleF capsule = GetCapsuleBounds();
            const int pad = 10;
            int screenX = Left + (int)Math.Floor(capsule.X) - pad;
            int screenY = Top + (int)Math.Floor(capsule.Y) - pad;
            int captureWidth = (int)Math.Ceiling(capsule.Width) + pad * 2;
            int captureHeight = (int)Math.Ceiling(capsule.Height) + pad * 2;
            if (captureWidth < 2 || captureHeight < 2) return;

            bool restoreVisible = Visible;
            if (restoreVisible)
            {
                Visible = false;
                Application.DoEvents();
            }

            Bitmap raw = null;
            try
            {
                raw = new Bitmap(captureWidth, captureHeight, PixelFormat.Format32bppPArgb);
                using (Graphics captureGraphics = Graphics.FromImage(raw))
                    captureGraphics.CopyFromScreen(screenX, screenY, 0, 0, new Size(captureWidth, captureHeight), CopyPixelOperation.SourceCopy);

                using (Bitmap blurred = BlurBitmap(raw, 4))
                {
                    glassBackdrop = DistortLiquidGlass(blurred);
                }
                glassBackdropOffset = new Point(pad - (int)Math.Floor(capsule.X), pad - (int)Math.Floor(capsule.Y));
            }
            catch
            {
                ReleaseGlassBackdrop();
            }
            finally
            {
                if (raw != null) raw.Dispose();
                if (restoreVisible) Visible = true;
            }
        }

        private static Bitmap BlurBitmap(Bitmap source, int downscale)
        {
            int targetWidth = Math.Max(1, source.Width / downscale);
            int targetHeight = Math.Max(1, source.Height / downscale);
            using (Bitmap small = new Bitmap(targetWidth, targetHeight, PixelFormat.Format32bppPArgb))
            {
                using (Graphics down = Graphics.FromImage(small))
                {
                    down.InterpolationMode = InterpolationMode.HighQualityBilinear;
                    down.DrawImage(source, 0, 0, targetWidth, targetHeight);
                }
                Bitmap result = new Bitmap(source.Width, source.Height, PixelFormat.Format32bppPArgb);
                using (Graphics up = Graphics.FromImage(result))
                {
                    up.InterpolationMode = InterpolationMode.HighQualityBilinear;
                    up.DrawImage(small, 0, 0, source.Width, source.Height);
                }
                return result;
            }
        }

        private static Bitmap DistortLiquidGlass(Bitmap source)
        {
            int width = source.Width;
            int height = source.Height;
            Bitmap dest = new Bitmap(width, height, PixelFormat.Format32bppPArgb);
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    float nx = x * 0.11f;
                    float ny = y * 0.11f;
                    int offsetX = (int)(Math.Sin(nx * 1.7f + ny * 0.6f) * 2.2f + Math.Sin(ny * 2.3f) * 1.2f);
                    int offsetY = (int)(Math.Cos(ny * 1.5f + nx * 0.4f) * 2.2f + Math.Cos(nx * 2.1f) * 1.2f);
                    int sampleX = Math.Max(0, Math.Min(width - 1, x + offsetX));
                    int sampleY = Math.Max(0, Math.Min(height - 1, y + offsetY));
                    dest.SetPixel(x, y, source.GetPixel(sampleX, sampleY));
                }
            }
            return dest;
        }

        public void SetLevel(float value)
        {
            if (IsDisposed) return;
            if (InvokeRequired)
            {
                try { BeginInvoke(new Action<float>(SetLevel), value); } catch { }
                return;
            }
            float energy = Math.Min(1f, Math.Max(0f, value * 3.4f));
            level = energy > level ? level + (energy - level) * 0.82f : level + (energy - level) * 0.22f;
            int middle = bands.Length / 2;
            for (int index = 0; index < bands.Length; index++)
            {
                float distance = Math.Abs(index - middle) / (float)middle;
                float voiceShape = 0.78f + 0.10f * (float)Math.Sin(index * 1.2 + animationTick * 0.09);
                float target = energy * Math.Max(0.42f, voiceShape) * (1f - distance * 0.12f);
                float response = target > bands[index] ? 0.84f : 0.26f;
                bands[index] += (target - bands[index]) * response;
            }
            RenderLayered();
        }

        private void RenderLayered()
        {
            if (!IsHandleCreated || IsDisposed || !Visible) return;
            const float cornerRadius = 13f;
            const float barSpacing = 5f;
            const float barStroke = 2f;

            using (Bitmap bitmap = new Bitmap(Width, Height, PixelFormat.Format32bppPArgb))
            using (Graphics graphics = Graphics.FromImage(bitmap))
            {
                graphics.Clear(Color.Transparent);
                graphics.SmoothingMode = SmoothingMode.AntiAlias;
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                float slideY = (1f - revealProgress) * (exiting ? 4f : 5f);
                graphics.TranslateTransform(0f, slideY);

                RectangleF capsule = GetCapsuleBounds();

                if (IsGlassTheme())
                    DrawLiquidGlassCapsule(graphics, capsule, cornerRadius);
                else
                    DrawStandardCapsule(graphics, capsule, cornerRadius);

                float centerY = capsule.Y + capsule.Height / 2f;
                float barsSpan = (bands.Length - 1) * barSpacing;
                float startX = capsule.X + (capsule.Width - barsSpan) / 2f;
                for (int index = 0; index < bands.Length; index++)
                {
                    float sample = Math.Max(level * 0.04f, Math.Min(1f, bands[index]));
                    float barHeight = 2f + sample * (capsule.Height - 8f);
                    Color barColor = GetBarColor(sample);
                    float x = startX + index * barSpacing;
                    if (IsGlassTheme())
                    {
                        using (Pen shadow = new Pen(Color.FromArgb(48, 0, 0, 0), barStroke))
                        {
                            shadow.StartCap = LineCap.Round;
                            shadow.EndCap = LineCap.Round;
                            graphics.DrawLine(shadow, x, centerY - barHeight / 2f + 0.8f, x, centerY + barHeight / 2f + 0.8f);
                        }
                    }
                    using (Pen bar = new Pen(barColor, barStroke))
                    {
                        bar.StartCap = LineCap.Round;
                        bar.EndCap = LineCap.Round;
                        graphics.DrawLine(bar, x, centerY - barHeight / 2f, x, centerY + barHeight / 2f);
                    }
                }

                graphics.ResetTransform();
                Present(bitmap);
            }
        }

        private void DrawStandardCapsule(Graphics graphics, RectangleF capsule, float cornerRadius)
        {
            for (int spread = 4; spread >= 1; spread--)
            {
                RectangleF glow = new RectangleF(
                    capsule.X - spread * 0.3f,
                    capsule.Y - spread * 0.08f + 1f,
                    capsule.Width + spread * 0.6f,
                    capsule.Height + spread * 0.45f);
                using (GraphicsPath shadowPath = RoundedRectangle(glow, cornerRadius + spread * 0.2f))
                using (SolidBrush shadow = new SolidBrush(Color.FromArgb(4 + spread * 2, 0, 0, 0)))
                    graphics.FillPath(shadow, shadowPath);
            }

            Color top;
            Color bottom;
            Color borderColor;
            GetThemeColors(out top, out bottom, out borderColor);
            using (GraphicsPath capsulePath = RoundedRectangle(capsule, cornerRadius))
            {
                using (LinearGradientBrush surface = new LinearGradientBrush(capsule, top, bottom, LinearGradientMode.Vertical))
                    graphics.FillPath(surface, capsulePath);
                DrawMatteBorder(graphics, capsule, cornerRadius, capsulePath, borderColor);
            }
        }

        private void DrawMatteBorder(Graphics graphics, RectangleF capsule, float cornerRadius, GraphicsPath capsulePath, Color borderColor)
        {
            if (String.Equals(theme, "Dark", StringComparison.OrdinalIgnoreCase))
            {
                using (Pen outer = new Pen(Color.FromArgb(255, 63, 63, 70), 1.15f))
                    graphics.DrawPath(outer, capsulePath);
                RectangleF inset = new RectangleF(capsule.X + 1f, capsule.Y + 1f, capsule.Width - 2f, capsule.Height - 2f);
                using (GraphicsPath insetPath = RoundedRectangle(inset, cornerRadius - 1f))
                using (Pen inner = new Pen(Color.FromArgb(255, 18, 18, 20), 0.9f))
                    graphics.DrawPath(inner, insetPath);
                RectangleF highlight = new RectangleF(capsule.X + 2f, capsule.Y + 1.5f, capsule.Width - 4f, capsule.Height * 0.38f);
                using (GraphicsPath highlightPath = RoundedRectangle(highlight, cornerRadius - 2f))
                using (Pen topLine = new Pen(Color.FromArgb(36, 113, 113, 122), 0.7f))
                    graphics.DrawPath(topLine, highlightPath);
                return;
            }
            if (String.Equals(theme, "Purple", StringComparison.OrdinalIgnoreCase))
            {
                using (Pen outer = new Pen(Color.FromArgb(255, 92, 82, 122), 1.1f))
                    graphics.DrawPath(outer, capsulePath);
                RectangleF inset = new RectangleF(capsule.X + 1f, capsule.Y + 1f, capsule.Width - 2f, capsule.Height - 2f);
                using (GraphicsPath insetPath = RoundedRectangle(inset, cornerRadius - 1f))
                using (Pen inner = new Pen(Color.FromArgb(255, 36, 30, 52), 0.85f))
                    graphics.DrawPath(inner, insetPath);
                return;
            }
            if (String.Equals(theme, "Mono", StringComparison.OrdinalIgnoreCase))
            {
                using (Pen outer = new Pen(Color.FromArgb(255, 212, 212, 216), 1.1f))
                    graphics.DrawPath(outer, capsulePath);
                RectangleF inset = new RectangleF(capsule.X + 1f, capsule.Y + 1f, capsule.Width - 2f, capsule.Height - 2f);
                using (GraphicsPath insetPath = RoundedRectangle(inset, cornerRadius - 1f))
                using (Pen inner = new Pen(Color.FromArgb(255, 12, 12, 14), 0.85f))
                    graphics.DrawPath(inner, insetPath);
                return;
            }
            using (Pen border = new Pen(borderColor, 1f))
                graphics.DrawPath(border, capsulePath);
        }

        private void DrawLiquidGlassCapsule(Graphics graphics, RectangleF capsule, float cornerRadius)
        {
            // Outer shadows from the reference: 0 0 8px, 0 2px 6px, 0 0 12px
            for (int spread = 1; spread <= 6; spread++)
            {
                float yShift = spread <= 2 ? spread * 0.35f : 0f;
                RectangleF outer = new RectangleF(
                    capsule.X - spread * 0.45f,
                    capsule.Y - spread * 0.25f + yShift,
                    capsule.Width + spread * 0.9f,
                    capsule.Height + spread * 0.65f);
                int alpha = spread <= 2 ? 8 + spread * 6 : 3 + spread;
                using (GraphicsPath outerPath = RoundedRectangle(outer, cornerRadius + spread * 0.15f))
                using (SolidBrush outerBrush = new SolidBrush(Color.FromArgb(alpha, 0, 0, 0)))
                    graphics.FillPath(outerBrush, outerPath);
            }
            RectangleF glow = new RectangleF(capsule.X - 4f, capsule.Y - 3f, capsule.Width + 8f, capsule.Height + 6f);
            using (GraphicsPath glowPath = RoundedRectangle(glow, cornerRadius + 2f))
            using (SolidBrush glowBrush = new SolidBrush(Color.FromArgb(38, 0, 0, 0)))
                graphics.FillPath(glowBrush, glowPath);

            using (GraphicsPath capsulePath = RoundedRectangle(capsule, cornerRadius))
            {
                GraphicsState clipState = graphics.Save();
                graphics.SetClip(capsulePath);

                if (glassBackdrop != null)
                {
                    graphics.DrawImage(
                        glassBackdrop,
                        capsule.X + glassBackdropOffset.X,
                        capsule.Y + glassBackdropOffset.Y);
                }
                else
                {
                    using (SolidBrush fallback = new SolidBrush(Color.FromArgb(48, 28, 32, 42)))
                        graphics.FillPath(fallback, capsulePath);
                }

                using (SolidBrush veil = new SolidBrush(Color.FromArgb(18, 255, 255, 255)))
                    graphics.FillPath(veil, capsulePath);

                DrawGlassInsetShadows(graphics, capsule, cornerRadius);
                graphics.Restore(clipState);

                DrawGlassRim(graphics, capsule, cornerRadius, capsulePath);
            }
        }

        private static void DrawGlassInsetShadows(Graphics graphics, RectangleF capsule, float cornerRadius)
        {
            RectangleF topLeft = new RectangleF(capsule.X, capsule.Y, capsule.Width * 0.72f, capsule.Height * 0.72f);
            using (GraphicsPath topLeftPath = RoundedRectangle(topLeft, cornerRadius))
            using (PathGradientBrush topLeftGlow = new PathGradientBrush(topLeftPath))
            {
                topLeftGlow.CenterColor = Color.FromArgb(42, 255, 255, 255);
                topLeftGlow.SurroundColors = new[] { Color.FromArgb(0, 255, 255, 255) };
                topLeftGlow.FocusScales = new PointF(0.2f, 0.2f);
                graphics.FillPath(topLeftGlow, topLeftPath);
            }

            RectangleF bottomRight = new RectangleF(
                capsule.X + capsule.Width * 0.28f,
                capsule.Y + capsule.Height * 0.28f,
                capsule.Width * 0.72f,
                capsule.Height * 0.72f);
            using (GraphicsPath bottomRightPath = RoundedRectangle(bottomRight, cornerRadius))
            using (PathGradientBrush bottomRightGlow = new PathGradientBrush(bottomRightPath))
            {
                bottomRightGlow.CenterColor = Color.FromArgb(96, 255, 255, 255);
                bottomRightGlow.SurroundColors = new[] { Color.FromArgb(0, 255, 255, 255) };
                bottomRightGlow.FocusScales = new PointF(0.78f, 0.78f);
                graphics.FillPath(bottomRightGlow, bottomRightPath);
            }

            RectangleF edgeBloom = new RectangleF(capsule.X + 1f, capsule.Y + 1f, capsule.Width - 2f, capsule.Height - 2f);
            using (GraphicsPath edgePath = RoundedRectangle(edgeBloom, cornerRadius - 1f))
            using (SolidBrush edgeBrush = new SolidBrush(Color.FromArgb(30, 255, 255, 255)))
                graphics.FillPath(edgeBrush, edgePath);

            RectangleF innerPool = new RectangleF(capsule.X, capsule.Y + capsule.Height * 0.45f, capsule.Width, capsule.Height * 0.55f);
            using (LinearGradientBrush innerPoolBrush = new LinearGradientBrush(
                innerPool, Color.FromArgb(0, 0, 0, 0), Color.FromArgb(36, 0, 0, 0), LinearGradientMode.Vertical))
                graphics.FillRectangle(innerPoolBrush, innerPool);
        }

        private static void DrawGlassRim(Graphics graphics, RectangleF capsule, float cornerRadius, GraphicsPath capsulePath)
        {
            RectangleF shine = new RectangleF(capsule.X + 2f, capsule.Y + 1.5f, capsule.Width - 4f, capsule.Height * 0.42f);
            using (GraphicsPath shinePath = RoundedRectangle(shine, cornerRadius - 1f))
            using (LinearGradientBrush gloss = new LinearGradientBrush(
                shine, Color.FromArgb(72, 255, 255, 255), Color.FromArgb(0, 255, 255, 255), LinearGradientMode.Vertical))
                graphics.FillPath(gloss, shinePath);

            using (Pen outerRim = new Pen(Color.FromArgb(120, 255, 255, 255), 1f))
                graphics.DrawPath(outerRim, capsulePath);

            RectangleF inset = new RectangleF(capsule.X + 1f, capsule.Y + 1f, capsule.Width - 2f, capsule.Height - 2f);
            using (GraphicsPath insetPath = RoundedRectangle(inset, cornerRadius - 1f))
            using (Pen innerRim = new Pen(Color.FromArgb(64, 255, 255, 255), 0.75f))
                graphics.DrawPath(innerRim, insetPath);
        }

        private bool IsGlassTheme()
        {
            return String.Equals(theme, "Glass", StringComparison.OrdinalIgnoreCase);
        }

        private void GetThemeColors(out Color top, out Color bottom, out Color borderColor)
        {
            if (IsGlassTheme())
            {
                top = Color.FromArgb(215, 255, 255, 255);
                bottom = Color.FromArgb(185, 214, 226, 242);
                borderColor = Color.FromArgb(230, 255, 255, 255);
                return;
            }
            if (String.Equals(theme, "Light", StringComparison.OrdinalIgnoreCase))
            {
                top = Color.FromArgb(255, 255, 255, 255);
                bottom = Color.FromArgb(255, 241, 241, 243);
                borderColor = Color.FromArgb(200, 161, 161, 170);
                return;
            }
            if (String.Equals(theme, "Mono", StringComparison.OrdinalIgnoreCase))
            {
                top = Color.FromArgb(255, 20, 20, 22);
                bottom = Color.FromArgb(255, 6, 6, 8);
                borderColor = Color.FromArgb(255, 212, 212, 216);
                return;
            }
            if (String.Equals(theme, "Purple", StringComparison.OrdinalIgnoreCase))
            {
                top = Color.FromArgb(255, 28, 24, 40);
                bottom = Color.FromArgb(255, 12, 10, 20);
                borderColor = Color.FromArgb(255, 92, 82, 122);
                return;
            }
            top = Color.FromArgb(255, 26, 26, 28);
            bottom = Color.FromArgb(255, 9, 9, 11);
            borderColor = Color.FromArgb(255, 63, 63, 70);
        }

        private Color GetBarColor(float sample)
        {
            int alpha = 125 + (int)(130 * sample);
            if (IsGlassTheme())
            {
                int grey = 102 + (int)(40 * sample);
                return Color.FromArgb(Math.Min(255, 220 + (int)(35 * sample)), grey, grey, grey);
            }
            if (String.Equals(theme, "Light", StringComparison.OrdinalIgnoreCase))
                return Color.FromArgb(Math.Min(255, alpha), 24 + (int)(18 * sample), 24 + (int)(18 * sample), 27);
            if (String.Equals(theme, "Mono", StringComparison.OrdinalIgnoreCase))
            {
                int grey = 180 + (int)(75 * sample);
                return Color.FromArgb(Math.Min(255, alpha), grey, grey, grey);
            }
            if (String.Equals(theme, "Purple", StringComparison.OrdinalIgnoreCase))
            {
                int red = 139 + (int)(31 * sample);
                int green = 124 + (int)(30 * sample);
                return Color.FromArgb(Math.Min(255, alpha), red, green, 255);
            }
            if (String.Equals(theme, "Dark", StringComparison.OrdinalIgnoreCase))
            {
                int grey = 128 + (int)(72 * sample);
                return Color.FromArgb(Math.Min(255, alpha), grey, grey, grey);
            }
            int fallback = 120 + (int)(60 * sample);
            return Color.FromArgb(Math.Min(255, alpha), fallback, fallback, fallback);
        }

        private void Present(Bitmap bitmap)
        {
            IntPtr screenDc = IntPtr.Zero;
            IntPtr memoryDc = IntPtr.Zero;
            IntPtr bitmapHandle = IntPtr.Zero;
            IntPtr previous = IntPtr.Zero;
            try
            {
                screenDc = GetDC(IntPtr.Zero);
                memoryDc = CreateCompatibleDC(screenDc);
                bitmapHandle = bitmap.GetHbitmap(Color.FromArgb(0));
                previous = SelectObject(memoryDc, bitmapHandle);
                NativePoint destination = new NativePoint(Left, Top);
                NativePoint source = new NativePoint(0, 0);
                NativeSize size = new NativeSize(Width, Height);
                BlendFunction blend = new BlendFunction();
                blend.BlendOp = 0;
                blend.SourceConstantAlpha = (byte)Math.Max(0, Math.Min(255, (int)(255f * revealProgress + 0.5f)));
                blend.AlphaFormat = SourceAlpha;
                UpdateLayeredWindow(Handle, screenDc, ref destination, ref size, memoryDc, ref source, 0, ref blend, LayeredAlpha);
            }
            finally
            {
                if (previous != IntPtr.Zero && memoryDc != IntPtr.Zero) SelectObject(memoryDc, previous);
                if (bitmapHandle != IntPtr.Zero) DeleteObject(bitmapHandle);
                if (memoryDc != IntPtr.Zero) DeleteDC(memoryDc);
                if (screenDc != IntPtr.Zero) ReleaseDC(IntPtr.Zero, screenDc);
            }
        }
    }

    public sealed class SettingsForm : Form
    {
        private readonly ConfigStore store;
        private readonly string appDirectory;
        private readonly ComboBox engineBox = new ComboBox();
        private readonly ComboBox hotkeyBox = new ComboBox();
        private readonly ComboBox styleBox = new ComboBox();
        private readonly ComboBox cleanupProviderBox = new ComboBox();
        private readonly CheckBox cleanupBox = new CheckBox();
        private readonly CheckBox contextBox = new CheckBox();
        private readonly CheckBox pasteBox = new CheckBox();
        private readonly CheckBox historyBox = new CheckBox();
        private readonly CheckBox recoveryBox = new CheckBox();
        private readonly CheckBox startupBox = new CheckBox();
        private readonly TextBox apiKeyBox = new TextBox();
        private readonly TextBox apiUrlBox = new TextBox();
        private readonly TextBox transcriptionModelBox = new TextBox();
        private readonly TextBox cleanupModelBox = new TextBox();
        private readonly TextBox openRouterKeyBox = new TextBox();
        private readonly TextBox openRouterUrlBox = new TextBox();
        private readonly TextBox openRouterModelBox = new TextBox();
        private readonly TextBox ollamaUrlBox = new TextBox();
        private readonly TextBox ollamaModelBox = new TextBox();
        private readonly TextBox groqKeyBox = new TextBox();
        private readonly TextBox groqModelBox = new TextBox();
        private readonly TrackBar micGainBar = new TrackBar();
        private readonly Label micGainLabel = new Label();
        private readonly ProgressBar micLevelBar = new ProgressBar();
        private readonly Label micTestStatus = new Label();
        private readonly Button micTestButton = new Button();
        private readonly Label latencyLabel = new Label();
        private readonly CheckBox turboBox = new CheckBox();
        private readonly CheckBox suppressNonSpeechBox = new CheckBox();
        private readonly CheckBox completionSoundBox = new CheckBox();
        private readonly CheckBox insertNotifyBox = new CheckBox();
        private readonly ComboBox overlayThemeBox = new ComboBox();
        private readonly TextBox dictionaryBox = new TextBox();
        private readonly TextBox snippetsBox = new TextBox();
        private readonly Label localStatus = new Label();
        private readonly ProgressBar localProgress = new ProgressBar();
        private readonly Button localInstallButton = new Button();
        private readonly Button chooseWhisperButton = new Button();
        private readonly Button chooseModelButton = new Button();
        private string whisperExe;
        private string whisperServer;
        private string whisperModel;
        private readonly WaveRecorder micTestRecorder = new WaveRecorder();
        private readonly Func<bool> microphoneBusy;
        private System.Windows.Forms.Timer micTestTimer;
        private float micTestPeak;
        private string micTestPath;
        public event Action<AppSettings, string, string> SettingsSaved;
        public event Action<string> HotkeyPreviewChanged;

        public SettingsForm(ConfigStore store, AppSettings settings, string appDirectory, Func<bool> microphoneBusy)
        {
            this.store = store;
            this.appDirectory = appDirectory;
            this.microphoneBusy = microphoneBusy ?? delegate { return false; };
            Text = "Flowtype Settings";
            Width = 760;
            Height = 780;
            MinimumSize = new Size(720, 740);
            StartPosition = FormStartPosition.CenterScreen;
            Font = AppFonts.Ui(9.25f, FontStyle.Regular);
            BackColor = UiTheme.Window;
            ForeColor = UiTheme.Text;
            Icon = FlowtypeApp.ProductIcon ?? SystemIcons.Application;

            Panel header = new Panel();
            header.Dock = DockStyle.Top;
            header.Height = 68;
            header.BackColor = UiTheme.Header;
            header.Padding = new Padding(20, 14, 20, 10);
            header.Paint += delegate(object sender, PaintEventArgs e)
            {
                using (Pen line = new Pen(UiTheme.Border))
                    e.Graphics.DrawLine(line, 0, header.Height - 1, header.Width, header.Height - 1);
            };
            PictureBox logoBox = new PictureBox();
            logoBox.Size = new Size(36, 36);
            logoBox.Location = new Point(20, 14);
            logoBox.SizeMode = PictureBoxSizeMode.Zoom;
            logoBox.BackColor = Color.Transparent;
            try
            {
                string logoPath = Path.Combine(appDirectory, "assets", "Flowtype-icon.png");
                if (File.Exists(logoPath)) logoBox.Image = Image.FromFile(logoPath);
            }
            catch { }
            Label title = new Label();
            title.Text = "Flowtype";
            title.Font = AppFonts.Ui(14f, FontStyle.Bold);
            title.ForeColor = UiTheme.Text;
            title.SetBounds(64, 16, 200, 28);
            Label subtitle = new Label();
            subtitle.Text = "Push-to-talk dictation";
            subtitle.Font = AppFonts.Ui(9f, FontStyle.Regular);
            subtitle.ForeColor = UiTheme.TextMuted;
            subtitle.SetBounds(64, 40, 260, 20);
            header.Controls.Add(logoBox);
            header.Controls.Add(title);
            header.Controls.Add(subtitle);

            TabControl tabs = new TabControl();
            tabs.Dock = DockStyle.Fill;
            tabs.Padding = new Point(16, 8);
            tabs.Font = AppFonts.Ui(9.25f, FontStyle.Regular);
            tabs.TabPages.Add(BuildGeneralTab());
            tabs.TabPages.Add(BuildCloudTab());
            tabs.TabPages.Add(BuildLocalTab());
            tabs.TabPages.Add(BuildPersonalizationTab());

            Panel footer = new Panel();
            footer.Dock = DockStyle.Bottom;
            footer.Height = 58;
            footer.BackColor = UiTheme.Header;
            footer.Padding = new Padding(16, 10, 16, 10);
            footer.Paint += delegate(object sender, PaintEventArgs e)
            {
                using (Pen line = new Pen(UiTheme.Border))
                    e.Graphics.DrawLine(line, 0, 0, footer.Width, 0);
            };

            FlowLayoutPanel actions = new FlowLayoutPanel();
            actions.Dock = DockStyle.Right;
            actions.FlowDirection = FlowDirection.RightToLeft;
            actions.WrapContents = false;
            actions.AutoSize = true;
            actions.Padding = new Padding(0);
            actions.Margin = new Padding(0);

            Button saveCloseButton = MakeActionButton("Save & close", true);
            saveCloseButton.Click += SaveClicked;
            Button applyButton = MakeActionButton("Apply", false);
            applyButton.Click += ApplyClicked;
            Button cancelButton = MakeActionButton("Cancel", false);
            cancelButton.Click += delegate { Close(); };
            actions.Controls.Add(saveCloseButton);
            actions.Controls.Add(applyButton);
            actions.Controls.Add(cancelButton);
            footer.Controls.Add(actions);

            Controls.Add(tabs);
            Controls.Add(footer);
            Controls.Add(header);

            AcceptButton = saveCloseButton;
            CancelButton = cancelButton;

            LoadValues(settings);
            LatencyStats.StatsUpdated += OnLatencyStatsUpdated;
            FormClosed += delegate
            {
                LatencyStats.StatsUpdated -= OnLatencyStatsUpdated;
                StopMicTest();
            };
            hotkeyBox.SelectedIndexChanged += delegate
            {
                Action<string> handler = HotkeyPreviewChanged;
                if (handler != null) handler(Convert.ToString(hotkeyBox.SelectedItem));
            };
        }

        private TabPage NewTab(string name)
        {
            TabPage page = new TabPage(name);
            page.BackColor = UiTheme.Window;
            page.AutoScroll = true;
            page.Padding = new Padding(4);
            return page;
        }

        private static Button MakeActionButton(string text, bool primary)
        {
            Button button = new Button();
            button.Text = text;
            button.AutoSize = true;
            button.MinimumSize = new Size(primary ? 118 : 88, 34);
            button.Padding = new Padding(12, 0, 12, 0);
            button.Margin = new Padding(6, 0, 0, 0);
            button.FlatStyle = FlatStyle.Flat;
            button.Font = AppFonts.Ui(9.25f, FontStyle.Regular);
            button.Cursor = Cursors.Hand;
            if (primary)
            {
                button.BackColor = UiTheme.Accent;
                button.ForeColor = Color.White;
                button.FlatAppearance.BorderColor = UiTheme.Accent;
            }
            else
            {
                button.BackColor = UiTheme.Surface;
                button.ForeColor = UiTheme.Text;
                button.FlatAppearance.BorderColor = UiTheme.Border;
            }
            button.FlatAppearance.BorderSize = 1;
            return button;
        }

        private static void StyleField(Control control)
        {
            control.Font = AppFonts.Ui(9.25f, FontStyle.Regular);
            control.ForeColor = UiTheme.Text;
            control.BackColor = UiTheme.Surface;
        }

        private TabPage BuildGeneralTab()
        {
            TabPage page = NewTab("General");
            Label intro = LabelAt("Hold your push-to-talk key anywhere on Windows, speak, release — cleaned text appears in the field you were using.", 24, 22, 650, 36);
            intro.Font = AppFonts.UiLarge(10.5f);
            page.Controls.Add(intro);

            page.Controls.Add(LabelAt("Speech engine", 24, 76, 170, 24));
            ConfigureDropDown(engineBox, 210, 72, 430);
            engineBox.Items.AddRange(new object[]
            {
                "Local — offline, private",
                "Groq — fast cloud (free tier)",
                "OpenAI — your API key"
            });
            page.Controls.Add(engineBox);
            page.Controls.Add(LabelAt("Push-to-talk key", 24, 120, 170, 24));
            ConfigureDropDown(hotkeyBox, 210, 116, 230);
            hotkeyBox.Items.AddRange(Hotkeys.Names.Cast<object>().ToArray());
            page.Controls.Add(hotkeyBox);
            page.Controls.Add(LabelAt("Writing style", 24, 164, 170, 24));
            ConfigureDropDown(styleBox, 210, 160, 230);
            styleBox.Items.AddRange(new object[] { "Natural", "Concise", "Formal", "Casual", "Verbatim" });
            page.Controls.Add(styleBox);
            page.Controls.Add(LabelAt("Voice capsule", 24, 208, 170, 24));
            ConfigureDropDown(overlayThemeBox, 210, 204, 230);
            overlayThemeBox.Items.AddRange(new object[] { "Dark", "Dark purple", "Light", "Mono (black & white)", "Liquid glass" });
            page.Controls.Add(overlayThemeBox);

            ConfigureCheck(cleanupBox, "Smart cleanup (fillers, punctuation, lists)", 24, 254, 540);
            page.Controls.Add(LabelAt("Cleanup engine", 24, 302, 170, 24));
            ConfigureDropDown(cleanupProviderBox, 210, 298, 430);
            cleanupProviderBox.Items.AddRange(new object[]
            {
                "Built-in — free, offline",
                "OpenRouter — cloud polish",
                "OpenAI — your key",
                "Ollama — local model"
            });
            page.Controls.Add(cleanupProviderBox);
            ConfigureCheck(contextBox, "Adapt cleanup to the active app/window", 24, 344, 540);
            ConfigureCheck(pasteBox, "Auto-paste into the focused field", 24, 382, 560);
            ConfigureCheck(historyBox, "Keep a local history of dictations", 24, 420, 540);
            ConfigureCheck(recoveryBox, "Save failed recordings to Recovery folder", 24, 458, 590);
            ConfigureCheck(startupBox, "Start with Windows", 24, 496, 540);
            page.Controls.AddRange(new Control[] { cleanupBox, contextBox, pasteBox, historyBox, recoveryBox, startupBox });

            Label perfTitle = LabelAt("Performance", 24, 540, 200, 24);
            perfTitle.Font = AppFonts.Ui(10f, FontStyle.Bold);
            page.Controls.Add(perfTitle);
            ConfigureCheck(turboBox, "Fast mode — quicker on long dictations", 24, 570, 620);
            ConfigureCheck(suppressNonSpeechBox, "Filter non-speech sounds (may drop quiet words)", 24, 602, 620);
            ConfigureCheck(completionSoundBox, "Sound effects on start and finish", 24, 634, 620);
            ConfigureCheck(insertNotifyBox, "Tray toast after each dictation", 24, 666, 620);
            page.Controls.AddRange(new Control[] { turboBox, suppressNonSpeechBox, completionSoundBox, insertNotifyBox });
            page.Controls.Add(LabelAt("Microphone boost", 24, 704, 140, 24));
            micGainBar.SetBounds(170, 700, 360, 45);
            micGainBar.Minimum = 8;
            micGainBar.Maximum = 25;
            micGainBar.TickFrequency = 1;
            micGainBar.ValueChanged += delegate { micGainLabel.Text = (micGainBar.Value / 10f).ToString("0.0", CultureInfo.InvariantCulture) + "×"; };
            page.Controls.Add(micGainBar);
            micGainLabel.SetBounds(540, 708, 60, 24);
            page.Controls.Add(micGainLabel);
            Label micHealthTitle = LabelAt("Microphone health", 24, 748, 200, 24);
            micHealthTitle.Font = AppFonts.Ui(10f, FontStyle.Bold);
            page.Controls.Add(micHealthTitle);
            micLevelBar.SetBounds(24, 778, 420, 18);
            micLevelBar.Minimum = 0;
            micLevelBar.Maximum = 100;
            micLevelBar.Style = ProgressBarStyle.Continuous;
            page.Controls.Add(micLevelBar);
            micTestButton.SetBounds(456, 770, 110, 34);
            micTestButton.Text = "Test 3s";
            micTestButton.Click += MicTestClicked;
            page.Controls.Add(micTestButton);
            micTestStatus.SetBounds(24, 816, 650, 32);
            micTestStatus.ForeColor = UiTheme.TextMuted;
            micTestStatus.Text = "Live level while testing. Speak normally for three seconds.";
            page.Controls.Add(micTestStatus);
            latencyLabel.SetBounds(24, 856, 650, 36);
            latencyLabel.ForeColor = UiTheme.TextMuted;
            latencyLabel.Text = LatencyStats.Summary;
            page.Controls.Add(latencyLabel);

            Label privacy = LabelAt("Successful audio is always deleted. Flowtype has no telemetry or account system.", 24, 900, 640, 40);
            privacy.ForeColor = UiTheme.TextMuted;
            page.Controls.Add(privacy);
            return page;
        }

        private TabPage BuildCloudTab()
        {
            TabPage page = NewTab("Cloud engines");
            Label intro = LabelAt("Optional cloud engines connect directly from this PC. Flowtype never receives your audio, text, or keys.", 24, 22, 650, 40);
            intro.Font = AppFonts.UiLarge(10.5f);
            page.Controls.Add(intro);

            Label groqTitle = LabelAt("Groq speech (recommended cloud option)", 24, 72, 500, 28);
            groqTitle.Font = AppFonts.Ui(10f, FontStyle.Bold);
            page.Controls.Add(groqTitle);
            Label groqHelp = LabelAt(
                "Free API keys at console.groq.com → API Keys → Create. Recommended model: whisper-large-v3-turbo (fast + accurate). Audio is sent to Groq; cleanup stays local unless you choose a cloud cleanup engine.",
                24, 102, 650, 54);
            groqHelp.ForeColor = Color.FromArgb(95, 100, 112);
            page.Controls.Add(groqHelp);
            AddTextField(page, "Groq API key", groqKeyBox, 24, 166, true);
            groqKeyBox.UseSystemPasswordChar = true;
            AddTextField(page, "Groq model", groqModelBox, 24, 222, false);
            Button groqTestButton = ButtonAt("Test Groq", 210, 278, 120, 34);
            groqTestButton.Click += async delegate
            {
                groqTestButton.Enabled = false;
                try
                {
                    await new GroqEngine().TestAsync(ReadValues(), groqKeyBox.Text.Trim());
                    MessageBox.Show(this, "Groq connection succeeded.", "Flowtype", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception exception) { MessageBox.Show(this, exception.Message, "Connection failed", MessageBoxButtons.OK, MessageBoxIcon.Error); }
                finally { groqTestButton.Enabled = true; }
            };
            page.Controls.Add(groqTestButton);

            Label openAiTitle = LabelAt("OpenAI", 24, 332, 400, 28);
            openAiTitle.Font = AppFonts.Ui(10f, FontStyle.Bold);
            page.Controls.Add(openAiTitle);
            AddTextField(page, "OpenAI API key", apiKeyBox, 24, 366, true);
            apiKeyBox.UseSystemPasswordChar = true;
            AddTextField(page, "API base URL", apiUrlBox, 24, 422, false);
            AddTextField(page, "Transcription model", transcriptionModelBox, 24, 478, false);
            AddTextField(page, "Cleanup model", cleanupModelBox, 24, 534, false);
            Button testButton = ButtonAt("Test connection", 210, 590, 150, 34);
            testButton.Click += async delegate
            {
                testButton.Enabled = false;
                try
                {
                    AppSettings current = ReadValues();
                    await new OpenAiEngine().TestAsync(current, apiKeyBox.Text.Trim());
                    MessageBox.Show(this, "Connection succeeded.", "Flowtype", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception exception) { MessageBox.Show(this, exception.Message, "Connection failed", MessageBoxButtons.OK, MessageBoxIcon.Error); }
                finally { testButton.Enabled = true; }
            };
            page.Controls.Add(testButton);
            Label note = LabelAt("OpenAI is optional. For speech-only cloud, Groq's free tier is usually faster and cheaper than OpenAI transcription.", 210, 646, 440, 48);
            note.ForeColor = Color.FromArgb(95, 100, 112);
            page.Controls.Add(note);

            Label routerTitle = LabelAt("OpenRouter cleanup", 24, 710, 400, 28);
            routerTitle.Font = AppFonts.Ui(10f, FontStyle.Bold);
            page.Controls.Add(routerTitle);
            page.Controls.Add(LabelAt("Local speech sends only the resulting text to OpenRouter for optional polish. The free router is not used in instant mode because provider queues can stall insertion.", 24, 742, 650, 54));
            AddTextField(page, "OpenRouter key", openRouterKeyBox, 24, 806, true);
            AddTextField(page, "OpenRouter URL", openRouterUrlBox, 24, 862, false);
            AddTextField(page, "Model", openRouterModelBox, 24, 918, false);
            Button routerTestButton = ButtonAt("Test OpenRouter", 210, 974, 150, 34);
            routerTestButton.Click += async delegate
            {
                routerTestButton.Enabled = false;
                try
                {
                    string resolvedModel = await new OpenRouterEngine().TestAsync(ReadValues(), openRouterKeyBox.Text.Trim());
                    MessageBox.Show(this, "OpenRouter connection succeeded.\r\n\r\nModel used: " + resolvedModel,
                        "Flowtype", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception exception) { MessageBox.Show(this, exception.Message, "Connection failed", MessageBoxButtons.OK, MessageBoxIcon.Error); }
                finally { routerTestButton.Enabled = true; }
            };
            page.Controls.Add(routerTestButton);
            return page;
        }

        private TabPage BuildLocalTab()
        {
            TabPage page = NewTab("Local");
            Label intro = LabelAt("Offline dictation uses the Instant English model (~60 MB). It stays warm in memory for fast repeat dictations. For higher accuracy online, switch to Groq in General settings.", 24, 22, 650, 48);
            intro.Font = AppFonts.UiLarge(10.5f);
            page.Controls.Add(intro);
            localStatus.SetBounds(24, 92, 650, 48);
            page.Controls.Add(localStatus);
            localInstallButton.SetBounds(24, 150, 210, 38);
            localInstallButton.Text = "Install local engine";
            localInstallButton.Click += InstallLocalClicked;
            page.Controls.Add(localInstallButton);
            localProgress.SetBounds(250, 200, 390, 24);
            localProgress.Visible = false;
            page.Controls.Add(localProgress);

            chooseWhisperButton.SetBounds(250, 150, 190, 38);
            chooseWhisperButton.Text = "Choose whisper-cli.exe…";
            chooseWhisperButton.Click += ChooseWhisperClicked;
            page.Controls.Add(chooseWhisperButton);
            chooseModelButton.SetBounds(450, 150, 190, 38);
            chooseModelButton.Text = "Choose model .bin…";
            chooseModelButton.Click += ChooseModelClicked;
            page.Controls.Add(chooseModelButton);
            Label manualNote = LabelAt("Downloads resume automatically. If GitHub or Hugging Face is blocked, download whisper-bin-x64.zip and ggml-base.en-q5_1.bin in your browser, then select them here.", 24, 196, 650, 42);
            manualNote.ForeColor = Color.FromArgb(95, 100, 112);
            page.Controls.Add(manualNote);

            Label polishTitle = LabelAt("Optional local polish with Ollama", 24, 256, 400, 28);
            polishTitle.Font = AppFonts.Ui(10f, FontStyle.Bold);
            page.Controls.Add(polishTitle);
            Label polishNote = LabelAt("If Ollama is already installed and running, enter one of your local model names. Leave it blank to use Flowtype's fast rule-based cleanup.", 24, 290, 640, 54);
            polishNote.ForeColor = Color.FromArgb(95, 100, 112);
            page.Controls.Add(polishNote);
            AddTextField(page, "Ollama URL", ollamaUrlBox, 24, 356, false);
            AddTextField(page, "Ollama model", ollamaModelBox, 24, 412, false);
            return page;
        }

        private TabPage BuildPersonalizationTab()
        {
            TabPage page = NewTab("Personalization");
            Label dictionaryTitle = LabelAt("Dictionary", 24, 22, 620, 26);
            dictionaryTitle.Font = AppFonts.Ui(10f, FontStyle.Bold);
            page.Controls.Add(dictionaryTitle);
            page.Controls.Add(LabelAt("One term per line. Use spoken => written for replacements, e.g. flow type => Flowtype.", 24, 50, 650, 34));
            dictionaryBox.SetBounds(24, 88, 650, 170);
            dictionaryBox.Multiline = true;
            dictionaryBox.ScrollBars = ScrollBars.Vertical;
            dictionaryBox.AcceptsReturn = true;
            page.Controls.Add(dictionaryBox);

            Label snippetsTitle = LabelAt("Voice snippets", 24, 286, 620, 26);
            snippetsTitle.Font = AppFonts.Ui(10f, FontStyle.Bold);
            page.Controls.Add(snippetsTitle);
            page.Controls.Add(LabelAt("One per line as trigger => expansion, e.g. my sign off => Cheers, Alex", 24, 314, 650, 32));
            snippetsBox.SetBounds(24, 352, 650, 150);
            snippetsBox.Multiline = true;
            snippetsBox.ScrollBars = ScrollBars.Vertical;
            snippetsBox.AcceptsReturn = true;
            page.Controls.Add(snippetsBox);
            return page;
        }

        private void LoadValues(AppSettings value)
        {
            engineBox.SelectedIndex = String.Equals(value.Engine, "OpenAI", StringComparison.OrdinalIgnoreCase) ? 2 :
                String.Equals(value.Engine, "Groq", StringComparison.OrdinalIgnoreCase) ? 1 : 0;
            hotkeyBox.SelectedItem = value.Hotkey;
            if (hotkeyBox.SelectedIndex < 0) hotkeyBox.SelectedIndex = 0;
            styleBox.SelectedItem = value.Style;
            if (styleBox.SelectedIndex < 0) styleBox.SelectedIndex = 0;
            cleanupBox.Checked = value.CleanupEnabled;
            cleanupProviderBox.SelectedIndex = value.CleanupProvider == "OpenRouter" ? 1 : value.CleanupProvider == "OpenAI" ? 2 : value.CleanupProvider == "Ollama" ? 3 : 0;
            contextBox.Checked = value.ContextEnabled;
            pasteBox.Checked = value.AutoPaste;
            historyBox.Checked = value.SaveHistory;
            recoveryBox.Checked = value.KeepFailedAudio;
            startupBox.Checked = value.StartWithWindows;
            apiKeyBox.Text = store.LoadApiKey();
            apiUrlBox.Text = value.ApiBaseUrl;
            transcriptionModelBox.Text = value.TranscriptionModel;
            cleanupModelBox.Text = value.CleanupModel;
            groqKeyBox.Text = store.LoadGroqKey();
            groqModelBox.Text = value.GroqTranscriptionModel;
            turboBox.Checked = value.TurboTranscription;
            suppressNonSpeechBox.Checked = value.SuppressNonSpeech;
            completionSoundBox.Checked = value.CompletionSound;
            insertNotifyBox.Checked = value.ShowInsertNotification;
            overlayThemeBox.SelectedIndex = OverlayThemeToIndex(value.OverlayTheme);
            micGainBar.Value = Math.Max(micGainBar.Minimum, Math.Min(micGainBar.Maximum, (int)Math.Round(value.MicGain * 10f)));
            micGainLabel.Text = value.MicGain.ToString("0.0", CultureInfo.InvariantCulture) + "×";
            latencyLabel.Text = LatencyStats.Summary;
            openRouterKeyBox.Text = store.LoadOpenRouterKey();
            openRouterUrlBox.Text = value.OpenRouterUrl;
            openRouterModelBox.Text = value.OpenRouterModel;
            ollamaUrlBox.Text = value.OllamaUrl;
            ollamaModelBox.Text = value.OllamaModel;
            whisperExe = value.WhisperExePath;
            whisperServer = value.WhisperServerPath;
            whisperModel = value.WhisperModelPath;
            value.LocalModelQuality = "Instant";
            dictionaryBox.Lines = value.Dictionary.ToArray();
            snippetsBox.Lines = value.Snippets.Select(pair => pair.Key + " => " + pair.Value).ToArray();
            UpdateLocalStatus();
        }

        private AppSettings ReadValues()
        {
            AppSettings value = AppSettings.Defaults();
            value.Engine = engineBox.SelectedIndex == 2 ? "OpenAI" : engineBox.SelectedIndex == 1 ? "Groq" : "Local";
            value.CleanupProvider = cleanupProviderBox.SelectedIndex == 1 ? "OpenRouter" : cleanupProviderBox.SelectedIndex == 2 ? "OpenAI" : cleanupProviderBox.SelectedIndex == 3 ? "Ollama" : "BuiltIn";
            value.Hotkey = Convert.ToString(hotkeyBox.SelectedItem);
            value.Style = Convert.ToString(styleBox.SelectedItem);
            value.CleanupEnabled = cleanupBox.Checked;
            value.ContextEnabled = contextBox.Checked;
            value.AutoPaste = pasteBox.Checked;
            value.SaveHistory = historyBox.Checked;
            value.KeepFailedAudio = recoveryBox.Checked;
            value.StartWithWindows = startupBox.Checked;
            value.TurboTranscription = turboBox.Checked;
            value.SuppressNonSpeech = suppressNonSpeechBox.Checked;
            value.CompletionSound = completionSoundBox.Checked;
            value.ShowInsertNotification = insertNotifyBox.Checked;
            value.OverlayTheme = OverlayThemeFromIndex(overlayThemeBox.SelectedIndex);
            value.MicGain = micGainBar.Value / 10f;
            value.GroqTranscriptionModel = groqModelBox.Text.Trim();
            value.ApiBaseUrl = apiUrlBox.Text.Trim();
            value.TranscriptionModel = transcriptionModelBox.Text.Trim();
            value.CleanupModel = cleanupModelBox.Text.Trim();
            value.OpenRouterUrl = openRouterUrlBox.Text.Trim();
            value.OpenRouterModel = openRouterModelBox.Text.Trim();
            value.WhisperExePath = whisperExe ?? "";
            value.WhisperServerPath = whisperServer ?? "";
            value.WhisperModelPath = whisperModel ?? "";
            value.LocalModelQuality = "Instant";
            value.OllamaUrl = ollamaUrlBox.Text.Trim();
            value.OllamaModel = ollamaModelBox.Text.Trim();
            value.Dictionary = dictionaryBox.Lines.Select(line => line.Trim()).Where(line => line.Length > 0).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
            value.Snippets = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (string line in snippetsBox.Lines)
            {
                int split = line.IndexOf("=>", StringComparison.Ordinal);
                if (split < 0) split = line.IndexOf('=');
                if (split > 0)
                {
                    string key = line.Substring(0, split).Trim();
                    string expansion = line.Substring(split + (line.Substring(split).StartsWith("=>") ? 2 : 1)).Trim();
                    if (key.Length > 0) value.Snippets[key] = expansion;
                }
            }
            value.Repair();
            return value;
        }

        private void ApplyClicked(object sender, EventArgs e)
        {
            TrySaveSettings(false);
        }

        private void SaveClicked(object sender, EventArgs e)
        {
            if (TrySaveSettings(true)) Close();
        }

        private bool TrySaveSettings(bool closeAfterSave)
        {
            AppSettings value = ReadValues();
            if ((value.Engine == "OpenAI" || (value.CleanupEnabled && value.CleanupProvider == "OpenAI")) && String.IsNullOrWhiteSpace(apiKeyBox.Text))
            {
                MessageBox.Show(this, "OpenAI speech or cleanup needs an OpenAI API key.", "Flowtype", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            if (value.CleanupEnabled && value.CleanupProvider == "OpenRouter" && String.IsNullOrWhiteSpace(openRouterKeyBox.Text))
            {
                MessageBox.Show(this, "OpenRouter cleanup needs an OpenRouter API key.", "Flowtype", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            if (value.Engine == "Groq" && String.IsNullOrWhiteSpace(groqKeyBox.Text))
            {
                MessageBox.Show(this, "Groq mode needs a free API key from console.groq.com.", "Flowtype", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            if (value.Engine == "Local" && (!File.Exists(value.WhisperExePath) || !File.Exists(value.WhisperModelPath)))
            {
                MessageBox.Show(this, "Install the local engine before saving Local mode.", "Flowtype", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            try
            {
                store.Save(value);
                store.SaveApiKey(apiKeyBox.Text.Trim());
                store.SaveOpenRouterKey(openRouterKeyBox.Text.Trim());
                store.SaveGroqKey(groqKeyBox.Text.Trim());
                Action<AppSettings, string, string> handler = SettingsSaved;
                if (handler != null) handler(value, apiKeyBox.Text.Trim(), openRouterKeyBox.Text.Trim());
                if (!closeAfterSave)
                    MessageBox.Show(this, "Settings applied.", "Flowtype", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return true;
            }
            catch (Exception exception)
            {
                MessageBox.Show(this, exception.Message, "Could not save settings", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        private async void InstallLocalClicked(object sender, EventArgs e)
        {
            localInstallButton.Enabled = false;
            chooseWhisperButton.Enabled = false;
            chooseModelButton.Enabled = false;
            localProgress.Visible = true;
            ControlBox = false;
            try
            {
                LocalInstallResult result = await new LocalEngineInstaller(appDirectory).InstallAsync(delegate(int percent, string message)
                {
                    if (IsDisposed) return;
                    localProgress.Value = Math.Max(0, Math.Min(100, percent));
                    localStatus.Text = message + "  " + percent.ToString(CultureInfo.InvariantCulture) + "%";
                });
                whisperExe = result.Executable;
                whisperServer = result.ServerExecutable;
                whisperModel = result.Model;
                UpdateLocalStatus();
                MessageBox.Show(this, "The local engine is ready.", "Flowtype", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception exception)
            {
                localStatus.Text = "Installation failed. You can retry safely.";
                MessageBox.Show(this, exception.Message, "Local setup failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                ControlBox = true;
                localInstallButton.Enabled = true;
                chooseWhisperButton.Enabled = true;
                chooseModelButton.Enabled = true;
                localProgress.Visible = false;
            }
        }

        private void ChooseWhisperClicked(object sender, EventArgs e)
        {
            using (OpenFileDialog dialog = new OpenFileDialog())
            {
                dialog.Title = "Choose whisper-cli.exe from the official Windows archive";
                dialog.Filter = "whisper-cli.exe|whisper-cli.exe|Programs (*.exe)|*.exe";
                dialog.CheckFileExists = true;
                if (dialog.ShowDialog(this) != DialogResult.OK) return;
                if (!String.Equals(Path.GetFileName(dialog.FileName), "whisper-cli.exe", StringComparison.OrdinalIgnoreCase))
                {
                    MessageBox.Show(this, "Choose whisper-cli.exe from whisper-bin-x64.zip.", "Flowtype", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                whisperExe = dialog.FileName;
                string directory = Path.GetDirectoryName(dialog.FileName);
                whisperServer = Directory.GetFiles(directory, "whisper-server.exe", SearchOption.AllDirectories).FirstOrDefault() ?? "";
                UpdateLocalStatus();
            }
        }

        private void ChooseModelClicked(object sender, EventArgs e)
        {
            using (OpenFileDialog dialog = new OpenFileDialog())
            {
                dialog.Title = "Choose a whisper.cpp GGML speech model";
                dialog.Filter = "Whisper models (*.bin)|*.bin|All files (*.*)|*.*";
                dialog.CheckFileExists = true;
                if (dialog.ShowDialog(this) != DialogResult.OK) return;
                long bytes = new FileInfo(dialog.FileName).Length;
                if (bytes < 30000000L)
                {
                    MessageBox.Show(this, "That model file is too small and may be an incomplete download.", "Flowtype", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                whisperModel = dialog.FileName;
                UpdateLocalStatus();
            }
        }

        private void UpdateLocalStatus()
        {
            bool ready = !String.IsNullOrWhiteSpace(whisperExe) && File.Exists(whisperExe) &&
                !String.IsNullOrWhiteSpace(whisperModel) && File.Exists(whisperModel);
            bool instantReady = ready && Path.GetFileName(whisperModel).IndexOf("base.en", StringComparison.OrdinalIgnoreCase) >= 0;
            localStatus.Text = instantReady ? "Ready — Instant model installed and will stay warm between dictations." :
                (ready ? "A non-Instant model is selected. Choose ggml-base.en-q5_1.bin or click Install." :
                "Not installed — one click downloads about 60 MB.");
            localStatus.ForeColor = instantReady ? Color.FromArgb(25, 128, 91) : Color.FromArgb(95, 100, 112);
            localInstallButton.Text = instantReady ? "Reinstall / update" : "Install local engine";
        }

        private static Label LabelAt(string text, int x, int y, int width, int height)
        {
            Label label = new Label();
            label.Text = text;
            label.SetBounds(x, y, width, height);
            return label;
        }

        private static Button ButtonAt(string text, int x, int y, int width, int height)
        {
            Button button = new Button();
            button.Text = text;
            button.SetBounds(x, y, width, height);
            return button;
        }

        private static void ConfigureDropDown(ComboBox box, int x, int y, int width)
        {
            box.SetBounds(x, y, width, 30);
            box.DropDownStyle = ComboBoxStyle.DropDownList;
        }

        private static void ConfigureCheck(CheckBox box, string text, int x, int y, int width)
        {
            box.Text = text;
            box.SetBounds(x, y, width, 30);
        }

        private static void AddTextField(Control parent, string label, TextBox box, int x, int y, bool password)
        {
            parent.Controls.Add(LabelAt(label, x, y + 5, 170, 25));
            box.SetBounds(x + 186, y, 430, 30);
            if (password) box.UseSystemPasswordChar = true;
            parent.Controls.Add(box);
        }

        private void OnLatencyStatsUpdated()
        {
            if (IsDisposed) return;
            if (InvokeRequired)
            {
                try { BeginInvoke(new Action(OnLatencyStatsUpdated)); } catch { }
                return;
            }
            latencyLabel.Text = LatencyStats.Summary;
        }

        private void MicTestClicked(object sender, EventArgs e)
        {
            if (micTestRecorder.IsRecording)
            {
                StopMicTest();
                return;
            }
            if (microphoneBusy())
            {
                MessageBox.Show(this, "Finish or cancel the current dictation before testing the microphone.", "Flowtype",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            micTestPeak = 0f;
            micLevelBar.Value = 0;
            micTestStatus.Text = "Listening… speak normally.";
            micTestButton.Text = "Stop test";
            micTestPath = Path.Combine(Path.GetTempPath(),
                "flowtype-mic-test-" + Guid.NewGuid().ToString("N").Substring(0, 8) + ".wav");
            micTestRecorder.MicGain = micGainBar.Value / 10f;
            micTestRecorder.LevelChanged += OnMicTestLevel;
            try
            {
                micTestRecorder.Start(micTestPath);
            }
            catch (Exception exception)
            {
                micTestRecorder.LevelChanged -= OnMicTestLevel;
                micTestButton.Text = "Test 3s";
                micTestStatus.Text = "Could not open the microphone: " + exception.Message;
                return;
            }
            micTestTimer = new System.Windows.Forms.Timer();
            micTestTimer.Interval = 3000;
            micTestTimer.Tick += delegate
            {
                micTestTimer.Stop();
                micTestTimer.Dispose();
                micTestTimer = null;
                FinishMicTest();
            };
            micTestTimer.Start();
        }

        private void OnMicTestLevel(float level)
        {
            if (IsDisposed) return;
            if (InvokeRequired)
            {
                try { BeginInvoke(new Action<float>(OnMicTestLevel), level); } catch { }
                return;
            }
            micTestPeak = Math.Max(micTestPeak, level);
            micLevelBar.Value = Math.Max(micLevelBar.Minimum, Math.Min(micLevelBar.Maximum, (int)Math.Round(level * 100f)));
        }

        private void FinishMicTest()
        {
            micTestRecorder.LevelChanged -= OnMicTestLevel;
            try
            {
                if (micTestRecorder.IsRecording) micTestRecorder.Stop();
            }
            catch { }
            try
            {
                if (!String.IsNullOrWhiteSpace(micTestPath) && File.Exists(micTestPath)) File.Delete(micTestPath);
            }
            catch { }
            micTestPath = "";
            micTestButton.Text = "Test 3s";
            micTestButton.Enabled = true;
            int peakPercent = (int)Math.Round(micTestPeak * 100f);
            if (micTestPeak < 0.08f)
                micTestStatus.Text = "Very quiet (" + peakPercent + "% peak) — raise Microphone boost or move closer.";
            else if (micTestPeak < 0.2f)
                micTestStatus.Text = "A bit quiet (" + peakPercent + "% peak) — try 1.4×–1.8× boost if words are missed.";
            else if (micTestPeak > 0.92f)
                micTestStatus.Text = "Very loud (" + peakPercent + "% peak) — lower boost to avoid clipping.";
            else micTestStatus.Text = "Good input level (" + peakPercent + "% peak).";
        }

        private void StopMicTest()
        {
            if (micTestTimer != null)
            {
                micTestTimer.Stop();
                micTestTimer.Dispose();
                micTestTimer = null;
            }
            if (micTestRecorder.IsRecording || !String.IsNullOrWhiteSpace(micTestPath)) FinishMicTest();
        }

        private static int OverlayThemeToIndex(string overlayTheme)
        {
            if (String.Equals(overlayTheme, "Purple", StringComparison.OrdinalIgnoreCase)) return 1;
            if (String.Equals(overlayTheme, "Light", StringComparison.OrdinalIgnoreCase)) return 2;
            if (String.Equals(overlayTheme, "Mono", StringComparison.OrdinalIgnoreCase)) return 3;
            if (String.Equals(overlayTheme, "Glass", StringComparison.OrdinalIgnoreCase)) return 4;
            return 0;
        }

        private static string OverlayThemeFromIndex(int index)
        {
            if (index == 1) return "Purple";
            if (index == 2) return "Light";
            if (index == 3) return "Mono";
            if (index == 4) return "Glass";
            return "Dark";
        }
    }

    public sealed class HistoryForm : Form
    {
        private readonly HistoryStore history;
        private readonly ListView list = new ListView();
        private readonly TextBox text = new TextBox();

        public HistoryForm(HistoryStore history)
        {
            this.history = history;
            Text = "Flowtype History";
            Width = 850;
            Height = 580;
            StartPosition = FormStartPosition.CenterScreen;
            Font = AppFonts.Ui(9.5f, FontStyle.Regular);
            Icon = SystemIcons.Information;

            ToolStrip toolbar = new ToolStrip();
            ToolStripButton copy = new ToolStripButton("Copy selected");
            ToolStripButton clear = new ToolStripButton("Clear history");
            toolbar.Items.Add(copy);
            toolbar.Items.Add(new ToolStripSeparator());
            toolbar.Items.Add(clear);
            copy.Click += delegate { if (!String.IsNullOrWhiteSpace(text.Text)) Clipboard.SetText(text.Text); };
            clear.Click += delegate
            {
                if (MessageBox.Show(this, "Delete all locally saved text history?", "Flowtype", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
                {
                    history.Clear();
                    Reload();
                }
            };

            SplitContainer split = new SplitContainer();
            split.Dock = DockStyle.Fill;
            split.SplitterDistance = 310;
            list.Dock = DockStyle.Fill;
            list.View = View.Details;
            list.FullRowSelect = true;
            list.HideSelection = false;
            list.Columns.Add("When", 125);
            list.Columns.Add("Application", 150);
            list.SelectedIndexChanged += delegate
            {
                if (list.SelectedItems.Count == 0) { text.Text = ""; return; }
                HistoryEntry entry = list.SelectedItems[0].Tag as HistoryEntry;
                text.Text = entry == null ? "" : entry.FinalText;
            };
            text.Dock = DockStyle.Fill;
            text.Multiline = true;
            text.ScrollBars = ScrollBars.Vertical;
            text.ReadOnly = true;
            text.BackColor = Color.White;
            text.Font = AppFonts.UiLarge(10.5f);
            split.Panel1.Controls.Add(list);
            split.Panel2.Controls.Add(text);
            Controls.Add(split);
            Controls.Add(toolbar);
            toolbar.Dock = DockStyle.Top;
            Reload();
        }

        private void Reload()
        {
            list.Items.Clear();
            text.Text = "";
            foreach (HistoryEntry entry in history.Load())
            {
                DateTime local = entry.CreatedUtc.Kind == DateTimeKind.Utc ? entry.CreatedUtc.ToLocalTime() : entry.CreatedUtc;
                ListViewItem item = new ListViewItem(local.ToString("g"));
                item.SubItems.Add(entry.Application ?? "");
                item.Tag = entry;
                list.Items.Add(item);
            }
        }
    }

    public sealed class FlowtypeContext : ApplicationContext
    {
        private readonly ConfigStore store;
        private readonly HistoryStore history;
        private readonly NotifyIcon tray;
        private readonly ToolStripMenuItem statusItem;
        private readonly ToolStripMenuItem toggleItem;
        private readonly ToolStripMenuItem dictionaryFixItem;
        private readonly RecordingOverlay overlay;
        private readonly WaveRecorder recorder;
        private readonly WhisperEngine whisperEngine;
        private readonly GroqEngine groqEngine;
        private readonly GlobalKeyHook hook;
        private readonly System.Windows.Forms.Timer chordPoller;
        private readonly System.Windows.Forms.Timer activationPoller;
        private readonly EventWaitHandle activationEvent;
        private readonly Control dispatcher;
        private AppSettings settings;
        private string apiKey;
        private string openRouterKey;
        private string groqKey;
        private int dictationGeneration;
        private ForegroundInfo target;
        private string recordingPath;
        private string lastDictationWord = "";
        private Stopwatch recordTimer;
        private bool hotkeyDown;
        private bool chordPolledDown;
        private bool processing;
        private bool shuttingDown;
        private DateTime hotkeyDownSince = DateTime.MinValue;
        private DateTime lastHotkeyRelease = DateTime.MinValue;
        private System.Windows.Forms.Timer pendingStartTimer;
        private System.Windows.Forms.Timer pendingStopTimer;
        private int chordReleaseStreak;
        private const int MinChordHoldMs = 45;
        private const int ReleaseGraceMs = 180;
        private SettingsForm settingsForm;
        private HistoryForm historyForm;

        public FlowtypeContext(EventWaitHandle activationEvent)
        {
            this.activationEvent = activationEvent;
            store = new ConfigStore();
            bool firstRun = store.IsFirstRun;
            settings = store.Load();
            bool bundledInstantReady = ActivateBundledInstantEngine(firstRun);
            apiKey = store.LoadApiKey();
            openRouterKey = store.LoadOpenRouterKey();
            groqKey = store.LoadGroqKey();
            history = new HistoryStore(store.HistoryPath);
            dispatcher = new Control();
            dispatcher.CreateControl();
            overlay = new RecordingOverlay();
            overlay.SetTheme(settings.OverlayTheme);
            recorder = new WaveRecorder();
            RecordingCue.Preload();
            recorder.MicGain = settings.MicGain;
            whisperEngine = new WhisperEngine();
            groqEngine = new GroqEngine();
            hook = new GlobalKeyHook(settings.Hotkey);
            hook.HotkeyChanged += OnHotkeyChanged;
            hook.CancelPressed += CancelRecording;
            recorder.LevelChanged += overlay.SetLevel;
            overlay.MaximumDurationReached += StopRecording;
            chordPoller = new System.Windows.Forms.Timer();
            chordPoller.Interval = 20;
            chordPoller.Tick += delegate
            {
                if (!Hotkeys.IsChord(settings.Hotkey)) return;
                bool down = NativeKeyState.IsWinCtrlDown();
                // Backup start when the low-level hook missed key-down.
                if (down && !hotkeyDown)
                {
                    chordReleaseStreak = 0;
                    chordPolledDown = true;
                    OnHotkeyChanged(true);
                    return;
                }
                // Backup stop only after release is stable — a single 20 ms poll flicker
                // used to cut recordings short and produce single-letter Whisper output.
                if (!down && hotkeyDown)
                {
                    chordReleaseStreak++;
                    if (chordReleaseStreak < 3) return;
                    chordReleaseStreak = 0;
                    chordPolledDown = false;
                    OnHotkeyChanged(false);
                    return;
                }
                chordReleaseStreak = 0;
                chordPolledDown = down;
            };
            chordPoller.Start();
            activationPoller = new System.Windows.Forms.Timer();
            activationPoller.Interval = 120;
            activationPoller.Tick += delegate
            {
                try { if (this.activationEvent != null && this.activationEvent.WaitOne(0)) ShowSettings(); }
                catch { }
            };
            activationPoller.Start();

            ContextMenuStrip menu = new ContextMenuStrip();
            statusItem = new ToolStripMenuItem("Ready — hold " + settings.Hotkey);
            statusItem.Enabled = false;
            toggleItem = new ToolStripMenuItem("Start dictating");
            toggleItem.Click += delegate { ToggleRecording(); };
            ToolStripMenuItem settingsItem = new ToolStripMenuItem("Settings…");
            settingsItem.Click += delegate { ShowSettings(); };
            ToolStripMenuItem historyItem = new ToolStripMenuItem("History…");
            historyItem.Click += delegate { ShowHistory(); };
            dictionaryFixItem = new ToolStripMenuItem("Add last word to dictionary…");
            dictionaryFixItem.Enabled = false;
            dictionaryFixItem.Click += delegate { AddLastWordToDictionary(); };
            ToolStripMenuItem recoveryItem = new ToolStripMenuItem("Open recovery folder");
            recoveryItem.Click += delegate { OpenFolder(store.RecoveryPath); };
            ToolStripMenuItem quitItem = new ToolStripMenuItem("Quit Flowtype");
            quitItem.Click += delegate { Quit(); };
            menu.Items.Add(statusItem);
            menu.Items.Add(toggleItem);
            menu.Items.Add(new ToolStripSeparator());
            menu.Items.Add(settingsItem);
            menu.Items.Add(historyItem);
            menu.Items.Add(dictionaryFixItem);
            menu.Items.Add(recoveryItem);
            menu.Items.Add(new ToolStripSeparator());
            menu.Items.Add(quitItem);

            tray = new NotifyIcon();
            tray.Icon = FlowtypeApp.ProductIcon ?? SystemIcons.Application;
            tray.Text = "Flowtype — hold " + settings.Hotkey + " to dictate";
            tray.ContextMenuStrip = menu;
            tray.Visible = true;
            tray.MouseClick += delegate(object sender, MouseEventArgs e)
            {
                if (e.Button == MouseButtons.Left) ShowSettings();
            };

            if (firstRun && !bundledInstantReady)
            {
                System.Windows.Forms.Timer firstRunTimer = new System.Windows.Forms.Timer();
                firstRunTimer.Interval = 700;
                firstRunTimer.Tick += delegate
                {
                    firstRunTimer.Stop();
                    firstRunTimer.Dispose();
                    ShowStartupExperience(firstRun);
                };
                firstRunTimer.Start();
            }
            else ShowStartupExperience(firstRun);
            if (settings.Engine == "Local")
            {
                WarmLocalEngine();
                RemoveLegacyLargeModel();
            }
            else if (settings.Engine == "Groq" && !String.IsNullOrWhiteSpace(groqKey)) WarmGroqEngine();
        }

        private bool IsReadyToDictate()
        {
            if (String.Equals(settings.Engine, "Groq", StringComparison.OrdinalIgnoreCase))
                return !String.IsNullOrWhiteSpace(groqKey);
            if (String.Equals(settings.Engine, "OpenAI", StringComparison.OrdinalIgnoreCase))
                return !String.IsNullOrWhiteSpace(apiKey);
            return File.Exists(settings.WhisperExePath) && File.Exists(settings.WhisperModelPath);
        }

        private void ShowStartupExperience(bool firstRun)
        {
            if (IsReadyToDictate())
            {
                ShowReadyWelcomeOnce();
                return;
            }
            if (!firstRun) return;
            ShowSettings();
            Notify("Quick setup", "Choose Groq (fastest) or install the local engine, then Save & apply.", ToolTipIcon.Info);
        }

        private void ShowReadyWelcomeOnce()
        {
            try
            {
                string marker = Path.Combine(store.Root, "ready-welcome-v1.shown");
                if (File.Exists(marker)) return;
                string engineLabel = String.Equals(settings.Engine, "Groq", StringComparison.OrdinalIgnoreCase) ? "Groq" :
                    String.Equals(settings.Engine, "OpenAI", StringComparison.OrdinalIgnoreCase) ? "OpenAI" : "Local";
                Notify("You're ready", "Hold " + settings.Hotkey + " anywhere to dictate. Engine: " + engineLabel + ".", ToolTipIcon.Info);
                File.WriteAllText(marker, DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture), new UTF8Encoding(false));
            }
            catch (Exception exception) { store.LogError(exception); }
        }

        private bool ActivateBundledInstantEngine(bool firstRun)
        {
            try
            {
                string whisperRoot = Path.Combine(FlowtypeApp.AppDirectory, "tools", "whisper");
                string executable = Path.Combine(whisperRoot, "bundled-bin", "whisper-cli.exe");
                string serverExecutable = Path.Combine(whisperRoot, "bundled-bin", "whisper-server.exe");
                string model = Path.Combine(whisperRoot, "models", "ggml-base.en-q5_1.bin");
                if (!File.Exists(executable) || !File.Exists(serverExecutable) || !File.Exists(model) ||
                    new FileInfo(model).Length < 50000000L) return false;

                string marker = Path.Combine(store.Root, "instant-engine-v1.ready");
                bool currentEngineMissing = !File.Exists(settings.WhisperExePath) || !File.Exists(settings.WhisperServerPath) ||
                    !File.Exists(settings.WhisperModelPath);
                if (!firstRun && File.Exists(marker) && !currentEngineMissing) return true;

                settings.Engine = "Local";
                settings.CleanupProvider = "BuiltIn";
                settings.WhisperExePath = executable;
                settings.WhisperServerPath = serverExecutable;
                settings.WhisperModelPath = model;
                settings.LocalModelQuality = "Instant";
                store.Save(settings);
                File.WriteAllText(marker, DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture), new UTF8Encoding(false));
                return true;
            }
            catch (Exception exception)
            {
                store.LogError(exception);
                return false;
            }
        }

        private void OnHotkeyChanged(bool down)
        {
            if (down)
            {
                if (hotkeyDown) return;
                hotkeyDown = true;
                hotkeyDownSince = DateTime.UtcNow;
                CancelPendingStop();
                CancelPendingStart();
                // Start immediately so leading syllables are not lost to a start debounce.
                try { dispatcher.BeginInvoke(new Action(StartRecording)); } catch { }
            }
            else
            {
                if (!hotkeyDown) return;
                hotkeyDown = false;
                lastHotkeyRelease = DateTime.UtcNow;
                CancelPendingStart();
                ScheduleStopRecording();
            }
        }

        private void CancelPendingStart()
        {
            if (pendingStartTimer == null) return;
            pendingStartTimer.Stop();
            pendingStartTimer.Dispose();
            pendingStartTimer = null;
        }

        private void ScheduleStopRecording()
        {
            CancelPendingStop();
            pendingStopTimer = new System.Windows.Forms.Timer();
            pendingStopTimer.Interval = ReleaseGraceMs;
            pendingStopTimer.Tick += delegate
            {
                pendingStopTimer.Stop();
                if (hotkeyDown) return;
                try { dispatcher.BeginInvoke(new Action(StopRecording)); } catch { }
            };
            pendingStopTimer.Start();
        }

        private void CancelPendingStop()
        {
            if (pendingStopTimer == null) return;
            pendingStopTimer.Stop();
            pendingStopTimer.Dispose();
            pendingStopTimer = null;
        }

        private void ToggleRecording()
        {
            if (recorder.IsRecording) StopRecording();
            else StartRecording();
        }

        private void StartRecording()
        {
            if (shuttingDown || recorder.IsRecording) return;
            if (processing) dictationGeneration++;
            try
            {
                if (settings.Engine == "OpenAI" && String.IsNullOrWhiteSpace(apiKey))
                {
                    Notify("OpenAI mode needs a key", "Open Settings and enter your own OpenAI API key.", ToolTipIcon.Warning);
                    ShowSettings();
                    return;
                }
                if (settings.Engine == "Groq" && String.IsNullOrWhiteSpace(groqKey))
                {
                    Notify("Groq mode needs a key", "Get a free API key at console.groq.com, then paste it in Settings.", ToolTipIcon.Warning);
                    ShowSettings();
                    return;
                }
                if (settings.Engine == "Local" && (!File.Exists(settings.WhisperExePath) || !File.Exists(settings.WhisperModelPath)))
                {
                    Notify("Local engine not installed", "Open Settings and click Install local engine.", ToolTipIcon.Warning);
                    ShowSettings();
                    return;
                }
                target = ForegroundContext.Capture(settings.ContextEnabled);
                recorder.MicGain = settings.MicGain;
                recordingPath = Path.Combine(store.RecoveryPath,
                    "Flowtype-" + DateTime.Now.ToString("yyyyMMdd-HHmmss") + "-" + Guid.NewGuid().ToString("N").Substring(0, 6) + ".wav");
                Exception lastMicError = null;
                for (int attempt = 0; attempt < 4; attempt++)
                {
                    try
                    {
                        recorder.Start(recordingPath);
                        lastMicError = null;
                        break;
                    }
                    catch (Exception exception)
                    {
                        lastMicError = exception;
                        if (attempt < 3) Thread.Sleep(35 * (attempt + 1));
                    }
                }
                if (lastMicError != null) throw lastMicError;
                processing = false;
                recordTimer = Stopwatch.StartNew();
                hook.CaptureEscape = true;
                overlay.ShowRecording(settings.Hotkey, settings.OverlayTheme);
                if (settings.CompletionSound) RecordingCue.PlayStart();
                statusItem.Text = "Listening… release " + settings.Hotkey;
                toggleItem.Text = "Stop and insert";
            }
            catch (Exception exception)
            {
                store.LogError(exception);
                overlay.ShowFailure(ShortMessage(exception.Message));
                Notify("Could not start recording", exception.Message, ToolTipIcon.Error);
                SetReady();
            }
        }

        private void StopRecording()
        {
            if (!recorder.IsRecording)
            {
                overlay.EnsureHidden();
                return;
            }
            try
            {
                // The capsule represents physical key-down only. Hide before
                // finalising the WAV so release always feels immediate.
                overlay.HideNow();
                hook.CaptureEscape = false;
                long recordMs = recordTimer != null ? recordTimer.ElapsedMilliseconds : 0;
                recordTimer = null;
                string path = recorder.Stop();
                toggleItem.Text = "Start dictating";
                FileInfo file = new FileInfo(path);
                if (recordMs < MinChordHoldMs || !file.Exists || file.Length < 5000)
                {
                    TryDelete(path);
                    overlay.HideNow();
                    SetReady();
                    return;
                }
                processing = true;
                statusItem.Text = "Writing…";
                int generation = ++dictationGeneration;
                ProcessRecording(path, generation, recordMs);
            }
            catch (Exception exception)
            {
                processing = false;
                store.LogError(exception);
                overlay.HideNow();
                Notify("Recording failed", exception.Message, ToolTipIcon.Error);
                SetReady();
            }
        }

        private async void ProcessRecording(string path, int generation, long recordMs)
        {
            string raw = "";
            Stopwatch totalTimer = Stopwatch.StartNew();
            long transcribeMs = 0;
            long cleanMs = 0;
            try
            {
                if (generation != dictationGeneration) return;
                Stopwatch transcribeTimer = Stopwatch.StartNew();
                SpeechTranscript transcript;
                if (settings.Engine == "OpenAI")
                {
                    transcript = new SpeechTranscript();
                    transcript.Text = await new OpenAiEngine().TranscribeAsync(path, settings, apiKey);
                }
                else if (settings.Engine == "Groq")
                {
                    transcript = await groqEngine.TranscribeAsync(path, settings, groqKey, target);
                }
                else transcript = await whisperEngine.TranscribeAsync(path, settings, target);
                transcribeTimer.Stop();
                transcribeMs = transcribeTimer.ElapsedMilliseconds;
                if (generation != dictationGeneration) return;
                raw = TextProcessor.StripPromptHallucinations(transcript.Text, settings, target);
                transcript.Text = raw;

                FileInfo audioInfo = new FileInfo(path);
                if (TranscriptionQuality.ShouldReject(raw, recordMs, audioInfo.Exists ? audioInfo.Length : 0))
                    throw new InvalidOperationException("Speech was too unclear to insert. Hold the hotkey a moment longer and try again.");

                string finalText = raw;
                if (settings.CleanupEnabled)
                {
                    Stopwatch cleanTimer = Stopwatch.StartNew();
                    try
                    {
                        if (settings.CleanupProvider == "OpenAI") finalText = await new OpenAiEngine().CleanupAsync(raw, target, settings, apiKey);
                        else if (settings.CleanupProvider == "OpenRouter") finalText = await new OpenRouterEngine().CleanupAsync(raw, target, settings, openRouterKey);
                        else if (settings.CleanupProvider == "Ollama") finalText = await new OllamaEngine().CleanupAsync(raw, target, settings);
                        else finalText = TextProcessor.Clean(transcript, settings, target);
                    }
                    catch (Exception cleanupError)
                    {
                        store.LogError(cleanupError);
                        finalText = TextProcessor.Clean(transcript, settings, target);
                    }
                    cleanTimer.Stop();
                    cleanMs = cleanTimer.ElapsedMilliseconds;
                }
                if (generation != dictationGeneration) return;
                bool pressEnter = TextProcessor.ExtractPressEnter(ref finalText);
                if (String.IsNullOrWhiteSpace(finalText) && !pressEnter) throw new InvalidOperationException("No speech was detected.");

                bool pasted;
                if (String.IsNullOrWhiteSpace(finalText)) pasted = settings.AutoPaste && ForegroundContext.IsSameTarget(target);
                else pasted = ForegroundContext.CopyAndPaste(finalText, target, settings.AutoPaste);
                if (pasted && pressEnter)
                {
                    Thread.Sleep(35);
                    ForegroundContext.PressEnter();
                }
                if (settings.SaveHistory)
                {
                    HistoryEntry entry = new HistoryEntry();
                    entry.CreatedUtc = DateTime.UtcNow;
                    entry.Application = target == null ? "" : target.AppLabel;
                    entry.RawText = raw;
                    entry.FinalText = finalText;
                    entry.Engine = settings.Engine;
                    history.Add(entry);
                }
                TryDelete(path);
                totalTimer.Stop();
                LatencyStats.Update(recordMs, transcribeMs, cleanMs, totalTimer.ElapsedMilliseconds);
                lastDictationWord = LastWord(finalText);
                if (String.IsNullOrWhiteSpace(lastDictationWord)) lastDictationWord = LastWord(raw);
                UpdateDictionaryFixItem();
                if (settings.CompletionSound) RecordingCue.PlayComplete();
                if (settings.ShowInsertNotification)
                {
                    if (pasted) Notify("Inserted", ShortPreview(finalText), ToolTipIcon.Info);
                    else Notify("Dictation copied", "The original field changed, so Flowtype put the result on your clipboard.", ToolTipIcon.Info);
                }
            }
            catch (Exception exception)
            {
                if (generation != dictationGeneration) return;
                store.LogError(exception);
                if (!settings.KeepFailedAudio) TryDelete(path);
                string suffix = settings.KeepFailedAudio ? " The recording is in Recovery." : "";
                Notify("Dictation failed", ShortMessage(exception.Message) + suffix, ToolTipIcon.Error);
            }
            finally
            {
                if (generation == dictationGeneration) processing = false;
                SetReady();
            }
        }

        private static string ShortPreview(string text)
        {
            if (String.IsNullOrWhiteSpace(text)) return "Done.";
            string compact = text.Replace("\r\n", " ").Replace('\n', ' ').Trim();
            return compact.Length <= 72 ? compact : compact.Substring(0, 69) + "…";
        }

        private void CancelRecording()
        {
            if (!recorder.IsRecording) return;
            try
            {
                recorder.Cancel();
                TryDelete(recordingPath);
            }
            catch (Exception exception) { store.LogError(exception); }
            hook.CaptureEscape = false;
            hotkeyDown = false;
            overlay.HideNow();
            toggleItem.Text = "Start dictating";
            SetReady();
        }

        private void SetReady()
        {
            statusItem.Text = "Ready — hold " + settings.Hotkey;
            tray.Text = "Flowtype — hold " + settings.Hotkey + " to dictate";
            if (!recorder.IsRecording) overlay.EnsureHidden();
        }

        private void UpdateDictionaryFixItem()
        {
            if (String.IsNullOrWhiteSpace(lastDictationWord))
            {
                dictionaryFixItem.Enabled = false;
                dictionaryFixItem.Text = "Add last word to dictionary…";
                return;
            }
            dictionaryFixItem.Enabled = true;
            dictionaryFixItem.Text = "Fix \"" + lastDictationWord + "\" in dictionary…";
        }

        private static string LastWord(string text)
        {
            if (String.IsNullOrWhiteSpace(text)) return "";
            string[] words = Regex.Split(text.Trim(), @"\s+");
            for (int index = words.Length - 1; index >= 0; index--)
            {
                string word = Regex.Replace(words[index], @"[^\w'-]", "");
                if (word.Length > 0) return word;
            }
            return "";
        }

        private void AddLastWordToDictionary()
        {
            if (String.IsNullOrWhiteSpace(lastDictationWord)) return;
            using (Form dialog = new Form())
            {
                dialog.Text = "Dictionary fix";
                dialog.FormBorderStyle = FormBorderStyle.FixedDialog;
                dialog.StartPosition = FormStartPosition.CenterScreen;
                dialog.ClientSize = new Size(420, 150);
                dialog.Font = AppFonts.Ui(9.5f, FontStyle.Regular);
                dialog.MaximizeBox = false;
                dialog.MinimizeBox = false;
                Label prompt = new Label();
                prompt.Text = "Heard as \"" + lastDictationWord + "\". Correct spelling:";
                prompt.SetBounds(16, 16, 388, 36);
                TextBox correctionBox = new TextBox();
                correctionBox.Text = lastDictationWord;
                correctionBox.SetBounds(16, 56, 388, 28);
                Button okButton = new Button();
                okButton.Text = "Add";
                okButton.DialogResult = DialogResult.OK;
                okButton.SetBounds(228, 100, 84, 32);
                Button cancelButton = new Button();
                cancelButton.Text = "Cancel";
                cancelButton.DialogResult = DialogResult.Cancel;
                cancelButton.SetBounds(320, 100, 84, 32);
                dialog.Controls.AddRange(new Control[] { prompt, correctionBox, okButton, cancelButton });
                dialog.AcceptButton = okButton;
                dialog.CancelButton = cancelButton;
                if (dialog.ShowDialog() != DialogResult.OK) return;
                string correction = correctionBox.Text.Trim();
                if (String.IsNullOrWhiteSpace(correction)) return;
                settings.Dictionary.RemoveAll(delegate(string entry)
                {
                    string[] map = Regex.Split(entry ?? "", @"\s*(?:=>|=)\s*", RegexOptions.None);
                    return map.Length > 0 && String.Equals(map[0].Trim(), lastDictationWord, StringComparison.OrdinalIgnoreCase);
                });
                settings.Dictionary.Add(lastDictationWord + " => " + correction);
                try
                {
                    store.Save(settings);
                    Notify("Dictionary updated", lastDictationWord + " → " + correction, ToolTipIcon.Info);
                }
                catch (Exception exception)
                {
                    store.LogError(exception);
                    Notify("Could not save dictionary", exception.Message, ToolTipIcon.Error);
                }
            }
        }

        private void ShowSettings()
        {
            if (settingsForm != null && !settingsForm.IsDisposed)
            {
                settingsForm.Activate();
                return;
            }
            settingsForm = new SettingsForm(store, settings, FlowtypeApp.AppDirectory,
                delegate { return recorder.IsRecording || processing; });
            settingsForm.HotkeyPreviewChanged += delegate(string hotkey)
            {
                if (String.IsNullOrWhiteSpace(hotkey)) return;
                settings.Hotkey = hotkey;
                hook.HotkeyName = hotkey;
                chordPolledDown = false;
                try { store.Save(settings); }
                catch (Exception exception) { store.LogError(exception); }
                SetReady();
            };
            settingsForm.SettingsSaved += delegate(AppSettings value, string key, string routerKey)
            {
                settings = value;
                apiKey = key;
                openRouterKey = routerKey;
                groqKey = store.LoadGroqKey();
                recorder.MicGain = settings.MicGain;
                hook.HotkeyName = settings.Hotkey;
                overlay.SetTheme(settings.OverlayTheme);
                SetReady();
                if (settings.Engine == "Local") WarmLocalEngine();
                else
                {
                    whisperEngine.Unload();
                    if (settings.Engine == "Groq" && !String.IsNullOrWhiteSpace(groqKey)) WarmGroqEngine();
                }
            };
            settingsForm.FormClosed += delegate { settingsForm = null; };
            settingsForm.Show();
            settingsForm.Activate();
        }

        private void ShowHistory()
        {
            if (historyForm != null && !historyForm.IsDisposed)
            {
                historyForm.Activate();
                return;
            }
            historyForm = new HistoryForm(history);
            historyForm.FormClosed += delegate { historyForm = null; };
            historyForm.Show();
            historyForm.Activate();
        }

        private void Notify(string title, string message, ToolTipIcon icon)
        {
            if (shuttingDown) return;
            tray.BalloonTipTitle = title;
            tray.BalloonTipText = ShortMessage(message);
            tray.BalloonTipIcon = icon;
            tray.ShowBalloonTip(3500);
        }

        private async void WarmLocalEngine()
        {
            try
            {
                statusItem.Text = "Loading local speech model…";
                await whisperEngine.WarmAsync(settings);
                if (!processing && !recorder.IsRecording) SetReady();
            }
            catch (Exception exception)
            {
                store.LogError(exception);
                if (!processing && !recorder.IsRecording) statusItem.Text = "Local engine will load on first use";
            }
        }

        private async void WarmGroqEngine()
        {
            try
            {
                statusItem.Text = "Connecting to Groq…";
                await groqEngine.WarmAsync(settings, groqKey);
                if (!processing && !recorder.IsRecording) SetReady();
            }
            catch (Exception exception)
            {
                store.LogError(exception);
                if (!processing && !recorder.IsRecording)
                    statusItem.Text = "Ready — Groq will connect on first dictation";
            }
        }

        private async void RemoveLegacyLargeModel()
        {
            try
            {
                string largePath = Path.Combine(FlowtypeApp.AppDirectory, "tools", "whisper", "models", "ggml-large-v3-turbo-q5_0.bin");
                if (File.Exists(largePath))
                {
                    whisperEngine.Unload();
                    File.Delete(largePath);
                }
                if (!String.IsNullOrWhiteSpace(settings.WhisperModelPath) &&
                    settings.WhisperModelPath.IndexOf("large-v3-turbo", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    LocalInstallResult result = await new LocalEngineInstaller(FlowtypeApp.AppDirectory).InstallAsync(delegate(int percent, string message) { });
                    settings.WhisperExePath = result.Executable;
                    settings.WhisperServerPath = result.ServerExecutable;
                    settings.WhisperModelPath = result.Model;
                    settings.LocalModelQuality = "Instant";
                    store.Save(settings);
                    await whisperEngine.WarmAsync(settings);
                }
            }
            catch (Exception exception) { store.LogError(exception); }
        }

        private static string ShortMessage(string value)
        {
            string text = Regex.Replace(value ?? "", @"\s+", " ").Trim();
            return text.Length <= 220 ? text : text.Substring(0, 217) + "…";
        }

        private static void OpenFolder(string path)
        {
            Directory.CreateDirectory(path);
            Process.Start(new ProcessStartInfo { FileName = "explorer.exe", Arguments = "\"" + path + "\"", UseShellExecute = true });
        }

        private static void TryDelete(string path)
        {
            try { if (!String.IsNullOrWhiteSpace(path) && File.Exists(path)) File.Delete(path); } catch { }
        }

        private void Quit()
        {
            shuttingDown = true;
            try { if (recorder.IsRecording) recorder.Cancel(); } catch { }
            try { hook.Dispose(); } catch { }
            try { chordPoller.Stop(); chordPoller.Dispose(); } catch { }
            try { activationPoller.Stop(); activationPoller.Dispose(); } catch { }
            try { recorder.Dispose(); } catch { }
            try { whisperEngine.Dispose(); } catch { }
            try { groqEngine.Dispose(); } catch { }
            try { overlay.Close(); overlay.Dispose(); } catch { }
            try { dispatcher.Dispose(); } catch { }
            tray.Visible = false;
            tray.Dispose();
            ExitThread();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && !shuttingDown) Quit();
            base.Dispose(disposing);
        }
    }

    public static class FlowtypeApp
    {
        private const string MutexName = @"Local\FlowtypeDesktop-9F33B64C";
        private const string ActivationEventName = @"Local\FlowtypeDesktop-Activate-9F33B64C";
        public static string AppDirectory { get; private set; }
        public static Icon ProductIcon { get; private set; }

        [STAThread]
        public static void Main()
        {
            try { Run(AppDomain.CurrentDomain.BaseDirectory); }
            catch (Exception exception)
            {
                try
                {
                    MessageBox.Show(
                        "Flowtype could not start:\r\n\r\n" + exception.Message,
                        "Flowtype",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }
                catch { }
            }
        }

        [STAThread]
        public static void Run(string appDirectory)
        {
            AppDirectory = appDirectory;
            LoadProductIcon();
            ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12;
            bool created;
            using (Mutex mutex = new Mutex(true, MutexName, out created))
            {
                if (!created)
                {
                    try
                    {
                        using (EventWaitHandle existing = EventWaitHandle.OpenExisting(ActivationEventName)) existing.Set();
                    }
                    catch { }
                    return;
                }
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                using (EventWaitHandle activation = new EventWaitHandle(false, EventResetMode.AutoReset, ActivationEventName))
                using (FlowtypeContext context = new FlowtypeContext(activation)) Application.Run(context);
                GC.KeepAlive(mutex);
            }
        }

        private static void LoadProductIcon()
        {
            try
            {
                string iconPath = Path.Combine(AppDirectory ?? "", "assets", "Flowtype.ico");
                if (File.Exists(iconPath))
                {
                    using (FileStream stream = File.OpenRead(iconPath))
                        ProductIcon = new Icon(stream);
                    return;
                }
            }
            catch { }
            try { ProductIcon = Icon.ExtractAssociatedIcon(Application.ExecutablePath); }
            catch { ProductIcon = SystemIcons.Application; }
        }
    }
}
