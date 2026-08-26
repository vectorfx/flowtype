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
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using System.Windows.Forms;
using Microsoft.Win32;

[assembly: System.Reflection.AssemblyVersion("1.3.71.0")]
[assembly: System.Reflection.AssemblyFileVersion("1.3.71.0")]

namespace Flowtype
{
    public sealed class AppSettings
    {
        public string Engine;
        public string CleanupProvider;
        public string Hotkey;
        public bool HandsFreeDoubleTap;
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
        public bool AutoCheckUpdates;
        public string SkippedUpdateVersion;
        public string LastUpdateCheckUtc;
        public string OverlayTheme;
        public string OverlayMark;
        public bool AgentModeEnabled;
        public string AgentHotkey;
        public string AgentEndpoint;
        public bool SpokenListsEnabled;
        public string SpokenBulletPhrase;
        public string SpokenNumberPhrase;
        public List<string> Dictionary;
        public Dictionary<string, string> Snippets;

        public static AppSettings Defaults()
        {
            AppSettings value = new AppSettings();
            value.Engine = "Local";
            value.CleanupProvider = "BuiltIn";
            value.Hotkey = "Win + Ctrl";
            value.HandsFreeDoubleTap = true;
            value.Style = "Natural";
            value.CleanupEnabled = true;
            value.ContextEnabled = true;
            value.AutoPaste = false;
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
            value.OllamaUrl = "http://127.0.0.1:11434";
            value.OllamaModel = "";
            value.GroqApiUrl = "https://api.groq.com/openai/v1";
            value.GroqTranscriptionModel = "whisper-large-v3-turbo";
            value.MicGain = 1.2f;
            value.TurboTranscription = true;
            value.SuppressNonSpeech = false;
            value.CompletionSound = true;
            value.ShowInsertNotification = false;
            value.AutoCheckUpdates = true;
            value.SkippedUpdateVersion = "";
            value.LastUpdateCheckUtc = "";
            value.OverlayTheme = "Dark";
            value.OverlayMark = "Orb";
            value.AgentModeEnabled = false;
            value.AgentHotkey = "Win + Alt";
            value.AgentEndpoint = "http://127.0.0.1:5599/ask";
            value.SpokenListsEnabled = true;
            value.SpokenBulletPhrase = "next point";
            value.SpokenNumberPhrase = "next number";
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
            if (String.IsNullOrWhiteSpace(OllamaUrl)) OllamaUrl = "http://127.0.0.1:11434";
            if (String.IsNullOrWhiteSpace(GroqApiUrl)) GroqApiUrl = "https://api.groq.com/openai/v1";
            if (String.IsNullOrWhiteSpace(GroqTranscriptionModel)) GroqTranscriptionModel = "whisper-large-v3-turbo";
            if (MicGain < 0.5f || MicGain > 3f) MicGain = 1.2f;
            if (String.Equals(Engine, "Groq", StringComparison.OrdinalIgnoreCase)) CleanupProvider = "BuiltIn";
            if (String.IsNullOrWhiteSpace(OverlayTheme)) OverlayTheme = "Dark";
            if (String.Equals(OverlayTheme, "Mono", StringComparison.OrdinalIgnoreCase)) OverlayTheme = "Dark";
            if (!String.Equals(OverlayTheme, "Glass", StringComparison.OrdinalIgnoreCase) &&
                !String.Equals(OverlayTheme, "Dark", StringComparison.OrdinalIgnoreCase) &&
                !String.Equals(OverlayTheme, "Purple", StringComparison.OrdinalIgnoreCase) &&
                !String.Equals(OverlayTheme, "Light", StringComparison.OrdinalIgnoreCase) &&
                !String.Equals(OverlayTheme, "Ember", StringComparison.OrdinalIgnoreCase))
                OverlayTheme = "Dark";
            if (String.IsNullOrWhiteSpace(OverlayMark)) OverlayMark = "Orb";
            if (!String.Equals(OverlayMark, "Orb", StringComparison.OrdinalIgnoreCase) &&
                !String.Equals(OverlayMark, "Hex", StringComparison.OrdinalIgnoreCase) &&
                !String.Equals(OverlayMark, "Iris", StringComparison.OrdinalIgnoreCase) &&
                !String.Equals(OverlayMark, "Grid", StringComparison.OrdinalIgnoreCase))
                OverlayMark = "Orb";
            if (SpokenBulletPhrase == null) SpokenBulletPhrase = "next point";
            if (SpokenNumberPhrase == null) SpokenNumberPhrase = "next number";
            if (String.IsNullOrWhiteSpace(AgentEndpoint) || !AgentBridge.IsLoopbackEndpoint(AgentEndpoint))
                AgentEndpoint = "http://127.0.0.1:5599/ask";
            bool agentChordKnown = false;
            foreach (string name in Hotkeys.Names)
                if (String.Equals(name, AgentHotkey, StringComparison.OrdinalIgnoreCase)) { agentChordKnown = true; break; }
            if (!agentChordKnown) AgentHotkey = "Win + Alt";
            // The agent chord must never shadow the dictation chord — transcript mode always wins.
            if (String.Equals(AgentHotkey, Hotkey, StringComparison.OrdinalIgnoreCase))
                AgentHotkey = String.Equals(Hotkey, "Win + Alt", StringComparison.OrdinalIgnoreCase) ? "Win + Shift" : "Win + Alt";
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
            if (TryLoadSansPair(root, "SpaceGrotesk-Regular.ttf", "SpaceGrotesk-Bold.ttf", "Space Grotesk")) return;
            if (TryLoadSingleFont(Path.Combine(root, "SpaceGrotesk-Regular.ttf"))) return;
            try
            {
                FontFamily sans = new FontFamily("Segoe UI Variable");
                ui = new Font(sans, 9.25f);
                uiLarge = new Font(sans, 10.25f);
                uiBold = new Font(sans, 9.75f, FontStyle.Bold);
                return;
            }
            catch { }
            ui = new Font("Segoe UI", 9.25f);
            uiLarge = new Font("Segoe UI", 10.25f);
            uiBold = new Font("Segoe UI", 9.75f, FontStyle.Bold);
        }

        private static bool TryLoadSansPair(string root, string regularFile, string boldFile, string familyName)
        {
            string regularPath = Path.Combine(root, regularFile);
            string boldPath = Path.Combine(root, boldFile);
            if (!File.Exists(regularPath)) return false;
            try
            {
                Collection.AddFontFile(regularPath);
                FontFamily regular = FindFamily(Collection, familyName);
                FontFamily bold = regular;
                if (File.Exists(boldPath))
                {
                    Collection.AddFontFile(boldPath);
                    bold = FindFamily(Collection, familyName) ?? regular;
                }
                if (regular == null) return false;
                ui = new Font(regular, 9.25f);
                uiLarge = new Font(regular, 10.5f);
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
                    " ms · clean " + LastCleanMs + " ms · pipeline " + LastTotalMs + " ms";
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

    public sealed class MicLevel
    {
        public string Band = "ok";
        public string Message = "";
        public int VoicePercent;

        public static MicLevel Evaluate(float rawPeak, float boostedPeak, float gain)
        {
            MicLevel result = new MicLevel();
            result.VoicePercent = (int)Math.Round(Math.Max(0f, Math.Min(1f, rawPeak)) * 100f);
            if (IsTooHot(boostedPeak) || rawPeak >= 0.85f)
            {
                result.Band = "hot";
                result.Message = "Too hot — lower boost. The meter is your real voice at the mic, not a quality score.";
                return result;
            }
            if (rawPeak >= 0.15f)
            {
                result.Band = "ok";
                result.Message = gain > 1.4f
                    ? "Voice is already loud at the mic. Lower boost toward 1.0–1.2× — 2.0× just raises noise."
                    : "Voice level is in the pocket. Leave boost where it is.";
                return result;
            }
            if (rawPeak >= 0.08f)
            {
                result.Band = "ok";
                result.Message = "Usable. Speak so the meter sits around 15–40%. Boost is optional.";
                return result;
            }
            result.Band = "quiet";
            result.Message = gain >= 1.8f
                ? "Still quiet at high boost. Move closer or use a louder mic — more boost will hiss."
                : "Quiet at the mic. Move closer, or raise boost one step if the meter stays under 10%.";
            return result;
        }

        public static bool ShouldRaiseBoost(float rawPeak, float gain)
        {
            return rawPeak < 0.08f && gain < 1.8f;
        }

        public static bool IsTooHot(float boostedPeak)
        {
            return boostedPeak >= 0.88f;
        }
    }

    public sealed class PendingInsert
    {
        public string Text = "";
        public bool PressEnter;
        public bool Undo;
        public ForegroundInfo Delivery;
        public bool AutoPaste;
        public int Sequence;
    }

    public sealed class OrderedInsertQueue
    {
        private int next = 1;
        private readonly Dictionary<int, PendingInsert> waiting = new Dictionary<int, PendingInsert>();
        private readonly HashSet<int> skipped = new HashSet<int>();

        public List<string> Complete(int sequence, string text)
        {
            List<PendingInsert> jobs = Finish(sequence, text == null ? null : new PendingInsert { Text = text, Sequence = sequence });
            List<string> values = new List<string>();
            foreach (PendingInsert job in jobs) values.Add(job.Text);
            return values;
        }

        public List<string> Skip(int sequence)
        {
            return Complete(sequence, (string)null);
        }

        public List<PendingInsert> Finish(int sequence, PendingInsert job)
        {
            if (job != null) waiting[sequence] = job;
            else skipped.Add(sequence);
            List<PendingInsert> drain = new List<PendingInsert>();
            while (waiting.ContainsKey(next) || skipped.Contains(next))
            {
                PendingInsert ready;
                if (waiting.TryGetValue(next, out ready))
                {
                    drain.Add(ready);
                    waiting.Remove(next);
                }
                else skipped.Remove(next);
                next++;
            }
            return drain;
        }
    }

    // Agent chord transport: the finished transcript is handed to ONE local runtime as a
    // plain POST. Flowtype never plans, never routes, never executes — if nothing is
    // listening the send fails closed and dictation is untouched.
    public static class AgentBridge
    {
        public sealed class Result
        {
            public string Reply;
            public long Ms;
            public bool IsQuestion;
            public bool IsPaste;
            public string Focus;
        }

        // The daemon answers only when the work is done, so the wait is the agent's
        // thinking time, not a network timeout. Ten minutes is the ceiling; the HUD
        // shows the clock the whole way.
        private const int RequestTimeoutMs = 600000;

        // The daemon mints a per-boot token into AppData. Loopback is not a trust
        // boundary — any web page the user visits can POST to 127.0.0.1 — so every
        // request carries the token and the daemon refuses anything without it.
        public static string TokenForStatus() { return Token(); }

        public static bool IsLoopbackEndpoint(string endpoint)
        {
            if (String.IsNullOrWhiteSpace(endpoint)) return false;
            try
            {
                Uri uri = new Uri(endpoint.Trim());
                if (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps) return false;
                string host = uri.Host ?? "";
                if (String.Equals(host, "localhost", StringComparison.OrdinalIgnoreCase)) return true;
                IPAddress address;
                if (!IPAddress.TryParse(host, out address)) return false;
                return IPAddress.IsLoopback(address);
            }
            catch
            {
                return false;
            }
        }

        private static string Token()
        {
            try
            {
                string path = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "Flowtype", "agent-token");
                return File.Exists(path) ? File.ReadAllText(path).Trim() : "";
            }
            catch { return ""; }
        }

        public static void Send(string transcript, string endpoint, ForegroundInfo seat, Action<Result> onDone, Action<string> onFailed)
        {
            if (!IsLoopbackEndpoint(endpoint))
            {
                if (onFailed != null) onFailed("Agent endpoint must be on this PC (127.0.0.1 or localhost).");
                return;
            }
            Thread worker = new Thread(delegate()
            {
                Stopwatch clock = Stopwatch.StartNew();
                try
                {
                    JavaScriptSerializer serializer = new JavaScriptSerializer();
                    Dictionary<string, object> body = new Dictionary<string, object>();
                    body["text"] = transcript ?? "";
                    body["source"] = "flowtype";
                    body["sentUtc"] = DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture);
                    // The seat: what the person was actually looking at when they spoke.
                    // It is the one thing a terminal agent can never know, and it is what
                    // makes "this", "that one" and "the file I've got open" resolvable.
                    if (seat != null)
                    {
                        Dictionary<string, object> context = new Dictionary<string, object>();
                        context["window"] = seat.Title ?? "";
                        context["app"] = seat.ProcessName ?? "";
                        body["context"] = context;
                    }
                    byte[] payload = Encoding.UTF8.GetBytes(serializer.Serialize(body));
                    HttpWebRequest request = (HttpWebRequest)WebRequest.Create(endpoint);
                    request.Method = "POST";
                    request.ContentType = "application/json; charset=utf-8";
                    request.Headers["X-Flowtype-Token"] = Token();
                    request.Timeout = RequestTimeoutMs;
                    request.ReadWriteTimeout = RequestTimeoutMs;
                    request.Proxy = null;
                    request.KeepAlive = false;
                    using (Stream stream = request.GetRequestStream())
                        stream.Write(payload, 0, payload.Length);
                    string raw;
                    using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                    using (StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8))
                        raw = reader.ReadToEnd();
                    clock.Stop();
                    Result result = new Result();
                    result.Ms = clock.ElapsedMilliseconds;
                    result.Reply = ExtractReply(raw);
                    result.IsQuestion = ExtractFlag(raw, "question");
                    result.IsPaste = ExtractFlag(raw, "paste");
                    result.Focus = ExtractText(raw, "focus");
                    if (onDone != null) onDone(result);
                }
                catch (Exception exception)
                {
                    if (onFailed != null) onFailed(ShortError(exception));
                }
            });
            worker.IsBackground = true;
            worker.Name = "AgentBridgeSend";
            worker.Start();
        }

        private static string ExtractReply(string raw)
        {
            if (String.IsNullOrWhiteSpace(raw)) return "done";
            try
            {
                JavaScriptSerializer serializer = new JavaScriptSerializer();
                Dictionary<string, object> parsed = serializer.Deserialize<Dictionary<string, object>>(raw);
                if (parsed != null)
                {
                    object reply;
                    if (parsed.TryGetValue("reply", out reply) && reply != null && Convert.ToString(reply).Trim().Length > 0)
                        return Convert.ToString(reply).Trim();
                    object status;
                    if (parsed.TryGetValue("status", out status)) return Convert.ToString(status);
                }
            }
            catch { }
            return raw.Trim();
        }

        // Sibling routes off the configured /ask endpoint, so one setting still
        // configures the whole bridge.
        public static string Sibling(string endpoint, string route)
        {
            if (String.IsNullOrWhiteSpace(endpoint)) return "";
            try
            {
                Uri uri = new Uri(endpoint);
                return uri.Scheme + "://" + uri.Host + ":" + uri.Port + route;
            }
            catch { return ""; }
        }

        public static void Abort(string endpoint)
        {
            if (!IsLoopbackEndpoint(endpoint)) return;
            string url = Sibling(endpoint, "/abort");
            if (url.Length == 0) return;
            Thread worker = new Thread(delegate()
            {
                try
                {
                    HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                    request.Method = "POST";
                    request.ContentLength = 0;
                    request.Headers["X-Flowtype-Token"] = Token();
                    request.Timeout = 4000;
                    request.Proxy = null;
                    using (request.GetResponse()) { }
                }
                catch { }
            });
            worker.IsBackground = true;
            worker.Name = "AgentBridgeAbort";
            worker.Start();
        }

        // Deferred notices: things the agent was asked to watch for, arriving later.
        public static void FetchNotices(string endpoint, Action<List<string>> onNotices)
        {
            if (!IsLoopbackEndpoint(endpoint)) return;
            string url = Sibling(endpoint, "/notices");
            if (url.Length == 0) return;
            Thread worker = new Thread(delegate()
            {
                try
                {
                    HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                    request.Method = "GET";
                    request.Headers["X-Flowtype-Token"] = Token();
                    request.Timeout = 3000;
                    request.ReadWriteTimeout = 3000;
                    request.Proxy = null;
                    string raw;
                    using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                    using (StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8))
                        raw = reader.ReadToEnd();
                    List<string> lines = new List<string>();
                    JavaScriptSerializer serializer = new JavaScriptSerializer();
                    Dictionary<string, object> parsed = serializer.Deserialize<Dictionary<string, object>>(raw);
                    object notices;
                    if (parsed != null && parsed.TryGetValue("notices", out notices))
                    {
                        // JavaScriptSerializer hands back an ArrayList for an untyped JSON
                        // array, never object[] — casting to the latter silently drops every
                        // notice. Enumerate the interface instead.
                        System.Collections.IEnumerable items = notices as System.Collections.IEnumerable;
                        if (items != null)
                            foreach (object item in items)
                            {
                                Dictionary<string, object> entry = item as Dictionary<string, object>;
                                if (entry == null) continue;
                                object text;
                                if (entry.TryGetValue("text", out text) && text != null)
                                    lines.Add(Convert.ToString(text));
                            }
                    }
                    if (lines.Count > 0 && onNotices != null) onNotices(lines);
                }
                catch { }
            });
            worker.IsBackground = true;
            worker.Name = "AgentBridgeNotices";
            worker.Start();
        }

        private static string ExtractText(string raw, string key)
        {
            if (String.IsNullOrWhiteSpace(raw)) return "";
            try
            {
                JavaScriptSerializer serializer = new JavaScriptSerializer();
                Dictionary<string, object> parsed = serializer.Deserialize<Dictionary<string, object>>(raw);
                object value;
                if (parsed != null && parsed.TryGetValue(key, out value) && value != null)
                    return Convert.ToString(value).Trim();
            }
            catch { }
            return "";
        }

        private static bool ExtractFlag(string raw, string key)
        {
            if (String.IsNullOrWhiteSpace(raw)) return false;
            try
            {
                JavaScriptSerializer serializer = new JavaScriptSerializer();
                Dictionary<string, object> parsed = serializer.Deserialize<Dictionary<string, object>>(raw);
                object value;
                if (parsed != null && parsed.TryGetValue(key, out value) && value != null)
                    return Convert.ToBoolean(value);
            }
            catch { }
            return false;
        }

        private static string ShortError(Exception exception)
        {
            WebException web = exception as WebException;
            if (web != null && web.Status == WebExceptionStatus.ConnectFailure) return "no listener on that port";
            if (web != null && web.Response != null)
            {
                try
                {
                    using (StreamReader reader = new StreamReader(web.Response.GetResponseStream(), Encoding.UTF8))
                    {
                        string body = reader.ReadToEnd();
                        string parsed = ExtractReply(body);
                        if (!String.IsNullOrWhiteSpace(parsed)) return parsed;
                    }
                }
                catch { }
            }
            return exception.Message;
        }
    }

    // Diagnostic trail for the agent chord: one line per hop so a dead chord names the
    // hop that swallowed it. Cheap, append-only, never throws.
    public static class AgentTrace
    {
        private static readonly object gate = new object();
        public static void Log(string message)
        {
            try
            {
                string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Flowtype", "agent-trace.log");
                lock (gate)
                    File.AppendAllText(path,
                        DateTime.Now.ToString("HH:mm:ss.fff", CultureInfo.InvariantCulture) + "  " + message + Environment.NewLine,
                        new UTF8Encoding(false));
            }
            catch { }
        }
    }

    // What the voice asked and what came back, in plain text you can open and read.
    // The HUD is a glance; this is the record — and the answer to "what did it just say?"
    public static class AgentReplyLog
    {
        private static readonly object gate = new object();

        public static string Path
        {
            get
            {
                return System.IO.Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Flowtype", "agent-replies.log");
            }
        }

        public static void Append(string ask, string reply, long ms, bool ok)
        {
            try
            {
                StringBuilder entry = new StringBuilder();
                entry.Append(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture));
                entry.Append(ok ? "  ASK  " : "  ASK (failed)  ");
                entry.AppendLine((ask ?? "").Trim());
                foreach (string line in (reply ?? "").Replace("\r\n", "\n").Replace("\r", "\n").Split('\n'))
                    entry.AppendLine("    " + line);
                if (ok) entry.AppendLine("    (" + (ms / 1000.0).ToString("0.0", CultureInfo.InvariantCulture) + "s)");
                entry.AppendLine();
                lock (gate)
                    File.AppendAllText(Path, entry.ToString(), new UTF8Encoding(false));
            }
            catch { }
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
            { "Win + Alt", 0 },
            { "Win + Shift", 0 },
            { "Ctrl + Shift", 0 },
            { "Right Ctrl", 0xA3 },
            { "Left Ctrl", 0xA2 },
            { "Right Alt", 0xA5 },
            { "Left Alt", 0xA4 },
            { "Caps Lock", 0x14 },
            { "Scroll Lock", 0x91 },
            { "Pause", 0x13 },
            { "F8", 0x77 },
            { "F9", 0x78 },
            { "F10", 0x79 },
            { "F12", 0x7B }
        };

        private static readonly HashSet<string> ModifierChords = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "Win + Ctrl", "Win + Alt", "Win + Shift", "Ctrl + Shift"
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

        public static bool IsModifierChord(string name)
        {
            return ModifierChords.Contains(name ?? "");
        }

        public static bool IsChord(string name)
        {
            return IsModifierChord(name);
        }
    }

    public sealed class HotkeyChordTracker
    {
        private bool winDown;
        private bool controlDown;
        private bool altDown;
        private bool shiftDown;
        private readonly string chordName;
        public bool Active { get; private set; }

        public HotkeyChordTracker(string chord)
        {
            chordName = String.IsNullOrWhiteSpace(chord) ? "Win + Ctrl" : chord;
        }

        public bool Update(uint key, bool down, out bool active)
        {
            if (!Handles(key, chordName))
            {
                active = Active;
                return false;
            }
            bool before = Active;
            if (key == 0x5B || key == 0x5C) winDown = down;
            if (key == 0x11 || key == 0xA2 || key == 0xA3) controlDown = down;
            if (key == 0x12 || key == 0xA4 || key == 0xA5) altDown = down;
            if (key == 0x10 || key == 0xA0 || key == 0xA1) shiftDown = down;
            Active = EvaluateActive();
            active = Active;
            return before != Active;
        }

        private bool EvaluateActive()
        {
            if (String.Equals(chordName, "Win + Alt", StringComparison.OrdinalIgnoreCase))
                return winDown && altDown;
            if (String.Equals(chordName, "Win + Shift", StringComparison.OrdinalIgnoreCase))
                return winDown && shiftDown;
            if (String.Equals(chordName, "Ctrl + Shift", StringComparison.OrdinalIgnoreCase))
                return controlDown && shiftDown;
            return winDown && controlDown;
        }

        public void Reset()
        {
            winDown = false;
            controlDown = false;
            altDown = false;
            shiftDown = false;
            Active = false;
        }

        public static bool Handles(uint key, string chordName)
        {
            if (String.Equals(chordName, "Win + Alt", StringComparison.OrdinalIgnoreCase))
                return key == 0x5B || key == 0x5C || key == 0x12 || key == 0xA4 || key == 0xA5;
            if (String.Equals(chordName, "Win + Shift", StringComparison.OrdinalIgnoreCase))
                return key == 0x5B || key == 0x5C || key == 0x10 || key == 0xA0 || key == 0xA1;
            if (String.Equals(chordName, "Ctrl + Shift", StringComparison.OrdinalIgnoreCase))
                return key == 0x11 || key == 0xA2 || key == 0xA3 || key == 0x10 || key == 0xA0 || key == 0xA1;
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

        private static bool WinDown()
        {
            return Down(0x5B) || Down(0x5C);
        }

        private static bool ControlDown()
        {
            return Down(0x11) || Down(0xA2) || Down(0xA3);
        }

        private static bool AltDown()
        {
            return Down(0x12) || Down(0xA4) || Down(0xA5);
        }

        private static bool ShiftDown()
        {
            return Down(0x10) || Down(0xA0) || Down(0xA1);
        }

        public static bool IsHotkeyDown(string hotkeyName)
        {
            if (String.Equals(hotkeyName, "Win + Ctrl", StringComparison.OrdinalIgnoreCase))
                return WinDown() && ControlDown();
            if (String.Equals(hotkeyName, "Win + Alt", StringComparison.OrdinalIgnoreCase))
                return WinDown() && AltDown();
            if (String.Equals(hotkeyName, "Win + Shift", StringComparison.OrdinalIgnoreCase))
                return WinDown() && ShiftDown();
            if (String.Equals(hotkeyName, "Ctrl + Shift", StringComparison.OrdinalIgnoreCase))
                return ControlDown() && ShiftDown();
            int code = Hotkeys.Code(hotkeyName);
            return code != 0 && Down(code);
        }

        public static bool IsWinCtrlDown()
        {
            return IsHotkeyDown("Win + Ctrl");
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
                if (raw.IndexOf("HandsFreeDoubleTap", StringComparison.OrdinalIgnoreCase) < 0) value.HandsFreeDoubleTap = true;
                if (raw.IndexOf("SpokenListsEnabled", StringComparison.OrdinalIgnoreCase) < 0) value.SpokenListsEnabled = true;
                string autoPasteMarker = Path.Combine(Root, "autopaste-default-v2.applied");
                if (!File.Exists(autoPasteMarker))
                {
                    value.AutoPaste = false;
                    try { File.WriteAllText(autoPasteMarker, DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture), new UTF8Encoding(false)); } catch { }
                }
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

        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern bool IsWindow(IntPtr hWnd);

        [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern IntPtr SendMessageTimeout(IntPtr hWnd, uint msg, IntPtr wParam, StringBuilder lParam, uint flags, uint timeoutMs, out IntPtr result);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr SendMessageTimeout(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam, uint flags, uint timeoutMs, out IntPtr result);

        private const uint SMTO_ABORTIFHUNG = 0x0002;
        private const uint CaretProbeTimeoutMs = 200;

        private static bool TrySendMessageTimeout(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam, out int value)
        {
            IntPtr result;
            IntPtr ok = SendMessageTimeout(hWnd, msg, wParam, lParam, SMTO_ABORTIFHUNG, CaretProbeTimeoutMs, out result);
            value = result.ToInt32();
            return ok != IntPtr.Zero;
        }

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern int GetClassName(IntPtr hWnd, StringBuilder className, int maxCount);

        [DllImport("user32.dll")]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint processId);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool GetGUIThreadInfo(uint threadId, ref GuiThreadInfo info);

        [DllImport("user32.dll")]
        private static extern void keybd_event(byte key, byte scan, uint flags, UIntPtr extraInfo);

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern IntPtr SendMessage(IntPtr hWnd, uint msg, IntPtr wParam, StringBuilder lParam);

        [DllImport("user32.dll")]
        private static extern IntPtr SendMessage(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

        private const uint KEYEVENTF_KEYUP = 0x0002;
        private const uint KEYEVENTF_EXTENDEDKEY = 0x0001;
        private const int ContextProbeChars = 16;
        private const string CaretProbeSentinel = "\uE100FLOWTYPE_CARET\uE100";
        private const uint EM_GETSEL = 0x00B0;
        private const uint WM_GETTEXT = 0x000D;
        private const uint WM_GETTEXTLENGTH = 0x000E;
        private const uint WM_PASTE = 0x0302;
        private static readonly object deliverGate = new object();
        private static int lastDeliveredGeneration = -1;
        private static DateTime lastPasteUtc = DateTime.MinValue;
        private static string lastPastePayload = "";
        private static Control clipboardMarshal;
        private static string pinnedClipboardText;
        private static bool pinnedClipboardHasText;
        private static bool clipboardRemembered;
        private static IntPtr lastAppendTarget = IntPtr.Zero;
        private static bool lastAppendEndedWithPunctuation;
        // Only guards against hook double-fires; a real repeated dictation ("lgtm" twice in a
        // row) must paste again, so this must stay well under record+transcribe turnaround.
        private const int PasteDebounceMs = 300;

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

        public static bool IsFlowtypeForeground()
        {
            ForegroundInfo current = Capture(false);
            return current != null && String.Equals(current.ProcessName, "Flowtype", StringComparison.OrdinalIgnoreCase);
        }

        public static bool LooksUnpasteableProcess(string processName)
        {
            string process = (processName ?? "").Trim().ToLowerInvariant();
            if (process.Length == 0) return false;
            if (process == "flowtype") return true;
            if (process == "consent" || process == "logonui" || process == "lockapp") return true;
            if (process == "searchhost" || process == "startmenuexperiencehost" || process == "shellexperiencehost") return true;
            if (process == "textinputhost" || process == "securityhealthsystray") return true;
            return false;
        }

        public static bool LooksUnpasteableClass(string className)
        {
            string name = (className ?? "").Trim();
            if (name.Length == 0) return false;
            if (String.Equals(name, "#32768", StringComparison.OrdinalIgnoreCase)) return true;
            if (String.Equals(name, "#32769", StringComparison.OrdinalIgnoreCase)) return true;
            if (String.Equals(name, "Shell_TrayWnd", StringComparison.OrdinalIgnoreCase)) return true;
            if (String.Equals(name, "Shell_SecondaryTrayWnd", StringComparison.OrdinalIgnoreCase)) return true;
            if (String.Equals(name, "NotifyIconOverflowWindow", StringComparison.OrdinalIgnoreCase)) return true;
            if (String.Equals(name, "Progman", StringComparison.OrdinalIgnoreCase)) return true;
            if (String.Equals(name, "WorkerW", StringComparison.OrdinalIgnoreCase)) return true;
            if (String.Equals(name, "ForegroundStaging", StringComparison.OrdinalIgnoreCase)) return true;
            if (String.Equals(name, "MultitaskingViewFrame", StringComparison.OrdinalIgnoreCase)) return true;
            if (String.Equals(name, "TaskSwitcherWnd", StringComparison.OrdinalIgnoreCase)) return true;
            if (String.Equals(name, "XamlExplorerHostIslandWindow", StringComparison.OrdinalIgnoreCase)) return true;
            if (String.Equals(name, "SysShadow", StringComparison.OrdinalIgnoreCase)) return true;
            return false;
        }

        // Consoles ignore Ctrl+V (or bind it to something else). They take Ctrl+Shift+V,
        // and we cannot read the buffer to prove the insert landed — so a "success" here
        // used to restore the previous clipboard and throw away a whole take.
        public static bool LooksLikeShiftPasteProcess(string processName)
        {
            string process = (processName ?? "").Trim().ToLowerInvariant();
            if (process.Length == 0) return false;
            switch (process)
            {
                case "windowsterminal":
                case "windowsterminalpreview":
                case "openconsole":
                case "conhost":
                case "wt":
                case "cmd":
                case "powershell":
                case "pwsh":
                case "alacritty":
                case "wezterm":
                case "wezterm-gui":
                case "mintty":
                case "putty":
                case "kitty":
                case "hyper":
                case "tabby":
                    return true;
                default:
                    return false;
            }
        }

        public static bool LooksLikeShiftPasteClass(string className)
        {
            string name = (className ?? "").Trim();
            if (name.Length == 0) return false;
            if (String.Equals(name, "ConsoleWindowClass", StringComparison.OrdinalIgnoreCase)) return true;
            if (String.Equals(name, "CASCADIA_HOSTING_WINDOW_CLASS", StringComparison.OrdinalIgnoreCase)) return true;
            if (String.Equals(name, "PseudoConsoleWindow", StringComparison.OrdinalIgnoreCase)) return true;
            if (String.Equals(name, "mintty", StringComparison.OrdinalIgnoreCase)) return true;
            if (String.Equals(name, "VirtualConsoleClass", StringComparison.OrdinalIgnoreCase)) return true;
            return false;
        }

        public static bool LooksLikeShiftPasteTarget(ForegroundInfo field)
        {
            if (field == null) return false;
            if (LooksLikeShiftPasteProcess(field.ProcessName)) return true;
            if (field.Handle != IntPtr.Zero && LooksLikeShiftPasteClass(WindowClass(field.Handle))) return true;
            return false;
        }

        public static bool NameLooksLikeTerminalPane(string name)
        {
            string value = (name ?? "").Trim();
            if (value.Length == 0) return false;
            if (value.IndexOf('.') >= 0) return false;
            return Regex.IsMatch(value,
                @"^(?:\d+:\s*)?(terminal(\s+\d+)?|powershell|pwsh|cmd|command prompt|bash|zsh|fish|wsl|ubuntu|debian|alpine|kali|mintty|git bash)(?:\s*[:#\-].*)?$",
                RegexOptions.IgnoreCase);
        }

        public static bool ShouldKeepDictationOnClipboard(ForegroundInfo intended, ForegroundInfo after)
        {
            if (LooksLikeShiftPasteTarget(intended)) return true;
            return PasteLikelyMissed(intended, after);
        }

        public static bool IsUnpasteableTarget(ForegroundInfo field)
        {
            if (field == null) return true;
            if (LooksUnpasteableProcess(field.ProcessName)) return true;
            if (field.Handle == IntPtr.Zero) return true;
            if (!IsWindow(field.Handle)) return true;
            if (LooksUnpasteableClass(WindowClass(field.Handle))) return true;
            return IsGuiBlocked(field.Handle);
        }

        public static string PrepareInsertText(string text, ForegroundInfo context)
        {
            if (String.IsNullOrEmpty(text)) return text ?? "";
            // Cursor/VS Code can't expose real caret neighbors — CaretFit heuristics strip
            // punctuation and break normal dictation. Keep legacy spacing-only there.
            if (IsCursorFamily(context))
                return PrepareInsertTextLegacySpacing(text, context);

            CaretNeighborhood caret = TryGetCaretNeighborhood(context);
            if (!caret.Available)
                caret = TryProbeCaretBySelection(context);
            bool midContinuity = CanAssumeMidSentenceContinuity(context);
            IntPtr identity = InsertIdentity(context);
            bool afterPriorSentence = identity != IntPtr.Zero
                && identity == lastAppendTarget
                && lastAppendEndedWithPunctuation;
            return CaretFit.Apply(text, caret, midContinuity, afterPriorSentence, HasRealTarget(context));
        }

        private static string PrepareInsertTextLegacySpacing(string text, ForegroundInfo context)
        {
            if (text.StartsWith(" ", StringComparison.Ordinal) || text.StartsWith("\n", StringComparison.Ordinal)) return text;
            char previous;
            if (TryGetCharBeforeCaret(context, out previous))
            {
                if (!Char.IsWhiteSpace(previous)) return " " + text;
                return text;
            }
            if (ShouldPrependFromLastInsert(context, text)) return " " + text;
            return text;
        }

        private static bool ShouldPrependFromLastInsert(ForegroundInfo context, string text)
        {
            if (context == null || context.FocusHandle == IntPtr.Zero) return false;
            if (context.FocusHandle != lastAppendTarget) return false;
            if (!lastAppendEndedWithPunctuation || text.Length == 0) return false;
            return Char.IsUpper(text[0]);
        }

        private static bool HasRealTarget(ForegroundInfo context)
        {
            return context != null && (context.FocusHandle != IntPtr.Zero || context.Handle != IntPtr.Zero);
        }

        public static void NoteSuccessfulInsert(ForegroundInfo context, string insertedText)
        {
            lastAppendTarget = InsertIdentity(context);
            string inserted = insertedText ?? "";
            bool endedWithBreak = inserted.Length > 0 && (inserted[inserted.Length - 1] == '\n' || inserted[inserted.Length - 1] == '\r');
            string trimmed = inserted.TrimEnd();
            lastAppendEndedWithPunctuation = endedWithBreak
                || (trimmed.Length > 0 && ".!?…".IndexOf(trimmed[trimmed.Length - 1]) >= 0);
        }

        private static IntPtr InsertIdentity(ForegroundInfo context)
        {
            if (context == null) return IntPtr.Zero;
            if (context.FocusHandle != IntPtr.Zero) return context.FocusHandle;
            return context.Handle;
        }

        private static bool CanAssumeMidSentenceContinuity(ForegroundInfo context)
        {
            IntPtr identity = InsertIdentity(context);
            if (identity == IntPtr.Zero) return false;
            if (identity != lastAppendTarget) return false;
            return !lastAppendEndedWithPunctuation;
        }

        public static CaretNeighborhood TryGetCaretNeighborhood(ForegroundInfo context)
        {
            if (context == null) return CaretNeighborhood.Unavailable();
            IntPtr hwnd = context.FocusHandle;
            if (hwnd == IntPtr.Zero || !IsWindow(hwnd))
            {
                if (context.Handle == IntPtr.Zero || !IsWindow(context.Handle))
                    return CaretNeighborhood.Unavailable();
                uint processId;
                uint threadId = GetWindowThreadProcessId(context.Handle, out processId);
                hwnd = FocusedWindow(threadId);
            }
            if (hwnd == IntPtr.Zero || !IsWindow(hwnd)) return CaretNeighborhood.Unavailable();

            // Timeouts everywhere: a plain SendMessage to a hung target (busy Electron/Office
            // window) blocks Flowtype's UI thread forever. Degrade to Unavailable instead.
            int selection;
            if (!TrySendMessageTimeout(hwnd, EM_GETSEL, IntPtr.Zero, IntPtr.Zero, out selection))
                return CaretNeighborhood.Unavailable();
            int caretStart = selection & 0xFFFF;
            int caretEnd = (selection >> 16) & 0xFFFF;
            bool hasSelection = caretStart != caretEnd;

            int length;
            if (!TrySendMessageTimeout(hwnd, WM_GETTEXTLENGTH, IntPtr.Zero, IntPtr.Zero, out length))
                return CaretNeighborhood.Unavailable();
            if (length < 0) return CaretNeighborhood.Unavailable();
            if (length == 0) return CaretNeighborhood.Known('\0', '\0', hasSelection);

            // EM_GETSEL packs positions into 16 bits, so anything past 65535 is unaddressable —
            // reading more than that is wasted cross-process marshalling.
            if (length > 65535) length = 65535;
            StringBuilder buffer = new StringBuilder(length + 2);
            IntPtr textResult;
            if (SendMessageTimeout(hwnd, WM_GETTEXT, new IntPtr(length + 1), buffer, SMTO_ABORTIFHUNG, CaretProbeTimeoutMs, out textResult) == IntPtr.Zero)
                return CaretNeighborhood.Unavailable();
            string fieldText = buffer.ToString();
            if (fieldText.Length == 0) return CaretNeighborhood.Known('\0', '\0', hasSelection);

            int insertAt = Math.Max(caretStart, caretEnd);
            // EM_GETSEL 0,0 is ambiguous: unsupported control, caret at start, or caret at end.
            // Guessing "end" made mid-field pastes (Cursor/Electron) get a bogus left char from the
            // last letter in the buffer → extra leading space and no trailing space before the next word.
            if (insertAt <= 0 && caretStart == 0 && caretEnd == 0 && !hasSelection)
                return CaretNeighborhood.Unavailable();
            if (insertAt < 0) insertAt = 0;
            if (insertAt > fieldText.Length) insertAt = fieldText.Length;

            char immediateLeft = insertAt > 0 ? fieldText[insertAt - 1] : '\0';
            char immediateRight = insertAt < fieldText.Length ? fieldText[insertAt] : '\0';
            char semanticLeft = CaretNeighborhood.ReadSemanticLeft(fieldText, insertAt);
            int leftStart = Math.Max(0, insertAt - 48);
            CaretNeighborhood value = CaretNeighborhood.Known(immediateLeft, immediateRight, hasSelection, semanticLeft);
            value.LeftSnippet = fieldText.Substring(leftStart, insertAt - leftStart);
            value.RightSnippet = insertAt < fieldText.Length
                ? fieldText.Substring(insertAt, Math.Min(8, fieldText.Length - insertAt))
                : "";
            return value;
        }

        private static bool CanProbeSelection(ForegroundInfo context)
        {
            if (context == null || IsCursorFamily(context)) return false;
            IntPtr hwnd = context.Handle;
            if (hwnd == IntPtr.Zero || !IsWindow(hwnd)) return false;
            IntPtr foreground = GetForegroundWindow();
            if (foreground == hwnd) return true;
            return context.FocusHandle != IntPtr.Zero && foreground == context.FocusHandle;
        }

        public static void SetClipboardMarshal(Control control)
        {
            clipboardMarshal = control;
        }

        public static void RememberClipboard()
        {
            pinnedClipboardHasText = false;
            pinnedClipboardText = null;
            clipboardRemembered = true;
            try
            {
                if (Clipboard.ContainsText())
                {
                    pinnedClipboardText = Clipboard.GetText();
                    pinnedClipboardHasText = true;
                }
            }
            catch { }
        }

        public static void ReapplyUserClipboard()
        {
            if (!clipboardRemembered) return;
            try
            {
                if (pinnedClipboardHasText && pinnedClipboardText != null)
                    Clipboard.SetText(pinnedClipboardText);
                else
                    Clipboard.Clear();
            }
            catch { }
        }

        public static void ForgetClipboard()
        {
            pinnedClipboardText = null;
            pinnedClipboardHasText = false;
            clipboardRemembered = false;
        }

        public static void RestoreRememberedClipboard(string dictationPayload)
        {
            try
            {
                string current = null;
                try
                {
                    if (Clipboard.ContainsText()) current = Clipboard.GetText();
                }
                catch { }
                if (!CanRestoreOver(current, dictationPayload, pinnedClipboardHasText ? pinnedClipboardText : null))
                    return;
                if (pinnedClipboardHasText && pinnedClipboardText != null)
                    Clipboard.SetText(pinnedClipboardText);
                else
                    Clipboard.Clear();
            }
            catch { }
            ForgetClipboard();
        }

        public static void ScheduleClipboardRestore(string dictationPayload)
        {
            string payload = dictationPayload;
            Control marshal = clipboardMarshal;
            ThreadPool.QueueUserWorkItem(delegate
            {
                Thread.Sleep(550);
                Action restore = delegate { RestoreRememberedClipboard(payload); };
                try
                {
                    if (marshal != null && !marshal.IsDisposed && marshal.IsHandleCreated)
                        marshal.BeginInvoke(restore);
                    else
                        restore();
                }
                catch
                {
                    try { restore(); } catch { }
                }
            });
        }

        public static bool CanRestoreOver(string currentClipboard, string dictationPayload)
        {
            return CanRestoreOver(currentClipboard, dictationPayload, null);
        }

        public static bool CanRestoreOver(string currentClipboard, string dictationPayload, string pinnedClipboard)
        {
            if (String.IsNullOrEmpty(currentClipboard)) return true;
            if (SameClipboardText(currentClipboard, dictationPayload)) return true;
            if (SameClipboardText(currentClipboard, pinnedClipboard)) return true;
            if (String.Equals(currentClipboard, CaretProbeSentinel, StringComparison.Ordinal))
                return true;
            return false;
        }

        private static bool SameClipboardText(string left, string right)
        {
            if (String.IsNullOrEmpty(left) || String.IsNullOrEmpty(right)) return false;
            if (String.Equals(left, right, StringComparison.Ordinal)) return true;
            return String.Equals(NormalizeClipboardText(left), NormalizeClipboardText(right), StringComparison.Ordinal);
        }

        private static string NormalizeClipboardText(string value)
        {
            return (value ?? "").Replace("\r\n", "\n").Replace('\r', '\n').Trim();
        }

        private static CaretNeighborhood TryProbeCaretBySelection(ForegroundInfo context)
        {
            if (!CanProbeSelection(context)) return CaretNeighborhood.Unavailable();

            string previous = null;
            bool hadPrevious = false;
            try
            {
                if (Clipboard.ContainsText())
                {
                    previous = Clipboard.GetText();
                    hadPrevious = true;
                }
            }
            catch { }

            try
            {
                Clipboard.SetText(CaretProbeSentinel);
            }
            catch
            {
                return CaretNeighborhood.Unavailable();
            }

            try
            {
                HoldKey(0x10, true, false);
                for (int index = 0; index < ContextProbeChars; index++)
                    PulseKey(0x25, true);
                HoldKey(0x10, false, false);
                HoldKey(0x11, true, false);
                PulseKey(0x43, false);
                HoldKey(0x11, false, false);
                Thread.Sleep(45);

                string copied = null;
                try
                {
                    if (Clipboard.ContainsText()) copied = Clipboard.GetText();
                }
                catch { }

                PulseKey(0x27, true);

                if (String.IsNullOrEmpty(copied) || String.Equals(copied, CaretProbeSentinel, StringComparison.Ordinal))
                    return CaretNeighborhood.Unavailable();
                if (copied.IndexOf(CaretProbeSentinel, StringComparison.Ordinal) >= 0)
                    return CaretNeighborhood.Unavailable();
                return CaretNeighborhood.FromSnippets(copied, "", false);
            }
            catch
            {
                return CaretNeighborhood.Unavailable();
            }
            finally
            {
                HoldKey(0x10, false, false);
                HoldKey(0x11, false, false);
                try
                {
                    if (clipboardRemembered) ReapplyUserClipboard();
                    else if (hadPrevious && previous != null) Clipboard.SetText(previous);
                    else Clipboard.Clear();
                }
                catch { }
            }
        }

        private static void HoldKey(byte vk, bool down, bool extended)
        {
            uint flags = down ? 0u : KEYEVENTF_KEYUP;
            if (extended) flags |= KEYEVENTF_EXTENDEDKEY;
            keybd_event(vk, 0, flags, UIntPtr.Zero);
        }

        private static void PulseKey(byte vk, bool extended)
        {
            HoldKey(vk, true, extended);
            HoldKey(vk, false, extended);
        }

        private static bool TryGetCharBeforeCaret(ForegroundInfo context, out char previous)
        {
            CaretNeighborhood caret = TryGetCaretNeighborhood(context);
            previous = caret.ImmediateLeft;
            return caret.Available && !caret.AtStart;
        }

        public static bool IsCursorFamily(ForegroundInfo context)
        {
            if (context == null) return false;
            string process = (context.ProcessName ?? "").Trim();
            return String.Equals(process, "Cursor", StringComparison.OrdinalIgnoreCase)
                || String.Equals(process, "Code", StringComparison.OrdinalIgnoreCase);
        }

        public static void ResetDeliverGuard(int generation)
        {
            lock (deliverGate)
            {
                if (generation > lastDeliveredGeneration) lastDeliveredGeneration = generation - 1;
            }
        }

        // True when the last DeliverDictation call reported success without actually pasting
        // (a swallowed duplicate). Read by the caller to keep the trailing Enter honest.
        public static bool LastDeliverySuppressed;

        // True when the last delivery was a console/terminal that wants Ctrl+Shift+V, so the
        // tray toast can tell the user the right paste chord instead of Ctrl+V.
        public static bool LastDeliveryNeedsShiftPaste;

        public static bool DeliverDictation(string text, ForegroundInfo original, bool keepOnClipboard, int generation)
        {
            lock (deliverGate)
            {
                LastDeliverySuppressed = false;
                LastDeliveryNeedsShiftPaste = false;
                if (generation >= 0 && generation == lastDeliveredGeneration)
                {
                    LastDeliverySuppressed = true;
                    return true;
                }
                bool inserted = DeliverDictationCore(text, original, keepOnClipboard);
                if (inserted && generation >= 0) lastDeliveredGeneration = generation;
                return inserted;
            }
        }

        public static bool DeliverDictation(string text, ForegroundInfo original, bool keepOnClipboard)
        {
            LastDeliverySuppressed = false;
            LastDeliveryNeedsShiftPaste = false;
            return DeliverDictationCore(text, original, keepOnClipboard);
        }

        private static bool DeliverDictationCore(string text, ForegroundInfo original, bool keepOnClipboard)
        {
            ForegroundInfo field = ResolveDeliveryTarget(original);
            if (field == null || IsFlowtypeForeground() || IsUnpasteableTarget(field))
            {
                string early = PrepareInsertText(text ?? "", original ?? field);
                if (early.Length == 0) early = (text ?? "").Trim();
                if (early.Length == 0) return false;
                LastDeliveryNeedsShiftPaste = NeedsShiftPaste(original) || NeedsShiftPaste(field);
                return RescueToClipboard(early);
            }

            string payload = PrepareInsertText(text ?? "", field);
            if (payload.Length == 0)
            {
                RestoreRememberedClipboard(null);
                return false;
            }

            // Caret probing can take a couple hundred ms — a popup may have landed since.
            field = ResolveDeliveryTarget(original ?? field);
            if (field == null || IsFlowtypeForeground() || IsUnpasteableTarget(field))
            {
                LastDeliveryNeedsShiftPaste = NeedsShiftPaste(original) || NeedsShiftPaste(field);
                return RescueToClipboard(payload);
            }

            if (String.Equals(payload, lastPastePayload, StringComparison.Ordinal) &&
                (DateTime.UtcNow - lastPasteUtc).TotalMilliseconds < PasteDebounceMs)
            {
                LastDeliverySuppressed = true;
                if (clipboardRemembered)
                    RestoreRememberedClipboard(payload);
                return true;
            }

            bool cursorFamily = IsCursorFamily(field);
            bool shiftPaste = NeedsShiftPaste(field);
            LastDeliveryNeedsShiftPaste = shiftPaste;
            if (!clipboardRemembered) RememberClipboard();

            Exception clipError = null;
            for (int attempt = 0; attempt < 6; attempt++)
            {
                try
                {
                    Clipboard.SetText(payload);
                    clipError = null;
                    break;
                }
                catch (Exception exception)
                {
                    clipError = exception;
                    Thread.Sleep(40 * (attempt + 1));
                }
            }
            if (clipError != null) throw clipError;

            ForegroundInfo live = ResolveDeliveryTarget(original ?? field);
            if (live == null || IsFlowtypeForeground() || IsUnpasteableTarget(live))
            {
                LastDeliveryNeedsShiftPaste = shiftPaste || NeedsShiftPaste(original) || NeedsShiftPaste(live);
                return RescueToClipboard(payload);
            }
            field = live;
            shiftPaste = shiftPaste || NeedsShiftPaste(field);
            LastDeliveryNeedsShiftPaste = shiftPaste;

            // Consoles need Ctrl+Shift+V. Everywhere else Ctrl+V — Unicode typing does not
            // reach Cursor's Electron composer.
            Thread.Sleep(cursorFamily || shiftPaste ? 50 : 20);
            SendPasteChord(shiftPaste);

            Thread.Sleep(shiftPaste ? 60 : 40);
            bool missed = shiftPaste || PasteLikelyMissed(field, Capture(false));
            if (missed)
            {
                lastPastePayload = payload;
                lastPasteUtc = DateTime.UtcNow;
                return RescueToClipboard(payload);
            }
            if (keepOnClipboard)
                ForgetClipboard();
            else
                ScheduleClipboardRestore(payload);
            lastPastePayload = payload;
            lastPasteUtc = DateTime.UtcNow;
            NoteSuccessfulInsert(field, payload);
            return true;
        }

        private static bool NeedsShiftPaste(ForegroundInfo field)
        {
            if (LooksLikeShiftPasteTarget(field)) return true;
            return IsCursorFamily(field) && FocusedPaneLooksLikeTerminal();
        }

        private static void SendPasteChord(bool shiftPaste)
        {
            HoldKey(0x11, true, false);
            if (shiftPaste) HoldKey(0x10, true, false);
            PulseKey(0x56, false);
            if (shiftPaste) HoldKey(0x10, false, false);
            HoldKey(0x11, false, false);
        }

        private static bool uiaLoadAttempted;
        private static PropertyInfo uiaFocusedProperty;

        private static bool FocusedPaneLooksLikeTerminal()
        {
            try
            {
                PropertyInfo focusedProperty = UiaFocusedElementProperty();
                if (focusedProperty == null) return false;
                object focused = focusedProperty.GetValue(null, null);
                if (focused == null) return false;
                PropertyInfo currentProperty = focused.GetType().GetProperty("Current");
                if (currentProperty == null) return false;
                object current = currentProperty.GetValue(focused, null);
                if (current == null) return false;
                Type currentType = current.GetType();
                string name = ReadAutomationString(currentType, current, "Name");
                if (NameLooksLikeTerminalPane(name)) return true;
                string automationId = ReadAutomationString(currentType, current, "AutomationId");
                if (automationId.IndexOf("terminal", StringComparison.OrdinalIgnoreCase) >= 0)
                    return true;
                string className = ReadAutomationString(currentType, current, "ClassName");
                if (className.IndexOf("TermControl", StringComparison.OrdinalIgnoreCase) >= 0)
                    return true;
                if (className.IndexOf("Terminal", StringComparison.OrdinalIgnoreCase) >= 0)
                    return true;
                return false;
            }
            catch
            {
                return false;
            }
        }

        private static string ReadAutomationString(Type type, object current, string propertyName)
        {
            PropertyInfo property = type.GetProperty(propertyName);
            if (property == null) return "";
            object value = property.GetValue(current, null);
            return Convert.ToString(value ?? "", CultureInfo.InvariantCulture);
        }

        private static PropertyInfo UiaFocusedElementProperty()
        {
            if (uiaLoadAttempted) return uiaFocusedProperty;
            uiaLoadAttempted = true;
            try
            {
                Assembly uia = Assembly.Load("UIAutomationClient, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35");
                Type elementType = uia.GetType("System.Windows.Automation.AutomationElement");
                if (elementType == null) return null;
                uiaFocusedProperty = elementType.GetProperty("FocusedElement");
            }
            catch
            {
                uiaFocusedProperty = null;
            }
            return uiaFocusedProperty;
        }

        private static ForegroundInfo ResolveDeliveryTarget(ForegroundInfo original)
        {
            ForegroundInfo field = Capture(false);
            if (original != null && original.Handle != IntPtr.Zero && field.Handle != original.Handle)
                return TryRefocus(original);
            return field;
        }

        private static bool RescueToClipboard(string payload)
        {
            if (String.IsNullOrEmpty(payload)) return false;
            ForgetClipboard();
            TryClipboardOnly(payload);
            return false;
        }

        private static bool PasteLikelyMissed(ForegroundInfo intended, ForegroundInfo after)
        {
            if (intended == null || after == null || after.Handle == IntPtr.Zero) return false;
            if (after.Handle == intended.Handle) return false;
            if (intended.FocusHandle != IntPtr.Zero && after.Handle == intended.FocusHandle) return false;
            if (IsUnpasteableTarget(after)) return true;
            if (!String.Equals(after.ProcessName ?? "", intended.ProcessName ?? "", StringComparison.OrdinalIgnoreCase))
                return true;
            string afterClass = WindowClass(after.Handle);
            if (LooksUnpasteableClass(afterClass)) return true;
            if (String.Equals(afterClass, "#32770", StringComparison.OrdinalIgnoreCase)) return true;
            return false;
        }

        private static string WindowClass(IntPtr hwnd)
        {
            if (hwnd == IntPtr.Zero) return "";
            try
            {
                StringBuilder buffer = new StringBuilder(256);
                GetClassName(hwnd, buffer, buffer.Capacity);
                return buffer.ToString();
            }
            catch { return ""; }
        }

        private const uint GUI_INMOVESIZE = 0x00000002;
        private const uint GUI_INMENUMODE = 0x00000004;
        private const uint GUI_SYSTEMMENUMODE = 0x00000008;
        private const uint GUI_POPUPMENUMODE = 0x00000010;

        private static bool IsGuiBlocked(IntPtr hwnd)
        {
            if (hwnd == IntPtr.Zero) return false;
            try
            {
                uint processId;
                uint threadId = GetWindowThreadProcessId(hwnd, out processId);
                GuiThreadInfo info = new GuiThreadInfo();
                info.Size = Marshal.SizeOf(typeof(GuiThreadInfo));
                if (!GetGUIThreadInfo(threadId, ref info)) return false;
                uint flags = info.Flags;
                if ((flags & (GUI_INMENUMODE | GUI_SYSTEMMENUMODE | GUI_POPUPMENUMODE | GUI_INMOVESIZE)) != 0)
                    return true;
                return false;
            }
            catch { return false; }
        }

        private delegate bool EnumWindowsCallback(IntPtr window, IntPtr parameter);

        [DllImport("user32.dll")]
        private static extern bool EnumWindows(EnumWindowsCallback callback, IntPtr parameter);

        [DllImport("user32.dll")]
        private static extern bool IsWindowVisible(IntPtr window);

        // Raise a window the agent just opened. This has to live here, not in the daemon:
        // Windows refuses a foreground change from a background process, and Flowtype is
        // the one process in the loop that owns the keyboard hook, so it is allowed to.
        // A page that opens behind the editor reads as nothing having happened at all.
        public static bool RaiseWindow(string hint)
        {
            if (String.IsNullOrWhiteSpace(hint)) return false;
            string needle = hint.Trim().ToLowerInvariant();
            IntPtr found = IntPtr.Zero;
            try
            {
                EnumWindows(delegate(IntPtr window, IntPtr parameter)
                {
                    if (!IsWindowVisible(window)) return true;
                    StringBuilder title = new StringBuilder(512);
                    GetWindowText(window, title, title.Capacity);
                    string caption = title.ToString();
                    if (caption.Trim().Length == 0) return true;

                    string process = "";
                    try
                    {
                        uint processId;
                        GetWindowThreadProcessId(window, out processId);
                        process = Process.GetProcessById((int)processId).ProcessName;
                    }
                    catch { }

                    if (caption.ToLowerInvariant().IndexOf(needle, StringComparison.Ordinal) >= 0
                        || process.ToLowerInvariant().IndexOf(needle, StringComparison.Ordinal) >= 0)
                    {
                        found = window;
                        return false;
                    }
                    return true;
                }, IntPtr.Zero);
            }
            catch { return false; }

            if (found == IntPtr.Zero) return false;
            try
            {
                SetForegroundWindow(found);
                Thread.Sleep(60);
                return GetForegroundWindow() == found;
            }
            catch { return false; }
        }

        private static ForegroundInfo TryRefocus(ForegroundInfo original)
        {
            try
            {
                if (!IsWindow(original.Handle)) return null;
                SetForegroundWindow(original.Handle);
                Thread.Sleep(80);
                ForegroundInfo current = Capture(false);
                return current.Handle == original.Handle ? current : null;
            }
            catch { return null; }
        }

        private static bool TryClipboardOnly(string payload)
        {
            Exception last = null;
            for (int attempt = 0; attempt < 6; attempt++)
            {
                try
                {
                    Clipboard.SetText(payload);
                    return false;
                }
                catch (Exception exception)
                {
                    last = exception;
                    Thread.Sleep(40 * (attempt + 1));
                }
            }
            if (last != null) throw last;
            return false;
        }

        public static void PressEnter()
        {
            keybd_event(0x0D, 0, 0, UIntPtr.Zero);
            keybd_event(0x0D, 0, KEYEVENTF_KEYUP, UIntPtr.Zero);
        }

        public static bool TryFocus(ForegroundInfo original)
        {
            return TryRefocus(original) != null;
        }

        public static void UndoLastInsert()
        {
            Thread.Sleep(20);
            keybd_event(0x11, 0, 0, UIntPtr.Zero);
            keybd_event(0x5A, 0, 0, UIntPtr.Zero);
            keybd_event(0x5A, 0, KEYEVENTF_KEYUP, UIntPtr.Zero);
            keybd_event(0x11, 0, KEYEVENTF_KEYUP, UIntPtr.Zero);
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
        private HotkeyChordTracker chordTracker;
        public string HotkeyName
        {
            get { return hotkeyName; }
            set
            {
                hotkeyName = String.IsNullOrWhiteSpace(value) ? "Right Ctrl" : value;
                primaryKey = Hotkeys.Code(hotkeyName);
                chordTracker = new HotkeyChordTracker(hotkeyName);
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
                if (!injected && Hotkeys.IsModifierChord(hotkeyName) && (down || up) && HotkeyChordTracker.Handles(data.vkCode, hotkeyName))
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
                // A modifier chord has no real primary key — Hotkeys.Code falls back to Right
                // Ctrl for chords, so without this guard a "Win + Alt" hook would swallow every
                // Right Ctrl press and fire phantom chord events.
                if (!injected && !Hotkeys.IsModifierChord(hotkeyName) && data.vkCode == (uint)primaryKey && (down || up))
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

        public void ResetChordTracker()
        {
            if (chordTracker != null) chordTracker.Reset();
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

    public sealed class PcmRing
    {
        private readonly byte[] data;
        private int next;
        private int count;

        public PcmRing(int byteCapacity)
        {
            int size = Math.Max(2, byteCapacity);
            if ((size & 1) != 0) size--;
            data = new byte[size];
        }

        public int Capacity
        {
            get { return data.Length; }
        }

        public void Write(byte[] chunk)
        {
            if (chunk == null || chunk.Length == 0) return;
            int offset = 0;
            int remaining = chunk.Length;
            if (remaining >= data.Length)
            {
                offset = remaining - data.Length;
                System.Buffer.BlockCopy(chunk, offset, data, 0, data.Length);
                next = 0;
                count = data.Length;
                return;
            }
            while (remaining > 0)
            {
                int take = Math.Min(data.Length - next, remaining);
                System.Buffer.BlockCopy(chunk, offset, data, next, take);
                next = (next + take) % data.Length;
                count = Math.Min(data.Length, count + take);
                offset += take;
                remaining -= take;
            }
        }

        public byte[] Snapshot()
        {
            if (count <= 0) return new byte[0];
            byte[] copy = new byte[count];
            if (count < data.Length)
            {
                System.Buffer.BlockCopy(data, 0, copy, 0, count);
                return copy;
            }
            int tail = data.Length - next;
            System.Buffer.BlockCopy(data, next, copy, 0, tail);
            if (next > 0) System.Buffer.BlockCopy(data, 0, copy, tail, next);
            return copy;
        }

        public void Clear()
        {
            next = 0;
            count = 0;
            Array.Clear(data, 0, data.Length);
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
        private const int PrerollMs = 400;
        private readonly object gate = new object();
        private readonly List<Buffer> buffers = new List<Buffer>();
        private readonly PcmRing preroll = new PcmRing(SampleRate * 2 * PrerollMs / 1000);
        private readonly ManualResetEventSlim drained = new ManualResetEventSlim(true);
        private WaveCallback callback;
        private IntPtr input;
        private FileStream rawStream;
        private string rawPath;
        private string wavePath;
        private readonly object micLock = new object();
        private volatile bool running;
        private volatile bool writingFile;
        private volatile bool takeActive;
        private volatile bool keepWarm = true;
        private int drainReturns;
        private int drainTarget;
        private bool deviceOpen;
        public float MicGain { get; set; }
        public event Action<AudioMeterReading> LevelChanged;

        public sealed class AudioMeterReading
        {
            public float Raw;
            public float Boosted;
            public float RawPeak;
            public float BoostedPeak;
        }

        public WaveRecorder()
        {
            MicGain = 1.2f;
        }

        public bool IsRecording { get { return takeActive; } }

        public void Prime()
        {
            lock (micLock)
            {
                keepWarm = true;
                if (takeActive || deviceOpen) return;
                try { OpenDevice(); }
                catch
                {
                    ReleaseDevice();
                }
            }
        }

        public void ReleaseWarm()
        {
            lock (micLock)
            {
                if (takeActive) return;
                keepWarm = false;
                preroll.Clear();
                StopDriver();
                ReleaseDevice();
            }
        }

        public void Start(string outputWavePath)
        {
            lock (micLock)
            {
                if (takeActive) throw new InvalidOperationException("The recorder is already running.");
                wavePath = outputWavePath;
                rawPath = outputWavePath + ".pcm";
                Directory.CreateDirectory(Path.GetDirectoryName(outputWavePath));
                rawStream = new FileStream(rawPath, FileMode.Create, FileAccess.Write, FileShare.Read);
                lock (gate)
                {
                    byte[] lead = preroll.Snapshot();
                    preroll.Clear();
                    if (lead.Length > 0) rawStream.Write(lead, 0, lead.Length);
                    writingFile = true;
                    takeActive = true;
                }
                try
                {
                    if (!deviceOpen) OpenDevice();
                }
                catch
                {
                    writingFile = false;
                    takeActive = false;
                    try { rawStream.Dispose(); } catch { }
                    rawStream = null;
                    ReleaseDevice();
                    throw;
                }
            }
        }

        private void OpenDevice()
        {
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
                input = IntPtr.Zero;
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
            running = true;
            deviceOpen = true;
            Check(waveInStart(input));
        }

        private void OnWaveMessage(IntPtr source, uint message, IntPtr instance, IntPtr parameter1, IntPtr parameter2)
        {
            if (message != DataMessage || parameter1 == IntPtr.Zero) return;
            WaveHeader header = (WaveHeader)Marshal.PtrToStructure(parameter1, typeof(WaveHeader));
            if (header.bytesRecorded > 0)
            {
                byte[] data = new byte[header.bytesRecorded];
                Marshal.Copy(header.data, data, 0, data.Length);
                float rawPeak = MeasurePeak(data);
                ApplyGainInPlace(data);
                float boostedPeak = MeasurePeak(data);
                lock (gate)
                {
                    preroll.Write(data);
                    if (writingFile && rawStream != null) rawStream.Write(data, 0, data.Length);
                }
                if (takeActive)
                {
                    AudioMeterReading reading = new AudioMeterReading();
                    reading.RawPeak = rawPeak;
                    reading.BoostedPeak = boostedPeak;
                    reading.Raw = BuildMeter(rawPeak);
                    reading.Boosted = BuildMeter(boostedPeak);
                    Action<AudioMeterReading> levelHandler = LevelChanged;
                    if (levelHandler != null) levelHandler(reading);
                }
            }
            if (running) waveInAddBuffer(input, parameter1, (uint)Marshal.SizeOf(typeof(WaveHeader)));
            else if (Interlocked.Increment(ref drainReturns) >= drainTarget) drained.Set();
        }

        private static float MeasurePeak(byte[] data)
        {
            float peak = 0;
            for (int index = 0; index + 1 < data.Length; index += 2)
            {
                short sample = (short)(data[index] | (data[index + 1] << 8));
                peak = Math.Max(peak, Math.Abs(sample / 32768f));
            }
            return peak;
        }

        private static float BuildMeter(float peak)
        {
            return Math.Min(1f, peak * 1.35f);
        }

        public static int EstimateWhisperPeakPercent(byte[] pcm)
        {
            if (pcm == null || pcm.Length < 2) return 0;
            int peak = 1;
            for (int index = 0; index + 1 < pcm.Length; index += 2)
            {
                int sample = AbsPcmSample((short)(pcm[index] | (pcm[index + 1] << 8)));
                if (sample > peak) peak = sample;
            }
            float target = 24000f;
            float normalize = peak < 1200 ? Math.Min(3.5f, target / peak) : Math.Min(2.2f, target / peak);
            if (peak >= 28000) normalize = Math.Min(1f, target / peak);
            else if (peak >= 20000) normalize = Math.Min(1f, normalize);
            if (normalize > 4.5f) normalize = 4.5f;
            int whisperPeak = SoftLimitSample((int)Math.Round(Math.Min(peak, 32767) * normalize));
            return (int)Math.Round(whisperPeak / 327.67f);
        }

        public static byte[] TrimSilence(byte[] pcm, int sampleRate, int padMs)
        {
            if (pcm == null || pcm.Length < 4) return pcm ?? new byte[0];
            int samples = pcm.Length / 2;
            int peak = 1;
            for (int index = 0; index < samples; index++)
            {
                int sample = AbsPcmSample((short)(pcm[index * 2] | (pcm[index * 2 + 1] << 8)));
                if (sample > peak) peak = sample;
            }
            int speechFloor = Math.Max(350, peak / 20);
            int edgeFloor = Math.Max(160, peak / 55);
            int first = -1;
            int last = -1;
            for (int index = 0; index < samples; index++)
            {
                int sample = AbsPcmSample((short)(pcm[index * 2] | (pcm[index * 2 + 1] << 8)));
                if (sample < speechFloor) continue;
                if (first < 0) first = index;
                last = index;
            }
            if (first < 0) return pcm;
            while (first > 0)
            {
                int sample = AbsPcmSample((short)(pcm[(first - 1) * 2] | (pcm[(first - 1) * 2 + 1] << 8)));
                if (sample < edgeFloor) break;
                first--;
            }
            while (last < samples - 1)
            {
                int sample = AbsPcmSample((short)(pcm[(last + 1) * 2] | (pcm[(last + 1) * 2 + 1] << 8)));
                if (sample < edgeFloor) break;
                last++;
            }
            int pad = Math.Max(0, (int)Math.Round(sampleRate * (padMs / 1000.0)));
            int start = Math.Max(0, first - pad);
            int end = Math.Min(samples - 1, last + pad);
            int keep = end - start + 1;
            if (keep * 2 >= pcm.Length) return pcm;
            if (keep < sampleRate / 5) return pcm;
            byte[] trimmed = new byte[keep * 2];
            System.Buffer.BlockCopy(pcm, start * 2, trimmed, 0, trimmed.Length);
            return trimmed;
        }

        public string Stop()
        {
            lock (micLock)
            {
                if (!takeActive) return wavePath;
                DrainCallbacks();
                writingFile = false;
                takeActive = false;
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
                preroll.Clear();
                ReleaseDevice();
                if (keepWarm)
                {
                    try { OpenDevice(); }
                    catch { ReleaseDevice(); }
                }
                return wavePath;
            }
        }

        private void DrainCallbacks()
        {
            if (input == IntPtr.Zero || buffers.Count == 0)
            {
                running = false;
                return;
            }
            drainTarget = buffers.Count;
            Interlocked.Exchange(ref drainReturns, 0);
            drained.Reset();
            running = false;
            waveInStop(input);
            waveInReset(input);
            drained.Wait(300);
        }

        private void StopDriver()
        {
            running = false;
            if (input == IntPtr.Zero) return;
            try { waveInStop(input); } catch { }
            try { waveInReset(input); } catch { }
            Thread.Sleep(20);
        }

        private void ReleaseDevice()
        {
            running = false;
            deviceOpen = false;
            if (input == IntPtr.Zero)
            {
                buffers.Clear();
                return;
            }
            int headerSize = Marshal.SizeOf(typeof(WaveHeader));
            foreach (Buffer buffer in buffers)
            {
                try { waveInUnprepareHeader(input, buffer.Header, (uint)headerSize); } catch { }
                try { Marshal.FreeHGlobal(buffer.Header); } catch { }
                try { Marshal.FreeHGlobal(buffer.Data); } catch { }
            }
            buffers.Clear();
            try { waveInClose(input); } catch { }
            input = IntPtr.Zero;
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
            pcm = TrimSilence(pcm, SampleRate, 160);
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
            keepWarm = false;
            try
            {
                if (takeActive) Cancel();
                else
                {
                    StopDriver();
                    ReleaseDevice();
                }
            }
            catch { }
            try { drained.Dispose(); } catch { }
        }
    }

    public static class FlowtypeVersion
    {
        private static readonly Version CurrentVersion = typeof(FlowtypeVersion).Assembly.GetName().Version;

        public static Version Current { get { return CurrentVersion; } }

        public static string CurrentLabel
        {
            get
            {
                return CurrentVersion.Major + "." + CurrentVersion.Minor + "." + CurrentVersion.Build;
            }
        }

        public static Version ParseTag(string tag)
        {
            if (String.IsNullOrWhiteSpace(tag)) return null;
            string value = tag.Trim();
            if (value.StartsWith("v", StringComparison.OrdinalIgnoreCase)) value = value.Substring(1);
            try { return new Version(value); }
            catch { return null; }
        }

        public static bool IsNewerThanCurrent(string tag)
        {
            Version remote = ParseTag(tag);
            if (remote == null) return false;
            return remote > CurrentVersion;
        }
    }

    public sealed class AppUpdater
    {
        private const string LatestReleaseUrl = "https://api.github.com/repos/vectorfx/flowtype/releases/latest";
        private readonly JavaScriptSerializer serializer = new JavaScriptSerializer();

        public sealed class ReleaseInfo
        {
            public string TagName = "";
            public string Name = "";
            public string DownloadUrl = "";
            public string Body = "";
        }

        public async Task<ReleaseInfo> FetchLatestAsync()
        {
            using (HttpClient http = new HttpClient())
            {
                http.Timeout = TimeSpan.FromSeconds(30);
                http.DefaultRequestHeaders.UserAgent.ParseAdd("Flowtype-Desktop/" + FlowtypeVersion.CurrentLabel);
                http.DefaultRequestHeaders.Accept.ParseAdd("application/vnd.github+json");
                string body = await http.GetStringAsync(LatestReleaseUrl);
                Dictionary<string, object> release = serializer.DeserializeObject(body) as Dictionary<string, object>;
                if (release == null) throw new InvalidOperationException("Could not read the latest release.");
                ReleaseInfo info = new ReleaseInfo();
                info.TagName = Convert.ToString(release.ContainsKey("tag_name") ? release["tag_name"] : "", CultureInfo.InvariantCulture).Trim();
                info.Name = Convert.ToString(release.ContainsKey("name") ? release["name"] : info.TagName, CultureInfo.InvariantCulture).Trim();
                info.Body = Convert.ToString(release.ContainsKey("body") ? release["body"] : "", CultureInfo.InvariantCulture).Trim();
                object assetsValue;
                if (!release.TryGetValue("assets", out assetsValue)) throw new InvalidOperationException("Release has no downloadable assets.");
                IEnumerable assets = assetsValue as IEnumerable;
                if (assets == null) throw new InvalidOperationException("Release has no downloadable assets.");
                info.DownloadUrl = PickReleaseZipUrl(assets);
                if (String.IsNullOrWhiteSpace(info.DownloadUrl)) throw new InvalidOperationException("Release ZIP not found on GitHub.");
                if (!info.DownloadUrl.StartsWith("https://github.com/vectorfx/flowtype/releases/download/", StringComparison.OrdinalIgnoreCase))
                    throw new InvalidOperationException("Unexpected update download URL.");
                return info;
            }
        }

        public static string PickReleaseZipUrl(IEnumerable assets)
        {
            string fullUrl = "";
            string liteUrl = "";
            if (assets == null) return "";
            foreach (object assetValue in assets)
            {
                Dictionary<string, object> asset = assetValue as Dictionary<string, object>;
                if (asset == null) continue;
                string name = Convert.ToString(asset.ContainsKey("name") ? asset["name"] : "", CultureInfo.InvariantCulture);
                string url = Convert.ToString(asset.ContainsKey("browser_download_url") ? asset["browser_download_url"] : "", CultureInfo.InvariantCulture).Trim();
                if (String.IsNullOrWhiteSpace(url)) continue;
                if (name.EndsWith("-Full.zip", StringComparison.OrdinalIgnoreCase)) fullUrl = url;
                else if (name.EndsWith("-Lite.zip", StringComparison.OrdinalIgnoreCase)) liteUrl = url;
            }
            return fullUrl.Length > 0 ? fullUrl : liteUrl;
        }

        public async Task DownloadAndInstallAsync(ReleaseInfo release, Action<int, string> progress)
        {
            if (release == null || String.IsNullOrWhiteSpace(release.DownloadUrl))
                throw new InvalidOperationException("No update package is available.");
            string tempRoot = Path.Combine(Path.GetTempPath(), "Flowtype-update-" + Guid.NewGuid().ToString("N"));
            string zipPath = Path.Combine(tempRoot, "flowtype.zip");
            string extractPath = Path.Combine(tempRoot, "package");
            Directory.CreateDirectory(tempRoot);
            try
            {
                if (progress != null) progress(5, "Downloading update…");
                using (HttpClient http = new HttpClient())
                {
                    http.Timeout = TimeSpan.FromMinutes(20);
                    http.DefaultRequestHeaders.UserAgent.ParseAdd("Flowtype-Desktop/" + FlowtypeVersion.CurrentLabel);
                    using (HttpResponseMessage response = await http.GetAsync(release.DownloadUrl, HttpCompletionOption.ResponseHeadersRead))
                    {
                        response.EnsureSuccessStatusCode();
                        long total = response.Content.Headers.ContentLength ?? 0;
                        using (Stream input = await response.Content.ReadAsStreamAsync())
                        using (FileStream output = new FileStream(zipPath, FileMode.Create, FileAccess.Write, FileShare.None))
                        {
                            byte[] buffer = new byte[65536];
                            long received = 0;
                            int read;
                            while ((read = await input.ReadAsync(buffer, 0, buffer.Length)) > 0)
                            {
                                await output.WriteAsync(buffer, 0, read);
                                received += read;
                                if (progress != null && total > 0)
                                {
                                    int percent = 5 + (int)Math.Min(70, received * 70 / total);
                                    progress(percent, "Downloading update…");
                                }
                            }
                        }
                    }
                }
                if (progress != null) progress(78, "Extracting update…");
                Directory.CreateDirectory(extractPath);
                ExtractZipSafely(zipPath, extractPath);
                string installer = Directory.GetFiles(extractPath, "Install-Flowtype.ps1", SearchOption.AllDirectories).FirstOrDefault();
                if (String.IsNullOrWhiteSpace(installer)) throw new InvalidOperationException("Installer script missing from update package.");
                string extractRoot = Path.GetFullPath(extractPath);
                if (!Path.GetFullPath(installer).StartsWith(extractRoot, StringComparison.OrdinalIgnoreCase))
                    throw new InvalidOperationException("Installer path was outside the update package.");
                if (progress != null) progress(92, "Installing update…");
                ProcessStartInfo start = new ProcessStartInfo();
                start.FileName = "powershell.exe";
                start.Arguments = "-NoProfile -ExecutionPolicy Bypass -WindowStyle Hidden -File \"" + installer + "\" -Silent";
                start.UseShellExecute = false;
                start.CreateNoWindow = true;
                Process.Start(start);
            }
            catch
            {
                try { if (Directory.Exists(tempRoot)) Directory.Delete(tempRoot, true); } catch { }
                throw;
            }
        }

        private static void ExtractZipSafely(string zipPath, string destination)
        {
            string root = Path.GetFullPath(destination);
            if (!root.EndsWith(Path.DirectorySeparatorChar.ToString(), StringComparison.Ordinal))
                root += Path.DirectorySeparatorChar;
            using (ZipArchive archive = ZipFile.OpenRead(zipPath))
            {
                foreach (ZipArchiveEntry entry in archive.Entries)
                {
                    string relative = (entry.FullName ?? "").Replace('/', Path.DirectorySeparatorChar);
                    if (relative.Length == 0) continue;
                    string full = Path.GetFullPath(Path.Combine(root, relative));
                    if (!full.StartsWith(root, StringComparison.OrdinalIgnoreCase))
                        throw new InvalidOperationException("Update package contained an unsafe path.");
                    if (String.IsNullOrEmpty(entry.Name))
                    {
                        Directory.CreateDirectory(full);
                        continue;
                    }
                    Directory.CreateDirectory(Path.GetDirectoryName(full));
                    entry.ExtractToFile(full, true);
                }
            }
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

            // A deliberately dictated letter ("P", "b.") or number ("5", "10", "100") is valid
            // output and must insert.
            string core = text.TrimEnd('.', '!', '?', ',', ' ');
            if (core.Length == 1 && Char.IsLetter(core[0])) return false;
            if (core.Length == 0) return true;
            bool allDigits = true;
            for (int index = 0; index < core.Length; index++)
                if (!Char.IsDigit(core[index])) { allDigits = false; break; }
            if (allDigits && core.Length <= 4) return false;

            // A lone non-letter character from a real clip is garbage.
            if (text.Length == 1 && recordMs >= 250) return true;

            // A multi-syllable clip that only produced 1-2 characters is almost always garbage.
            if (text.Length <= 2 && recordMs >= 350 && audioBytes >= 8000) return true;

            // Whisper fills hold-to-talk silence with YouTube outros. A real "thank you" is
            // a short clip; several seconds of audio that decode to only thanks is silence.
            if (recordMs >= 4000 && IsStandaloneThanksPhrase(text)) return true;

            return false;
        }

        public static bool IsStandaloneThanksPhrase(string text)
        {
            if (String.IsNullOrWhiteSpace(text)) return false;
            string core = Regex.Replace(text.Trim(), @"[.!?""']+$", "").Trim();
            if (Regex.IsMatch(core, @"^(?:thank you|thanks)$", RegexOptions.IgnoreCase)) return true;
            return Regex.IsMatch(core, @"^(?:thank you|thanks)\s+for\s+(?:watching|listening|tuning in)$", RegexOptions.IgnoreCase);
        }

        public static bool IsLikelyEmbeddedHallucination(string segmentText, double durationSeconds, double gapBeforeSeconds, double gapAfterSeconds)
        {
            string text = (segmentText ?? "").Trim();
            if (text.Length != 1) return false;
            if (String.Equals(text, "I", StringComparison.OrdinalIgnoreCase) ||
                String.Equals(text, "a", StringComparison.OrdinalIgnoreCase)) return false;
            if (durationSeconds > 0.45) return false;
            // Only silence on BOTH sides marks a hallucination; a letter spoken right before
            // or after other words ("P as in Peter") pauses on one side only and must survive.
            if (gapBeforeSeconds >= 0.25 && gapAfterSeconds >= 0.25) return true;
            return false;
        }

        public static bool IsLikelyThanksHallucination(string segmentText, double gapBeforeSeconds, double gapAfterSeconds)
        {
            if (!IsStandaloneThanksPhrase(segmentText)) return false;
            // Trailing/leading/isolated thanks after a thinking pause — not "thank you for coming".
            return gapBeforeSeconds >= 0.3 && gapAfterSeconds >= 0.25;
        }
    }

    public static class AudioTranscriptionTimeouts
    {
        private const int MinTurboSeconds = 60;
        private const int MinStandardSeconds = 90;
        private const int MaxSeconds = 600;

        public static TimeSpan ForWavFile(string wavePath, bool turbo)
        {
            return ForAudioSeconds(EstimateWavDurationSeconds(wavePath), turbo);
        }

        public static TimeSpan ForAudioSeconds(double audioSeconds, bool turbo)
        {
            int floor = turbo ? MinTurboSeconds : MinStandardSeconds;
            double scale = turbo ? 1.5 : 2.5;
            int timeoutSeconds = (int)Math.Min(MaxSeconds, Math.Max(floor, audioSeconds * scale + 30));
            return TimeSpan.FromSeconds(timeoutSeconds);
        }

        public static double EstimateWavDurationSeconds(string wavePath)
        {
            try
            {
                FileInfo info = new FileInfo(wavePath);
                if (!info.Exists || info.Length <= 44) return 0;
                return (info.Length - 44) / 32000.0;
            }
            catch { return 0; }
        }
    }

    public enum JoinState
    {
        Unknown,
        SentenceStart,
        ClauseContinue,
        MidSentence,
        MidWord,
        AfterOpen
    }

    public struct CaretNeighborhood
    {
        public bool Available;
        public char ImmediateLeft;
        public char SemanticLeft;
        public char ImmediateRight;
        public bool HasSelection;
        public string LeftSnippet;
        public string RightSnippet;

        public bool AtStart
        {
            get { return Available && ImmediateLeft == '\0' && SemanticLeft == '\0'; }
        }

        public static CaretNeighborhood Unavailable()
        {
            CaretNeighborhood value = new CaretNeighborhood();
            value.Available = false;
            value.LeftSnippet = "";
            value.RightSnippet = "";
            return value;
        }

        public static CaretNeighborhood Known(char immediateLeft, char immediateRight, bool hasSelection)
        {
            char semantic = immediateLeft;
            if (Char.IsWhiteSpace(immediateLeft)) semantic = '\0';
            return Known(immediateLeft, immediateRight, hasSelection, semantic);
        }

        public static CaretNeighborhood Known(char immediateLeft, char immediateRight, bool hasSelection, char semanticLeft)
        {
            CaretNeighborhood value = new CaretNeighborhood();
            value.Available = true;
            value.ImmediateLeft = immediateLeft;
            value.ImmediateRight = immediateRight;
            value.HasSelection = hasSelection;
            value.SemanticLeft = semanticLeft;
            value.LeftSnippet = InferLeftSnippet(immediateLeft, semanticLeft);
            value.RightSnippet = immediateRight == '\0' ? "" : immediateRight.ToString();
            return value;
        }

        public static CaretNeighborhood FromSnippets(string leftSnippet, string rightSnippet)
        {
            return FromSnippets(leftSnippet, rightSnippet, false);
        }

        public static CaretNeighborhood FromSnippets(string leftSnippet, string rightSnippet, bool hasSelection)
        {
            leftSnippet = leftSnippet ?? "";
            rightSnippet = rightSnippet ?? "";
            char immediateLeft = leftSnippet.Length > 0 ? leftSnippet[leftSnippet.Length - 1] : '\0';
            char immediateRight = rightSnippet.Length > 0 ? rightSnippet[0] : '\0';
            char semanticLeft = ReadSemanticLeft(leftSnippet, leftSnippet.Length);
            CaretNeighborhood value = Known(immediateLeft, immediateRight, hasSelection, semanticLeft);
            value.LeftSnippet = leftSnippet;
            value.RightSnippet = rightSnippet;
            return value;
        }

        public static char ReadSemanticLeft(string text, int insertAt)
        {
            if (String.IsNullOrEmpty(text) || insertAt <= 0) return '\0';
            if (insertAt > text.Length) insertAt = text.Length;
            for (int index = insertAt - 1; index >= 0; index--)
            {
                char value = text[index];
                if (value == '\n' || value == '\r') return '\n';
                if (!Char.IsWhiteSpace(value)) return value;
            }
            return '\0';
        }

        private static string InferLeftSnippet(char immediateLeft, char semanticLeft)
        {
            if (semanticLeft != '\0' && immediateLeft != '\0' && Char.IsWhiteSpace(immediateLeft) && immediateLeft != semanticLeft)
                return semanticLeft.ToString() + immediateLeft.ToString();
            if (immediateLeft != '\0') return immediateLeft.ToString();
            if (semanticLeft != '\0') return semanticLeft.ToString();
            return "";
        }
    }

    public static class CaretFit
    {
        private static readonly Regex AbbreviationPeriod = new Regex(
            @"\b(?:etc|vs|mr|mrs|ms|dr|prof|inc|ltd|jr|sr|st|approx|e\.g|i\.e)\.$",
            RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

        private static readonly Regex TrailingAbbreviation = new Regex(
            @"\b(?:etc|vs|mr|mrs|ms|dr|prof|inc|ltd|jr|sr|st|approx|e\.g|i\.e)\.\s*$",
            RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

        public static string Apply(string text, CaretNeighborhood caret, bool assumeMidContinuity)
        {
            return Apply(text, caret, assumeMidContinuity, false, false);
        }

        public static string Apply(string text, CaretNeighborhood caret, bool assumeMidContinuity, bool assumeAfterPriorSentence)
        {
            return Apply(text, caret, assumeMidContinuity, assumeAfterPriorSentence, false);
        }

        public static string Apply(string text, CaretNeighborhood caret, bool assumeMidContinuity, bool assumeAfterPriorSentence, bool allowSoftFragmentWhenUnread)
        {
            if (String.IsNullOrEmpty(text)) return text ?? "";

            bool alreadyLeftPadded = text.StartsWith(" ", StringComparison.Ordinal) || text.StartsWith("\n", StringComparison.Ordinal);
            bool selection = caret.Available && caret.HasSelection;
            JoinState state = Classify(caret, assumeMidContinuity, assumeAfterPriorSentence);
            bool continueJoin = IsContinue(state);

            string fitted = text;
            if (continueJoin)
            {
                fitted = LowercaseForMidSentence(fitted);
                fitted = StripForcedTrailingPeriod(fitted);
            }

            if (selection) return fitted;

            if (!caret.Available)
            {
                if (state == JoinState.MidSentence || state == JoinState.ClauseContinue)
                    return fitted;
                if (state == JoinState.SentenceStart)
                    return alreadyLeftPadded ? fitted : EnsureLeadingSpace(fitted, true);
                if (!allowSoftFragmentWhenUnread) return text;
                return alreadyLeftPadded ? text : EnsureLeadingSpace(text, true);
            }

            bool needLeading = false;
            bool needTrailing = false;
            if (state != JoinState.MidWord)
            {
                if (!alreadyLeftPadded
                    && !StartsWithGluePunct(fitted)
                    && caret.ImmediateLeft != '\0'
                    && !Char.IsWhiteSpace(caret.ImmediateLeft)
                    && !IsNoSpaceAfter(caret.ImmediateLeft))
                    needLeading = true;

                if (NeedsSpaceBefore(caret.ImmediateRight) && !EndsWithJoinSpace(fitted))
                    needTrailing = true;
            }
            if (needLeading) fitted = " " + fitted;
            if (needTrailing) fitted = fitted + " ";
            return fitted;
        }

        public static JoinState Classify(CaretNeighborhood caret, bool assumeMidContinuity, bool assumeAfterPriorSentence)
        {
            if (!caret.Available)
            {
                if (assumeMidContinuity) return JoinState.MidSentence;
                if (assumeAfterPriorSentence) return JoinState.SentenceStart;
                return JoinState.Unknown;
            }

            if (caret.AtStart) return JoinState.SentenceStart;

            char left = caret.ImmediateLeft;
            char right = caret.ImmediateRight;
            char semantic = caret.SemanticLeft;

            if (!caret.HasSelection && IsWordChar(left) && IsWordChar(right))
                return JoinState.MidWord;
            if (IsOpenGlue(left))
                return JoinState.AfterOpen;
            if (IsClausePunct(semantic))
                return JoinState.ClauseContinue;
            if (IsSentenceBoundary(semantic))
            {
                if (LooksLikeAbbreviation(caret.LeftSnippet))
                    return JoinState.MidSentence;
                return JoinState.SentenceStart;
            }
            return JoinState.MidSentence;
        }

        private static bool IsContinue(JoinState state)
        {
            return state == JoinState.ClauseContinue
                || state == JoinState.MidSentence
                || state == JoinState.MidWord
                || state == JoinState.AfterOpen;
        }

        private static bool IsShortFragment(string text)
        {
            if (String.IsNullOrWhiteSpace(text) || text.IndexOf('\n') >= 0) return false;
            string trimmed = text.Trim();
            if (trimmed.Length == 0 || trimmed.Length > 60) return false;
            int words = 0;
            foreach (string part in Regex.Split(trimmed, @"\s+"))
            {
                if (part.Length == 0) continue;
                words++;
                if (words > 6) return false;
            }
            return words >= 1;
        }

        private static bool NeedsSpaceBefore(char right)
        {
            if (right == '\0' || Char.IsWhiteSpace(right)) return false;
            if (IsNoSpaceBefore(right)) return false;
            return Char.IsLetterOrDigit(right) || right == '(' || right == '[' || right == '{' || right == '"' || right == '\'' || right == '“' || right == '‘';
        }

        private static bool IsNoSpaceBefore(char value)
        {
            return value == ')' || value == ']' || value == '}' || value == ',' || value == '.' ||
                   value == '!' || value == '?' || value == ':' || value == ';' || value == '%' ||
                   value == '”' || value == '’';
        }

        private static bool EndsWithJoinSpace(string text)
        {
            if (String.IsNullOrEmpty(text)) return false;
            char last = text[text.Length - 1];
            return Char.IsWhiteSpace(last);
        }

        private static string EnsureLeadingSpace(string text, bool needed)
        {
            if (!needed || text.Length == 0) return text;
            if (Char.IsWhiteSpace(text[0])) return text;
            return " " + text;
        }

        private static bool IsSentenceBoundary(char value)
        {
            return value == '.' || value == '?' || value == '!' || value == '…' || value == '\n';
        }

        private static bool IsClausePunct(char value)
        {
            return value == ',' || value == ';' || value == ':' || value == '—' || value == '–';
        }

        private static bool IsOpenGlue(char value)
        {
            return value == '(' || value == '[' || value == '{' || value == '"' || value == '\'' ||
                   value == '“' || value == '‘';
        }

        private static bool IsWordChar(char value)
        {
            return Char.IsLetterOrDigit(value) || value == '\'';
        }

        private static bool LooksLikeAbbreviation(string snippet)
        {
            if (String.IsNullOrEmpty(snippet)) return false;
            return TrailingAbbreviation.IsMatch(snippet);
        }

        private static bool StartsWithGluePunct(string text)
        {
            if (String.IsNullOrEmpty(text)) return false;
            int index = 0;
            while (index < text.Length && Char.IsWhiteSpace(text[index])) index++;
            if (index >= text.Length) return false;
            char value = text[index];
            return value == ',' || value == '.' || value == ';' || value == ':' || value == '!' ||
                   value == '?' || value == ')' || value == ']' || value == '}' || value == '%' ||
                   value == '”' || value == '’';
        }

        private static bool IsNoSpaceAfter(char value)
        {
            return value == '(' || value == '[' || value == '{' || value == '"' || value == '\'' ||
                   value == '“' || value == '‘' || value == '/' || value == '\\' || value == '-' ||
                   value == '@' || value == '#';
        }

        private static string LowercaseForMidSentence(string text)
        {
            if (text.Length == 0) return text;
            int index = 0;
            while (index < text.Length && Char.IsWhiteSpace(text[index])) index++;
            if (index >= text.Length) return text;

            string firstToken = FirstToken(text, index);
            if (IsProtectedPronounI(firstToken)) return text;
            if (IsProtectedAcronymOrBrand(firstToken)) return text;

            char first = text[index];
            if (!Char.IsLetter(first) || !Char.IsUpper(first)) return text;
            if (firstToken.Length >= 2 && Char.IsLetter(firstToken[1]) && Char.IsUpper(firstToken[1]))
                return text;

            char[] chars = text.ToCharArray();
            chars[index] = Char.ToLower(first, CultureInfo.CurrentCulture);
            return new string(chars);
        }

        private static string FirstToken(string text, int start)
        {
            int end = start;
            while (end < text.Length)
            {
                char value = text[end];
                if (Char.IsWhiteSpace(value) || value == '.' || value == '?' || value == '!' || value == ',')
                    break;
                end++;
            }
            return text.Substring(start, end - start);
        }

        private static bool IsProtectedPronounI(string token)
        {
            return Regex.IsMatch(token ?? "", @"^I('m|'ll|'d|'ve|'re|’m|’ll|’d|’ve|’re)?$", RegexOptions.IgnoreCase);
        }

        private static bool IsProtectedAcronymOrBrand(string token)
        {
            if (String.IsNullOrEmpty(token) || token.Length < 2) return false;
            int letters = 0;
            int uppers = 0;
            for (int index = 0; index < token.Length; index++)
            {
                char value = token[index];
                if (!Char.IsLetter(value)) continue;
                letters++;
                if (Char.IsUpper(value)) uppers++;
            }
            return letters >= 2 && uppers == letters;
        }

        private static string StripForcedTrailingPeriod(string text)
        {
            if (String.IsNullOrEmpty(text)) return text;
            string trimmed = text.TrimEnd();
            if (!trimmed.EndsWith(".", StringComparison.Ordinal)) return text;
            if (trimmed.EndsWith("?", StringComparison.Ordinal) || trimmed.EndsWith("!", StringComparison.Ordinal))
                return text;
            if (AbbreviationPeriod.IsMatch(trimmed)) return text;
            if (Regex.IsMatch(trimmed, @"[.!?][^\s].*\.$")) return text;
            string without = trimmed.Substring(0, trimmed.Length - 1);
            if (text.Length > trimmed.Length) without += text.Substring(trimmed.Length);
            return without;
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

        public static bool IsUndoLastCommand(string text)
        {
            string core = Regex.Replace((text ?? "").Trim(), @"[.!?""']+$", "").Trim();
            return Regex.IsMatch(core, @"^(?:scratch that|undo that|delete that|undo last)$", RegexOptions.IgnoreCase);
        }

        public static bool IsLightCleanup(string text)
        {
            if (String.IsNullOrWhiteSpace(text) || text.IndexOf('\n') >= 0) return false;
            int words = 0;
            foreach (string part in Regex.Split(text.Trim(), @"\s+"))
                if (part.Length > 0) words++;
            return words > 0 && words <= 8;
        }

        public static bool TryParseDictionaryEntry(string entry, out string from, out string to)
        {
            from = "";
            to = "";
            if (String.IsNullOrWhiteSpace(entry)) return false;
            string[] map = Regex.Split(entry.Trim(), @"\s*(?:=>|→|=)\s*");
            if (map.Length != 2) return false;
            from = map[0].Trim();
            to = map[1].Trim();
            return from.Length > 0 && to.Length > 0;
        }

        public static string ApplySnippets(string text, AppSettings settings)
        {
            if (String.IsNullOrWhiteSpace(text) || settings == null || settings.Snippets == null) return text ?? "";
            foreach (KeyValuePair<string, string> snippet in settings.Snippets)
            {
                if (!String.IsNullOrWhiteSpace(snippet.Key))
                    text = Regex.Replace(text, @"(?<!\w)" + Regex.Escape(snippet.Key) + @"(?!\w)",
                        delegate { return snippet.Value ?? ""; }, RegexOptions.IgnoreCase);
            }
            return text;
        }

        public static string ApplyDictionaryReplacements(string text, AppSettings settings)
        {
            if (String.IsNullOrWhiteSpace(text) || settings == null || settings.Dictionary == null) return text ?? "";
            List<string> froms = new List<string>();
            List<string> tos = new List<string>();
            foreach (string entry in settings.Dictionary)
            {
                string from;
                string to;
                if (!TryParseDictionaryEntry(entry, out from, out to) || from.Length < 2) continue;
                froms.Add(from);
                tos.Add(to);
                text = Regex.Replace(text, @"\b" + Regex.Escape(from) + @"\b", delegate { return to; }, RegexOptions.IgnoreCase);
            }
            if (froms.Count == 0) return text;
            return Regex.Replace(text, @"\b[A-Za-z][A-Za-z'-]{1,}\b", delegate(Match match)
            {
                string word = match.Value;
                for (int index = 0; index < froms.Count; index++)
                {
                    if (String.Equals(word, tos[index], StringComparison.OrdinalIgnoreCase)) return word;
                    if (String.Equals(word, froms[index], StringComparison.OrdinalIgnoreCase)) return tos[index];
                    if (IsSpokenVariant(word, froms[index])) return tos[index];
                }
                return word;
            });
        }

        public static string ApplyAlwaysEdits(string text, AppSettings settings)
        {
            return FormatSpokenLists(ApplyDictionaryReplacements(ApplySnippets(text, settings), settings), settings);
        }

        private static bool IsSpokenVariant(string heard, string spoken)
        {
            if (String.IsNullOrEmpty(heard) || String.IsNullOrEmpty(spoken) || spoken.Length < 3) return false;
            if (Math.Abs(heard.Length - spoken.Length) > 2) return false;
            if (Char.ToLowerInvariant(heard[0]) != Char.ToLowerInvariant(spoken[0])) return false;
            if (String.Equals(heard, spoken, StringComparison.OrdinalIgnoreCase)) return false;
            int distance = LevenshteinDistance(heard, spoken);
            int maxDistance = spoken.Length <= 4 ? 1 : 2;
            return distance > 0 && distance <= maxDistance;
        }

        public static string Clean(string input, AppSettings settings, ForegroundInfo context)
        {
            if (String.IsNullOrWhiteSpace(input)) return "";
            string text = StripPromptHallucinations(input.Trim(), settings, context);
            text = RemoveExactDuplicateBlocks(text);
            bool light = IsLightCleanup(input);
            text = ApplyAlwaysEdits(text, settings);

            text = ApplyFuzzyDictionary(text, settings, context);

            // Whisper often splits "not" into "no t" (sometimes after a comma or dash). Heal that
            // before self-correction backtracking, which otherwise reads "word, no t" as "word → t".
            text = Regex.Replace(text, @"\bno\s+t\b", "not", RegexOptions.IgnoreCase);
            text = Regex.Replace(text, @"\bno,\s*not\b", "not", RegexOptions.IgnoreCase);
            text = Regex.Replace(text, @"(\w),\s*not\b", "$1 not", RegexOptions.IgnoreCase);
            text = Regex.Replace(text, @"(\w)-not\b", "$1 not", RegexOptions.IgnoreCase);

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

            text = FormatSpokenLists(text, settings);
            text = FormatNumbered(text);
            if (!light) text = FormatInferredList(text);

            // Guards keep the words usable as ordinary nouns ("a long period of time",
            // "the Oxford comma", "his colon") — only command usage converts.
            text = Regex.Replace(text, @"(?<!\b(?:oxford|serial|a|the|another|one))\s+comma\b", ",", RegexOptions.IgnoreCase);
            text = Regex.Replace(text, @"\s+(?:period|full stop)\b(?!\s+(?:of|in|for|to|when|where|that|between|during|is|was|has|had)\b)", ".", RegexOptions.IgnoreCase);
            text = Regex.Replace(text, @"\s+question mark\b", "?", RegexOptions.IgnoreCase);
            text = Regex.Replace(text, @"\s+exclamation (?:mark|point)\b", "!", RegexOptions.IgnoreCase);
            text = Regex.Replace(text, @"(?<!\b(?:his|her|my|your|their|the|a))\s+colon\b", ":", RegexOptions.IgnoreCase);
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
            // Emails, URLs, filenames, and abbreviations ("user@example.com",
            // "github.com", "flowtype.cs", "e.g.") must not be split or re-capitalized by
            // the sentence-spacing passes below.
            List<string> protectedTokens = new List<string>();
            text = ProtectDottedTokens(text, protectedTokens);
            text = Regex.Replace(text, @"[ \t]+([,.;:!?])", "$1");
            text = Regex.Replace(text, @"([,.;:!?])(?=[A-Za-z])", "$1 ");
            text = Regex.Replace(text, @"[ \t]{2,}", " ");
            text = Regex.Replace(text, @" *\n *", "\n");
            text = Regex.Replace(text, @"\n{3,}", "\n\n");
            text = Paragraphize(text, context).Trim();
            if (IsTerminalContext(context)) return RestoreDottedTokens(NormalizePunctuationSpacing(text.TrimEnd('.', ' ')), protectedTokens);
            text = Capitalize(text);
            if (!Regex.IsMatch(text, @"[.!?…,:;\)\]\""']$", RegexOptions.None) && !text.Contains("\n")) text += ".";
            return RestoreDottedTokens(NormalizePunctuationSpacing(ApplyApplicationStyle(text, settings, context)), protectedTokens);
        }

        // Lowercase-after-dot keeps sentence joins like "can.But" eligible for spacing while
        // shielding real dotted tokens; the email alternation catches mixed-case addresses.
        private static readonly Regex ProtectedTokenPattern = new Regex(
            @"[A-Za-z0-9][A-Za-z0-9_+-]*(?:\.[a-z0-9_-]+)+(?:@[A-Za-z0-9.-]+)?|[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+",
            RegexOptions.Compiled);

        private static string ProtectDottedTokens(string text, List<string> stash)
        {
            return ProtectedTokenPattern.Replace(text, delegate(Match match)
            {
                stash.Add(match.Value);
                return "\uE000" + (stash.Count - 1).ToString(CultureInfo.InvariantCulture) + "\uE001";
            });
        }

        private static string RestoreDottedTokens(string text, List<string> stash)
        {
            for (int index = 0; index < stash.Count; index++)
                text = text.Replace("\uE000" + index.ToString(CultureInfo.InvariantCulture) + "\uE001", stash[index]);
            return text;
        }

        public static string NormalizePunctuationSpacing(string text)
        {
            if (String.IsNullOrWhiteSpace(text)) return text ?? "";
            List<string> stash = new List<string>();
            text = ProtectDottedTokens(text, stash);
            text = Regex.Replace(text, @"([.!?])([A-Za-z""'])", "$1 $2");
            text = Regex.Replace(text, @"([,;:])([A-Za-z""'])", "$1 $2");
            return RestoreDottedTokens(text, stash);
        }

        public static string SpokenListCleanupHint(AppSettings settings)
        {
            if (settings == null || !settings.SpokenListsEnabled)
                return " Leave ordinary words such as 'next point' as words; do not turn them into list markers. ";
            string bullet = String.IsNullOrWhiteSpace(settings.SpokenBulletPhrase) ? "next point" : settings.SpokenBulletPhrase.Trim();
            string numbered = settings.SpokenNumberPhrase == null ? "next number" : settings.SpokenNumberPhrase.Trim();
            StringBuilder hint = new StringBuilder(" ");
            hint.Append("When the speaker says \"" + bullet + "\", start a new Markdown bullet and omit those command words. A single occurrence is enough. ");
            if (numbered.Length > 0)
                hint.Append("When they say \"" + numbered + "\", start a new numbered item and omit those command words. ");
            hint.Append("Text before the first command is the first item unless it is clearly a heading. ");
            return hint.ToString();
        }

        public static List<string> SpokenCommandPhrases(AppSettings settings)
        {
            List<string> phrases = new List<string>();
            HashSet<string> seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            if (settings == null || !settings.SpokenListsEnabled) return phrases;
            foreach (string phrase in ParseSpokenListPhrases(settings.SpokenBulletPhrase, true))
                if (seen.Add(phrase)) phrases.Add(phrase);
            foreach (string phrase in ParseSpokenListPhrases(settings.SpokenNumberPhrase, false))
                if (seen.Add(phrase)) phrases.Add(phrase);
            return phrases;
        }

        private static readonly string[] BuiltInBulletAliases = new string[]
        {
            "bullet point", "next bullet", "new bullet", "another point", "final point"
        };

        private static List<string> ParseSpokenListPhrases(string field, bool includeBulletAliases)
        {
            List<string> phrases = new List<string>();
            HashSet<string> seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (string piece in Regex.Split(field ?? "", @"\s*,\s*"))
                AddSpokenPhrase(phrases, seen, piece);
            if (includeBulletAliases)
                foreach (string alias in BuiltInBulletAliases)
                    AddSpokenPhrase(phrases, seen, alias);
            phrases.Sort(delegate(string left, string right) { return right.Length.CompareTo(left.Length); });
            return phrases;
        }

        private static void AddSpokenPhrase(List<string> phrases, HashSet<string> seen, string piece)
        {
            string value = Regex.Replace((piece ?? "").Trim(), @"\s+", " ");
            value = Regex.Replace(value, @"[^A-Za-z0-9' -]", "");
            value = value.Trim(' ', '-', '\'');
            if (value.Length < 4 || !Regex.IsMatch(value, @"[A-Za-z]")) return;
            if (value.Length > 40) value = value.Substring(0, 40).Trim();
            if (seen.Add(value)) phrases.Add(value);
        }

        private static Regex BuildSpokenMarkerRegex(List<string> phrases)
        {
            if (phrases == null || phrases.Count == 0) return null;
            List<string> parts = new List<string>();
            foreach (string phrase in phrases)
            {
                string[] words = Regex.Split(phrase, @"\s+");
                if (words.Length == 0 || (words.Length == 1 && words[0].Length == 0)) continue;
                parts.Add(String.Join(@"[.,]?\s+", words.Select(word => Regex.Escape(word)).ToArray()));
            }
            if (parts.Count == 0) return null;
            return new Regex(
                @"(?:^|[\s,;:])(?<!\b(?:the|a|an|this|that|my|our|your)\s)(?:" + String.Join("|", parts.ToArray()) + @")\s*[:,]?\s+",
                RegexOptions.IgnoreCase);
        }

        private static string FormatSpokenLists(string text, AppSettings settings)
        {
            if (settings == null || !settings.SpokenListsEnabled) return text;
            if (String.IsNullOrWhiteSpace(text) || LooksLikeFormattedList(text)) return text;

            List<SpokenListHit> hits = new List<SpokenListHit>();
            CollectSpokenListHits(hits, text, BuildSpokenMarkerRegex(ParseSpokenListPhrases(settings.SpokenBulletPhrase, true)), false);
            CollectSpokenListHits(hits, text, BuildSpokenMarkerRegex(ParseSpokenListPhrases(settings.SpokenNumberPhrase, false)), true);
            if (hits.Count == 0) return text;
            hits.Sort(delegate(SpokenListHit left, SpokenListHit right) { return left.Index.CompareTo(right.Index); });

            List<SpokenListHit> markers = new List<SpokenListHit>();
            int occupied = -1;
            foreach (SpokenListHit hit in hits)
            {
                if (hit.Index < occupied) continue;
                markers.Add(hit);
                occupied = hit.Index + hit.Length;
            }
            if (markers.Count == 0) return text;

            bool numbered = markers[0].Numbered;
            string prefix = text.Substring(0, markers[0].Index).Trim();
            string heading = "";
            List<string> items = new List<string>();
            if (prefix.Length > 0)
            {
                if (LooksLikeListIntro(prefix) || prefix.EndsWith(":")) heading = prefix.TrimEnd(':');
                else items.Add(TrimConnectorEdges(prefix));
            }
            for (int index = 0; index < markers.Count; index++)
            {
                int start = markers[index].Index + markers[index].Length;
                int end = index + 1 < markers.Count ? markers[index + 1].Index : text.Length;
                if (end <= start) continue;
                string item = TrimConnectorEdges(text.Substring(start, end - start));
                if (IsSubstantiveListItem(item)) items.Add(item);
            }
            if (items.Count == 0) return text;
            // "apples next point" with nothing after the command is not a list.
            if (items.Count == 1 && heading.Length == 0 && prefix.Length > 0) return text;

            StringBuilder output = new StringBuilder();
            if (heading.Length > 0) output.Append(heading + ":\n");
            for (int index = 0; index < items.Count; index++)
            {
                if (numbered) output.Append((index + 1).ToString(CultureInfo.InvariantCulture) + ". " + items[index] + "\n");
                else output.Append("- " + items[index] + "\n");
            }
            return output.ToString().TrimEnd();
        }

        private static void CollectSpokenListHits(List<SpokenListHit> hits, string text, Regex marker, bool numbered)
        {
            if (marker == null || hits == null) return;
            foreach (Match match in marker.Matches(text))
                hits.Add(new SpokenListHit { Index = match.Index, Length = match.Length, Numbered = numbered });
        }

        private static bool LooksLikeFormattedList(string text)
        {
            return Regex.IsMatch(text ?? "", @"(?m)^\s*(?:[-*]|\d+\.)\s+\S");
        }

        private struct SpokenListHit
        {
            public int Index;
            public int Length;
            public bool Numbered;
        }

        // Ordinal markers ("first", "number two") anchor a spoken list; continuation markers
        // ("then", "next") only count once an anchor was seen, so plain prose that happens to
        // contain "then" never becomes a list. Articles before an ordinal ("the first time")
        // mark adjectival use, not enumeration.
        private static readonly Regex NumberedMarkerPattern = new Regex(
            @"(?<=^|[\s,;:])(?<!\b(?:the|a|an|my|our|your|his|her|their|its|this|that|at|of|for|very)\s)(?<ordinal>(?:first|second|third|fourth|fifth|sixth|seventh|eighth|ninth)(?:ly|\s+of\s+all)?|(?:number|step)\s+(?:one|two|three|four|five|six|seven|eight|nine))\s*[:,]?\s+" +
            @"|(?<=[,;]\s{0,4})(?<continuation>and\s+then|then|next|after\s+that)\s*[:,]?\s+" +
            @"|(?<=^|[\s,;:])(?<terminal>finally|lastly)\s*[:,]?\s+",
            RegexOptions.IgnoreCase);

        private static int OrdinalValue(string marker)
        {
            string value = Regex.Replace(marker.ToLowerInvariant(), @"\s+", " ").Trim();
            value = Regex.Replace(value, @"^(?:number|step) ", "");
            if (value.StartsWith("first")) return 1;
            if (value.StartsWith("second") || value == "two") return 2;
            if (value.StartsWith("third") || value == "three") return 3;
            if (value.StartsWith("fourth") || value == "four") return 4;
            if (value.StartsWith("fifth") || value == "five") return 5;
            if (value.StartsWith("sixth") || value == "six") return 6;
            if (value.StartsWith("seventh") || value == "seven") return 7;
            if (value.StartsWith("eighth") || value == "eight") return 8;
            if (value.StartsWith("ninth") || value == "nine") return 9;
            if (value == "one") return 1;
            return -1;
        }

        private static string FormatNumbered(string text)
        {
            if (LooksLikeFormattedList(text)) return text;
            MatchCollection matches = NumberedMarkerPattern.Matches(text);
            if (matches.Count < 2) return FormatCardinalList(text);

            List<Match> markers = new List<Match>();
            int expected = 0;
            int ordinalCount = 0;
            bool terminalSeen = false;
            foreach (Match match in matches)
            {
                if (match.Groups["ordinal"].Success)
                {
                    int value = OrdinalValue(match.Groups["ordinal"].Value);
                    // Out-of-order ordinals mean prose ("second thoughts", "first ... first"),
                    // not an enumerated list — leave the text untouched.
                    if (terminalSeen || value != expected + 1) return FormatCardinalList(text);
                    expected = value;
                    ordinalCount++;
                    markers.Add(match);
                }
                else if (match.Groups["terminal"].Success)
                {
                    if (markers.Count == 0) continue;
                    if (terminalSeen) return FormatCardinalList(text);
                    terminalSeen = true;
                    expected++;
                    markers.Add(match);
                }
                else
                {
                    if (markers.Count == 0) continue;
                    if (terminalSeen) return FormatCardinalList(text);
                    expected++;
                    markers.Add(match);
                }
            }
            if (markers.Count < 2) return FormatCardinalList(text);
            // A two-item list needs two explicit ordinals ("first X second Y"); an anchor plus a
            // single "then" is normal prose ("first let me check, then we can decide").
            if (markers.Count == 2 && ordinalCount < 2) return FormatCardinalList(text);

            string prefix = text.Substring(0, markers[0].Index).Trim();
            List<string> items = new List<string>();
            for (int index = 0; index < markers.Count; index++)
            {
                int start = markers[index].Index + markers[index].Length;
                int end = index + 1 < markers.Count ? markers[index + 1].Index : text.Length;
                string item = TrimConnectorEdges(text.Substring(start, end - start));
                // A connector-only or empty fragment between markers means this was prose,
                // not an enumeration — never emit "And" as a list item.
                if (!IsSubstantiveListItem(item)) return text;
                items.Add(item);
            }
            StringBuilder output = new StringBuilder();
            if (prefix.Length > 0) output.Append(prefix.TrimEnd(':', ',') + ":\n");
            for (int index = 0; index < items.Count; index++) output.Append((index + 1).ToString(CultureInfo.InvariantCulture) + ". " + items[index] + "\n");
            return output.ToString().TrimEnd();
        }

        private static readonly HashSet<string> ConnectorOnlyItems = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "and", "or", "then", "also", "so", "but", "plus", "next", "and then", "after that",
            "it", "that", "this", "a", "an", "the"
        };

        private static string TrimConnectorEdges(string item)
        {
            string value = (item ?? "").Trim(' ', ',', '.', ';', ':');
            value = Regex.Replace(value, @"^(?:(?:and|or|then|also|so|but|plus)\b[\s,]*)+", "", RegexOptions.IgnoreCase);
            value = Regex.Replace(value, @"(?:[\s,]+(?:and|or|then|also|so|but|plus))+[\s,.]*$", "", RegexOptions.IgnoreCase);
            return value.Trim(' ', ',', '.', ';', ':');
        }

        private static bool IsSubstantiveListItem(string item)
        {
            if (String.IsNullOrWhiteSpace(item)) return false;
            if (!Regex.IsMatch(item, @"[A-Za-z0-9]")) return false;
            return !ConnectorOnlyItems.Contains(item.Trim());
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
                string item = TrimConnectorEdges(text.Substring(start, end - start));
                if (!IsSubstantiveListItem(item)) return text;
                items.Add(item);
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
                @"^(?<head>.*?\b(?:here are|here is|here's|the following|top\s+(?:\d+|one|two|three|four|five|six|seven|eight|nine|ten)|(?:my|our|the)\s+(?:list|checklist))\b.*?(?::|\s+are\b|\binclude(?:s)?\b))\s*(?<body>.+)$",
                RegexOptions.IgnoreCase);
            if (!list.Success)
            {
                list = Regex.Match(text,
                    @"^(?<head>.*?\b(?:here are|the following|top\s+(?:\d+|one|two|three|four|five|six|seven|eight|nine|ten))\b[^,]{0,48})\s*,\s*(?<body>.+)$",
                    RegexOptions.IgnoreCase);
            }
            if (!list.Success)
            {
                list = Regex.Match(text,
                    @"^(?<head>.*?\b(?:things|points|steps|reasons|goals|priorities|options|items|tasks|changes)\b\s*:\s*)(?<body>.+)$",
                    RegexOptions.IgnoreCase);
            }
            if (!list.Success) return text;

            string body = list.Groups["body"].Value.Trim();
            string[] parts = Regex.Split(body, @"\s*(?:;|\b(?:and then|then|next|also|finally)\b)\s*", RegexOptions.IgnoreCase)
                .SelectMany(value => Regex.Split(value, @"(?<=\S)\s*,\s*(?:and\s+)?(?=\S)", RegexOptions.None))
                .Select(value => TrimConnectorEdges(value))
                .Where(value => IsSubstantiveListItem(value))
                .ToArray();
            if (parts.Length < 3 || parts.Length > 9 || parts.Any(value => value.Length > 80)) return text;
            if (parts.Any(value => Regex.IsMatch(value, @"\b(?:that|which|because|since|when|while|although|though|if|unless)\b", RegexOptions.IgnoreCase)))
                return text;

            StringBuilder output = new StringBuilder(list.Groups["head"].Value.Trim().TrimEnd(':') + ":\n");
            foreach (string item in parts) output.Append("- " + item + "\n");
            return output.ToString().TrimEnd();
        }

        private static string FormatProsodicList(SpeechTranscript transcript, AppSettings settings, ForegroundInfo context)
        {
            List<SpeechSegment> phrases = transcript.Segments.Where(value => value != null && !String.IsNullOrWhiteSpace(value.Text)).ToList();
            List<SpeechSegment> wordPhrases = BuildWordPhrases(transcript.Words);
            if (wordPhrases.Count > phrases.Count) phrases = wordPhrases;
            if (phrases.Count < 3 || phrases.Count > 9) return "";
            string first = phrases[0].Text.Trim();
            if (!LooksLikeListIntro(first)) return "";
            int realPauses = 0;
            for (int index = 1; index < phrases.Count; index++)
                if (phrases[index].Start - phrases[index - 1].End >= 0.28) realPauses++;
            if (realPauses < 2) return "";

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
            string text = value ?? "";
            if (Regex.IsMatch(text,
                @"\b(?:here are|here is|here's|the following|top\s+(?:\d+|one|two|three|four|five|six|seven|eight|nine|ten)|(?:my|our|the)\s+(?:list|checklist))\b",
                RegexOptions.IgnoreCase))
                return true;
            return Regex.IsMatch(text,
                @"\b(?:things|points|steps|reasons|goals|priorities|options|items|tasks|changes)\b\s*:",
                RegexOptions.IgnoreCase);
        }

        private static readonly HashSet<string> FuzzyProtectedWords = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "final", "finally", "fine", "find", "finger", "finish", "fire", "first", "fish", "five",
            "literally", "logic", "little", "live", "long", "look", "testing", "test", "text", "talk",
            "tomorrow", "today", "tonight", "true", "try", "turn", "tail", "take", "tell", "than",
            "awesome", "about", "after", "again", "also", "always", "another", "around", "because",
            "before", "being", "between", "call", "called", "come", "coming", "could", "down", "doing",
            "every", "everything", "from", "going", "good", "great", "have", "having", "just", "know",
            "like", "make", "maybe", "more", "most", "much", "need", "never", "only", "other", "over",
            "pretty", "really", "right", "said", "same", "some", "something", "still", "that", "their",
            "them", "then", "there", "these", "they", "think", "this", "those", "through", "very",
            "want", "well", "were", "what", "when", "where", "which", "while", "will", "with", "would",
            "yeah", "yes", "your"
        };

        private static bool IsChatProcess(string processName)
        {
            string app = (processName ?? "").ToLowerInvariant();
            return app.Contains("discord") || app.Contains("slack") || app.Contains("teams")
                || app.Contains("telegram") || app.Contains("whatsapp") || app.Contains("signal")
                || app.Contains("messenger") || app.Contains("element") || app.Contains("skype")
                || app.Contains("zoom");
        }

        private static string ApplyFuzzyDictionary(string text, AppSettings settings, ForegroundInfo context)
        {
            List<string> userTerms = new List<string>();
            List<string> titleTerms = new List<string>();
            CollectCanonicalTerms(settings, context, userTerms, titleTerms);
            if (userTerms.Count == 0 && titleTerms.Count == 0) return text;
            return Regex.Replace(text, @"\b[A-Za-z][A-Za-z'-]{2,}\b", delegate(Match match)
            {
                string word = match.Value;
                if (FuzzyProtectedWords.Contains(word)) return word;
                foreach (string term in userTerms)
                {
                    if (String.Equals(term, word, StringComparison.OrdinalIgnoreCase))
                        return PreserveCase(word, term);
                }
                foreach (string term in titleTerms)
                {
                    if (String.Equals(term, word, StringComparison.OrdinalIgnoreCase))
                        return PreserveCase(word, term);
                }
                string best = null;
                int bestDistance = int.MaxValue;
                int tieCount = 0;
                foreach (string term in userTerms)
                    ConsiderFuzzyTerm(word, term, false, ref best, ref bestDistance, ref tieCount);
                // Window-title tokens are auto-harvested guesses, not user intent — hold them
                // to a stricter bar so an active tab name can't rewrite normal words
                // ("world" must never become "Word" because Word is open).
                foreach (string term in titleTerms)
                    ConsiderFuzzyTerm(word, term, true, ref best, ref bestDistance, ref tieCount);
                return best != null && tieCount == 1 ? PreserveCase(word, best) : word;
            });
        }

        private static void ConsiderFuzzyTerm(string word, string term, bool titleTerm, ref string best, ref int bestDistance, ref int tieCount)
        {
            if (Math.Abs(term.Length - word.Length) > 2) return;
            if (!Char.Equals(Char.ToLowerInvariant(word[0]), Char.ToLowerInvariant(term[0]))) return;
            // Title tokens may only repair same-length substitution typos ("setsings" →
            // "Settings"); insertions/deletions land on real words ("tracing" → "Tracking").
            if (titleTerm && (term.Length < 6 || term.Length != word.Length)) return;
            int distance = LevenshteinDistance(word, term);
            int maxDistance = titleTerm ? 1 : (term.Length <= 5 ? 1 : 2);
            if (distance <= 0 || distance > maxDistance) return;
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

        private static void CollectCanonicalTerms(AppSettings settings, ForegroundInfo context, List<string> terms, List<string> titleTerms)
        {
            HashSet<string> seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            Action<string, int> add = delegate(string value, int minLength)
            {
                if (String.IsNullOrWhiteSpace(value)) return;
                value = value.Trim();
                if (value.Length < minLength || seen.Contains(value)) return;
                seen.Add(value);
                terms.Add(value);
            };
            if (settings != null)
            {
                foreach (string entry in settings.Dictionary)
                {
                    string from;
                    string to;
                    if (TryParseDictionaryEntry(entry, out from, out to))
                        add(to, 2);
                    else add(entry, 4);
                }
                foreach (KeyValuePair<string, string> snippet in settings.Snippets)
                {
                    add(snippet.Key, 4);
                    add(snippet.Value, 4);
                }
            }
            Action<string> addTitle = delegate(string value)
            {
                if (String.IsNullOrWhiteSpace(value)) return;
                value = value.Trim();
                if (value.Length < 4 || seen.Contains(value)) return;
                seen.Add(value);
                titleTerms.Add(value);
            };
            if (context != null && !String.IsNullOrWhiteSpace(context.Title))
            {
                string title = context.Title.Trim();
                if (IsChatProcess(context.ProcessName))
                {
                    string chatLabel = Regex.Split(title, @"\s*[—\-|]\s*")[0].Trim();
                    addTitle(chatLabel);
                }
                else
                {
                    addTitle(title);
                    foreach (Match token in Regex.Matches(title, @"\b[A-Za-z][A-Za-z'-]{3,}\b"))
                        addTitle(token.Value);
                }
            }
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
                @"\b(?<old>[A-Za-z][A-Za-z'-]*)\s*[,….—-]+\s*(?:sorry|I mean|scratch that)\s*[,.:—-]?\s*(?<new>[A-Za-z][A-Za-z'-]{1,})\b",
                "${new}", RegexOptions.IgnoreCase);
            // Corrective "no" needs punctuation on BOTH sides ("Tuesday, no, Wednesday") —
            // existential "no" ("the door, no answer") has none after and must survive.
            text = Regex.Replace(text,
                @"\b(?<old>[A-Za-z][A-Za-z'-]*)\s*[,….—-]+\s*no\s*[,.:—-]\s*(?<new>[A-Za-z][A-Za-z'-]{1,})\b",
                "${new}", RegexOptions.IgnoreCase);
            // "start over" only wipes the preamble inside a corrective frame ("let me start
            // over") — the bare verb phrase ("it will start over from zero") is normal speech.
            text = Regex.Replace(text,
                @"^.{0,160}?\b(?:let(?:'s|\s+me)?|actually|wait|no|okay|scratch that)[,\s]+(?:start over|never mind)\b\s*[,.:—-]?\s*", "", RegexOptions.IgnoreCase);
            text = Regex.Replace(text,
                @"\b(as\s+(?:a|an|the)\s+)([A-Za-z'-]+(?:\s+[A-Za-z'-]+){0,2})\s*(?:[,….]+\s*)?\1([A-Za-z'-]+(?:\s+[A-Za-z'-]+){0,2})\b",
                "$1$3", RegexOptions.IgnoreCase);
            return text;
        }

        public static string StripPromptHallucinations(string text, AppSettings settings, ForegroundInfo context)
        {
            if (String.IsNullOrWhiteSpace(text)) return "";
            text = text.Trim();
            text = RemoveInlinePromptEcho(text);

            // Whisper often echoes the STT prompt at the tail of longer clips.
            Match targetWindow = Regex.Match(text, @"\bTarget window\b", RegexOptions.IgnoreCase);
            if (targetWindow.Success && targetWindow.Index >= 20)
            {
                string tail = text.Substring(targetWindow.Index);
                bool tailLooksLikeGarbage = Regex.IsMatch(tail,
                    @"(?:Outro to|Camp\.|[%$]{1,2}|P\.\$|Target window\s+\d[^.!?]{0,40}[.!?])",
                    RegexOptions.IgnoreCase);
                if (tailLooksLikeGarbage)
                    text = text.Substring(0, targetWindow.Index).TrimEnd(' ', '\t', '-', '–', '—', ',');
            }

            Match preferredTerms = Regex.Match(text, @"\bPreferred names and spellings\b", RegexOptions.IgnoreCase);
            if (preferredTerms.Success && preferredTerms.Index >= 20)
                text = text.Substring(0, preferredTerms.Index).TrimEnd(' ', '\t', '-', '–', '—', ',');

            text = Regex.Replace(text, @"[\s\-–—,]*\b(?:Target window|Outro to)\b.*$", "", RegexOptions.IgnoreCase);
            text = Regex.Replace(text, @"[\s,]*(?:Camp\.\d|P\.\$[%&$#@]*).*$", "", RegexOptions.IgnoreCase);
            text = Regex.Replace(text, @"[\s,]*[A-Za-z0-9.\s]*[%&$#@]{2,}[A-Za-z0-9.%&$#@\s]*$", "");
            return StripThanksHallucinations(RemoveEmbeddedGlitches(text.Trim()));
        }

        public static string StripThanksHallucinations(string text)
        {
            if (String.IsNullOrWhiteSpace(text)) return text ?? "";
            text = text.Trim();
            const string outro = @"(?:thanks|thank\s+you)\s+for\s+(?:watching|listening|tuning\s+in)";
            const string bare = @"(?:thank\s+you|thanks)";
            const int minRemainder = 30;

            for (int pass = 0; pass < 3; pass++)
            {
                string current = text;
                text = Regex.Replace(text, @"^" + outro + @"[.!?,]*\s*", "", RegexOptions.IgnoreCase).Trim();
                text = Regex.Replace(text, @"[\s,;:]+" + outro + @"[.!?]*\s*$", "", RegexOptions.IgnoreCase).Trim();

                Match lead = Regex.Match(text, @"^" + bare + @"[.!?]\s+", RegexOptions.IgnoreCase);
                if (lead.Success)
                {
                    string rest = text.Substring(lead.Length).TrimStart();
                    if (rest.Length >= minRemainder) text = rest;
                }

                Match leadCap = Regex.Match(text, @"^(?:Thank you|Thanks)\s+(?=[A-Z])");
                if (leadCap.Success)
                {
                    string rest = text.Substring(leadCap.Length).TrimStart();
                    if (rest.Length >= minRemainder) text = rest;
                }

                Match trail = Regex.Match(text, @"[.!?]\s+" + bare + @"[.!?]*\s*$", RegexOptions.IgnoreCase);
                if (trail.Success)
                {
                    string rest = text.Substring(0, trail.Index + 1).TrimEnd();
                    if (rest.Length >= minRemainder) text = rest;
                }

                Match trailCap = Regex.Match(text, @"\s+(?:Thank you|Thanks)[.!?]*\s*$");
                if (trailCap.Success)
                {
                    string rest = text.Substring(0, trailCap.Index).TrimEnd();
                    if (rest.Length >= minRemainder) text = rest;
                }

                if (String.Equals(text, current, StringComparison.Ordinal)) break;
            }

            return text.Trim();
        }

        private static string RemoveInlinePromptEcho(string text)
        {
            if (String.IsNullOrWhiteSpace(text)) return text ?? "";
            text = Regex.Replace(text,
                @"[\s\-–—,]*\bTarget window\s+\d+\s*[xX][^.!?]{0,40}?(?=\s+and\s+(?:we|I|they|it|the|then|also)\b)",
                " ", RegexOptions.IgnoreCase);
            text = Regex.Replace(text, @"\s+\bOutro to\b[^.!?]{0,80}(?=\s+and\s+(?:we|I|they|it|the|then|also)\b)", " ", RegexOptions.IgnoreCase);
            text = Regex.Replace(text, @"\s+\b(?:Camp\.\d|P\.\$[%&$#@]+)[^.!?]{0,80}(?=\s+and\s+(?:we|I|they|it|the|then|also)\b)", " ", RegexOptions.IgnoreCase);
            return Regex.Replace(text, @"[ \t]{2,}", " ").Trim();
        }

        public static string RemoveEmbeddedGlitches(string text)
        {
            if (String.IsNullOrWhiteSpace(text) || text.Length < 25) return text ?? "";

            // Whisper often inserts a lone letter during a breath or audio gap in longer dictation.
            for (int pass = 0; pass < 3; pass++)
            {
                string current = text;
                string next = Regex.Replace(text,
                    @"(?<=\S{2,})\s+(?<glitch>[B-DF-HJ-NP-TV-Zb-df-hj-np-tv-z])(?=\s+\S{2,})",
                    delegate(Match match)
                    {
                        string letter = match.Groups["glitch"].Value;
                        int index = match.Index;
                        string before = index >= 18 ? current.Substring(index - 18, 18) : current.Substring(0, index);
                        int afterStart = index + match.Length;
                        string after = current.Substring(afterStart, Math.Min(8, current.Length - afterStart));
                        if (String.Equals(letter, "X", StringComparison.OrdinalIgnoreCase)
                            && Regex.IsMatch(before, @"Generation\s+$", RegexOptions.IgnoreCase)) return match.Value;
                        // A nearby cue word means the letter was dictated on purpose, not glitched.
                        if (Regex.IsMatch(before, @"\b(?:letter|letters|press|type|typed|hit|key|option|plan|section|column|row|drive|vitamin|grade|as in)\b[\s\S]*$", RegexOptions.IgnoreCase)) return match.Value;
                        if (Regex.IsMatch(after, @"^\s*(?:as in|for|key)\b", RegexOptions.IgnoreCase)) return match.Value;
                        return " ";
                    });
                if (String.Equals(next, text, StringComparison.Ordinal)) break;
                text = next;
            }

            text = Regex.Replace(text, @"[ \t]{2,}", " ");
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

        public static string RemoveExactDuplicateBlocks(string text)
        {
            if (String.IsNullOrWhiteSpace(text)) return text ?? "";
            string trimmed = CollapseConsecutiveDuplicateLines(CollapseConsecutiveDuplicateParagraphs(text.Trim()));

            for (int pass = 0; pass < 6; pass++)
            {
                string next = CollapseHalfDuplicates(trimmed);
                next = CollapsePeriodDuplicates(next);
                if (String.Equals(next, trimmed, StringComparison.Ordinal)) break;
                trimmed = next;
            }

            Match split = Regex.Match(trimmed, @"^(?<first>.+?)\s*---\s*(?<second>.+)\s*$", RegexOptions.Singleline);
            if (split.Success)
            {
                string first = split.Groups["first"].Value.Trim();
                string second = split.Groups["second"].Value.Trim();
                if (first.Length >= 12 && BlocksEqual(first, second)) return first;
            }

            return trimmed;
        }

        private static string CollapseHalfDuplicates(string text)
        {
            int mid = text.Length / 2;
            if (mid < 10) return text;
            string left = text.Substring(0, mid).TrimEnd();
            string right = text.Substring(mid).TrimStart();
            if (BlocksEqual(left, right)) return left;
            return text;
        }

        private static string CollapsePeriodDuplicates(string text)
        {
            for (int period = 1; period <= text.Length / 3; period++)
            {
                if (text.Length % period != 0) continue;
                string unit = text.Substring(0, period);
                if (unit.Length < 8) continue;
                bool match = true;
                for (int index = period; index < text.Length; index += period)
                {
                    if (!text.Substring(index, period).Equals(unit, StringComparison.Ordinal))
                    {
                        match = false;
                        break;
                    }
                }
                if (match) return unit;
            }
            return text;
        }

        private static string CollapseConsecutiveDuplicateParagraphs(string text)
        {
            string[] parts = Regex.Split(text, @"\n\s*\n");
            if (parts.Length < 2) return text;
            List<string> kept = new List<string>();
            foreach (string part in parts)
            {
                string value = part.Trim();
                if (value.Length == 0) continue;
                if (kept.Count > 0 && BlocksEqual(kept[kept.Count - 1], value)) continue;
                kept.Add(value);
            }
            if (kept.Count < parts.Length) return String.Join("\n\n", kept.ToArray());
            return text;
        }

        private static string CollapseConsecutiveDuplicateLines(string text)
        {
            string[] lines = text.Replace("\r\n", "\n").Split('\n');
            if (lines.Length < 2) return text;
            List<string> kept = new List<string>();
            foreach (string line in lines)
            {
                string value = line.TrimEnd();
                if (kept.Count > 0 && BlocksEqual(kept[kept.Count - 1], value)) continue;
                kept.Add(value);
            }
            if (kept.Count < lines.Length) return String.Join("\n", kept.ToArray());
            return text;
        }

        private static bool BlocksEqual(string left, string right)
        {
            string a = (left ?? "").Trim();
            string b = (right ?? "").Trim();
            return String.Equals(a, b, StringComparison.Ordinal) ||
                String.Equals(a, b, StringComparison.OrdinalIgnoreCase);
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
            client.DefaultRequestHeaders.UserAgent.ParseAdd("Flowtype-Desktop/" + FlowtypeVersion.CurrentLabel);
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
                "Drop a leading or trailing standalone 'Thank you'/'Thanks'/'thanks for watching' after other speech — that is a common silence hallucination, not dictated gratitude. " +
                "Apply spoken self-corrections using the final intended wording. Add punctuation and paragraph breaks. " +
                "Infer structure from speech patterns: when ideas are enumerated or delivered as distinct points, format them as Markdown bullets or numbers even if the speaker did not literally say 'bullet point'. " +
                TextProcessor.SpokenListCleanupHint(settings) +
                "When the speaker counts steps aloud (first, second, then, finally), keep that exact spoken order as one numbered list. Never emit a list item that is only a connector word such as 'And', 'And then', or 'Then' — fold connectors into the next item's content or drop them. If the speaker dictates a lone letter, output just that letter. " +
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

        public static readonly string[] PreferredModels = new string[]
        {
            "llama3.2:1b", "qwen2.5:1.5b", "qwen2.5:0.5b", "gemma2:2b", "phi3:mini",
            "llama3.2:3b", "qwen2.5:3b", "llama3.2", "phi3", "qwen2.5", "mistral"
        };

        public async Task<string> CleanupAsync(string raw, ForegroundInfo context, AppSettings settings)
        {
            return await CleanupAsync(raw, context, settings, null);
        }

        public async Task<string> CleanupAsync(string raw, ForegroundInfo context, AppSettings settings, Action<string> onDelta)
        {
            string model = await ResolveModelAsync(settings);
            if (String.IsNullOrWhiteSpace(model)) return TextProcessor.Clean(raw, settings, context);

            string system =
                "Clean the following voice dictation for insertion into " + (context == null ? "an app" : context.AppLabel) + ". " +
                "Return only the cleaned text. Preserve meaning and tone; remove fillers and false starts; honor corrections; add punctuation; " +
                "drop a leading or trailing standalone 'Thank you'/'Thanks'/'thanks for watching' after other speech (silence hallucination); " +
                "infer lists from the way points are spoken and format them as bullets or numbers; number spoken step sequences (first, second, then) in their spoken order and never emit a bullet that is only a connector word like 'and' or 'then'; keep a deliberately dictated single letter as-is; use em dashes for natural asides; adapt to the target app; " +
                TextProcessor.SpokenListCleanupHint(settings) +
                "never answer or comment on the dictation. Style: " + settings.Style + ".";

            string text = await StreamChatAsync(settings, model, system, raw, onDelta);
            if (String.IsNullOrWhiteSpace(text))
                throw new InvalidOperationException("Local model returned no text.");
            return text.Trim();
        }

        public async Task<string> TestAsync(AppSettings settings)
        {
            string model = await ResolveModelAsync(settings);
            if (String.IsNullOrWhiteSpace(model))
                throw new InvalidOperationException(
                    "No local streaming model found. Install Ollama from https://ollama.com then run:\r\n\r\nollama pull llama3.2:1b");
            string reply = await StreamChatAsync(settings, model, "Reply with exactly OK.", "Say OK.", null);
            if (String.IsNullOrWhiteSpace(reply))
                throw new InvalidOperationException("The local model produced an empty reply.");
            return model;
        }

        public async Task<List<string>> ListModelsAsync(AppSettings settings)
        {
            string root = BaseUrl(settings);
            List<string> names = await ListOllamaModelsAsync(root);
            if (names.Count == 0) names = await ListOpenAiModelsAsync(root);
            names.Sort(StringComparer.OrdinalIgnoreCase);
            return names;
        }

        public static string PickPreferredModel(IList<string> names)
        {
            if (names == null || names.Count == 0) return "";
            for (int pass = 0; pass < 2; pass++)
            {
                foreach (string preferred in PreferredModels)
                {
                    foreach (string name in names)
                    {
                        if (name == null) continue;
                        if (pass == 0 && String.Equals(name, preferred, StringComparison.OrdinalIgnoreCase))
                            return name;
                        if (pass == 1 && name.StartsWith(preferred, StringComparison.OrdinalIgnoreCase))
                            return name;
                    }
                }
            }
            return names[0] ?? "";
        }

        private static readonly Regex StreamTextField = new Regex(
            "\"(?:content|response)\"\\s*:\\s*\"((?:\\\\.|[^\"\\\\])*)\"",
            RegexOptions.CultureInvariant);

        public static string ExtractStreamDelta(string jsonLine)
        {
            if (String.IsNullOrWhiteSpace(jsonLine)) return "";
            string line = jsonLine.Trim();
            if (line.StartsWith("data:", StringComparison.OrdinalIgnoreCase))
                line = line.Substring(5).Trim();
            if (line.Length == 0 || String.Equals(line, "[DONE]", StringComparison.OrdinalIgnoreCase))
                return "";
            Match match = StreamTextField.Match(line);
            if (!match.Success) return "";
            return UnescapeJsonString(match.Groups[1].Value);
        }

        private static string UnescapeJsonString(string value)
        {
            if (String.IsNullOrEmpty(value) || value.IndexOf('\\') < 0) return value ?? "";
            return value
                .Replace("\\n", "\n")
                .Replace("\\r", "\r")
                .Replace("\\t", "\t")
                .Replace("\\\"", "\"")
                .Replace("\\\\", "\\");
        }

        private async Task<string> ResolveModelAsync(AppSettings settings)
        {
            if (settings != null && !String.IsNullOrWhiteSpace(settings.OllamaModel))
                return settings.OllamaModel.Trim();
            try
            {
                List<string> names = await ListModelsAsync(settings);
                return PickPreferredModel(names);
            }
            catch
            {
                return "";
            }
        }

        private async Task<string> StreamChatAsync(AppSettings settings, string model, string system, string user, Action<string> onDelta)
        {
            string root = BaseUrl(settings);
            Exception last = null;
            try
            {
                return await StreamOllamaChatAsync(root, model, system, user, onDelta);
            }
            catch (Exception exception) { last = exception; }
            try
            {
                return await StreamOllamaGenerateAsync(root, model, system + "\n\n" + user, onDelta);
            }
            catch (Exception exception) { last = exception; }
            try
            {
                return await StreamOpenAiChatAsync(root, model, system, user, onDelta);
            }
            catch (Exception exception) { last = exception; }
            if (last != null) throw last;
            throw new InvalidOperationException("Local streaming model did not answer.");
        }

        private async Task<string> StreamOllamaChatAsync(string root, string model, string system, string user, Action<string> onDelta)
        {
            object[] messages = new object[]
            {
                new Dictionary<string, object> { { "role", "system" }, { "content", system } },
                new Dictionary<string, object> { { "role", "user" }, { "content", user } }
            };
            Dictionary<string, object> payload = new Dictionary<string, object>();
            payload["model"] = model;
            payload["messages"] = messages;
            payload["stream"] = true;
            return await PostStreamAsync(root + "/api/chat", payload, onDelta);
        }

        private async Task<string> StreamOllamaGenerateAsync(string root, string model, string prompt, Action<string> onDelta)
        {
            Dictionary<string, object> payload = new Dictionary<string, object>();
            payload["model"] = model;
            payload["prompt"] = prompt;
            payload["stream"] = true;
            return await PostStreamAsync(root + "/api/generate", payload, onDelta);
        }

        private async Task<string> StreamOpenAiChatAsync(string root, string model, string system, string user, Action<string> onDelta)
        {
            object[] messages = new object[]
            {
                new Dictionary<string, object> { { "role", "system" }, { "content", system } },
                new Dictionary<string, object> { { "role", "user" }, { "content", user } }
            };
            Dictionary<string, object> payload = new Dictionary<string, object>();
            payload["model"] = model;
            payload["messages"] = messages;
            payload["stream"] = true;
            payload["max_tokens"] = 3000;
            payload["temperature"] = 0.1;
            return await PostStreamAsync(root.TrimEnd('/') + "/v1/chat/completions", payload, onDelta);
        }

        private async Task<string> PostStreamAsync(string url, Dictionary<string, object> payload, Action<string> onDelta)
        {
            using (HttpClient client = new HttpClient())
            using (StringContent content = new StringContent(serializer.Serialize(payload), Encoding.UTF8, "application/json"))
            {
                client.Timeout = TimeSpan.FromMinutes(2);
                client.DefaultRequestHeaders.UserAgent.ParseAdd("Flowtype-Desktop/" + FlowtypeVersion.CurrentLabel);
                using (HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, url))
                {
                    request.Content = content;
                    HttpResponseMessage response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
                    using (response)
                    {
                        if (!response.IsSuccessStatusCode)
                        {
                            string errorBody = await response.Content.ReadAsStringAsync();
                            throw new InvalidOperationException(ApiHelpers.ErrorMessage(errorBody, response.StatusCode));
                        }
                        StringBuilder output = new StringBuilder();
                        using (Stream stream = await response.Content.ReadAsStreamAsync())
                        using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
                        {
                            string line;
                            while ((line = await reader.ReadLineAsync()) != null)
                            {
                                string delta = ExtractStreamDelta(line);
                                if (delta.Length == 0) continue;
                                output.Append(delta);
                                if (onDelta != null) onDelta(output.ToString());
                            }
                        }
                        return output.ToString().Trim();
                    }
                }
            }
        }

        private async Task<List<string>> ListOllamaModelsAsync(string root)
        {
            List<string> names = new List<string>();
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    client.Timeout = TimeSpan.FromSeconds(3);
                    string body = await client.GetStringAsync(root + "/api/tags");
                    Dictionary<string, object> rootObject = serializer.DeserializeObject(body) as Dictionary<string, object>;
                    object models;
                    if (rootObject == null || !rootObject.TryGetValue("models", out models)) return names;
                    IEnumerable list = models as IEnumerable;
                    if (list == null) return names;
                    foreach (object item in list)
                    {
                        Dictionary<string, object> model = item as Dictionary<string, object>;
                        object name;
                        if (model != null && model.TryGetValue("name", out name))
                        {
                            string value = Convert.ToString(name, CultureInfo.InvariantCulture);
                            if (!String.IsNullOrWhiteSpace(value)) names.Add(value.Trim());
                        }
                    }
                }
            }
            catch { }
            return names;
        }

        private async Task<List<string>> ListOpenAiModelsAsync(string root)
        {
            List<string> names = new List<string>();
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    client.Timeout = TimeSpan.FromSeconds(3);
                    string body = await client.GetStringAsync(root.TrimEnd('/') + "/v1/models");
                    Dictionary<string, object> rootObject = serializer.DeserializeObject(body) as Dictionary<string, object>;
                    object data;
                    if (rootObject == null || !rootObject.TryGetValue("data", out data)) return names;
                    IEnumerable list = data as IEnumerable;
                    if (list == null) return names;
                    foreach (object item in list)
                    {
                        Dictionary<string, object> model = item as Dictionary<string, object>;
                        object id;
                        if (model != null && model.TryGetValue("id", out id))
                        {
                            string value = Convert.ToString(id, CultureInfo.InvariantCulture);
                            if (!String.IsNullOrWhiteSpace(value)) names.Add(value.Trim());
                        }
                    }
                }
            }
            catch { }
            return names;
        }

        private static string BaseUrl(AppSettings settings)
        {
            string value = settings == null ? "" : (settings.OllamaUrl ?? "").Trim();
            if (String.IsNullOrWhiteSpace(value)) value = "http://127.0.0.1:11434";
            return value.TrimEnd('/');
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
            client.DefaultRequestHeaders.UserAgent.ParseAdd("Flowtype-Desktop/" + FlowtypeVersion.CurrentLabel);
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
                "Drop a leading or trailing standalone 'Thank you'/'Thanks'/'thanks for watching' after other speech — that is a common silence hallucination, not dictated gratitude. " +
                "Infer lists from rhythm and enumerated ideas even when the speaker does not literally say 'bullet point'. " +
                TextProcessor.SpokenListCleanupHint(settings) +
                "When steps are counted aloud (first, second, then, finally), number them in that spoken order; never emit a list item that is only a connector word such as 'And' or 'And then'. Keep a deliberately dictated lone letter as-is. Use natural em dashes for real asides or pivots without overusing them. " +
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
            // Per-request deadlines come from CancellationTokenSource so the warmed keep-alive
            // connection can be reused by transcription; this is only the hard ceiling.
            client.Timeout = TimeSpan.FromSeconds(610);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", key);
            client.DefaultRequestHeaders.UserAgent.ParseAdd("Flowtype-Desktop/" + FlowtypeVersion.CurrentLabel);
            boundKey = key;
            return client;
        }

        public async Task WarmAsync(AppSettings settings, string apiKey)
        {
            if (String.IsNullOrWhiteSpace(apiKey)) return;
            string url = settings.GroqApiUrl.TrimEnd('/') + "/models";
            using (CancellationTokenSource warmCts = new CancellationTokenSource(TimeSpan.FromSeconds(20)))
            using (HttpResponseMessage response = await GetClient(apiKey).GetAsync(url, warmCts.Token))
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
            // Reuse the warmed keep-alive client — a fresh HttpClient per dictation pays a full
            // DNS+TCP+TLS handshake every time, exactly the latency WarmAsync exists to prepay.
            HttpClient http = GetClient(apiKey);
            using (MultipartFormDataContent form = new MultipartFormDataContent())
            using (FileStream stream = File.OpenRead(wavePath))
            using (StreamContent audio = new StreamContent(stream))
            using (CancellationTokenSource cts = new CancellationTokenSource(AudioTranscriptionTimeouts.ForWavFile(wavePath, false)))
            {
                audio.Headers.ContentType = new MediaTypeHeaderValue("audio/wav");
                form.Add(audio, "file", Path.GetFileName(wavePath));
                form.Add(new StringContent(model), "model");
                form.Add(new StringContent("json"), "response_format");
                form.Add(new StringContent("en"), "language");
                form.Add(new StringContent("0"), "temperature");
                string prompt = WhisperEngine.BuildPrompt(settings, context);
                if (prompt.Length > 0) form.Add(new StringContent(prompt), "prompt");
                HttpResponseMessage response = await http.PostAsync(url, form, cts.Token);
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
                client.Timeout = AudioTranscriptionTimeouts.ForWavFile(wavePath, turbo);
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
                if (prompt.Length > 0) form.Add(new StringContent(prompt), "prompt");
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
                FilterEmbeddedHallucinationSegments(transcript);
                return transcript;
            }
        }

        private static void FilterEmbeddedHallucinationSegments(SpeechTranscript transcript)
        {
            if (transcript == null || transcript.Segments == null || transcript.Segments.Count < 2) return;
            List<SpeechSegment> kept = new List<SpeechSegment>();
            for (int index = 0; index < transcript.Segments.Count; index++)
            {
                SpeechSegment segment = transcript.Segments[index];
                SpeechSegment previous = index > 0 ? transcript.Segments[index - 1] : null;
                SpeechSegment next = index + 1 < transcript.Segments.Count ? transcript.Segments[index + 1] : null;
                double gapBefore = previous == null ? 0 : Math.Max(0, segment.Start - previous.End);
                // End-of-recording is silence, not a zero gap — a trailing lone letter after a
                // breath is Whisper's most common hallucination. A leading letter keeps gap 0 so
                // deliberate spellings ("P as in Peter") are never eaten.
                double gapAfter = next == null ? 9.0 : Math.Max(0, next.Start - segment.End);
                double duration = Math.Max(0, segment.End - segment.Start);
                bool cueBefore = previous != null && Regex.IsMatch(previous.Text ?? "",
                    @"\b(?:letter|letters|press|type|typed|hit|key|option|plan|section|column|row|drive|vitamin|grade|as in|is)\s*[,.:;]?\s*$",
                    RegexOptions.IgnoreCase);
                if (!cueBefore && TranscriptionQuality.IsLikelyEmbeddedHallucination(segment.Text, duration, gapBefore, gapAfter)) continue;
                double thanksGapBefore = previous == null ? Math.Max(0, segment.Start) : gapBefore;
                if (TranscriptionQuality.IsLikelyThanksHallucination(segment.Text, thanksGapBefore, gapAfter)) continue;
                kept.Add(segment);
            }
            if (kept.Count == 0 || kept.Count >= transcript.Segments.Count) return;
            transcript.Segments = kept;
            StringBuilder rebuilt = new StringBuilder();
            foreach (SpeechSegment segment in kept)
            {
                if (segment == null || String.IsNullOrWhiteSpace(segment.Text)) continue;
                if (rebuilt.Length > 0) rebuilt.Append(' ');
                rebuilt.Append(segment.Text.Trim());
            }
            if (rebuilt.Length > 0) transcript.Text = rebuilt.ToString();
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
                string from;
                string to;
                if (TextProcessor.TryParseDictionaryEntry(entry, out from, out to))
                {
                    if (from.Length >= 2) terms.Add(from);
                    if (to.Length >= 2) terms.Add(to);
                    continue;
                }
                string value = (entry ?? "").Trim();
                if (!ShouldPrimeWhisperTerm(value)) continue;
                terms.Add(value);
            }
            foreach (string phrase in TextProcessor.SpokenCommandPhrases(settings))
                if (!String.IsNullOrWhiteSpace(phrase)) terms.Add(phrase);
            StringBuilder prompt = new StringBuilder();
            if (terms.Count > 0) prompt.Append(String.Join(", ", terms.Distinct(StringComparer.OrdinalIgnoreCase).ToArray()) + ".");
            // Window title is passed to LLM cleanup only — including "Target window:" here
            // makes Whisper echo it into the transcript on longer clips.
            return prompt.ToString().Replace("\"", "'").Trim();
        }

        private static bool ShouldPrimeWhisperTerm(string term)
        {
            if (String.IsNullOrWhiteSpace(term)) return false;
            term = term.Trim();
            if (term.Contains(" ")) return true;
            if (term.Any(ch => !Char.IsLetter(ch))) return true;
            return term.Length >= 10;
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
                        client.DefaultRequestHeaders.UserAgent.ParseAdd("Flowtype-Desktop/" + FlowtypeVersion.CurrentLabel);
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

    public static class GlassChrome
    {
        public static bool BackdropReadsDark(int rgbSum, int sampleCount)
        {
            if (sampleCount <= 0) return false;
            return (rgbSum / sampleCount) < 270;
        }

        public static Color Ink(bool onDark)
        {
            return onDark
                ? Color.FromArgb(255, 236, 238, 244)
                : Color.FromArgb(255, 36, 42, 52);
        }

        public static Color Bar(bool onDark, float sample)
        {
            sample = Math.Max(0f, Math.Min(1f, sample));
            if (onDark)
            {
                int zinc = 196 + (int)(52 * sample);
                return Color.FromArgb(Math.Min(255, 235 + (int)(20 * sample)), zinc, zinc, Math.Min(255, zinc + 8));
            }
            int grey = 72 + (int)(48 * sample);
            return Color.FromArgb(Math.Min(255, 220 + (int)(35 * sample)), grey, grey, Math.Min(255, grey + 10));
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
        private bool processingMode;
        private string processingPreview = "";
        private const int CompactOverlayWidth = 120;
        private const int StreamOverlayWidth = 304;
        private string theme = "Dark";
        private string mark = "Orb";
        private Bitmap glassBackdrop;
        private Point glassBackdropOffset;
        private bool pendingGlassRecapture;
        private bool glassOnDark;
        private float revealProgress = 1f;
        private bool exiting;
        private int overlaySession;
        private int pendingHideSession;
        private readonly Stopwatch revealClock = new Stopwatch();
        private const int RevealInMs = 160;
        private const int RevealOutMs = 120;
        public event Action MaximumDurationReached;

        public void SetTheme(string value)
        {
            theme = String.IsNullOrWhiteSpace(value) ? "Dark" : value.Trim();
        }

        public void SetMark(string value)
        {
            mark = String.IsNullOrWhiteSpace(value) ? "Orb" : value.Trim();
        }

        public RecordingOverlay()
        {
            FormBorderStyle = FormBorderStyle.None;
            ShowInTaskbar = false;
            TopMost = true;
            Width = 120;
            Height = 42;
            timer = new System.Windows.Forms.Timer();
            timer.Interval = 32;
            timer.Tick += delegate
            {
                UpdateRevealAnimation();
                animationTick++;
                if (pendingGlassRecapture && IsGlassTheme() && Visible && !exiting)
                {
                    pendingGlassRecapture = false;
                    CaptureGlassBackdrop();
                }
                if (processingMode) DriveProcessingBands();
                else
                {
                    level *= 0.84f;
                    for (int index = 0; index < bands.Length; index++) bands[index] *= 0.82f;
                }
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
            GraphicsPath path = new GraphicsPath();
            float max = Math.Min(bounds.Width, bounds.Height) / 2f;
            if (max < 0.6f)
            {
                path.AddRectangle(bounds);
                return path;
            }
            radius = Math.Max(0.5f, Math.Min(radius, max));
            float diameter = radius * 2f;
            path.AddArc(bounds.Left, bounds.Top, diameter, diameter, 180, 90);
            path.AddArc(bounds.Right - diameter, bounds.Top, diameter, diameter, 270, 90);
            path.AddArc(bounds.Right - diameter, bounds.Bottom - diameter, diameter, diameter, 0, 90);
            path.AddArc(bounds.Left, bounds.Bottom - diameter, diameter, diameter, 90, 90);
            path.CloseFigure();
            return path;
        }

        public void ShowRecording(string hotkey, string overlayTheme, string overlayMark)
        {
            overlaySession++;
            SetTheme(overlayTheme);
            SetMark(overlayMark);
            processingMode = false;
            processingPreview = "";
            Width = CompactOverlayWidth;
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

        public void ShowProcessing()
        {
            overlaySession++;
            processingMode = true;
            processingPreview = "";
            Width = CompactOverlayWidth;
            elapsed.Reset();
            maxRaised = false;
            exiting = false;
            timer.Start();
            if (!Visible)
            {
                revealProgress = 0f;
                revealClock.Restart();
                PositionOverlay();
                if (IsGlassTheme()) CaptureGlassBackdrop();
                Show();
            }
            else if (revealProgress >= 1f)
                revealProgress = 1f;
            RenderLayered();
        }

        public void SetProcessingPreview(string text)
        {
            if (IsDisposed) return;
            if (InvokeRequired)
            {
                try { BeginInvoke(new Action<string>(SetProcessingPreview), text ?? ""); }
                catch { }
                return;
            }
            processingPreview = text ?? "";
            int want = processingMode && processingPreview.Trim().Length > 0 ? StreamOverlayWidth : CompactOverlayWidth;
            if (Width != want)
            {
                Width = want;
                PositionOverlay();
                if (IsGlassTheme() && Visible && !exiting) CaptureGlassBackdrop();
            }
        }

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
            processingMode = false;
            processingPreview = "";
            Width = CompactOverlayWidth;
            revealProgress = 1f;
            Hide();
        }

        private RectangleF GetCapsuleBounds()
        {
            float capsuleWidth = processingMode && processingPreview.Trim().Length > 0 ? 286f : 104f;
            const float capsuleHeight = 26f;
            float x = (Width - capsuleWidth) / 2f;
            float y = (Height - capsuleHeight) / 2f;
            return new RectangleF(
                (float)Math.Floor(x) + 0.5f,
                (float)Math.Floor(y) + 0.5f,
                capsuleWidth,
                capsuleHeight);
        }

        private void ReleaseGlassBackdrop()
        {
            if (glassBackdrop == null) return;
            glassBackdrop.Dispose();
            glassBackdrop = null;
            glassOnDark = false;
        }

        private void CaptureGlassBackdrop()
        {
            ReleaseGlassBackdrop();
            bool firstHandle = !IsHandleCreated;
            if (firstHandle) CreateHandle();
            PositionOverlay();
            ClearLayeredWindow();

            RectangleF capsule = GetCapsuleBounds();
            const int pad = 10;
            int screenX = Left + (int)Math.Floor(capsule.X) - pad;
            int screenY = Top + (int)Math.Floor(capsule.Y) - pad;
            int captureWidth = (int)Math.Ceiling(capsule.Width) + pad * 2;
            int captureHeight = (int)Math.Ceiling(capsule.Height) + pad * 2;
            if (captureWidth < 2 || captureHeight < 2) return;

            bool restoreVisible = Visible;
            Hide();
            Application.DoEvents();

            Bitmap raw = null;
            try
            {
                raw = new Bitmap(captureWidth, captureHeight, PixelFormat.Format32bppPArgb);
                using (Graphics captureGraphics = Graphics.FromImage(raw))
                    captureGraphics.CopyFromScreen(screenX, screenY, 0, 0, new Size(captureWidth, captureHeight), CopyPixelOperation.SourceCopy);

                if (BackdropIsUnusable(raw))
                {
                    pendingGlassRecapture = true;
                    return;
                }

                using (Bitmap blurred = BlurBitmap(raw, 3))
                    glassBackdrop = DistortLiquidGlass(blurred);
                glassBackdropOffset = new Point(
                    (int)Math.Floor(capsule.X) - pad,
                    (int)Math.Floor(capsule.Y) - pad);
                int rgbSum;
                int sampleCount;
                glassOnDark = TrySampleBackdrop(raw, out rgbSum, out sampleCount)
                    && GlassChrome.BackdropReadsDark(rgbSum, sampleCount);
                if (firstHandle) pendingGlassRecapture = true;
            }
            catch
            {
                ReleaseGlassBackdrop();
                pendingGlassRecapture = true;
            }
            finally
            {
                if (raw != null) raw.Dispose();
                if (restoreVisible) Visible = true;
            }
        }

        private void ClearLayeredWindow()
        {
            if (!IsHandleCreated || IsDisposed) return;
            using (Bitmap clear = new Bitmap(Width, Height, PixelFormat.Format32bppPArgb))
            {
                using (Graphics graphics = Graphics.FromImage(clear))
                    graphics.Clear(Color.Transparent);
                Present(clear);
            }
        }

        private static bool BackdropIsUnusable(Bitmap source)
        {
            int rgbSum;
            int sampleCount;
            if (!TrySampleBackdrop(source, out rgbSum, out sampleCount)) return true;
            int dark = 0;
            for (int row = 0; row < 5; row++)
            {
                for (int column = 0; column < 5; column++)
                {
                    int x = Math.Max(0, Math.Min(source.Width - 1, (column * (source.Width - 1)) / 4));
                    int y = Math.Max(0, Math.Min(source.Height - 1, (row * (source.Height - 1)) / 4));
                    Color pixel = source.GetPixel(x, y);
                    if (pixel.R + pixel.G + pixel.B < 48) dark++;
                }
            }
            return dark * 4 >= sampleCount * 3;
        }

        private static bool TrySampleBackdrop(Bitmap source, out int rgbSum, out int sampleCount)
        {
            rgbSum = 0;
            sampleCount = 0;
            if (source == null || source.Width < 2 || source.Height < 2) return false;
            for (int row = 0; row < 5; row++)
            {
                for (int column = 0; column < 5; column++)
                {
                    int x = Math.Max(0, Math.Min(source.Width - 1, (column * (source.Width - 1)) / 4));
                    int y = Math.Max(0, Math.Min(source.Height - 1, (row * (source.Height - 1)) / 4));
                    Color pixel = source.GetPixel(x, y);
                    rgbSum += pixel.R + pixel.G + pixel.B;
                    sampleCount++;
                }
            }
            return sampleCount > 0;
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
            ApplyLevel(value);
        }

        public void SetLevel(WaveRecorder.AudioMeterReading reading)
        {
            SetLevel(reading == null ? 0f : reading.Boosted);
        }

        private void ApplyLevel(float value)
        {
            if (processingMode) return;
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
        }

        private void DriveProcessingBands()
        {
            float phase = animationTick * 0.17f;
            int count = bands.Length;
            float scan = (animationTick % 52) / 52f;
            for (int index = 0; index < count; index++)
            {
                float t = count <= 1 ? 0.5f : index / (float)(count - 1);
                float idle = 0.14f + 0.09f * (float)Math.Sin(phase + index * 0.58);
                float dist = Math.Abs(t - scan);
                if (dist > 0.5f) dist = 1f - dist;
                float pulse = Math.Max(0f, 1f - dist * 4.4f);
                bands[index] = idle + pulse * 0.78f;
            }
            level = 0.32f + 0.07f * (float)Math.Sin(phase * 1.35);
        }

        private void RenderLayered()
        {
            if (!IsHandleCreated || IsDisposed || !Visible) return;

            using (Bitmap bitmap = new Bitmap(Width, Height, PixelFormat.Format32bppPArgb))
            using (Graphics graphics = Graphics.FromImage(bitmap))
            {
                graphics.Clear(Color.Transparent);
                graphics.SmoothingMode = SmoothingMode.AntiAlias;
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.PixelOffsetMode = PixelOffsetMode.Half;

                float slideY = (1f - revealProgress) * (exiting ? 4f : 5f);
                graphics.TranslateTransform(0f, slideY);

                RectangleF capsule = GetCapsuleBounds();
                float cornerRadius = capsule.Height / 2f;
                if (IsGlassTheme())
                    DrawLiquidGlassCapsule(graphics, capsule, cornerRadius);
                else
                    DrawStandardCapsule(graphics, capsule, cornerRadius);

                DrawInstrumentChrome(graphics, capsule, cornerRadius);
                DrawStatusMark(graphics, capsule);
                DrawVoiceBands(graphics, capsule);

                graphics.ResetTransform();
                Present(bitmap);
            }
        }

        private void DrawInstrumentChrome(Graphics graphics, RectangleF capsule, float cornerRadius)
        {
            float dividerX = (float)Math.Round(capsule.X + 24f) + 0.5f;
            float dividerTop = capsule.Y + 6f;
            float dividerBottom = capsule.Bottom - 6f;
            Color ink = GetChromeInk();
            int dividerAlpha = IsGlassTheme() ? (glassOnDark ? 120 : 50) : 38;
            using (Pen divider = new Pen(Color.FromArgb(dividerAlpha, ink), 1f))
                graphics.DrawLine(divider, dividerX, dividerTop, dividerX, dividerBottom);
        }

        private void DrawStatusMark(Graphics graphics, RectangleF capsule)
        {
            float cx = capsule.X + 12.2f;
            float cy = capsule.Y + capsule.Height / 2f;
            Color ink = GetChromeInk();
            if (processingMode)
            {
                if (String.Equals(mark, "Grid", StringComparison.OrdinalIgnoreCase))
                {
                    DrawLiveGrid(graphics, cx, cy, true);
                    return;
                }
                using (Pen track = new Pen(Color.FromArgb(IsGlassTheme() ? 64 : 42, ink), 1f))
                    graphics.DrawEllipse(track, cx - 5.4f, cy - 5.4f, 10.8f, 10.8f);
                float start = (animationTick * 13f) % 360f;
                using (Pen arc = new Pen(GetBarColor(0.92f), 1.55f))
                {
                    arc.StartCap = LineCap.Round;
                    arc.EndCap = LineCap.Round;
                    graphics.DrawArc(arc, cx - 5.4f, cy - 5.4f, 10.8f, 10.8f, start, 108f);
                }
                using (SolidBrush core = new SolidBrush(GetBarColor(0.7f)))
                    graphics.FillEllipse(core, cx - 1.2f, cy - 1.2f, 2.4f, 2.4f);
                return;
            }

            if (String.Equals(mark, "Hex", StringComparison.OrdinalIgnoreCase))
                DrawLiveHex(graphics, cx, cy);
            else if (String.Equals(mark, "Iris", StringComparison.OrdinalIgnoreCase))
                DrawLiveIris(graphics, cx, cy);
            else if (String.Equals(mark, "Grid", StringComparison.OrdinalIgnoreCase))
                DrawLiveGrid(graphics, cx, cy, false);
            else
                DrawLiveOrb(graphics, cx, cy);
        }

        private void DrawLiveGrid(Graphics graphics, float cx, float cy, bool processing)
        {
            const int cols = 5;
            const int rows = 5;
            const float gap = 2.15f;
            const float dot = 0.78f;
            // 5×5 minus the four corners — same voice columns, circular silhouette.
            const float radiusSq = 5.5f;
            float live = Math.Max(0f, Math.Min(1f, level));
            float progress = processing ? ((animationTick % 55) / 55f) * 24f : 0f;
            float originX = cx - (cols - 1) * gap / 2f;
            float originY = cy - (rows - 1) * gap / 2f;
            float midCol = (cols - 1) / 2f;
            float midRow = (rows - 1) / 2f;
            for (int col = 0; col < cols; col++)
            {
                float colPhase = processing
                    ? progress * 0.52f + col * 1.15f
                    : animationTick * 0.07f + col * 1.15f;
                float wave = (float)((Math.Sin(colPhase) + 1.0) * 0.5);
                int fill = processing
                    ? (int)Math.Round(1 + wave * 4)
                    : (int)Math.Round(1 + Math.Max(0.18f, live) * (0.45f + 0.55f * wave) * 4);
                if (fill < 1) fill = 1;
                if (fill > 5) fill = 5;
                int topLit = 5 - fill;
                for (int row = 0; row < rows; row++)
                {
                    float dx = col - midCol;
                    float dy = row - midRow;
                    if (dx * dx + dy * dy > radiusSq) continue;
                    float amount = row > topLit ? 0.94f : (row == topLit ? 1f : 0.08f);
                    DrawLiveDot(graphics, originX + col * gap, originY + row * gap, dot, amount);
                }
            }
        }

        private void DrawLiveHex(Graphics graphics, float cx, float cy)
        {
            float live = Math.Max(0f, Math.Min(1f, level));
            float breathe = 0.5f + 0.5f * (float)Math.Sin(animationTick * 0.085);
            float pulse = 0.28f + 0.42f * live + 0.22f * breathe * (0.4f + 0.6f * live);
            DrawLiveDot(graphics, cx, cy, 1.65f, Math.Min(1f, pulse + 0.12f));
            for (int spoke = 0; spoke < 6; spoke++)
            {
                double angle = spoke * Math.PI / 3.0 - Math.PI / 2.0;
                float x = cx + (float)Math.Cos(angle) * 4.7f;
                float y = cy + (float)Math.Sin(angle) * 4.7f;
                DrawLiveDot(graphics, x, y, 1.4f, pulse * 0.92f);
            }
        }

        private void DrawLiveIris(Graphics graphics, float cx, float cy)
        {
            float live = Math.Max(0f, Math.Min(1f, level));
            float breathe = 0.5f + 0.5f * (float)Math.Sin(animationTick * 0.085);
            float pulse = 0.18f + 0.62f * live + 0.16f * breathe * (0.3f + 0.7f * live);
            Color ink = GetBarColor(Math.Min(1f, 0.55f + pulse));
            float ringR = 6.15f;
            using (Pen track = new Pen(Color.FromArgb((int)(40 + 70 * pulse), ink), 1.15f))
                graphics.DrawEllipse(track, cx - ringR, cy - ringR, ringR * 2f, ringR * 2f);
            float fillR = 1.35f + 3.55f * pulse;
            using (GraphicsPath fill = new GraphicsPath())
            {
                fill.AddEllipse(cx - fillR, cy - fillR, fillR * 2f, fillR * 2f);
                using (PathGradientBrush glow = new PathGradientBrush(fill))
                {
                    glow.CenterColor = Color.FromArgb((int)(200 + 55 * pulse), ink);
                    glow.SurroundColors = new[] { Color.FromArgb((int)(30 + 50 * pulse), ink) };
                    glow.FocusScales = new PointF(0.38f, 0.38f);
                    graphics.FillPath(glow, fill);
                }
            }
            using (SolidBrush core = new SolidBrush(Color.FromArgb((int)(220 + 35 * pulse), GetBarColor(1f))))
                graphics.FillEllipse(core, cx - 1.15f, cy - 1.15f, 2.3f, 2.3f);
        }

        private void DrawLiveDot(Graphics graphics, float x, float y, float radius, float amount)
        {
            amount = Math.Max(0.08f, Math.Min(1f, amount));
            Color color = GetBarColor(amount);
            int alpha = Math.Max(28, Math.Min(255, (int)(38 + 217 * amount)));
            using (SolidBrush fill = new SolidBrush(Color.FromArgb(alpha, color)))
                graphics.FillEllipse(fill, x - radius, y - radius, radius * 2f, radius * 2f);
        }

        private void DrawLiveOrb(Graphics graphics, float cx, float cy)
        {
            float live = Math.Max(0f, Math.Min(1f, level));
            float breathe = 0.5f + 0.5f * (float)Math.Sin(animationTick * 0.085);
            float pulse = 0.22f + 0.58f * live + 0.18f * breathe * (0.35f + 0.65f * live);
            Color ink = GetBarColor(Math.Min(1f, 0.5f + pulse));
            Color peak = GetBarColor(Math.Min(1f, pulse + 0.25f));
            float haloR = 7.0f + 1.6f * live;
            float bodyR = 4.2f + 0.9f * pulse;
            float coreR = 1.45f + 0.75f * pulse;

            using (GraphicsPath halo = new GraphicsPath())
            {
                halo.AddEllipse(cx - haloR, cy - haloR, haloR * 2f, haloR * 2f);
                using (PathGradientBrush glow = new PathGradientBrush(halo))
                {
                    glow.CenterColor = Color.FromArgb((int)(24 + 120 * pulse), ink);
                    glow.SurroundColors = new[] { Color.FromArgb(0, ink) };
                    glow.FocusScales = new PointF(0.2f, 0.2f);
                    graphics.FillPath(glow, halo);
                }
            }

            using (GraphicsPath body = new GraphicsPath())
            {
                body.AddEllipse(cx - bodyR, cy - bodyR, bodyR * 2f, bodyR * 2f);
                using (PathGradientBrush fill = new PathGradientBrush(body))
                {
                    fill.CenterColor = Color.FromArgb((int)(170 + 85 * pulse), peak);
                    fill.SurroundColors = new[] { Color.FromArgb((int)(40 + 80 * pulse), ink) };
                    fill.FocusScales = new PointF(0.4f, 0.4f);
                    graphics.FillPath(fill, body);
                }
            }

            float specX = cx - bodyR * 0.28f;
            float specY = cy - bodyR * 0.32f;
            using (SolidBrush spec = new SolidBrush(Color.FromArgb((int)(36 + 70 * pulse), 255, 255, 255)))
                graphics.FillEllipse(spec, specX - 1.15f, specY - 0.9f, 2.3f, 1.8f);
            using (SolidBrush core = new SolidBrush(Color.FromArgb((int)(205 + 50 * pulse), peak)))
                graphics.FillEllipse(core, cx - coreR, cy - coreR, coreR * 2f, coreR * 2f);
        }

        private void DrawVoiceBands(Graphics graphics, RectangleF capsule)
        {
            float left = capsule.X + 29f;
            float right = capsule.Right - 8f;
            if (processingMode && processingPreview.Trim().Length > 0)
            {
                DrawProcessingPreview(graphics, left, right, capsule);
                return;
            }
            float region = right - left;
            const int barPx = 3;
            const int gapPx = 2;
            int total = bands.Length * barPx + (bands.Length - 1) * gapPx;
            float startX = (float)Math.Round(left + (region - total) / 2f);
            float centerY = capsule.Y + capsule.Height / 2f;
            float maxTravel = capsule.Height - 10f;
            float mid = (bands.Length - 1) / 2f;
            for (int index = 0; index < bands.Length; index++)
            {
                float envelope = 0.62f + 0.38f * (float)Math.Cos((index - mid) / Math.Max(1f, mid) * Math.PI * 0.5);
                float sample = Math.Max(0.08f, Math.Min(1f, bands[index] * envelope));
                float barHeight = 3.2f + sample * maxTravel;
                float x = startX + index * (barPx + gapPx);
                float y = (float)Math.Round(centerY - barHeight / 2f);
                RectangleF bar = new RectangleF(x, y, barPx, (float)Math.Round(barHeight));
                Color peak = GetBarColor(sample);
                Color dim = GetBarColor(sample * 0.42f);
                using (GraphicsPath barPath = RoundedRectangle(bar, barPx / 2f))
                using (LinearGradientBrush fill = new LinearGradientBrush(bar, peak, dim, LinearGradientMode.Vertical))
                    graphics.FillPath(fill, barPath);
            }
        }

        private void DrawProcessingPreview(Graphics graphics, float left, float right, RectangleF capsule)
        {
            string text = processingPreview.Replace('\r', ' ').Replace('\n', ' ').Trim();
            if (text.Length == 0) return;
            float maxWidth = Math.Max(8f, right - left);
            float y = capsule.Y + 4.5f;
            Color ink = GetChromeInk();
            using (Font font = new Font("Segoe UI", 8.25f, FontStyle.Regular, GraphicsUnit.Point))
            using (SolidBrush brush = new SolidBrush(Color.FromArgb(IsGlassTheme() ? (glassOnDark ? 220 : 180) : 200, ink)))
            {
                if (graphics.MeasureString(text, font).Width > maxWidth)
                {
                    while (text.Length > 1 && graphics.MeasureString("…" + text, font).Width > maxWidth)
                        text = text.Substring(1);
                    text = "…" + text;
                }
                graphics.DrawString(text, font, brush, left, y);
            }
        }

        private Color GetChromeInk()
        {
            if (IsGlassTheme()) return GlassChrome.Ink(glassOnDark);
            if (String.Equals(theme, "Light", StringComparison.OrdinalIgnoreCase))
                return Color.FromArgb(255, 24, 24, 27);
            if (String.Equals(theme, "Purple", StringComparison.OrdinalIgnoreCase))
                return Color.FromArgb(255, 196, 184, 255);
            if (String.Equals(theme, "Ember", StringComparison.OrdinalIgnoreCase))
                return Color.FromArgb(255, 236, 196, 148);
            return Color.FromArgb(255, 228, 228, 232);
        }

        private void DrawStandardCapsule(Graphics graphics, RectangleF capsule, float cornerRadius)
        {
            for (int spread = 3; spread >= 1; spread--)
            {
                RectangleF glow = capsule;
                glow.Inflate(spread * 0.4f, spread * 0.4f);
                glow.Y += spread * 0.12f;
                using (GraphicsPath shadowPath = RoundedRectangle(glow, cornerRadius + spread * 0.12f))
                using (SolidBrush shadow = new SolidBrush(Color.FromArgb(5 + spread * 3, 0, 0, 0)))
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
            Color outer = borderColor;
            if (String.Equals(theme, "Dark", StringComparison.OrdinalIgnoreCase))
                outer = Color.FromArgb(255, 86, 86, 94);
            else if (String.Equals(theme, "Purple", StringComparison.OrdinalIgnoreCase))
                outer = Color.FromArgb(255, 108, 96, 148);
            else if (String.Equals(theme, "Ember", StringComparison.OrdinalIgnoreCase))
                outer = Color.FromArgb(255, 168, 92, 42);
            else if (String.Equals(theme, "Light", StringComparison.OrdinalIgnoreCase))
                outer = Color.FromArgb(255, 148, 148, 156);

            using (Pen rim = new Pen(outer, 1f))
                graphics.DrawPath(rim, capsulePath);
        }

        private void DrawLiquidGlassCapsule(Graphics graphics, RectangleF capsule, float cornerRadius)
        {
            RectangleF halo = capsule;
            halo.Inflate(1.15f, 1.15f);
            using (GraphicsPath haloPath = RoundedRectangle(halo, cornerRadius + 0.4f))
            using (SolidBrush haloBrush = new SolidBrush(Color.FromArgb(28, 0, 0, 0)))
                graphics.FillPath(haloBrush, haloPath);

            using (GraphicsPath capsulePath = RoundedRectangle(capsule, cornerRadius))
            {
                GraphicsState clipState = graphics.Save();
                graphics.SetClip(capsulePath);

                if (glassBackdrop != null)
                    graphics.DrawImage(glassBackdrop, glassBackdropOffset.X, glassBackdropOffset.Y);
                else
                {
                    using (SolidBrush fallback = new SolidBrush(Color.FromArgb(200, 236, 240, 246)))
                        graphics.FillPath(fallback, capsulePath);
                }

                using (SolidBrush frost = new SolidBrush(Color.FromArgb(52, 255, 255, 255)))
                    graphics.FillPath(frost, capsulePath);

                RectangleF shine = new RectangleF(capsule.X + 1.5f, capsule.Y + 1f, capsule.Width - 3f, capsule.Height * 0.42f);
                using (GraphicsPath shinePath = RoundedRectangle(shine, Math.Max(1f, cornerRadius - 1.2f)))
                using (LinearGradientBrush gloss = new LinearGradientBrush(
                    shine, Color.FromArgb(54, 255, 255, 255), Color.FromArgb(0, 255, 255, 255), LinearGradientMode.Vertical))
                    graphics.FillPath(gloss, shinePath);

                graphics.Restore(clipState);

                using (Pen rim = new Pen(Color.FromArgb(150, 255, 255, 255), 1f))
                    graphics.DrawPath(rim, capsulePath);
            }
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
            if (String.Equals(theme, "Ember", StringComparison.OrdinalIgnoreCase))
            {
                top = Color.FromArgb(255, 18, 8, 4);
                bottom = Color.FromArgb(255, 6, 2, 1);
                borderColor = Color.FromArgb(255, 168, 92, 42);
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
            borderColor = Color.FromArgb(255, 86, 86, 94);
        }

        private Color GetBarColor(float sample)
        {
            sample = Math.Max(0f, Math.Min(1f, sample));
            int alpha = 210 + (int)(45 * sample);
            if (IsGlassTheme()) return GlassChrome.Bar(glassOnDark, sample);
            if (String.Equals(theme, "Light", StringComparison.OrdinalIgnoreCase))
                return Color.FromArgb(255, 28 + (int)(12 * sample), 28 + (int)(12 * sample), 32);
            if (String.Equals(theme, "Ember", StringComparison.OrdinalIgnoreCase))
            {
                int red = 196 + (int)(52 * sample);
                int green = 92 + (int)(132 * sample);
                int blue = 36 + (int)(150 * sample);
                return Color.FromArgb(255, Math.Min(255, red), Math.Min(255, green), Math.Min(255, blue));
            }
            if (String.Equals(theme, "Purple", StringComparison.OrdinalIgnoreCase))
            {
                int red = 168 + (int)(48 * sample);
                int green = 148 + (int)(50 * sample);
                return Color.FromArgb(255, red, green, 255);
            }
            int zinc = 168 + (int)(80 * sample);
            return Color.FromArgb(Math.Min(255, alpha), zinc, zinc, Math.Min(255, zinc + 6));
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
        private readonly CheckBox handsFreeBox = new CheckBox();
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
        private readonly CheckBox autoUpdateBox = new CheckBox();
        private readonly ComboBox overlayThemeBox = new ComboBox();
        private readonly ComboBox overlayMarkBox = new ComboBox();
        private readonly TextBox dictionaryBox = new TextBox();
        private readonly TextBox snippetsBox = new TextBox();
        private readonly CheckBox spokenListsBox = new CheckBox();
        private readonly TextBox spokenBulletBox = new TextBox();
        private readonly TextBox spokenNumberBox = new TextBox();
        private readonly CheckBox agentEnabledBox = new CheckBox();
        private readonly ComboBox agentHotkeyBox = new ComboBox();
        private readonly TextBox agentEndpointBox = new TextBox();
        private readonly Label agentStatusLabel = new Label();
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
        private float micTestRawPeak;
        private float micTestBoostedPeak;
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
            tabs.TabPages.Add(BuildAgentTab());
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
            Label intro = LabelAt("Hold your push-to-talk key to dictate. Double-press quickly for hands-free mode, then press the key once more to finish.", 24, 22, 650, 36);
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
            ConfigureDropDown(hotkeyBox, 210, 116, 430);
            hotkeyBox.Items.AddRange(Hotkeys.Names.Cast<object>().ToArray());
            page.Controls.Add(hotkeyBox);
            ConfigureCheck(handsFreeBox, "Double-press key for hands-free mode (talk without holding)", 24, 148, 620);
            page.Controls.Add(handsFreeBox);
            Label handsFreeHint = LabelAt("Press the same key again (or Escape) to finish and insert.", 42, 178, 600, 20);
            handsFreeHint.ForeColor = UiTheme.TextMuted;
            handsFreeHint.Font = AppFonts.Ui(8.75f, FontStyle.Regular);
            page.Controls.Add(handsFreeHint);
            page.Controls.Add(LabelAt("Writing style", 24, 204, 170, 24));
            ConfigureDropDown(styleBox, 210, 200, 230);
            styleBox.Items.AddRange(new object[] { "Natural", "Concise", "Formal", "Casual", "Verbatim" });
            page.Controls.Add(styleBox);
            page.Controls.Add(LabelAt("Voice capsule", 24, 248, 92, 24));
            ConfigureDropDown(overlayThemeBox, 118, 244, 168);
            overlayThemeBox.Items.AddRange(new object[] { "Dark", "Dark purple", "Light", "Ember", "Liquid glass" });
            page.Controls.Add(overlayThemeBox);
            page.Controls.Add(LabelAt("Live mark", 300, 248, 72, 24));
            ConfigureDropDown(overlayMarkBox, 374, 244, 150);
            overlayMarkBox.Items.AddRange(new object[] { "Orb", "Hex", "Iris", "Grid" });
            page.Controls.Add(overlayMarkBox);

            ConfigureCheck(cleanupBox, "Smart cleanup (fillers, punctuation, lists)", 24, 294, 540);
            page.Controls.Add(LabelAt("Cleanup engine", 24, 342, 170, 24));
            ConfigureDropDown(cleanupProviderBox, 210, 338, 430);
            cleanupProviderBox.Items.AddRange(new object[]
            {
                "Built-in — free, offline",
                "OpenRouter — cloud polish",
                "OpenAI — your key",
                "Ollama — local streaming model"
            });
            page.Controls.Add(cleanupProviderBox);
            ConfigureCheck(contextBox, "Adapt cleanup to the active app/window", 24, 384, 540);
            ConfigureCheck(historyBox, "Keep a local history of dictations", 24, 422, 540);
            ConfigureCheck(recoveryBox, "Save failed recordings to Recovery folder", 24, 460, 590);
            ConfigureCheck(startupBox, "Start with Windows", 24, 498, 540);
            ConfigureCheck(autoUpdateBox, "Check for updates automatically", 24, 536, 620);
            page.Controls.AddRange(new Control[] { cleanupBox, contextBox, historyBox, recoveryBox, startupBox, autoUpdateBox });

            Label optionalTitle = LabelAt("Optional", 24, 580, 200, 24);
            optionalTitle.Font = AppFonts.Ui(10f, FontStyle.Bold);
            optionalTitle.ForeColor = UiTheme.TextMuted;
            page.Controls.Add(optionalTitle);
            ConfigureCheck(pasteBox, "Leave each dictation on the clipboard after insert", 24, 610, 620);
            page.Controls.Add(pasteBox);
            Label pasteHint = LabelAt("Off by default. Inserts into your field, then puts back whatever you had copied.", 42, 640, 600, 32);
            pasteHint.ForeColor = UiTheme.TextMuted;
            pasteHint.Font = AppFonts.Ui(8.75f, FontStyle.Regular);
            page.Controls.Add(pasteHint);

            Label perfTitle = LabelAt("Performance", 24, 682, 200, 24);
            perfTitle.Font = AppFonts.Ui(10f, FontStyle.Bold);
            page.Controls.Add(perfTitle);
            ConfigureCheck(turboBox, "Fast mode — quicker on long dictations", 24, 712, 620);
            ConfigureCheck(suppressNonSpeechBox, "Filter non-speech sounds (may drop quiet words)", 24, 744, 620);
            ConfigureCheck(completionSoundBox, "Sound effects on start and finish", 24, 776, 620);
            ConfigureCheck(insertNotifyBox, "Tray toast after each dictation", 24, 808, 620);
            page.Controls.AddRange(new Control[] { turboBox, suppressNonSpeechBox, completionSoundBox, insertNotifyBox });
            page.Controls.Add(LabelAt("Microphone boost", 24, 846, 140, 24));
            micGainBar.SetBounds(170, 842, 360, 45);
            micGainBar.Minimum = 8;
            micGainBar.Maximum = 25;
            micGainBar.TickFrequency = 1;
            micGainBar.ValueChanged += delegate
            {
                float gain = micGainBar.Value / 10f;
                micGainLabel.Text = gain.ToString("0.0", CultureInfo.InvariantCulture) + "×";
                if (micTestRecorder.IsRecording) micTestRecorder.MicGain = gain;
            };
            page.Controls.Add(micGainBar);
            micGainLabel.SetBounds(540, 850, 60, 24);
            page.Controls.Add(micGainLabel);
            Label micHealthTitle = LabelAt("Microphone health", 24, 888, 420, 24);
            micHealthTitle.Font = AppFonts.Ui(10f, FontStyle.Bold);
            page.Controls.Add(micHealthTitle);
            micLevelBar.SetBounds(24, 918, 420, 18);
            micLevelBar.Minimum = 0;
            micLevelBar.Maximum = 100;
            micLevelBar.Style = ProgressBarStyle.Continuous;
            page.Controls.Add(micLevelBar);
            micTestButton.SetBounds(456, 910, 110, 34);
            micTestButton.Text = "Test 3s";
            micTestButton.Click += MicTestClicked;
            page.Controls.Add(micTestButton);
            micTestStatus.SetBounds(24, 948, 650, 72);
            micTestStatus.ForeColor = UiTheme.TextMuted;
            micTestStatus.AutoSize = false;
            micTestStatus.Text = "The bar is your real voice at the mic (not Whisper). Speak normally and aim for 15–40%. Boost is only for quiet mics — 2× is not a quality score.";
            page.Controls.Add(micTestStatus);
            latencyLabel.SetBounds(24, 1024, 650, 22);
            latencyLabel.ForeColor = UiTheme.TextMuted;
            latencyLabel.Font = AppFonts.Ui(8.75f, FontStyle.Regular);
            latencyLabel.Text = LatencyStats.Summary;
            page.Controls.Add(latencyLabel);

            Label privacy = LabelAt("Successful audio is always deleted. Flowtype has no telemetry or account system.", 24, 1052, 640, 40);
            privacy.ForeColor = UiTheme.TextMuted;
            page.Controls.Add(privacy);
            return page;
        }

        private TabPage BuildAgentTab()
        {
            TabPage page = NewTab("Agent");
            Label intro = LabelAt(
                "Agent mode is a second push-to-talk key. Instead of typing what you said, it hands the ask to an AI agent already running on this PC — and that agent does the work. Your dictation key is untouched.",
                24, 22, 660, 56);
            intro.Font = AppFonts.UiLarge(10.5f);
            page.Controls.Add(intro);

            ConfigureCheck(agentEnabledBox, "Enable agent mode", 24, 88, 420);
            agentEnabledBox.Font = AppFonts.Ui(10f, FontStyle.Bold);
            page.Controls.Add(agentEnabledBox);

            page.Controls.Add(LabelAt("Agent key", 24, 130, 170, 24));
            ConfigureDropDown(agentHotkeyBox, 210, 126, 260);
            agentHotkeyBox.Items.AddRange(Hotkeys.Names.Cast<object>().ToArray());
            page.Controls.Add(agentHotkeyBox);
            Label chordHint = LabelAt("Must differ from your dictation key. Hold it, speak the ask, release.", 210, 158, 460, 20);
            chordHint.ForeColor = UiTheme.TextMuted;
            chordHint.Font = AppFonts.Ui(8.75f, FontStyle.Regular);
            page.Controls.Add(chordHint);

            AddTextField(page, "Agent endpoint", agentEndpointBox, 24, 190, false);
            Label endpointHint = LabelAt(
                "Must be on this PC (127.0.0.1 or localhost). Any local runtime that accepts a JSON POST — the bundled daemon, OpenCode, Codex, n8n, or your own script. Flowtype only sends the words; it never runs anything itself.",
                210, 226, 460, 44);
            endpointHint.ForeColor = UiTheme.TextMuted;
            endpointHint.Font = AppFonts.Ui(8.75f, FontStyle.Regular);
            page.Controls.Add(endpointHint);

            Button testAgentButton = ButtonAt("Test connection", 210, 278, 150, 34);
            testAgentButton.Click += delegate { TestAgentEndpoint(testAgentButton); };
            page.Controls.Add(testAgentButton);

            agentStatusLabel.SetBounds(374, 284, 300, 24);
            agentStatusLabel.ForeColor = UiTheme.TextMuted;
            agentStatusLabel.Font = AppFonts.Ui(9f, FontStyle.Regular);
            agentStatusLabel.Text = "Not checked yet.";
            page.Controls.Add(agentStatusLabel);

            Label runTitle = LabelAt("Start the bundled agent daemon", 24, 336, 500, 26);
            runTitle.Font = AppFonts.Ui(10f, FontStyle.Bold);
            page.Controls.Add(runTitle);
            TextBox runBox = new TextBox();
            runBox.ReadOnly = true;
            runBox.SetBounds(24, 368, 646, 30);
            runBox.Font = new Font(FontFamily.GenericMonospace, 9f);
            runBox.Text = "python agent-bridge\\flowtype_agentd.py";
            page.Controls.Add(runBox);
            Label runHint = LabelAt(
                "Keeps one agent session warm so an ask answers in seconds. Every ask and reply is written to agent-bridge\\flight-recorder.jsonl, so what the voice did is always answerable.",
                24, 404, 646, 44);
            runHint.ForeColor = UiTheme.TextMuted;
            runHint.Font = AppFonts.Ui(8.75f, FontStyle.Regular);
            page.Controls.Add(runHint);

            Label safety = LabelAt(
                "Safety: the agent runs under its own permission model, in its own process. Turning agent mode off here disables the key completely — dictation keeps working.",
                24, 458, 646, 44);
            safety.ForeColor = UiTheme.TextMuted;
            safety.Font = AppFonts.Ui(8.75f, FontStyle.Regular);
            page.Controls.Add(safety);
            return page;
        }

        private async void TestAgentEndpoint(Button button)
        {
            string endpoint = agentEndpointBox.Text.Trim();
            if (endpoint.Length == 0)
            {
                agentStatusLabel.Text = "Enter an endpoint first.";
                return;
            }
            if (!AgentBridge.IsLoopbackEndpoint(endpoint))
            {
                agentStatusLabel.ForeColor = Color.FromArgb(176, 58, 46);
                agentStatusLabel.Text = "Must be 127.0.0.1 or localhost.";
                return;
            }
            button.Enabled = false;
            agentStatusLabel.ForeColor = UiTheme.TextMuted;
            agentStatusLabel.Text = "Checking…";
            try
            {
                string statusUrl = endpoint;
                int lastSlash = endpoint.LastIndexOf('/');
                if (lastSlash > "https://".Length) statusUrl = endpoint.Substring(0, lastSlash) + "/status";
                using (HttpClient client = new HttpClient())
                {
                    client.Timeout = TimeSpan.FromSeconds(5);
                    client.DefaultRequestHeaders.Add("X-Flowtype-Token", AgentBridge.TokenForStatus());
                    string body = await client.GetStringAsync(statusUrl);
                    bool warm = body.IndexOf("\"warm\":true", StringComparison.OrdinalIgnoreCase) >= 0;
                    agentStatusLabel.ForeColor = Color.FromArgb(24, 128, 74);
                    agentStatusLabel.Text = warm ? "Connected — session is warm." : "Connected.";
                }
            }
            catch (Exception exception)
            {
                agentStatusLabel.ForeColor = Color.FromArgb(176, 58, 46);
                string reason = exception.InnerException != null ? exception.InnerException.Message : exception.Message;
                agentStatusLabel.Text = reason.Length > 60 ? "Nothing listening there." : "Nothing listening there.";
            }
            finally { button.Enabled = true; }
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

            Label polishTitle = LabelAt("Local streaming model", 24, 256, 400, 28);
            polishTitle.Font = AppFonts.Ui(10f, FontStyle.Bold);
            page.Controls.Add(polishTitle);
            Label polishNote = LabelAt("Optional polish after Whisper. Works with Ollama, LM Studio, or llama.cpp on this PC. Tokens stream on the capsule, then Flowtype pastes once into the field you were in — nothing is uploaded. Find local models turns this on for dictation. Leave the model blank and Flowtype will pick a small one if any are installed.", 24, 290, 640, 54);
            polishNote.ForeColor = Color.FromArgb(95, 100, 112);
            page.Controls.Add(polishNote);
            AddTextField(page, "Local URL", ollamaUrlBox, 24, 356, false);
            AddTextField(page, "Model name", ollamaModelBox, 24, 412, false);
            Button findModelsButton = ButtonAt("Find local models", 210, 468, 160, 34);
            findModelsButton.Click += async delegate
            {
                findModelsButton.Enabled = false;
                try
                {
                    List<string> names = await new OllamaEngine().ListModelsAsync(ReadValues());
                    if (names.Count == 0)
                    {
                        MessageBox.Show(this,
                            "No local models answered at that URL.\r\n\r\nInstall Ollama from https://ollama.com then run:\r\nollama pull llama3.2:1b\r\n\r\nLM Studio and llama.cpp work too — point the URL at their OpenAI-compatible server.",
                            "Flowtype", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }
                    string pick = OllamaEngine.PickPreferredModel(names);
                    if (!String.IsNullOrWhiteSpace(pick)) ollamaModelBox.Text = pick;
                    UseLocalStreamingModel(pick);
                    MessageBox.Show(this,
                        "Found " + names.Count.ToString(CultureInfo.InvariantCulture) + " local model" + (names.Count == 1 ? "" : "s") + ".\r\n\r\nUsing: " + pick + "\r\n\r\nDictation will polish through this model and type the result into the field you were in.\r\n\r\n" + String.Join("\r\n", names.ToArray()),
                        "Flowtype", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception exception) { MessageBox.Show(this, exception.Message, "Local model", MessageBoxButtons.OK, MessageBoxIcon.Error); }
                finally { findModelsButton.Enabled = true; }
            };
            page.Controls.Add(findModelsButton);
            Button testOllamaButton = ButtonAt("Test stream", 380, 468, 120, 34);
            testOllamaButton.Click += async delegate
            {
                testOllamaButton.Enabled = false;
                try
                {
                    string model = await new OllamaEngine().TestAsync(ReadValues());
                    UseLocalStreamingModel(model);
                    MessageBox.Show(this, "Local streaming model answered.\r\n\r\nModel: " + model + "\r\n\r\nDictation will now polish through it and type into the focused field.",
                        "Flowtype", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception exception) { MessageBox.Show(this, exception.Message, "Connection failed", MessageBoxButtons.OK, MessageBoxIcon.Error); }
                finally { testOllamaButton.Enabled = true; }
            };
            page.Controls.Add(testOllamaButton);
            return page;
        }

        private TabPage BuildPersonalizationTab()
        {
            TabPage page = NewTab("Personalization");
            Label listsTitle = LabelAt("Spoken lists", 24, 22, 620, 26);
            listsTitle.Font = AppFonts.Ui(10f, FontStyle.Bold);
            page.Controls.Add(listsTitle);
            ConfigureCheck(spokenListsBox, "Enable spoken list commands", 24, 50, 620);
            page.Controls.Add(spokenListsBox);
            page.Controls.Add(LabelAt("New bullet when you say", 24, 90, 186, 24));
            spokenBulletBox.SetBounds(210, 86, 430, 30);
            spokenBulletBox.MaxLength = 120;
            StyleField(spokenBulletBox);
            page.Controls.Add(spokenBulletBox);
            page.Controls.Add(LabelAt("New number when you say", 24, 126, 186, 24));
            spokenNumberBox.SetBounds(210, 122, 430, 30);
            spokenNumberBox.MaxLength = 120;
            StyleField(spokenNumberBox);
            page.Controls.Add(spokenNumberBox);
            Label listsHint = LabelAt("Say the phrase during a take to start a new item. Extra phrases can be comma-separated. Also understands bullet point, next bullet, and similar.", 24, 158, 650, 36);
            listsHint.ForeColor = UiTheme.TextMuted;
            listsHint.Font = AppFonts.Ui(8.75f, FontStyle.Regular);
            page.Controls.Add(listsHint);
            spokenListsBox.CheckedChanged += delegate { SyncSpokenListFields(); };

            Label dictionaryTitle = LabelAt("Dictionary", 24, 210, 620, 26);
            dictionaryTitle.Font = AppFonts.Ui(10f, FontStyle.Bold);
            page.Controls.Add(dictionaryTitle);
            page.Controls.Add(LabelAt("One term per line. Use spoken => written so Whisper misspellings still convert, e.g. eppi => epa or flow type => Flowtype.", 24, 238, 650, 34));
            dictionaryBox.SetBounds(24, 276, 650, 150);
            dictionaryBox.Multiline = true;
            dictionaryBox.ScrollBars = ScrollBars.Vertical;
            dictionaryBox.AcceptsReturn = true;
            page.Controls.Add(dictionaryBox);

            Label snippetsTitle = LabelAt("Voice snippets", 24, 444, 620, 26);
            snippetsTitle.Font = AppFonts.Ui(10f, FontStyle.Bold);
            page.Controls.Add(snippetsTitle);
            page.Controls.Add(LabelAt("One per line as trigger => expansion, e.g. my sign off => Cheers, Alex", 24, 472, 650, 32));
            snippetsBox.SetBounds(24, 508, 650, 140);
            snippetsBox.Multiline = true;
            snippetsBox.ScrollBars = ScrollBars.Vertical;
            snippetsBox.AcceptsReturn = true;
            page.Controls.Add(snippetsBox);
            return page;
        }

        private void SyncSpokenListFields()
        {
            bool on = spokenListsBox.Checked;
            spokenBulletBox.Enabled = on;
            spokenNumberBox.Enabled = on;
        }

        private void LoadValues(AppSettings value)
        {
            engineBox.SelectedIndex = String.Equals(value.Engine, "OpenAI", StringComparison.OrdinalIgnoreCase) ? 2 :
                String.Equals(value.Engine, "Groq", StringComparison.OrdinalIgnoreCase) ? 1 : 0;
            hotkeyBox.SelectedItem = value.Hotkey;
            if (hotkeyBox.SelectedIndex < 0) hotkeyBox.SelectedIndex = 0;
            handsFreeBox.Checked = value.HandsFreeDoubleTap;
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
            autoUpdateBox.Checked = value.AutoCheckUpdates;
            overlayThemeBox.SelectedIndex = OverlayThemeToIndex(value.OverlayTheme);
            overlayMarkBox.SelectedIndex = OverlayMarkToIndex(value.OverlayMark);
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
            spokenListsBox.Checked = value.SpokenListsEnabled;
            spokenBulletBox.Text = value.SpokenBulletPhrase ?? "next point";
            spokenNumberBox.Text = value.SpokenNumberPhrase ?? "next number";
            SyncSpokenListFields();
            agentEnabledBox.Checked = value.AgentModeEnabled;
            agentHotkeyBox.SelectedItem = value.AgentHotkey;
            if (agentHotkeyBox.SelectedIndex < 0) agentHotkeyBox.SelectedIndex = 0;
            agentEndpointBox.Text = value.AgentEndpoint;
            UpdateLocalStatus();
        }

        private AppSettings ReadValues()
        {
            AppSettings value = AppSettings.Defaults();
            value.Engine = engineBox.SelectedIndex == 2 ? "OpenAI" : engineBox.SelectedIndex == 1 ? "Groq" : "Local";
            value.CleanupProvider = cleanupProviderBox.SelectedIndex == 1 ? "OpenRouter" : cleanupProviderBox.SelectedIndex == 2 ? "OpenAI" : cleanupProviderBox.SelectedIndex == 3 ? "Ollama" : "BuiltIn";
            value.Hotkey = Convert.ToString(hotkeyBox.SelectedItem);
            value.HandsFreeDoubleTap = handsFreeBox.Checked;
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
            value.AutoCheckUpdates = autoUpdateBox.Checked;
            value.OverlayTheme = OverlayThemeFromIndex(overlayThemeBox.SelectedIndex);
            value.OverlayMark = OverlayMarkFromIndex(overlayMarkBox.SelectedIndex);
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
            value.AgentModeEnabled = agentEnabledBox.Checked;
            value.AgentHotkey = Convert.ToString(agentHotkeyBox.SelectedItem);
            value.AgentEndpoint = agentEndpointBox.Text.Trim();
            value.SpokenListsEnabled = spokenListsBox.Checked;
            value.SpokenBulletPhrase = spokenBulletBox.Text.Trim();
            value.SpokenNumberPhrase = spokenNumberBox.Text.Trim();
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
            if (!AgentBridge.IsLoopbackEndpoint(agentEndpointBox.Text.Trim()) && agentEndpointBox.Text.Trim().Length > 0)
            {
                MessageBox.Show(this, "The agent endpoint must be on this PC (127.0.0.1 or localhost). Flowtype will not send spoken asks off the machine.", "Flowtype", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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

        private void UseLocalStreamingModel(string model)
        {
            if (!String.IsNullOrWhiteSpace(model)) ollamaModelBox.Text = model.Trim();
            cleanupBox.Checked = true;
            if (cleanupProviderBox.Items.Count > 3) cleanupProviderBox.SelectedIndex = 3;
            try
            {
                AppSettings value = ReadValues();
                store.Save(value);
                store.SaveApiKey(apiKeyBox.Text.Trim());
                store.SaveOpenRouterKey(openRouterKeyBox.Text.Trim());
                store.SaveGroqKey(groqKeyBox.Text.Trim());
                Action<AppSettings, string, string> handler = SettingsSaved;
                if (handler != null) handler(value, apiKeyBox.Text.Trim(), openRouterKeyBox.Text.Trim());
            }
            catch { }
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
            micTestRawPeak = 0f;
            micTestBoostedPeak = 0f;
            micLevelBar.Value = 0;
            micTestStatus.Text = String.Format(CultureInfo.InvariantCulture,
                "Listening at {0:0.0}× boost… speak normally.", micGainBar.Value / 10f);
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

        private void OnMicTestLevel(WaveRecorder.AudioMeterReading reading)
        {
            if (IsDisposed || reading == null) return;
            if (InvokeRequired)
            {
                try { BeginInvoke(new Action<WaveRecorder.AudioMeterReading>(OnMicTestLevel), reading); } catch { }
                return;
            }
            micTestRawPeak = Math.Max(micTestRawPeak, reading.RawPeak);
            micTestBoostedPeak = Math.Max(micTestBoostedPeak, reading.BoostedPeak);
            int voicePercent = (int)Math.Round(Math.Max(0f, Math.Min(1f, reading.RawPeak)) * 100f);
            micLevelBar.Value = Math.Max(micLevelBar.Minimum, Math.Min(micLevelBar.Maximum, voicePercent));
            micTestStatus.Text = String.Format(CultureInfo.InvariantCulture,
                "{0:0.0}× boost · voice at mic {1}% (target 15–40%)",
                micGainBar.Value / 10f,
                (int)Math.Round(micTestRawPeak * 100f));
        }

        private void FinishMicTest()
        {
            micTestRecorder.LevelChanged -= OnMicTestLevel;
            float gain = micGainBar.Value / 10f;
            try
            {
                if (micTestRecorder.IsRecording) micTestRecorder.Stop();
            }
            catch { }
            try
            {
                if (!String.IsNullOrWhiteSpace(micTestPath) && File.Exists(micTestPath))
                    File.Delete(micTestPath);
            }
            catch { }
            micTestPath = "";
            micTestButton.Text = "Test 3s";
            micTestButton.Enabled = true;
            MicLevel advice = MicLevel.Evaluate(micTestRawPeak, micTestBoostedPeak, gain);
            micLevelBar.Value = Math.Max(micLevelBar.Minimum, Math.Min(micLevelBar.Maximum, advice.VoicePercent));
            micTestStatus.Text = String.Format(CultureInfo.InvariantCulture,
                "{0:0.0}× boost · voice at mic {1}% (target 15–40%). {2}",
                gain, advice.VoicePercent, advice.Message);
        }

        private static float MeasurePeakPercent(byte[] pcm)
        {
            return MeasurePeak(pcm) * 100f;
        }

        private static float MeasurePeak(byte[] pcm)
        {
            if (pcm == null || pcm.Length < 2) return 0f;
            float peak = 0;
            for (int index = 0; index + 1 < pcm.Length; index += 2)
            {
                short sample = (short)(pcm[index] | (pcm[index + 1] << 8));
                peak = Math.Max(peak, Math.Abs(sample / 32768f));
            }
            return peak;
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
            if (String.Equals(overlayTheme, "Ember", StringComparison.OrdinalIgnoreCase)) return 3;
            if (String.Equals(overlayTheme, "Glass", StringComparison.OrdinalIgnoreCase)) return 4;
            return 0;
        }

        private static string OverlayThemeFromIndex(int index)
        {
            if (index == 1) return "Purple";
            if (index == 2) return "Light";
            if (index == 3) return "Ember";
            if (index == 4) return "Glass";
            return "Dark";
        }

        private static int OverlayMarkToIndex(string overlayMark)
        {
            if (String.Equals(overlayMark, "Hex", StringComparison.OrdinalIgnoreCase)) return 1;
            if (String.Equals(overlayMark, "Iris", StringComparison.OrdinalIgnoreCase)) return 2;
            if (String.Equals(overlayMark, "Grid", StringComparison.OrdinalIgnoreCase)) return 3;
            return 0;
        }

        private static string OverlayMarkFromIndex(int index)
        {
            if (index == 1) return "Hex";
            if (index == 2) return "Iris";
            if (index == 3) return "Grid";
            return "Orb";
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

    // The agent HUD. Deliberately nothing like the dictation capsule: a terminal panel
    // in the corner, monospace, with a run id and a live spinner. You should never have
    // to wonder which chord you are holding.
    public sealed class AgentOverlay : Form
    {
        public enum Stage { Listening, Working, Done, Failed }

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

        private static readonly string[] SpinnerFrames = { "|", "/", "-", "\\" };
        private static readonly Color Shell = Color.FromArgb(250, 13, 17, 23);
        private static readonly Color ShellEdge = Color.FromArgb(255, 34, 44, 58);
        private static readonly Color Accent = Color.FromArgb(255, 88, 214, 141);
        private static readonly Color AccentDim = Color.FromArgb(255, 46, 122, 82);
        private static readonly Color Amber = Color.FromArgb(255, 233, 179, 74);
        private static readonly Color Danger = Color.FromArgb(255, 232, 105, 96);
        private static readonly Color Ink = Color.FromArgb(255, 226, 232, 240);
        private static readonly Color InkDim = Color.FromArgb(255, 122, 134, 154);

        private readonly System.Windows.Forms.Timer timer;
        private readonly Stopwatch clock = new Stopwatch();
        private readonly float[] bars = new float[24];
        private readonly Font mono;
        private readonly Font monoSmall;
        private readonly Font monoBold;
        private Stage stage = Stage.Listening;
        private string runId = "0000";
        private string chordLabel = "Win + Alt";
        private string message = "";
        private string askLine = "";
        private string[] replyLines = new string[0];
        private string endpointLabel = "";
        private int tick;
        private float level;
        private long finishedMs;
        private bool isQuestion;
        private bool isNotice;
        private System.Windows.Forms.Timer autoHide;

        private const int CompactWidth = 470;
        private const int CompactHeight = 104;
        private const int ReadoutWidth = 560;
        private const int ReplyLineHeight = 19;
        private const int MaxReplyLines = 14;

        public AgentOverlay()
        {
            FormBorderStyle = FormBorderStyle.None;
            ShowInTaskbar = false;
            TopMost = true;
            Width = CompactWidth;
            Height = CompactHeight;
            mono = MonoFont(10.5f, FontStyle.Regular);
            monoSmall = MonoFont(8.25f, FontStyle.Regular);
            monoBold = MonoFont(10.5f, FontStyle.Bold);
            timer = new System.Windows.Forms.Timer();
            timer.Interval = 60;
            timer.Tick += delegate
            {
                tick++;
                // A readout you are still reading must not vanish. Hovering the panel
                // restarts the dismiss countdown, so the answer waits as long as you do.
                if (autoHide != null && (stage == Stage.Done || stage == Stage.Failed) && CursorInside())
                {
                    autoHide.Stop();
                    autoHide.Start();
                }
                if (stage == Stage.Listening)
                {
                    for (int index = bars.Length - 1; index > 0; index--) bars[index] = bars[index - 1];
                    bars[0] = level;
                    level *= 0.86f;
                }
                Render();
            };
        }

        private static Font MonoFont(float size, FontStyle style)
        {
            string[] candidates = { "Cascadia Mono", "Cascadia Code", "Consolas", "Lucida Console" };
            foreach (string name in candidates)
            {
                try
                {
                    Font candidate = new Font(name, size, style, GraphicsUnit.Point);
                    if (String.Equals(candidate.Name, name, StringComparison.OrdinalIgnoreCase)) return candidate;
                    candidate.Dispose();
                }
                catch { }
            }
            return new Font(FontFamily.GenericMonospace, size, style, GraphicsUnit.Point);
        }

        protected override bool ShowWithoutActivation { get { return true; } }

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams value = base.CreateParams;
                value.ExStyle |= 0x08000000 | 0x00000080 | 0x00000020 | 0x00080000;
                return value;
            }
        }

        public void SetLevel(float value)
        {
            if (stage != Stage.Listening) return;
            level = Math.Max(level, Math.Max(0f, Math.Min(1f, value)));
        }

        public void ShowListening(string chord, string endpoint)
        {
            CancelAutoHide();
            chordLabel = String.IsNullOrWhiteSpace(chord) ? "Win + Alt" : chord;
            endpointLabel = ShortEndpoint(endpoint);
            runId = DateTime.Now.ToString("HHmmss", CultureInfo.InvariantCulture);
            message = "";
            askLine = "";
            replyLines = new string[0];
            stage = Stage.Listening;
            Array.Clear(bars, 0, bars.Length);
            level = 0f;
            clock.Restart();
            timer.Start();
            SetPanelSize(CompactWidth, CompactHeight);
            Position();
            if (!Visible) Show();
            Render();
        }

        public void ShowWorking(string ask)
        {
            CancelAutoHide();
            stage = Stage.Working;
            message = (ask ?? "").Trim();
            if (message.Length > 0) askLine = message;
            clock.Restart();
            timer.Start();
            SetPanelSize(CompactWidth, CompactHeight);
            Position();
            if (!Visible) Show();
            Render();
        }

        public void ShowDone(string reply, long ms)
        {
            ShowDone(reply, ms, false);
        }

        // A question coming back is the bridge working, not failing: the agent needs one
        // thing before it acts, so the panel waits in amber and tells you to just answer.
        public void ShowDone(string reply, long ms, bool question)
        {
            finishedMs = ms;
            isQuestion = question;
            isNotice = false;
            Finish(Stage.Done, String.IsNullOrWhiteSpace(reply) ? "done" : reply.Trim());
        }

        public void ShowFailed(string error)
        {
            finishedMs = 0;
            isQuestion = false;
            isNotice = false;
            Finish(Stage.Failed, String.IsNullOrWhiteSpace(error) ? "no listener" : error.Trim());
        }

        // Something you asked it to watch for, arriving on its own. The ask that armed it
        // was minutes or hours ago, so the panel says what it was watching, not "done".
        public void ShowNotice(string text)
        {
            finishedMs = 0;
            isQuestion = false;
            isNotice = true;
            runId = DateTime.Now.ToString("HHmmss", CultureInfo.InvariantCulture);
            askLine = "watching for this earlier";
            Finish(Stage.Done, String.IsNullOrWhiteSpace(text) ? "done" : text.Trim());
        }

        // The readout grows to fit the actual answer and stays up long enough to read it.
        // A one-line panel that flashes for four seconds is the same as no answer at all.
        private void Finish(Stage which, string text)
        {
            CancelAutoHide();
            stage = which;
            message = text;
            replyLines = WrapReply(text, ReadoutWidth - 76f);
            clock.Reset();
            timer.Start();
            SetPanelSize(ReadoutWidth, 88 + replyLines.Length * ReplyLineHeight);
            Position();
            if (!Visible) Show();
            Render();
            // A question is waiting on you, so it waits properly — the reading-time
            // formula is for answers, not for prompts.
            ScheduleHide(isQuestion || isNotice ? 45000 : DwellFor(text));
        }

        // Reading time, not a fixed timeout: ~42ms a character, floor four seconds,
        // ceiling forty-five, and hovering holds it open past any of that.
        private static int DwellFor(string text)
        {
            int length = (text ?? "").Length;
            int dwell = 2200 + length * 42;
            if (dwell < 4200) dwell = 4200;
            if (dwell > 45000) dwell = 45000;
            return dwell;
        }

        private void SetPanelSize(int width, int height)
        {
            if (Width == width && Height == height) return;
            Width = width;
            Height = height;
        }

        private bool CursorInside()
        {
            if (!Visible) return false;
            try { return Bounds.Contains(Cursor.Position); }
            catch { return false; }
        }

        private string[] WrapReply(string text, float maxWidth)
        {
            List<string> lines = new List<string>();
            string body = (text ?? "").Replace("\r\n", "\n").Replace("\r", "\n");
            using (Bitmap scratch = new Bitmap(1, 1))
            using (Graphics probe = Graphics.FromImage(scratch))
            {
                foreach (string paragraph in body.Split('\n'))
                {
                    string trimmed = paragraph.Trim();
                    if (trimmed.Length == 0)
                    {
                        if (lines.Count > 0 && lines.Count < MaxReplyLines) lines.Add("");
                        continue;
                    }
                    string current = "";
                    foreach (string word in trimmed.Split(' '))
                    {
                        if (word.Length == 0) continue;
                        string candidate = current.Length == 0 ? word : current + " " + word;
                        if (probe.MeasureString(candidate, mono).Width <= maxWidth) { current = candidate; continue; }
                        if (current.Length > 0) { lines.Add(current); current = word; }
                        else { lines.Add(HardCut(probe, word, maxWidth, lines)); current = ""; }
                        if (lines.Count >= MaxReplyLines) break;
                    }
                    if (current.Length > 0 && lines.Count < MaxReplyLines) lines.Add(current);
                    if (lines.Count >= MaxReplyLines) break;
                }
            }
            if (lines.Count == 0) lines.Add(body.Trim());
            if (lines.Count >= MaxReplyLines) lines[MaxReplyLines - 1] = lines[MaxReplyLines - 1] + " …";
            return lines.ToArray();
        }

        // A single unbroken token (a path, a URL) still has to land on the panel.
        private string HardCut(Graphics probe, string word, float maxWidth, List<string> sink)
        {
            string head = word;
            while (head.Length > 1 && probe.MeasureString(head, mono).Width > maxWidth)
                head = head.Substring(0, head.Length - 1);
            string rest = word.Substring(head.Length);
            if (rest.Length > 0 && sink.Count + 1 < MaxReplyLines) sink.Add(head);
            else return head;
            return HardCut(probe, rest, maxWidth, sink);
        }

        public void HideNow()
        {
            CancelAutoHide();
            timer.Stop();
            clock.Reset();
            if (Visible) Hide();
        }

        private void ScheduleHide(int delay)
        {
            autoHide = new System.Windows.Forms.Timer();
            autoHide.Interval = delay;
            autoHide.Tick += delegate { HideNow(); };
            autoHide.Start();
        }

        private void CancelAutoHide()
        {
            if (autoHide == null) return;
            autoHide.Stop();
            autoHide.Dispose();
            autoHide = null;
        }

        private static string ShortEndpoint(string endpoint)
        {
            if (String.IsNullOrWhiteSpace(endpoint)) return "";
            try
            {
                Uri uri = new Uri(endpoint);
                return uri.Host + ":" + uri.Port;
            }
            catch { return endpoint; }
        }

        private void Position()
        {
            Rectangle area = Screen.FromPoint(Cursor.Position).WorkingArea;
            Location = new Point(area.Right - Width - 18, area.Bottom - Height - 18);
        }

        private static GraphicsPath Rounded(RectangleF bounds, float radius)
        {
            GraphicsPath path = new GraphicsPath();
            float diameter = radius * 2f;
            path.AddArc(bounds.Left, bounds.Top, diameter, diameter, 180, 90);
            path.AddArc(bounds.Right - diameter, bounds.Top, diameter, diameter, 270, 90);
            path.AddArc(bounds.Right - diameter, bounds.Bottom - diameter, diameter, diameter, 0, 90);
            path.AddArc(bounds.Left, bounds.Bottom - diameter, diameter, diameter, 90, 90);
            path.CloseFigure();
            return path;
        }

        private Color StageColor()
        {
            if (stage == Stage.Done && isQuestion) return Amber;
            if (stage == Stage.Done) return Accent;
            if (stage == Stage.Failed) return Danger;
            if (stage == Stage.Working) return Amber;
            return Accent;
        }

        private void Render()
        {
            if (!Visible || IsDisposed) return;
            using (Bitmap surface = new Bitmap(Width, Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb))
            {
                using (Graphics canvas = Graphics.FromImage(surface))
                {
                    canvas.SmoothingMode = SmoothingMode.AntiAlias;
                    canvas.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
                    canvas.Clear(Color.Transparent);
                    PaintPanel(canvas);
                }
                PushLayered(surface);
            }
        }

        private void PaintPanel(Graphics canvas)
        {
            RectangleF shell = new RectangleF(1f, 1f, Width - 2f, Height - 2f);
            Color edge = StageColor();
            using (GraphicsPath body = Rounded(shell, 9f))
            using (SolidBrush fill = new SolidBrush(Shell))
            using (Pen border = new Pen(Color.FromArgb(150, edge), 1.4f))
            {
                canvas.FillPath(fill, body);
                canvas.DrawPath(border, body);
            }
            using (GraphicsPath inner = Rounded(new RectangleF(shell.Left + 1.6f, shell.Top + 1.6f, shell.Width - 3.2f, shell.Height - 3.2f), 8f))
            using (Pen hairline = new Pen(ShellEdge, 1f))
                canvas.DrawPath(hairline, inner);

            // Title strip: what this is, which run, where it is going.
            float titleY = shell.Top + 9f;
            using (SolidBrush dim = new SolidBrush(InkDim))
            {
                canvas.DrawString("flowtype", monoSmall, dim, shell.Left + 14f, titleY);
                SizeF head = canvas.MeasureString("flowtype", monoSmall);
                using (SolidBrush hot = new SolidBrush(edge))
                    canvas.DrawString("::agent", monoSmall, hot, shell.Left + 14f + head.Width - 4f, titleY);
                string right = "run " + runId + (endpointLabel.Length > 0 ? "  " + endpointLabel : "");
                SizeF rightSize = canvas.MeasureString(right, monoSmall);
                canvas.DrawString(right, monoSmall, dim, shell.Right - 14f - rightSize.Width, titleY);
            }
            using (Pen rule = new Pen(ShellEdge, 1f))
                canvas.DrawLine(rule, shell.Left + 13f, titleY + 17f, shell.Right - 13f, titleY + 17f);

            float lineY = titleY + 26f;
            float promptX = shell.Left + 14f;
            using (SolidBrush hot = new SolidBrush(edge))
                canvas.DrawString(">", monoBold, hot, promptX, lineY);
            float textX = promptX + 16f;

            if (stage == Stage.Listening) PaintListening(canvas, textX, lineY, shell);
            else if (stage == Stage.Working) PaintWorking(canvas, textX, lineY, shell);
            else PaintFinished(canvas, textX, lineY, shell, edge);
        }

        private void PaintListening(Graphics canvas, float textX, float lineY, RectangleF shell)
        {
            using (SolidBrush ink = new SolidBrush(Ink))
                canvas.DrawString("listening", mono, ink, textX, lineY);
            // Live level trace — the agent panel's own signal, not the capsule's orb.
            float baseX = textX + 96f;
            float mid = lineY + 11f;
            for (int index = 0; index < bars.Length; index++)
            {
                float value = Math.Max(0.05f, Math.Min(1f, bars[index]));
                float height = 3f + value * 17f;
                float x = baseX + index * 6.5f;
                if (x > shell.Right - 22f) break;
                int alpha = (int)(70 + 140 * value);
                using (SolidBrush brush = new SolidBrush(Color.FromArgb(alpha, AccentDim.R + 40, AccentDim.G + 60, AccentDim.B + 40)))
                    canvas.FillRectangle(brush, x, mid - height / 2f, 3.2f, height);
            }
            using (SolidBrush dim = new SolidBrush(InkDim))
                canvas.DrawString("release " + chordLabel + " to send", monoSmall, dim, textX, lineY + 26f);
            using (SolidBrush rec = new SolidBrush(tick % 16 < 9 ? Danger : Color.FromArgb(90, Danger)))
                canvas.FillEllipse(rec, shell.Right - 26f, lineY + 6f, 9f, 9f);
        }

        private void PaintWorking(Graphics canvas, float textX, float lineY, RectangleF shell)
        {
            string frame = SpinnerFrames[(tick / 2) % SpinnerFrames.Length];
            using (SolidBrush hot = new SolidBrush(Amber))
                canvas.DrawString(frame, monoBold, hot, textX, lineY);
            using (SolidBrush ink = new SolidBrush(Ink))
                canvas.DrawString(Ellipsize(canvas, message, mono, shell.Width - 130f), mono, ink, textX + 18f, lineY);
            string elapsed = (clock.ElapsedMilliseconds / 1000.0).ToString("0.0", CultureInfo.InvariantCulture) + "s";
            using (SolidBrush dim = new SolidBrush(InkDim))
            {
                SizeF size = canvas.MeasureString(elapsed, monoSmall);
                canvas.DrawString(elapsed, monoSmall, dim, shell.Right - 16f - size.Width, lineY + 3f);
                canvas.DrawString("agent working", monoSmall, dim, textX, lineY + 26f);
            }
            // Progress shuttle: motion without a fake percentage.
            float trackY = lineY + 44f;
            float trackLeft = textX;
            float trackWidth = shell.Right - 16f - trackLeft;
            using (SolidBrush track = new SolidBrush(Color.FromArgb(60, 120, 140, 160)))
                canvas.FillRectangle(track, trackLeft, trackY, trackWidth, 2f);
            float shuttle = (float)((Math.Sin(tick * 0.09) + 1.0) / 2.0) * (trackWidth - 58f);
            using (SolidBrush hot = new SolidBrush(Amber))
                canvas.FillRectangle(hot, trackLeft + shuttle, trackY, 58f, 2f);
        }

        private void PaintFinished(Graphics canvas, float textX, float lineY, RectangleF shell, Color edge)
        {
            // Echo of what it heard, so "what did you just do" is already answered.
            if (askLine.Length > 0)
                using (SolidBrush dim = new SolidBrush(InkDim))
                    canvas.DrawString(Ellipsize(canvas, askLine, mono, shell.Width - 60f), mono, dim, textX, lineY);

            float bodyY = askLine.Length > 0 ? lineY + 24f : lineY;
            string glyph = stage == Stage.Failed ? "!!" : (isQuestion ? "??" : (isNotice ? "->" : "OK"));
            using (SolidBrush hot = new SolidBrush(edge))
                canvas.DrawString(glyph, monoBold, hot, shell.Left + 14f, bodyY);
            using (SolidBrush ink = new SolidBrush(Ink))
                for (int index = 0; index < replyLines.Length; index++)
                    canvas.DrawString(replyLines[index], mono, ink, textX + 30f, bodyY + index * ReplyLineHeight);

            float tailY = bodyY + replyLines.Length * ReplyLineHeight + 5f;
            using (SolidBrush dim = new SolidBrush(InkDim))
            {
                string tail;
                if (stage == Stage.Failed) tail = "kept on clipboard";
                else if (isQuestion) tail = "hold " + chordLabel + " and answer";
                else if (isNotice) tail = "you asked me to watch for this";
                else tail = "done in " + (finishedMs / 1000.0).ToString("0.0", CultureInfo.InvariantCulture) + "s";
                canvas.DrawString(tail, monoSmall, dim, textX, tailY);
                string hint = CursorInside() ? "held — move away to dismiss" : "hover to hold  ·  logged";
                SizeF hintSize = canvas.MeasureString(hint, monoSmall);
                canvas.DrawString(hint, monoSmall, dim, shell.Right - 16f - hintSize.Width, tailY);
            }
        }

        private static string Ellipsize(Graphics canvas, string value, Font font, float maxWidth)
        {
            if (String.IsNullOrEmpty(value)) return "";
            string text = value.Replace("\r", " ").Replace("\n", " ").Trim();
            if (canvas.MeasureString(text, font).Width <= maxWidth) return text;
            while (text.Length > 4 && canvas.MeasureString(text + "...", font).Width > maxWidth)
                text = text.Substring(0, text.Length - 1);
            return text + "...";
        }

        private void PushLayered(Bitmap surface)
        {
            IntPtr screenDc = GetDC(IntPtr.Zero);
            IntPtr memoryDc = CreateCompatibleDC(screenDc);
            IntPtr bitmapHandle = IntPtr.Zero;
            IntPtr previous = IntPtr.Zero;
            try
            {
                bitmapHandle = surface.GetHbitmap(Color.FromArgb(0));
                previous = SelectObject(memoryDc, bitmapHandle);
                NativeSize size = new NativeSize(surface.Width, surface.Height);
                NativePoint source = new NativePoint(0, 0);
                NativePoint destination = new NativePoint(Left, Top);
                BlendFunction blend = new BlendFunction();
                blend.BlendOp = 0;
                blend.BlendFlags = 0;
                blend.SourceConstantAlpha = 255;
                blend.AlphaFormat = SourceAlphaFormat;
                UpdateLayeredWindow(Handle, screenDc, ref destination, ref size, memoryDc, ref source, 0, ref blend, LayeredAlphaFlag);
            }
            catch { }
            finally
            {
                if (previous != IntPtr.Zero) SelectObject(memoryDc, previous);
                if (bitmapHandle != IntPtr.Zero) DeleteObject(bitmapHandle);
                DeleteDC(memoryDc);
                ReleaseDC(IntPtr.Zero, screenDc);
            }
        }

        private const byte SourceAlphaFormat = 0x01;
        private const int LayeredAlphaFlag = 0x00000002;
    }

    // Tray art for agent mode: the same silhouette people already know, wearing a
    // terminal prompt so a glance at the tray says which mode is armed.
    public static class AgentTrayIcon
    {
        private static Icon cached;

        public static Icon Get()
        {
            if (cached != null) return cached;
            try
            {
                using (Bitmap bitmap = new Bitmap(32, 32, System.Drawing.Imaging.PixelFormat.Format32bppArgb))
                {
                    using (Graphics canvas = Graphics.FromImage(bitmap))
                    {
                        canvas.SmoothingMode = SmoothingMode.AntiAlias;
                        canvas.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;
                        canvas.Clear(Color.Transparent);
                        using (GraphicsPath path = new GraphicsPath())
                        {
                            path.AddArc(1, 1, 10, 10, 180, 90);
                            path.AddArc(21, 1, 10, 10, 270, 90);
                            path.AddArc(21, 21, 10, 10, 0, 90);
                            path.AddArc(1, 21, 10, 10, 90, 90);
                            path.CloseFigure();
                            using (SolidBrush fill = new SolidBrush(Color.FromArgb(255, 13, 17, 23)))
                                canvas.FillPath(fill, path);
                            using (Pen border = new Pen(Color.FromArgb(255, 88, 214, 141), 2f))
                                canvas.DrawPath(border, path);
                        }
                        using (Pen stroke = new Pen(Color.FromArgb(255, 88, 214, 141), 2.4f))
                        {
                            stroke.StartCap = System.Drawing.Drawing2D.LineCap.Round;
                            stroke.EndCap = System.Drawing.Drawing2D.LineCap.Round;
                            canvas.DrawLines(stroke, new Point[] { new Point(9, 10), new Point(15, 16), new Point(9, 22) });
                            canvas.DrawLine(stroke, 17, 22, 24, 22);
                        }
                    }
                    cached = Icon.FromHandle(bitmap.GetHicon());
                }
            }
            catch { cached = null; }
            return cached;
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
        private readonly ToolStripMenuItem copyLastItem;
        private readonly ToolStripMenuItem undoLastItem;
        private readonly RecordingOverlay overlay;
        private readonly WaveRecorder recorder;
        private readonly WhisperEngine whisperEngine;
        private readonly GroqEngine groqEngine;
        private readonly GlobalKeyHook hook;
        private readonly System.Windows.Forms.Timer chordPoller;
        private readonly System.Windows.Forms.Timer activationPoller;
        private readonly EventWaitHandle activationEvent;
        private readonly Control dispatcher;
        private readonly OrderedInsertQueue insertQueue = new OrderedInsertQueue();
        private readonly object jobGate = new object();
        private AppSettings settings;
        private string apiKey;
        private string openRouterKey;
        private string groqKey;
        private int nextJobSequence;
        private int inflightJobs;
        private bool canUndoInsert;
        private ForegroundInfo lastInsertTarget;
        private ForegroundInfo target;
        private string recordingPath;
        private string lastDictationWord = "";
        private string lastDictationText = "";
        private int lastRememberedGeneration = -1;
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
        // Extra audio after key-up. DrainCallbacks now keeps the last in-flight buffers, so
        // this grace actually lands in the file instead of being eaten on stop. Too high is
        // lag plus Whisper "thank you" dead-air.
        private const int ReleaseGraceMs = 150;
        private bool latchedRecording;
        private bool awaitingDoubleTap;
        private bool handsFreeStopPending;
        private bool notifyRecordingLimit;
        private System.Windows.Forms.Timer doubleTapTimer;
        private const int DoubleTapWindowMs = 450;
        private const int ShortPressMs = 280;
        private SettingsForm settingsForm;
        private HistoryForm historyForm;
        private IntPtr lastForegroundWindow;
        private GlobalKeyHook agentHook;
        private bool agentHotkeyDown;
        private bool pendingAgentTake;
        private bool currentTakeIsAgent;
        private ForegroundInfo agentSeat;
        private volatile bool agentInFlight;
        private bool agentAborted;
        private System.Windows.Forms.Timer agentNoticeTimer;
        private ToolStripMenuItem agentToggleItem;
        private readonly AgentOverlay agentOverlay = new AgentOverlay();
        private readonly AppUpdater appUpdater = new AppUpdater();
        private bool updateCheckRunning;
        private bool updateInstallRunning;
        private System.Windows.Forms.Timer updateRecheckTimer;

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
            ForegroundContext.SetClipboardMarshal(dispatcher);
            overlay = new RecordingOverlay();
            overlay.SetTheme(settings.OverlayTheme);
            overlay.SetMark(settings.OverlayMark);
            recorder = new WaveRecorder();
            RecordingCue.Preload();
            recorder.MicGain = settings.MicGain;
            ThreadPool.QueueUserWorkItem(delegate
            {
                try { recorder.Prime(); }
                catch (Exception exception) { store.LogError(exception); }
            });
            whisperEngine = new WhisperEngine();
            groqEngine = new GroqEngine();
            hook = new GlobalKeyHook(settings.Hotkey);
            hook.HotkeyChanged += OnHotkeyChanged;
            // Never run cancel (WAV finalization, file IO) inside the low-level keyboard hook
            // callback — a slow callback gets the hook silently removed by Windows and the
            // hotkey dies until restart. Defer to the UI thread like the hotkey path does.
            hook.CancelPressed += delegate { try { dispatcher.BeginInvoke(new Action(CancelRecording)); } catch { } };
            if (settings.AgentModeEnabled) EnableAgentHook();
            AgentTrace.Log("startup: AgentModeEnabled=" + settings.AgentModeEnabled
                + " hook=" + (agentHook != null ? "armed(" + settings.AgentHotkey + ")" : "null"));
            recorder.LevelChanged += overlay.SetLevel;
            recorder.LevelChanged += delegate(WaveRecorder.AudioMeterReading reading)
            {
                if (currentTakeIsAgent && reading != null) agentOverlay.SetLevel(reading.Boosted);
            };
            overlay.MaximumDurationReached += OnMaximumDurationReached;
            chordPoller = new System.Windows.Forms.Timer();
            chordPoller.Interval = 20;
            chordPoller.Tick += delegate
            {
                if (!Hotkeys.IsModifierChord(settings.Hotkey)) return;
                bool down = NativeKeyState.IsHotkeyDown(settings.Hotkey);
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
                try { ReconcileAfterForegroundChange(); }
                catch { }
            };
            activationPoller.Start();

            ContextMenuStrip menu = new ContextMenuStrip();
            statusItem = new ToolStripMenuItem("Ready — hold " + settings.Hotkey);
            statusItem.Enabled = false;
            toggleItem = new ToolStripMenuItem("Start dictating");
            toggleItem.Click += delegate { ToggleRecording(); };
            agentToggleItem = new ToolStripMenuItem(AgentToggleLabel());
            agentToggleItem.Checked = settings.AgentModeEnabled;
            agentToggleItem.Image = AgentModeGlyph();
            agentToggleItem.Click += delegate { ToggleAgentMode(); };
            ToolStripMenuItem settingsItem = new ToolStripMenuItem("Settings…");
            settingsItem.Click += delegate { ShowSettings(); };
            ToolStripMenuItem historyItem = new ToolStripMenuItem("History…");
            historyItem.Click += delegate { ShowHistory(); };
            dictionaryFixItem = new ToolStripMenuItem("Add last word to dictionary…");
            dictionaryFixItem.Enabled = false;
            dictionaryFixItem.Click += delegate { AddLastWordToDictionary(); };
            copyLastItem = new ToolStripMenuItem("Copy last dictation");
            copyLastItem.Enabled = false;
            copyLastItem.Click += delegate { CopyLastDictation(); };
            undoLastItem = new ToolStripMenuItem("Undo last dictation");
            undoLastItem.Enabled = false;
            undoLastItem.Click += delegate { UndoLastDictation(lastInsertTarget); };
            ToolStripMenuItem agentLogItem = new ToolStripMenuItem("Open agent replies log");
            agentLogItem.Click += delegate { OpenAgentReplyLog(); };
            ToolStripMenuItem recoveryItem = new ToolStripMenuItem("Open recovery folder");
            recoveryItem.Click += delegate { OpenFolder(store.RecoveryPath); };
            ToolStripMenuItem updateItem = new ToolStripMenuItem("Check for updates…");
            updateItem.Click += delegate { CheckForUpdates(false); };
            ToolStripMenuItem quitItem = new ToolStripMenuItem("Quit Flowtype");
            quitItem.Click += delegate { Quit(); };
            menu.Items.Add(statusItem);
            menu.Items.Add(toggleItem);
            menu.Items.Add(agentToggleItem);
            menu.Items.Add(new ToolStripSeparator());
            menu.Items.Add(settingsItem);
            menu.Items.Add(historyItem);
            menu.Items.Add(copyLastItem);
            menu.Items.Add(undoLastItem);
            menu.Items.Add(dictionaryFixItem);
            menu.Items.Add(agentLogItem);
            menu.Items.Add(recoveryItem);
            menu.Items.Add(updateItem);
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
            ScheduleAutomaticUpdateCheck();
        }

        private void ScheduleAutomaticUpdateCheck()
        {
            if (!settings.AutoCheckUpdates) return;
            System.Windows.Forms.Timer updateTimer = new System.Windows.Forms.Timer();
            updateTimer.Interval = 5000;
            updateTimer.Tick += delegate
            {
                updateTimer.Stop();
                updateTimer.Dispose();
                CheckForUpdates(true);
            };
            updateTimer.Start();
            if (updateRecheckTimer != null) return;
            updateRecheckTimer = new System.Windows.Forms.Timer();
            updateRecheckTimer.Interval = 6 * 60 * 60 * 1000;
            updateRecheckTimer.Tick += delegate { CheckForUpdates(true); };
            updateRecheckTimer.Start();
        }

        private async void CheckForUpdates(bool silent)
        {
            if (updateCheckRunning || updateInstallRunning || shuttingDown) return;
            updateCheckRunning = true;
            try
            {
                AppUpdater.ReleaseInfo release = await appUpdater.FetchLatestAsync();
                settings.LastUpdateCheckUtc = DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture);
                try { store.Save(settings); } catch (Exception exception) { store.LogError(exception); }
                if (!FlowtypeVersion.IsNewerThanCurrent(release.TagName))
                {
                    if (!silent) Notify("You're up to date", "Flowtype " + FlowtypeVersion.CurrentLabel + " is the latest release.", ToolTipIcon.Info);
                    return;
                }
                if (!String.IsNullOrWhiteSpace(settings.SkippedUpdateVersion) &&
                    String.Equals(settings.SkippedUpdateVersion, release.TagName, StringComparison.OrdinalIgnoreCase)) return;
                if (silent)
                {
                    Notify("Updating Flowtype", "Installing " + release.TagName.TrimStart('v', 'V') + "…", ToolTipIcon.Info);
                    await InstallUpdateAsync(release);
                    return;
                }
                string message = "Flowtype " + release.TagName.TrimStart('v', 'V') + " is available (you have " + FlowtypeVersion.CurrentLabel + ").";
                if (!String.IsNullOrWhiteSpace(release.Body))
                {
                    string notes = ShortMessage(release.Body);
                    if (notes.Length > 0) message += "\n\n" + notes;
                }
                message += "\n\nUpdate now? Flowtype will restart.";
                DialogResult choice = MessageBox.Show(message, "Update available", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Information);
                if (choice == DialogResult.Cancel)
                {
                    settings.SkippedUpdateVersion = release.TagName;
                    try { store.Save(settings); } catch (Exception exception) { store.LogError(exception); }
                    return;
                }
                if (choice != DialogResult.Yes) return;
                await InstallUpdateAsync(release);
            }
            catch (Exception exception)
            {
                store.LogError(exception);
                if (!silent) Notify("Update check failed", exception.Message, ToolTipIcon.Warning);
            }
            finally
            {
                updateCheckRunning = false;
            }
        }

        private async Task InstallUpdateAsync(AppUpdater.ReleaseInfo release)
        {
            if (updateInstallRunning) return;
            updateInstallRunning = true;
            statusItem.Text = "Downloading update…";
            try
            {
                await appUpdater.DownloadAndInstallAsync(release, delegate(int percent, string message)
                {
                    try
                    {
                        dispatcher.BeginInvoke(new Action(delegate
                        {
                            statusItem.Text = message + "  " + percent.ToString(CultureInfo.InvariantCulture) + "%";
                        }));
                    }
                    catch { }
                });
                Notify("Installing update", "Flowtype will restart in a moment.", ToolTipIcon.Info);
                Quit();
            }
            catch (Exception exception)
            {
                store.LogError(exception);
                Notify("Update failed", exception.Message, ToolTipIcon.Error);
                SetReady();
            }
            finally
            {
                updateInstallRunning = false;
            }
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

        private void OnMaximumDurationReached()
        {
            notifyRecordingLimit = true;
            StopRecording();
        }

        private void ReconcileAfterForegroundChange()
        {
            IntPtr foreground = ForegroundContext.Capture(false).Handle;
            if (foreground == lastForegroundWindow) return;
            lastForegroundWindow = foreground;
            if (recorder.IsRecording || processing) return;

            bool physicalDown = Hotkeys.IsModifierChord(settings.Hotkey) && NativeKeyState.IsHotkeyDown(settings.Hotkey);
            if (hotkeyDown && !physicalDown)
            {
                hotkeyDown = false;
                CancelPendingStop();
                CancelDoubleTapTimer();
                ResetRecordingMode();
            }
            chordReleaseStreak = 0;
            chordPolledDown = physicalDown;
            hook.ResetChordTracker();
            if (agentHook != null)
            {
                bool agentPhysicalDown = Hotkeys.IsModifierChord(settings.AgentHotkey)
                    && NativeKeyState.IsHotkeyDown(settings.AgentHotkey);
                if (agentHotkeyDown && !agentPhysicalDown) agentHotkeyDown = false;
                agentHook.ResetChordTracker();
            }
        }

        private void OnHotkeyChanged(bool down)
        {
            if (down)
            {
                CancelDoubleTapTimer();

                if (recorder.IsRecording && latchedRecording)
                {
                    latchedRecording = false;
                    awaitingDoubleTap = false;
                    handsFreeStopPending = true;
                    hotkeyDown = true;
                    CancelPendingStop();
                    try { dispatcher.BeginInvoke(new Action(StopRecording)); } catch { }
                    return;
                }

                if (awaitingDoubleTap && recorder.IsRecording)
                {
                    awaitingDoubleTap = false;
                    latchedRecording = true;
                    hotkeyDown = true;
                    CancelPendingStop();
                    UpdateRecordingStatus();
                    return;
                }

                if (hotkeyDown) return;
                hotkeyDown = true;
                hotkeyDownSince = DateTime.UtcNow;
                pendingAgentTake = false;
                CancelPendingStop();
                CancelPendingStart();
                try { dispatcher.BeginInvoke(new Action(StartRecording)); } catch { }
            }
            else
            {
                if (!hotkeyDown) return;
                hotkeyDown = false;
                lastHotkeyRelease = DateTime.UtcNow;
                CancelPendingStart();

                if (handsFreeStopPending)
                {
                    handsFreeStopPending = false;
                    return;
                }

                if (latchedRecording) return;

                long pressMs = (long)(DateTime.UtcNow - hotkeyDownSince).TotalMilliseconds;
                if (settings.HandsFreeDoubleTap && pressMs <= ShortPressMs && recorder.IsRecording)
                {
                    awaitingDoubleTap = true;
                    ScheduleDoubleTapTimeout();
                    return;
                }

                ScheduleStopRecording();
            }
        }

        // Agent chord: strict hold-to-talk. No hands-free latch, no double tap — a take
        // that reaches an agent should always be a deliberate, bounded hold.
        private void OnAgentHotkeyChanged(bool down)
        {
            if (down)
            {
                if (agentHotkeyDown) return;
                agentHotkeyDown = true;
                // "No — stop, don't send it." While an ask is in the air the same chord is
                // the brake, not the trigger. Nothing else in reach is fast enough.
                if (agentInFlight)
                {
                    AgentTrace.Log("chord down while in flight -> abort");
                    AbortAgentAsk();
                    return;
                }
                if (recorder.IsRecording || hotkeyDown)
                {
                    AgentTrace.Log("chord down IGNORED: recording=" + recorder.IsRecording + " mainDown=" + hotkeyDown);
                    return;
                }
                pendingAgentTake = true;
                AgentTrace.Log("chord down: queuing agent StartRecording");
                CancelPendingStop();
                CancelPendingStart();
                try { dispatcher.BeginInvoke(new Action(StartRecording)); } catch { }
            }
            else
            {
                if (!agentHotkeyDown) return;
                agentHotkeyDown = false;
                AgentTrace.Log("chord up: currentTakeIsAgent=" + currentTakeIsAgent);
                if (!currentTakeIsAgent)
                {
                    pendingAgentTake = false;
                    return;
                }
                ScheduleStopRecording();
            }
        }

        private void EnableAgentHook()
        {
            if (agentHook != null) return;
            try
            {
                agentHook = new GlobalKeyHook(settings.AgentHotkey);
                agentHook.HotkeyChanged += OnAgentHotkeyChanged;
                StartAgentNoticePoll();
            }
            catch (Exception exception)
            {
                store.LogError(exception);
                agentHook = null;
            }
        }

        private void DisableAgentHook()
        {
            if (agentHook == null) return;
            try { agentHook.Dispose(); } catch { }
            agentHook = null;
            agentHotkeyDown = false;
            pendingAgentTake = false;
            StopAgentNoticePoll();
        }

        // A deferred notice can land long after the ask that armed it, so the only way to
        // see it is to keep asking. Four seconds against a loopback port costs nothing.
        private void StartAgentNoticePoll()
        {
            if (agentNoticeTimer != null) return;
            agentNoticeTimer = new System.Windows.Forms.Timer();
            agentNoticeTimer.Interval = 4000;
            agentNoticeTimer.Tick += delegate { PollAgentNotices(); };
            agentNoticeTimer.Start();
        }

        private void StopAgentNoticePoll()
        {
            if (agentNoticeTimer == null) return;
            agentNoticeTimer.Stop();
            agentNoticeTimer.Dispose();
            agentNoticeTimer = null;
        }

        private string AgentToggleLabel()
        {
            return "Agent chord — hold " + settings.AgentHotkey;
        }

        private static Image AgentModeGlyph()
        {
            try
            {
                Icon icon = AgentTrayIcon.Get();
                return icon == null ? null : icon.ToBitmap();
            }
            catch { return null; }
        }

        // The tray icon states which mode is live: product mark for dictation, terminal
        // mark while an agent take is in the air.
        private void SetTrayAgentState(bool agentActive)
        {
            if (tray == null) return;
            try
            {
                if (agentActive)
                {
                    Icon agentIcon = AgentTrayIcon.Get();
                    if (agentIcon != null) tray.Icon = agentIcon;
                    tray.Text = "Flowtype — agent working…";
                }
                else
                {
                    tray.Icon = FlowtypeApp.ProductIcon ?? SystemIcons.Application;
                    tray.Text = "Flowtype — hold " + settings.Hotkey + " to dictate";
                }
            }
            catch { }
        }

        private void ToggleAgentMode()
        {
            settings.AgentModeEnabled = !settings.AgentModeEnabled;
            if (settings.AgentModeEnabled) EnableAgentHook();
            else DisableAgentHook();
            if (agentToggleItem != null)
            {
                agentToggleItem.Checked = settings.AgentModeEnabled && agentHook != null;
                agentToggleItem.Text = AgentToggleLabel();
            }
            if (settings.AgentModeEnabled && agentHook == null)
            {
                settings.AgentModeEnabled = false;
                Notify("Agent chord unavailable", "Could not install the agent hotkey hook.", ToolTipIcon.Warning);
            }
            else if (settings.AgentModeEnabled)
            {
                Notify("Agent chord on", "Hold " + settings.AgentHotkey + " and speak to send the take to " + settings.AgentEndpoint + ".", ToolTipIcon.Info);
            }
            try { store.Save(settings); }
            catch (Exception exception) { store.LogError(exception); }
        }

        private void SyncAgentHook()
        {
            if (agentToggleItem != null) agentToggleItem.Text = AgentToggleLabel();
            if (agentHook == null) return;
            if (!String.Equals(agentHook.HotkeyName, settings.AgentHotkey, StringComparison.OrdinalIgnoreCase))
                agentHook.HotkeyName = settings.AgentHotkey;
        }

        private void SendToAgent(string ask)
        {
            string endpoint = settings.AgentEndpoint;
            string preview = ask.Length > 90 ? ask.Substring(0, 90) + "..." : ask;
            ForegroundInfo seat = agentSeat;
            agentInFlight = true;
            try { dispatcher.BeginInvoke(new Action(delegate { agentOverlay.ShowWorking(preview); SetTrayAgentState(true); })); }
            catch { }
            AgentBridge.Send(ask, endpoint, seat,
                delegate(AgentBridge.Result result)
                {
                    agentInFlight = false;
                    AgentTrace.Log("agent replied in " + result.Ms + "ms"
                        + (result.IsQuestion ? " (question)" : "")
                        + (result.IsPaste ? " (paste)" : "") + ": " + result.Reply);
                    AgentReplyLog.Append(ask, result.Reply, result.Ms, true);
                    try
                    {
                        dispatcher.BeginInvoke(new Action(delegate
                        {
                            if (agentAborted) { agentAborted = false; return; }
                            if (!String.IsNullOrWhiteSpace(result.Focus))
                            {
                                bool raised = ForegroundContext.RaiseWindow(result.Focus);
                                AgentTrace.Log("focus '" + result.Focus + "' -> " + (raised ? "raised" : "not found"));
                            }
                            if (result.IsPaste) TypeAgentReply(result, seat);
                            else agentOverlay.ShowDone(result.Reply, result.Ms, result.IsQuestion);
                            SetTrayAgentState(false);
                            if (settings.CompletionSound) RecordingCue.PlayComplete();
                        }));
                    }
                    catch { }
                },
                delegate(string error)
                {
                    agentInFlight = false;
                    AgentTrace.Log("POST FAILED: " + error);
                    AgentReplyLog.Append(ask, error, 0, false);
                    try
                    {
                        dispatcher.BeginInvoke(new Action(delegate
                        {
                            if (agentAborted) { agentAborted = false; return; }
                            try { Clipboard.SetText(ask); } catch { }
                            agentOverlay.ShowFailed(error);
                            SetTrayAgentState(false);
                        }));
                    }
                    catch { }
                });
        }

        // "Type this in the box I've got open." The composer and the paster have always
        // been on the same key; this is the wire between them. Delivery reuses the
        // dictation path, so the seat lock applies — it will not paste into a thief window.
        private void TypeAgentReply(AgentBridge.Result result, ForegroundInfo seat)
        {
            string text = (result.Reply ?? "").Trim();
            if (text.Length == 0) { agentOverlay.ShowDone("nothing to type", result.Ms); return; }
            bool typed = ForegroundContext.DeliverDictation(text, seat, true);
            string where = seat != null ? seat.AppLabel : "the last window";
            if (typed) agentOverlay.ShowDone("typed into " + where + " — " + Preview(text), result.Ms);
            else agentOverlay.ShowFailed("could not reach " + where + " — on your clipboard");
        }

        private static string Preview(string text)
        {
            string flat = text.Replace("\r", " ").Replace("\n", " ").Trim();
            return flat.Length > 120 ? flat.Substring(0, 120) + "…" : flat;
        }

        private void AbortAgentAsk()
        {
            agentAborted = true;
            agentInFlight = false;
            AgentBridge.Abort(settings.AgentEndpoint);
            AgentReplyLog.Append("(abort)", "stopped by chord press", 0, false);
            try
            {
                dispatcher.BeginInvoke(new Action(delegate
                {
                    agentOverlay.ShowFailed("stopped");
                    SetTrayAgentState(false);
                }));
            }
            catch { }
        }

        // Deferred notices land here: things the agent was asked to watch for, arriving
        // minutes or hours after the ask that armed them.
        private void PollAgentNotices()
        {
            if (agentHook == null || agentInFlight) return;
            AgentBridge.FetchNotices(settings.AgentEndpoint, delegate(List<string> lines)
            {
                try
                {
                    dispatcher.BeginInvoke(new Action(delegate
                    {
                        foreach (string line in lines) AgentReplyLog.Append("(notice)", line, 0, true);
                        agentOverlay.ShowNotice(String.Join("\n", lines.ToArray()));
                        if (settings.CompletionSound) RecordingCue.PlayComplete();
                    }));
                }
                catch { }
            });
        }

        private void ScheduleDoubleTapTimeout()
        {
            CancelDoubleTapTimer();
            doubleTapTimer = new System.Windows.Forms.Timer();
            doubleTapTimer.Interval = DoubleTapWindowMs;
            doubleTapTimer.Tick += delegate
            {
                doubleTapTimer.Stop();
                doubleTapTimer.Dispose();
                doubleTapTimer = null;
                awaitingDoubleTap = false;
                if (!latchedRecording && !hotkeyDown)
                    try { dispatcher.BeginInvoke(new Action(StopRecording)); } catch { }
            };
            doubleTapTimer.Start();
        }

        private void CancelDoubleTapTimer()
        {
            if (doubleTapTimer == null) return;
            doubleTapTimer.Stop();
            doubleTapTimer.Dispose();
            doubleTapTimer = null;
        }

        private void ResetRecordingMode()
        {
            latchedRecording = false;
            awaitingDoubleTap = false;
            handsFreeStopPending = false;
            currentTakeIsAgent = false;
            pendingAgentTake = false;
            CancelDoubleTapTimer();
        }

        private void UpdateRecordingStatus()
        {
            if (latchedRecording)
            {
                statusItem.Text = "Hands-free — press " + settings.Hotkey + " to finish";
                toggleItem.Text = "Stop and insert";
                return;
            }
            if (currentTakeIsAgent)
            {
                statusItem.Text = "Listening (agent)… release " + settings.AgentHotkey;
                toggleItem.Text = "Stop and send to agent";
                return;
            }
            statusItem.Text = "Listening… release " + settings.Hotkey;
            toggleItem.Text = "Stop and insert";
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
            if (recorder.IsRecording)
            {
                latchedRecording = false;
                awaitingDoubleTap = false;
                CancelDoubleTapTimer();
                StopRecording();
            }
            else StartRecording();
        }

        private void StartRecording()
        {
            if (shuttingDown || recorder.IsRecording) return;
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
                currentTakeIsAgent = pendingAgentTake;
                pendingAgentTake = false;
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
                // Seat and overlay after the mic is already rolling so the first word is not
                // lost to window capture or waveInOpen.
                agentSeat = currentTakeIsAgent ? ForegroundContext.Capture(false) : null;
                AgentTrace.Log("StartRecording: agent=" + currentTakeIsAgent
                    + (agentSeat != null ? " seat=" + agentSeat.ProcessName + " | " + agentSeat.Title : ""));
                target = ForegroundContext.Capture(settings.ContextEnabled);
                ForegroundContext.RememberClipboard();
                recordTimer = Stopwatch.StartNew();
                hook.CaptureEscape = true;
                // Agent takes get the terminal HUD; dictation keeps the glass capsule.
                if (currentTakeIsAgent) agentOverlay.ShowListening(settings.AgentHotkey, settings.AgentEndpoint);
                else overlay.ShowRecording(settings.Hotkey, settings.OverlayTheme, settings.OverlayMark);
                if (settings.CompletionSound) RecordingCue.PlayStart();
                UpdateRecordingStatus();
            }
            catch (Exception exception)
            {
                store.LogError(exception);
                overlay.ShowFailure(ShortMessage(exception.Message));
                Notify("Could not start recording", exception.Message, ToolTipIcon.Error);
                ResetRecordingMode();
                SetReady();
            }
        }

        private void StopRecording()
        {
            CancelDoubleTapTimer();
            awaitingDoubleTap = false;
            if (!recorder.IsRecording)
            {
                overlay.EnsureHidden();
                ResetRecordingMode();
                return;
            }
            latchedRecording = false;
            try
            {
                // An agent take never borrows the dictation capsule — the HUD owns its whole life.
                if (currentTakeIsAgent) overlay.EnsureHidden();
                else overlay.ShowProcessing();
                hook.CaptureEscape = false;
                long recordMs = recordTimer != null ? recordTimer.ElapsedMilliseconds : 0;
                recordTimer = null;
                string path = recorder.Stop();
                toggleItem.Text = "Start dictating";
                FileInfo file = new FileInfo(path);
                if (recordMs < MinChordHoldMs || !file.Exists || file.Length < 5000)
                {
                    AgentTrace.Log("short take discarded: ms=" + recordMs + " bytes=" + (file.Exists ? file.Length : 0)
                        + " agent=" + currentTakeIsAgent);
                    if (currentTakeIsAgent) agentOverlay.HideNow();
                    TryDelete(path);
                    ResetRecordingMode();
                    if (inflightJobs == 0)
                    {
                        overlay.HideNow();
                        SetReady();
                    }
                    else
                    {
                        overlay.ShowProcessing();
                        statusItem.Text = "Writing…";
                    }
                    return;
                }
                int sequence;
                lock (jobGate)
                {
                    sequence = ++nextJobSequence;
                    inflightJobs++;
                    processing = true;
                }
                bool agentTake = currentTakeIsAgent;
                currentTakeIsAgent = false;
                AgentTrace.Log("StopRecording: agent=" + agentTake + " recordMs=" + recordMs);
                if (agentTake)
                {
                    overlay.EnsureHidden();
                    agentOverlay.ShowWorking("transcribing…");
                }
                statusItem.Text = agentTake ? "Asking agent…" : "Writing…";
                ProcessRecording(path, sequence, recordMs, target, agentTake);
            }
            catch (Exception exception)
            {
                store.LogError(exception);
                Notify("Recording failed", exception.Message, ToolTipIcon.Error);
                ResetRecordingMode();
                bool writing;
                lock (jobGate) writing = inflightJobs > 0;
                if (writing)
                {
                    overlay.ShowProcessing();
                    statusItem.Text = "Writing…";
                }
                else
                {
                    processing = false;
                    overlay.HideNow();
                    SetReady();
                }
            }
        }

        private async void ProcessRecording(string path, int sequence, long recordMs, ForegroundInfo intended, bool agentTake)
        {
            string raw = "";
            Stopwatch totalTimer = Stopwatch.StartNew();
            long transcribeMs = 0;
            long cleanMs = 0;
            bool queued = false;
            try
            {
                Stopwatch transcribeTimer = Stopwatch.StartNew();
                SpeechTranscript transcript;
                if (settings.Engine == "OpenAI")
                {
                    transcript = new SpeechTranscript();
                    transcript.Text = await new OpenAiEngine().TranscribeAsync(path, settings, apiKey);
                }
                else if (settings.Engine == "Groq")
                {
                    transcript = await groqEngine.TranscribeAsync(path, settings, groqKey, intended);
                }
                else transcript = await whisperEngine.TranscribeAsync(path, settings, intended);
                transcribeTimer.Stop();
                transcribeMs = transcribeTimer.ElapsedMilliseconds;
                ForegroundInfo delivery = ForegroundContext.Capture(settings.ContextEnabled);
                bool intendedReal = intended != null && intended.Handle != IntPtr.Zero
                    && !String.Equals(intended.ProcessName, "Flowtype", StringComparison.OrdinalIgnoreCase);
                if (intendedReal && delivery != null && delivery.Handle != intended.Handle) delivery = intended;
                raw = TextProcessor.StripPromptHallucinations(transcript.Text, settings, delivery);
                raw = TextProcessor.RemoveExactDuplicateBlocks(raw);
                raw = TextProcessor.ApplyAlwaysEdits(raw, settings);
                transcript.Text = raw;

                if (agentTake)
                {
                    // Agent takes never enter the paste path: release the insert-order slot,
                    // hand the raw take to the configured local runtime, and stay instant.
                    DrainInserts(insertQueue.Finish(sequence, null));
                    queued = true;
                    string ask = (raw ?? "").Trim();
                    AgentTrace.Log("agent branch: ask=" + (ask.Length > 60 ? ask.Substring(0, 60) + "..." : ask));
                    if (ask.Length == 0)
                    {
                        agentOverlay.ShowFailed("no speech detected");
                        throw new InvalidOperationException("No speech was detected.");
                    }
                    SendToAgent(ask);
                    if (settings.SaveHistory)
                    {
                        HistoryEntry agentEntry = new HistoryEntry();
                        agentEntry.CreatedUtc = DateTime.UtcNow;
                        agentEntry.Application = "Agent chord";
                        agentEntry.RawText = raw;
                        agentEntry.FinalText = ask;
                        agentEntry.Engine = settings.Engine;
                        history.Add(agentEntry);
                    }
                    TryDelete(path);
                    totalTimer.Stop();
                    LatencyStats.Update(recordMs, transcribeMs, 0, totalTimer.ElapsedMilliseconds);
                    return;
                }

                if (TextProcessor.IsUndoLastCommand(raw))
                {
                    DrainInserts(insertQueue.Finish(sequence, new PendingInsert { Undo = true, Delivery = delivery, Sequence = sequence }));
                    queued = true;
                    TryDelete(path);
                    return;
                }

                FileInfo audioInfo = new FileInfo(path);
                if (TranscriptionQuality.ShouldReject(raw, recordMs, audioInfo.Exists ? audioInfo.Length : 0))
                    throw new InvalidOperationException("Speech was too unclear to insert. Hold the hotkey a moment longer and try again.");

                string finalText = raw;
                if (settings.CleanupEnabled)
                {
                    Stopwatch cleanTimer = Stopwatch.StartNew();
                    try
                    {
                        if (settings.CleanupProvider == "OpenAI") finalText = await new OpenAiEngine().CleanupAsync(raw, delivery, settings, apiKey);
                        else if (settings.CleanupProvider == "OpenRouter") finalText = await new OpenRouterEngine().CleanupAsync(raw, delivery, settings, openRouterKey);
                        else if (settings.CleanupProvider == "Ollama")
                        {
                            Action<string> onDelta = delegate(string soFar)
                            {
                                try { overlay.SetProcessingPreview(soFar); }
                                catch { }
                            };
                            finalText = await new OllamaEngine().CleanupAsync(raw, delivery, settings, onDelta);
                        }
                        else finalText = TextProcessor.Clean(transcript, settings, delivery);
                    }
                    catch (Exception cleanupError)
                    {
                        store.LogError(cleanupError);
                        finalText = TextProcessor.Clean(transcript, settings, delivery);
                    }
                    cleanTimer.Stop();
                    cleanMs = cleanTimer.ElapsedMilliseconds;
                }
                finalText = TextProcessor.ApplyAlwaysEdits(finalText, settings);
                finalText = TextProcessor.NormalizePunctuationSpacing(finalText);
                bool pressEnter = TextProcessor.ExtractPressEnter(ref finalText);
                if (String.IsNullOrWhiteSpace(finalText) && !pressEnter) throw new InvalidOperationException("No speech was detected.");

                PendingInsert job = new PendingInsert();
                job.Text = finalText;
                job.PressEnter = pressEnter;
                job.Delivery = delivery;
                job.AutoPaste = settings.AutoPaste;
                job.Sequence = sequence;
                DrainInserts(insertQueue.Finish(sequence, job));
                queued = true;
                if (settings.SaveHistory)
                {
                    HistoryEntry entry = new HistoryEntry();
                    entry.CreatedUtc = DateTime.UtcNow;
                    entry.Application = delivery == null ? "" : delivery.AppLabel;
                    entry.RawText = raw;
                    entry.FinalText = finalText;
                    entry.Engine = settings.Engine;
                    history.Add(entry);
                }
                TryDelete(path);
                totalTimer.Stop();
                LatencyStats.Update(recordMs, transcribeMs, cleanMs, totalTimer.ElapsedMilliseconds);
            }
            catch (Exception exception)
            {
                AgentTrace.Log("ProcessRecording error: " + exception.Message);
                store.LogError(exception);
                RememberDictation(raw, sequence);
                if (!queued) DrainInserts(insertQueue.Finish(sequence, null));
                if (!settings.KeepFailedAudio) TryDelete(path);
                string suffix = settings.KeepFailedAudio ? " The recording is in Recovery." : "";
                Notify("Dictation failed", ShortMessage(exception.Message) + suffix, ToolTipIcon.Error);
            }
            finally
            {
                bool idle;
                lock (jobGate)
                {
                    inflightJobs--;
                    if (inflightJobs < 0) inflightJobs = 0;
                    idle = inflightJobs == 0 && !recorder.IsRecording;
                    if (idle) processing = false;
                }
                if (notifyRecordingLimit)
                {
                    notifyRecordingLimit = false;
                    Notify("Recording limit", "Dictation stopped at the 10-minute limit.", ToolTipIcon.Info);
                }
                if (idle)
                {
                    overlay.HideNow();
                    SetReady();
                }
                else if (recorder.IsRecording) UpdateRecordingStatus();
                else
                {
                    overlay.ShowProcessing();
                    statusItem.Text = "Writing…";
                }
            }
        }

        private void DrainInserts(List<PendingInsert> jobs)
        {
            if (jobs == null) return;
            foreach (PendingInsert job in jobs)
            {
                if (job == null) continue;
                if (job.Undo)
                {
                    UndoLastDictation(job.Delivery ?? lastInsertTarget);
                    continue;
                }
                bool inserted;
                bool enterSafe;
                if (String.IsNullOrWhiteSpace(job.Text))
                {
                    // Bare "press enter": the pulse is session-wide, so it must prove the
                    // intended window still has the seat — refocus it or refuse, never fire
                    // into whatever stole focus while Whisper was thinking.
                    inserted = !ForegroundContext.IsFlowtypeForeground()
                        && (job.Delivery == null || job.Delivery.Handle == IntPtr.Zero
                            || ForegroundContext.IsSameTarget(job.Delivery)
                            || ForegroundContext.TryFocus(job.Delivery));
                    enterSafe = inserted;
                }
                else
                {
                    inserted = ForegroundContext.DeliverDictation(job.Text, job.Delivery, job.AutoPaste, job.Sequence);
                    // A suppressed duplicate reports success without pasting — pulsing Enter
                    // there would double-send the previous commit.
                    enterSafe = inserted && !ForegroundContext.LastDeliverySuppressed;
                }
                if (enterSafe && job.PressEnter)
                {
                    Thread.Sleep(35);
                    ForegroundContext.PressEnter();
                }
                if (inserted && !String.IsNullOrWhiteSpace(job.Text))
                {
                    RememberDictation(job.Text, job.Sequence);
                    lastDictationWord = LastWord(job.Text);
                    UpdateDictionaryFixItem();
                    lastInsertTarget = job.Delivery;
                    canUndoInsert = true;
                    if (undoLastItem != null) undoLastItem.Enabled = true;
                    if (settings.CompletionSound) RecordingCue.PlayComplete();
                    if (settings.ShowInsertNotification) Notify("Inserted", ShortPreview(job.Text), ToolTipIcon.Info);
                }
                else if (!String.IsNullOrWhiteSpace(job.Text))
                {
                    RememberDictation(job.Text, job.Sequence);
                    lastDictationWord = LastWord(job.Text);
                    UpdateDictionaryFixItem();
                    if (ForegroundContext.LastDeliveryNeedsShiftPaste)
                    {
                        statusItem.Text = "Copied — click your field and press Ctrl+Shift+V";
                        Notify("Kept on clipboard", "This field wants Ctrl+Shift+V. Your take is on the clipboard if nothing appeared.", ToolTipIcon.Info);
                    }
                    else
                    {
                        statusItem.Text = "Copied — click your field and press Ctrl+V";
                        Notify("Kept on clipboard", "Couldn't drop it in, so it's on your clipboard. Click the field and press Ctrl+V.", ToolTipIcon.Info);
                    }
                }
            }
        }

        private void UndoLastDictation(ForegroundInfo intended)
        {
            if (!canUndoInsert)
            {
                Notify("Nothing to undo", "No recent dictation to reverse.", ToolTipIcon.Info);
                return;
            }
            if (intended != null && intended.Handle != IntPtr.Zero)
                ForegroundContext.TryFocus(intended);
            ForegroundContext.UndoLastInsert();
            canUndoInsert = false;
            if (undoLastItem != null) undoLastItem.Enabled = false;
            statusItem.Text = "Undid last dictation";
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
            agentHotkeyDown = false;
            agentOverlay.HideNow();
            ResetRecordingMode();
            toggleItem.Text = "Start dictating";
            bool writing;
            lock (jobGate) writing = inflightJobs > 0;
            if (writing)
            {
                overlay.ShowProcessing();
                statusItem.Text = "Writing…";
            }
            else
            {
                overlay.HideNow();
                SetReady();
            }
        }

        private void SetReady()
        {
            ResetRecordingMode();
            statusItem.Text = "Ready — hold " + settings.Hotkey;
            tray.Text = "Flowtype — hold " + settings.Hotkey + " to dictate";
            if (!recorder.IsRecording && inflightJobs == 0) overlay.EnsureHidden();
        }

        private void RememberDictation(string text, int sequence)
        {
            if (String.IsNullOrWhiteSpace(text)) return;
            if (sequence < lastRememberedGeneration) return;
            lastRememberedGeneration = sequence;
            lastDictationText = text;
            if (copyLastItem != null) copyLastItem.Enabled = true;
        }

        private void CopyLastDictation()
        {
            if (String.IsNullOrWhiteSpace(lastDictationText)) return;
            try
            {
                Clipboard.SetText(lastDictationText);
                statusItem.Text = "Last dictation copied — press Ctrl+V";
            }
            catch (Exception exception) { store.LogError(exception); }
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
                    string from;
                    string to;
                    if (TextProcessor.TryParseDictionaryEntry(entry, out from, out to))
                        return String.Equals(from, lastDictationWord, StringComparison.OrdinalIgnoreCase);
                    return String.Equals((entry ?? "").Trim(), lastDictationWord, StringComparison.OrdinalIgnoreCase);
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
            try { recorder.ReleaseWarm(); } catch { }
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
                SyncAgentHook();
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
                // Settings can turn agent mode on or off, so the hook must follow the file.
                if (settings.AgentModeEnabled) EnableAgentHook(); else DisableAgentHook();
                SyncAgentHook();
                if (agentToggleItem != null)
                {
                    agentToggleItem.Checked = settings.AgentModeEnabled && agentHook != null;
                    agentToggleItem.Text = AgentToggleLabel();
                }
                overlay.SetTheme(settings.OverlayTheme);
            overlay.SetMark(settings.OverlayMark);
                SetReady();
                if (settings.Engine == "Local") WarmLocalEngine();
                else
                {
                    whisperEngine.Unload();
                    if (settings.Engine == "Groq" && !String.IsNullOrWhiteSpace(groqKey)) WarmGroqEngine();
                }
            };
            settingsForm.FormClosed += delegate
            {
                settingsForm = null;
                ThreadPool.QueueUserWorkItem(delegate
                {
                    try { recorder.Prime(); }
                    catch (Exception exception) { store.LogError(exception); }
                });
            };
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

        private static void OpenAgentReplyLog()
        {
            try
            {
                string path = AgentReplyLog.Path;
                Directory.CreateDirectory(Path.GetDirectoryName(path));
                if (!File.Exists(path))
                    File.WriteAllText(path, "No agent replies yet. Hold the agent chord and ask for something." + Environment.NewLine,
                        new UTF8Encoding(false));
                Process.Start(new ProcessStartInfo { FileName = path, UseShellExecute = true });
            }
            catch { }
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
            try { if (agentHook != null) agentHook.Dispose(); } catch { }
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
                // The text pipeline uses ~55 distinct inline regex patterns per dictation; the
                // default 15-entry static cache thrashes and re-parses every pattern each run.
                Regex.CacheSize = 128;
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
