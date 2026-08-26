using System;
using System.Collections.Generic;
using System.Drawing;
using Flowtype;

namespace Flowtype.Tests
{
    public sealed class TestRunner
    {
        public int RunAll()
        {
            int failures = 0;
            failures += AssertEqual("self correction", "I want pizza no I want pasta.", TextProcessor.Clean("I want pizza no I want pasta.", AppSettings.Defaults()));
            failures += AssertEqual("repeated words", "The cat.", TextProcessor.Clean("the the cat", AppSettings.Defaults()));
            failures += AssertEqual("fuzzy dictionary", "Open Settings.", FuzzySettingsTest());
            failures += AssertEqual("prompt echo", "How do you think the integration will be",
                TextProcessor.StripPromptHallucinations(
                    "How do you think the integration will be -- Target window 2x.% Outro to all. Target window 1x.% Outro to on its end of the video. Target window 1x. Camp.1 P.$%&P%k.99.",
                    AppSettings.Defaults(), null));
            failures += AssertEqual("whisper repeat", "The team shipped the feature yesterday.",
                TextProcessor.Clean("The team shipped the feature yesterday. The team shipped the feature yesterday. The team shipped the feature yesterday.", AppSettings.Defaults()));
            failures += AssertFalse(TranscriptionQuality.ShouldReject("T", 500, 12000));
            failures += AssertFalse(TranscriptionQuality.ShouldReject("P.", 800, 20000));
            failures += AssertTrue(TranscriptionQuality.ShouldReject("~", 500, 12000));
            failures += AssertTrue(TranscriptionQuality.ShouldReject(".", 500, 12000));
            failures += AssertFalse(TranscriptionQuality.ShouldReject("5", 400, 12000));
            failures += AssertFalse(TranscriptionQuality.ShouldReject("10", 500, 12000));
            failures += AssertFalse(TranscriptionQuality.ShouldReject("100.", 600, 20000));
            failures += AssertFalse(TranscriptionQuality.ShouldReject("no", 500, 12000));
            failures += AssertFalse(TranscriptionQuality.ShouldReject("Thank you.", 900, 20000));
            failures += AssertTrue(TranscriptionQuality.ShouldReject("Thank you.", 4500, 140000));
            failures += AssertTrue(TranscriptionQuality.ShouldReject("Thanks for watching.", 4000, 120000));
            failures += AssertEqual("trailing thanks hallucination",
                "Yo bro, this shit keeps saying thank you.",
                TextProcessor.Clean("Yo bro, this shit keeps saying thank you. Thank you.", AppSettings.Defaults()));
            failures += AssertEqual("leading thanks hallucination",
                "Yo bro, this shit keeps saying thank you.",
                TextProcessor.Clean("Thank you. Yo bro, this shit keeps saying thank you.", AppSettings.Defaults()));
            failures += AssertEqual("thanks for watching hallucination",
                "The team shipped the feature yesterday.",
                TextProcessor.Clean("The team shipped the feature yesterday. Thanks for watching.", AppSettings.Defaults()));
            failures += AssertEqual("genuine short thanks kept", "Thank you.", TextProcessor.Clean("thank you", AppSettings.Defaults()));
            failures += AssertEqual("short closer thanks kept",
                "Let me know. Thanks.",
                TextProcessor.Clean("Let me know. Thanks.", AppSettings.Defaults()));
            failures += AssertEqual("inline thank you kept",
                "I told them thank you and left.",
                TextProcessor.Clean("I told them thank you and left", AppSettings.Defaults()));
            AppSettings epaDictionary = AppSettings.Defaults();
            epaDictionary.Dictionary.Add("eppi => epa");
            failures += AssertEqual("dictionary exact eppi",
                "The epa ruling came through.",
                TextProcessor.Clean("the eppi ruling came through", epaDictionary));
            failures += AssertEqual("dictionary whisper variant eppy",
                "The epa ruling came through.",
                TextProcessor.Clean("the eppy ruling came through", epaDictionary));
            failures += AssertEqual("dictionary still applies without cleanup path",
                "the epa ruling came through",
                TextProcessor.ApplyDictionaryReplacements("the eppy ruling came through", epaDictionary));
            failures += AssertTrue(TextProcessor.IsUndoLastCommand("scratch that"));
            failures += AssertTrue(TextProcessor.IsUndoLastCommand("Undo that."));
            failures += AssertFalse(TextProcessor.IsUndoLastCommand("I said scratch that yesterday"));
            failures += AssertTrue(TextProcessor.IsLightCleanup("yeah"));
            failures += AssertTrue(TextProcessor.IsLightCleanup("ok send it"));
            failures += AssertFalse(TextProcessor.IsLightCleanup("first get milk second get bread third get eggs"));
            failures += AssertEqual("short take does not become a list",
                "First get milk.",
                TextProcessor.Clean("first get milk", AppSettings.Defaults()));
            failures += AssertFalse(MicLevel.ShouldRaiseBoost(0.32f, 2.0f));
            failures += AssertTrue(MicLevel.ShouldRaiseBoost(0.04f, 1.2f));
            failures += AssertTrue(MicLevel.IsTooHot(0.94f));
            failures += AssertEqual("mic advice not 2x when voice is already loud",
                "ok",
                MicLevel.Evaluate(0.32f, 0.64f, 2.0f).Band);
            OrderedInsertQueue queue = new OrderedInsertQueue();
            List<string> early = queue.Complete(2, "second");
            failures += AssertTrue(early.Count == 0);
            List<string> drained = queue.Complete(1, "first");
            failures += AssertEqual("insert queue order", "first|second", String.Join("|", drained.ToArray()));
            OrderedInsertQueue skipQueue = new OrderedInsertQueue();
            skipQueue.Skip(1);
            List<string> afterFail = skipQueue.Complete(2, "kept");
            failures += AssertEqual("insert queue skips failed", "kept", String.Join("|", afterFail.ToArray()));
            byte[] padded = MakePcmTone(8000, 0.30, 0.50, 0.50);
            byte[] trimmed = WaveRecorder.TrimSilence(padded, 16000, 100);
            double trimmedSeconds = trimmed.Length / 32000.0;
            failures += AssertTrue(trimmedSeconds > 0.40 && trimmedSeconds < 0.65);
            byte[] quietEdges = MakePcmWithQuietEdges(400, 0.18, 12000, 0.40);
            byte[] keptEdges = WaveRecorder.TrimSilence(quietEdges, 16000, 120);
            double keptSeconds = keptEdges.Length / 32000.0;
            failures += AssertTrue("quiet consonants at start and end stay in the take",
                keptSeconds > 0.72 && keptSeconds < 0.95);
            PcmRing ring = new PcmRing(3200);
            ring.Write(MakePcmTone(1000, 0.20, 0, 0));
            ring.Write(MakePcmTone(2000, 0.10, 0, 0));
            byte[] snapshot = ring.Snapshot();
            failures += AssertTrue("preroll ring keeps only the latest capacity", snapshot.Length == 3200);
            failures += AssertTrue("preroll ring wrap keeps the newest samples",
                SnapshotPeak(snapshot) == 2000);
            ring.Clear();
            failures += AssertTrue("preroll ring clear empties snapshot", ring.Snapshot().Length == 0);
            failures += AssertEqual("trailing thanks without period",
                "So we need to finish the project by Friday and then send it.",
                TextProcessor.Clean("So we need to finish the project by Friday and then send it Thank you", AppSettings.Defaults()));
            failures += AssertEqual("leading thanks without period",
                "Yo bro, this shit keeps saying thank you.",
                TextProcessor.Clean("Thank you Yo bro, this shit keeps saying thank you.", AppSettings.Defaults()));
            failures += AssertTrue(TranscriptionQuality.IsLikelyThanksHallucination("Thank you.", 0.6, 9.0));
            failures += AssertFalse(TranscriptionQuality.IsLikelyThanksHallucination("Thank you.", 0.05, 9.0));
            failures += AssertFalse(TranscriptionQuality.IsLikelyThanksHallucination("Thank you for coming.", 0.8, 9.0));
            failures += AssertEqual("embedded lone T",
                "So we need to finish the project by Friday and then send it to the client for review.",
                TextProcessor.Clean("So we need to finish the project by Friday and T then send it to the client for review", AppSettings.Defaults()));
            failures += AssertEqual("mid prompt echo",
                "The integration is working well and we should ship tomorrow.",
                TextProcessor.Clean("The integration is working well Target window 1x garbage and we should ship tomorrow", AppSettings.Defaults()));
            failures += AssertTrue(TranscriptionQuality.IsLikelyEmbeddedHallucination("T", 0.08, 0.4, 0.5));
            failures += AssertFalse(TranscriptionQuality.IsLikelyEmbeddedHallucination("I", 0.08, 0.4, 0.5));
            failures += AssertFalse(TranscriptionQuality.IsLikelyEmbeddedHallucination("P", 0.3, 0.4, 0.1));
            failures += AssertEqual("single letter dictation", "P.", TextProcessor.Clean("P", AppSettings.Defaults()));
            failures += AssertEqual("cue letter kept",
                "The drive letter is P okay.",
                TextProcessor.Clean("the drive letter is P okay", AppSettings.Defaults()));
            failures += AssertContains("1.", TextProcessor.Clean("first get milk second get bread third get eggs", AppSettings.Defaults()));
            failures += AssertContains("2. Get bread", TextProcessor.Clean("first get milk second get bread third get eggs", AppSettings.Defaults()));
            failures += AssertContains("3. Email the team",
                TextProcessor.Clean("first check the logs, then restart the server, then email the team", AppSettings.Defaults()));
            failures += AssertEqual("no numbered list from prose mentioning ordinals",
                "When I say first or second it puts it into a one and two order.",
                TextProcessor.Clean("when I say first or second it puts it into a one and two order", AppSettings.Defaults()));
            failures += AssertEqual("anchor plus lone then stays prose",
                "First let me check the logs, then we can decide.",
                TextProcessor.Clean("first let me check the logs, then we can decide", AppSettings.Defaults()));
            failures += AssertEqual("sentence-start then stays prose",
                "First of all thanks for coming. Then we discussed the roadmap. Next quarter looks good.",
                TextProcessor.Clean("First of all thanks for coming. Then we discussed the roadmap. Next quarter looks good.", AppSettings.Defaults()));
            failures += AssertContains("- ", TextProcessor.Clean("bullet point apples bullet point bananas bullet point cherries", AppSettings.Defaults()));
            failures += AssertContains("- Apples\n- Bananas",
                TextProcessor.Clean("bullet point apples and bullet point bananas", AppSettings.Defaults()));
            failures += AssertContains("- Buy milk\n- Get eggs\n- Call mom",
                TextProcessor.Clean("buy milk next point get eggs next point call mom", AppSettings.Defaults()));
            failures += AssertContains("- Buy milk",
                TextProcessor.Clean("next point buy milk", AppSettings.Defaults()));
            failures += AssertContains("1. Milk\n2. Bread",
                TextProcessor.Clean("next number milk next number bread", AppSettings.Defaults()));
            AppSettings customLists = AppSettings.Defaults();
            customLists.SpokenBulletPhrase = "new item";
            customLists.SpokenNumberPhrase = "new step";
            failures += AssertContains("- Milk\n- Eggs",
                TextProcessor.Clean("new item milk new item eggs", customLists));
            failures += AssertContains("1. Mix\n2. Bake",
                TextProcessor.Clean("new step mix new step bake", customLists));
            failures += AssertEqual("custom phrase replaces next point",
                "Next point milk next point eggs.",
                TextProcessor.Clean("next point milk next point eggs", customLists));
            AppSettings listsOff = AppSettings.Defaults();
            listsOff.SpokenListsEnabled = false;
            failures += AssertEqual("spoken lists can be turned off",
                "Next point apples next point bananas.",
                TextProcessor.Clean("next point apples next point bananas", listsOff));
            failures += AssertEqual("the next point stays prose",
                "The next point after lunch is the budget.",
                TextProcessor.Clean("the next point after lunch is the budget", AppSettings.Defaults()));
            AppSettings repairedLists = AppSettings.Defaults();
            repairedLists.SpokenBulletPhrase = null;
            repairedLists.SpokenNumberPhrase = null;
            repairedLists.Repair();
            failures += AssertEqual("bullet phrase defaulted", "next point", repairedLists.SpokenBulletPhrase);
            failures += AssertEqual("number phrase defaulted", "next number", repairedLists.SpokenNumberPhrase);
            failures += AssertContains("next point", WhisperEngine.BuildPrompt(AppSettings.Defaults(), null));
            failures += AssertContains("- buy milk\n- get eggs",
                TextProcessor.ApplyAlwaysEdits("buy milk next point get eggs", AppSettings.Defaults()));
            failures += AssertContains("2. Restart the server",
                TextProcessor.Clean("first of all check the logs, second of all restart the server, finally email the team", AppSettings.Defaults()));
            failures += AssertEqual("period noun kept",
                "That went on for a long period of time.",
                TextProcessor.Clean("that went on for a long period of time", AppSettings.Defaults()));
            failures += AssertEqual("oxford comma noun kept",
                "You forgot the oxford comma in that sentence.",
                TextProcessor.Clean("you forgot the oxford comma in that sentence", AppSettings.Defaults()));
            failures += AssertEqual("existential no kept",
                "I knocked on the door, no answer.",
                TextProcessor.Clean("I knocked on the door, no answer", AppSettings.Defaults()));
            failures += AssertEqual("corrective no still works",
                "I want pasta.",
                TextProcessor.Clean("I want pizza, no, pasta", AppSettings.Defaults()));
            failures += AssertEqual("start over verb kept",
                "Once you press reset it will start over from the beginning.",
                TextProcessor.Clean("once you press reset it will start over from the beginning", AppSettings.Defaults()));
            failures += AssertEqual("let me start over wipes preamble",
                "Here is the real sentence.",
                TextProcessor.Clean("blah blah let me start over here is the real sentence", AppSettings.Defaults()));
            failures += AssertEqual("email preserved",
                "Send it to user@example.com.",
                TextProcessor.Clean("send it to user@example.com", AppSettings.Defaults()));
            failures += AssertEqual("filename preserved",
                "Open flowtype.cs.",
                TextProcessor.Clean("open flowtype.cs", AppSettings.Defaults()));
            failures += AssertEqual("url preserved in normalize",
                "See github.com for details.",
                TextProcessor.NormalizePunctuationSpacing("See github.com for details."));
            failures += AssertEqual("title token not fuzzed onto real word",
                "I was tracing the bug.",
                TextProcessor.Clean("I was tracing the bug", AppSettings.Defaults(), NotepadContext("Tracking - Notepad")));
            failures += AssertEqual("no inferred list from prose",
                "The main points are that we should move fast, stay focused, and ship on time.",
                TextProcessor.Clean("The main points are that we should move fast, stay focused, and ship on time", AppSettings.Defaults()));
            failures += AssertEqual("no inferred list from goals prose",
                "My goals for this week are exercise, reading, and finishing the prototype.",
                TextProcessor.Clean("My goals for this week are exercise, reading, and finishing the prototype", AppSettings.Defaults()));
            failures += AssertContains("- Better tooling",
                TextProcessor.Clean("here are the top three things we need, better tooling, clearer specs, and more time", AppSettings.Defaults()));
            failures += AssertEqual("not preserved", "Not.", TextProcessor.Clean("not", AppSettings.Defaults()));
            failures += AssertEqual("split not healed", "Not.", TextProcessor.Clean("no t", AppSettings.Defaults()));
            failures += AssertEqual("split not in sentence", "I do not want.",
                TextProcessor.Clean("I do, no t want", AppSettings.Defaults()));
            failures += AssertEqual("split not after dash", "Do not.",
                TextProcessor.Clean("do-no t", AppSettings.Defaults()));
            failures += AssertEqual("no spurious t from backtrack", "Want not.",
                TextProcessor.Clean("want, no t", AppSettings.Defaults()));
            failures += AssertEqual("final not replaced in discord chat", "I'm doing some final testing now",
                TextProcessor.Clean("I'm doing some final testing now", AppSettings.Defaults(), DiscordContext("PinBal")));
            failures += AssertEqual("finnal not fuzzy to contact name", "I'm doing some finnal testing now",
                TextProcessor.Clean("I'm doing some finnal testing now", AppSettings.Defaults(), DiscordContext("PinBal")));
            failures += AssertEqual("discord contact name still corrected", "Hey PinBal",
                TextProcessor.Clean("hey pinbal", AppSettings.Defaults(), DiscordContext("PinBal")));
            failures += AssertEqual("spoken period", "Hello.", TextProcessor.Clean("hello period", AppSettings.Defaults()));
            failures += AssertEqual("spoken question", "Ready?", TextProcessor.Clean("ready question mark", AppSettings.Defaults()));
            failures += AssertTrue(Hotkeys.IsChord("Win + Ctrl"));
            failures += AssertFalse(Hotkeys.IsChord("Right Ctrl"));
            failures += AssertTrue(Hotkeys.IsModifierChord("Win + Alt"));
            failures += AssertFalse(Hotkeys.IsModifierChord("F8"));
            failures += AssertEqual("default engine", "Local", AppSettings.Defaults().Engine);
            failures += AssertEqual("default cleanup", "BuiltIn", AppSettings.Defaults().CleanupProvider);
            AppSettings repaired = AppSettings.Defaults();
            repaired.CleanupProvider = "OpenRouter";
            repaired.OpenRouterModel = "openrouter/free";
            repaired.Repair();
            failures += AssertEqual("openrouter free blocked", "BuiltIn", repaired.CleanupProvider);
            failures += AssertEqual("prefer 1b local model",
                "llama3.2:1b",
                OllamaEngine.PickPreferredModel(new string[] { "mistral:latest", "llama3.2:1b", "llama3.1:8b" }));
            failures += AssertEqual("prefer prefix when exact missing",
                "qwen2.5:1.5b-instruct",
                OllamaEngine.PickPreferredModel(new string[] { "qwen2.5:1.5b-instruct", "mistral:latest" }));
            failures += AssertEqual("first model when none preferred",
                "custom-finetune",
                OllamaEngine.PickPreferredModel(new string[] { "custom-finetune" }));
            failures += AssertEqual("empty local model list", "", OllamaEngine.PickPreferredModel(new string[0]));
            failures += AssertEqual("ollama generate delta", "Hel", OllamaEngine.ExtractStreamDelta("{\"response\":\"Hel\",\"done\":false}"));
            failures += AssertEqual("ollama chat delta", "lo", OllamaEngine.ExtractStreamDelta("{\"message\":{\"role\":\"assistant\",\"content\":\"lo\"},\"done\":false}"));
            failures += AssertEqual("openai sse delta", "!", OllamaEngine.ExtractStreamDelta("data: {\"choices\":[{\"delta\":{\"content\":\"!\"}}]}"));
            failures += AssertEqual("openai done ignored", "", OllamaEngine.ExtractStreamDelta("data: [DONE]"));
            failures += AssertTrue(AgentBridge.IsLoopbackEndpoint("http://127.0.0.1:5599/ask"));
            failures += AssertTrue(AgentBridge.IsLoopbackEndpoint("http://localhost:5599/ask"));
            failures += AssertTrue(AgentBridge.IsLoopbackEndpoint("http://[::1]:5599/ask"));
            failures += AssertFalse(AgentBridge.IsLoopbackEndpoint("http://192.168.1.10:5599/ask"));
            failures += AssertFalse(AgentBridge.IsLoopbackEndpoint("https://example.com/ask"));
            failures += AssertFalse(AgentBridge.IsLoopbackEndpoint(""));
            AppSettings remoteAgent = AppSettings.Defaults();
            remoteAgent.AgentEndpoint = "http://10.0.0.8:5599/ask";
            remoteAgent.Repair();
            failures += AssertEqual("remote agent endpoint reset", "http://127.0.0.1:5599/ask", remoteAgent.AgentEndpoint);
            failures += AssertTimeout("short clip turbo floor", 60, AudioTranscriptionTimeouts.ForWavFile("", true).TotalSeconds);
            failures += AssertTimeout("short clip standard floor", 90, AudioTranscriptionTimeouts.ForWavFile("", false).TotalSeconds);
            failures += AssertTimeout("ten minute clip", 600, AudioTranscriptionTimeouts.ForAudioSeconds(600, false).TotalSeconds);
            failures += AssertEqual("duplicate block removed",
                "Hello world this is a test.",
                TextProcessor.RemoveExactDuplicateBlocks("Hello world this is a test. --- Hello world this is a test."));
            failures += AssertEqual("no space when field empty",
                "Next sentence.",
                ForegroundContext.PrepareInsertText("Next sentence.", new ForegroundInfo()));
            failures += AssertEqual("period spacing",
                "can. But also reducing the cost.",
                TextProcessor.NormalizePunctuationSpacing("can.But also reducing the cost."));
            failures += AssertEqual("triple duplicate collapsed",
                "Hello world test.",
                TextProcessor.RemoveExactDuplicateBlocks("Hello world test.Hello world test.Hello world test."));
            failures += RunCaretFitTests();
            ForegroundInfo cursorFamily = new ForegroundInfo();
            cursorFamily.ProcessName = "Cursor";
            failures += AssertTrue(ForegroundContext.IsCursorFamily(cursorFamily));
            failures += AssertTrue(FlowtypeVersion.IsNewerThanCurrent("v1.3.71"));
            failures += AssertFalse(FlowtypeVersion.IsNewerThanCurrent("v" + FlowtypeVersion.CurrentLabel));
            failures += AssertEqual("version label", "1.3.70", FlowtypeVersion.CurrentLabel);
            failures += AssertTrue(ForegroundContext.CanRestoreOver("hello from dictation", "hello from dictation"));
            failures += AssertTrue(ForegroundContext.CanRestoreOver("", "hello from dictation"));
            failures += AssertTrue(ForegroundContext.CanRestoreOver(null, "hello from dictation"));
            failures += AssertTrue(ForegroundContext.CanRestoreOver("hello from dictation\r\n", "hello from dictation"));
            failures += AssertTrue(ForegroundContext.CanRestoreOver("my copied text", "spoken", "my copied text"));
            failures += AssertFalse(ForegroundContext.CanRestoreOver("I copied this first", "hello from dictation"));
            AppSettings monoTheme = AppSettings.Defaults();
            monoTheme.OverlayTheme = "Mono";
            monoTheme.Repair();
            failures += AssertEqual("mono theme migrated", "Dark", monoTheme.OverlayTheme);
            failures += AssertTrue("glass ink on dark backdrop is light", GlassChrome.Ink(true).R > 180);
            failures += AssertTrue("glass ink on light backdrop is dark", GlassChrome.Ink(false).R < 80);
            failures += AssertTrue("glass bars on dark backdrop are light", GlassChrome.Bar(true, 0.7f).R > 180);
            failures += AssertTrue("vscode dark editor counts as dark glass", GlassChrome.BackdropReadsDark(90 * 25, 25));
            failures += AssertFalse(GlassChrome.BackdropReadsDark(720 * 25, 25));
            AppSettings emberTheme = AppSettings.Defaults();
            emberTheme.OverlayTheme = "Ember";
            emberTheme.Repair();
            failures += AssertEqual("ember theme kept", "Ember", emberTheme.OverlayTheme);
            AppSettings hexMark = AppSettings.Defaults();
            hexMark.OverlayMark = "Hex";
            hexMark.Repair();
            failures += AssertEqual("hex mark kept", "Hex", hexMark.OverlayMark);
            AppSettings gridMark = AppSettings.Defaults();
            gridMark.OverlayMark = "Grid";
            gridMark.Repair();
            failures += AssertEqual("grid mark kept", "Grid", gridMark.OverlayMark);
            AppSettings badMark = AppSettings.Defaults();
            badMark.OverlayMark = "Spinner";
            badMark.Repair();
            failures += AssertEqual("unknown mark becomes orb", "Orb", badMark.OverlayMark);
            failures += AssertTrue(ForegroundContext.LooksUnpasteableProcess("Flowtype"));
            failures += AssertTrue(ForegroundContext.LooksUnpasteableProcess("consent"));
            failures += AssertTrue(ForegroundContext.LooksUnpasteableProcess("SearchHost"));
            failures += AssertFalse(ForegroundContext.LooksUnpasteableProcess("notepad"));
            failures += AssertFalse(ForegroundContext.LooksUnpasteableProcess("Cursor"));
            failures += AssertTrue(ForegroundContext.LooksUnpasteableClass("Shell_TrayWnd"));
            failures += AssertTrue(ForegroundContext.LooksUnpasteableClass("Progman"));
            failures += AssertTrue(ForegroundContext.LooksUnpasteableClass("#32768"));
            failures += AssertFalse(ForegroundContext.LooksUnpasteableClass("Chrome_WidgetWin_1"));
            failures += AssertFalse(ForegroundContext.LooksUnpasteableClass("Edit"));
            failures += AssertTrue(ForegroundContext.IsUnpasteableTarget(null));
            failures += AssertTrue(ForegroundContext.IsUnpasteableTarget(new ForegroundInfo()));
            failures += AssertTrue(ForegroundContext.LooksLikeShiftPasteProcess("WindowsTerminal"));
            failures += AssertTrue(ForegroundContext.LooksLikeShiftPasteProcess("pwsh"));
            failures += AssertTrue(ForegroundContext.LooksLikeShiftPasteProcess("cmd"));
            failures += AssertTrue(ForegroundContext.LooksLikeShiftPasteClass("ConsoleWindowClass"));
            failures += AssertTrue(ForegroundContext.LooksLikeShiftPasteClass("CASCADIA_HOSTING_WINDOW_CLASS"));
            failures += AssertFalse(ForegroundContext.LooksLikeShiftPasteProcess("notepad"));
            failures += AssertFalse(ForegroundContext.LooksLikeShiftPasteProcess("Cursor"));
            failures += AssertFalse(ForegroundContext.LooksLikeShiftPasteProcess("powershell_ise"));
            failures += AssertTrue(ForegroundContext.NameLooksLikeTerminalPane("Terminal 1"));
            failures += AssertTrue(ForegroundContext.NameLooksLikeTerminalPane("powershell"));
            failures += AssertTrue(ForegroundContext.NameLooksLikeTerminalPane("1: pwsh"));
            failures += AssertFalse(ForegroundContext.NameLooksLikeTerminalPane("Flowtype.cs"));
            failures += AssertFalse(ForegroundContext.NameLooksLikeTerminalPane("terminal.ts"));
            ForegroundInfo terminalField = new ForegroundInfo();
            terminalField.ProcessName = "WindowsTerminal";
            terminalField.Handle = new IntPtr(5);
            ForegroundInfo terminalAfter = new ForegroundInfo();
            terminalAfter.ProcessName = "WindowsTerminal";
            terminalAfter.Handle = new IntPtr(5);
            failures += AssertTrue(ForegroundContext.LooksLikeShiftPasteTarget(terminalField));
            failures += AssertTrue(ForegroundContext.ShouldKeepDictationOnClipboard(terminalField, terminalAfter));
            ForegroundInfo notepadField = new ForegroundInfo();
            notepadField.ProcessName = "notepad";
            notepadField.Handle = new IntPtr(8);
            ForegroundInfo notepadAfter = new ForegroundInfo();
            notepadAfter.ProcessName = "notepad";
            notepadAfter.Handle = new IntPtr(8);
            failures += AssertFalse(ForegroundContext.LooksLikeShiftPasteTarget(notepadField));
            failures += AssertFalse(ForegroundContext.ShouldKeepDictationOnClipboard(notepadField, notepadAfter));
            ForegroundInfo trayAfter = new ForegroundInfo();
            trayAfter.ProcessName = "explorer";
            trayAfter.Handle = new IntPtr(9);
            failures += AssertTrue(ForegroundContext.ShouldKeepDictationOnClipboard(notepadField, trayAfter));
            ForegroundInfo cursorEditor = new ForegroundInfo();
            cursorEditor.ProcessName = "Cursor";
            cursorEditor.Title = "Flowtype.cs - flowtype - Cursor";
            cursorEditor.Handle = new IntPtr(11);
            failures += AssertFalse(ForegroundContext.LooksLikeShiftPasteTarget(cursorEditor));
            failures += AssertFalse(ForegroundContext.ShouldKeepDictationOnClipboard(cursorEditor, cursorEditor));
            return failures;
        }

        private static int RunCaretFitTests()
        {
            int failures = 0;
            CaretNeighborhood midAfterWord = CaretNeighborhood.Known('d', ' ', false);
            CaretNeighborhood midAfterSpace = CaretNeighborhood.Known(' ', 't', false);
            CaretNeighborhood afterPeriod = CaretNeighborhood.Known('.', ' ', false);
            CaretNeighborhood afterPeriodSpace = CaretNeighborhood.Known(' ', '\0', false, '.');
            CaretNeighborhood emptyField = CaretNeighborhood.Known('\0', '\0', false);
            CaretNeighborhood afterOpenParen = CaretNeighborhood.Known('(', '\0', false);
            CaretNeighborhood selectionMid = CaretNeighborhood.Known(' ', ' ', true, 'e');
            CaretNeighborhood selectionSentence = CaretNeighborhood.Known('\0', ' ', true);
            CaretNeighborhood unread = CaretNeighborhood.Unavailable();

            failures += AssertEqual("mid insert lower + strip period",
                " quick fix",
                CaretFit.Apply("Quick fix.", midAfterWord, false));
            failures += AssertEqual("mid after space trailing only",
                "quick fix ",
                CaretFit.Apply("Quick fix.", midAfterSpace, false));
            failures += AssertEqual("user bug macro space shifter",
                "if it doesn't ",
                CaretFit.Apply("If it doesn't.", CaretNeighborhood.Known(' ', 's', false), false));
            failures += AssertEqual("sentence start keeps polish",
                "Quick fix.",
                CaretFit.Apply("Quick fix.", afterPeriodSpace, false));
            failures += AssertEqual("empty field keeps polish",
                "Quick fix.",
                CaretFit.Apply("Quick fix.", emptyField, false));
            failures += AssertEqual("preserve I mid-sentence",
                " I think so",
                CaretFit.Apply("I think so.", midAfterWord, false));
            failures += AssertEqual("preserve I'm mid-sentence",
                " I'm ready",
                CaretFit.Apply("I'm ready.", midAfterWord, false));
            failures += AssertEqual("preserve API acronym",
                " API is down",
                CaretFit.Apply("API is down.", midAfterWord, false));
            failures += AssertEqual("preserve OK acronym",
                " OK",
                CaretFit.Apply("OK.", midAfterWord, false));
            failures += AssertEqual("keep question mark",
                " does this work?",
                CaretFit.Apply("Does this work?", midAfterWord, false));
            failures += AssertEqual("no space after open paren",
                "quick",
                CaretFit.Apply("Quick.", afterOpenParen, false));
            failures += AssertEqual("selection no join space",
                "the",
                CaretFit.Apply("The.", selectionMid, false));
            failures += AssertEqual("selection at sentence start keeps capital",
                "That.",
                CaretFit.Apply("That.", selectionSentence, false));
            failures += AssertEqual("after period six people",
                " Six people.",
                CaretFit.Apply("Six people.", CaretNeighborhood.Known('.', '\0', false, '.'), false));
            failures += AssertEqual("unread long keeps sentence polish",
                " This is a longer dictation that should stay polished as a full sentence because it is not a short mid fragment anymore.",
                CaretFit.Apply("This is a longer dictation that should stay polished as a full sentence because it is not a short mid fragment anymore.", unread, false, false, true));
            failures += AssertEqual("unread short keeps sentence polish",
                " Quick fix.",
                CaretFit.Apply("Quick fix.", unread, false, false, true));
            failures += AssertEqual("unread without soft stays polished",
                "Quick fix.",
                CaretFit.Apply("Quick fix.", unread, false, false, false));
            failures += AssertEqual("unread continuity uses mid fit",
                "quick fix",
                CaretFit.Apply("Quick fix.", unread, true, false, true));
            failures += AssertEqual("space after sentence punct",
                " Next.",
                CaretFit.Apply("Next.", afterPeriod, false));
            failures += AssertEqual("clean then fit mid",
                " quick fix",
                CaretFit.Apply(TextProcessor.Clean("quick fix", AppSettings.Defaults()), midAfterWord, false));
            ForegroundInfo cursor = new ForegroundInfo();
            cursor.ProcessName = "Cursor";
            cursor.FocusHandle = new IntPtr(1);
            failures += AssertEqual("cursor skips caret fit keeps punctuation",
                "Quick fix.",
                ForegroundContext.PrepareInsertText("Quick fix.", cursor));
            failures += AssertEqual("cursor skips mid fragment strip",
                "If it doesn't.",
                ForegroundContext.PrepareInsertText("If it doesn't.", cursor));

            ForegroundInfo docs = new ForegroundInfo();
            docs.ProcessName = "chrome";
            docs.Handle = new IntPtr(42);
            docs.FocusHandle = IntPtr.Zero;
            ForegroundContext.NoteSuccessfulInsert(docs, "okay so this one");
            failures += AssertEqual("docs continuation lowercases without extra space",
                "is what I was using",
                ForegroundContext.PrepareInsertText("Is what I was using.", docs));
            ForegroundContext.NoteSuccessfulInsert(docs, "Okay so this one.");
            failures += AssertEqual("docs after period keeps sentence polish",
                " Next sentence.",
                ForegroundContext.PrepareInsertText("Next sentence.", docs));
            ForegroundInfo otherDocs = new ForegroundInfo();
            otherDocs.ProcessName = "chrome";
            otherDocs.Handle = new IntPtr(99);
            otherDocs.FocusHandle = IntPtr.Zero;
            failures += AssertEqual("docs different window keeps sentence polish",
                " Is what I was using.",
                ForegroundContext.PrepareInsertText("Is what I was using.", otherDocs));
            failures += AssertEqual("cursor still skips continuation lowercase",
                "Is what I was using.",
                ForegroundContext.PrepareInsertText("Is what I was using.", cursor));

            CaretNeighborhood afterCommaSpace = CaretNeighborhood.FromSnippets("We paused, ", "");
            CaretNeighborhood afterComma = CaretNeighborhood.FromSnippets("We paused,", "");
            CaretNeighborhood afterPeriodSnippet = CaretNeighborhood.FromSnippets("We paused. ", "");
            CaretNeighborhood midBetweenWords = CaretNeighborhood.FromSnippets("hello ", "world");
            CaretNeighborhood emptyDoc = CaretNeighborhood.FromSnippets("", "");
            CaretNeighborhood alreadySpaced = CaretNeighborhood.FromSnippets("hello ", "");
            CaretNeighborhood midWord = CaretNeighborhood.FromSnippets("test", "ed");
            CaretNeighborhood afterOpenQuote = CaretNeighborhood.FromSnippets("He said \"", "");

            failures += AssertEqual("classify after comma space",
                "ClauseContinue",
                CaretFit.Classify(afterCommaSpace, false, false).ToString());
            failures += AssertEqual("classify after period",
                "SentenceStart",
                CaretFit.Classify(afterPeriodSnippet, false, false).ToString());
            failures += AssertEqual("classify mid sentence",
                "MidSentence",
                CaretFit.Classify(alreadySpaced, false, false).ToString());
            failures += AssertEqual("classify empty doc",
                "SentenceStart",
                CaretFit.Classify(emptyDoc, false, false).ToString());
            failures += AssertEqual("classify mid word",
                "MidWord",
                CaretFit.Classify(midWord, false, false).ToString());
            failures += AssertEqual("classify after open quote",
                "AfterOpen",
                CaretFit.Classify(afterOpenQuote, false, false).ToString());
            failures += AssertEqual("classify abbreviation not sentence",
                "MidSentence",
                CaretFit.Classify(CaretNeighborhood.FromSnippets("See e.g. ", ""), false, false).ToString());
            failures += AssertEqual("after comma lowercase space",
                " and then we shipped it",
                CaretFit.Apply("And then we shipped it.", afterComma, false));
            failures += AssertEqual("after comma space lowercase",
                "and then we shipped it",
                CaretFit.Apply("And then we shipped it.", afterCommaSpace, false));
            failures += AssertEqual("after period capital space",
                "Next words.",
                CaretFit.Apply("Next words.", afterPeriodSnippet, false));
            failures += AssertEqual("mid sentence caret lowercase space",
                "quick fix ",
                CaretFit.Apply("Quick fix.", midBetweenWords, false));
            failures += AssertEqual("empty doc keeps capital",
                "Hello there.",
                CaretFit.Apply("Hello there.", emptyDoc, false));
            failures += AssertEqual("already spaced left no double space",
                "next words",
                CaretFit.Apply("Next words.", alreadySpaced, false));
            failures += AssertEqual("incoming glue punct no space",
                ", and then we continued",
                CaretFit.Apply(", and then we continued.", CaretNeighborhood.FromSnippets("first word", ""), false));
            failures += AssertEqual("and then after comma",
                "and then",
                CaretFit.Apply("And then.", afterCommaSpace, false));
            failures += AssertEqual("mid word no invented spaces",
                "ing",
                CaretFit.Apply("Ing.", midWord, false));
            failures += AssertEqual("downcase whisper capital on continue",
                "this continues the clause",
                CaretFit.Apply("This continues the clause.", afterCommaSpace, false));
            failures += AssertEqual("docs-like two takes join",
                " for the signal",
                CaretFit.Apply("For the signal.", CaretNeighborhood.FromSnippets("We paused, and wait", ""), false));
            failures += AssertEqual("after em dash continues lowercase",
                " and kept going",
                CaretFit.Apply("And kept going.", CaretNeighborhood.FromSnippets("We paused —", ""), false));
            failures += AssertEqual("classify after paragraph break",
                "SentenceStart",
                CaretFit.Classify(CaretNeighborhood.FromSnippets("ended\n  ", ""), false, false).ToString());
            failures += AssertEqual("after newline keeps capital",
                "The next topic.",
                CaretFit.Apply("The next topic.", CaretNeighborhood.FromSnippets("ended\n  ", ""), false));
            ForegroundContext.NoteSuccessfulInsert(docs, "okay so this one\n\n");
            failures += AssertEqual("docs after new paragraph keeps capital",
                " Next paragraph.",
                ForegroundContext.PrepareInsertText("Next paragraph.", docs));
            return failures;
        }

        private static int AssertTimeout(string name, int expectedSeconds, double actualSeconds)
        {
            if (Math.Abs(expectedSeconds - actualSeconds) < 0.5)
            {
                Console.WriteLine("PASS " + name);
                return 0;
            }
            Console.WriteLine("FAIL " + name + " expected=" + expectedSeconds + " actual=" + actualSeconds);
            return 1;
        }

        private static byte[] MakePcmTone(int amplitude, double toneSeconds, double leadSilenceSeconds, double trailSilenceSeconds)
        {
            int lead = (int)Math.Round(leadSilenceSeconds * 16000);
            int tone = (int)Math.Round(toneSeconds * 16000);
            int trail = (int)Math.Round(trailSilenceSeconds * 16000);
            byte[] pcm = new byte[(lead + tone + trail) * 2];
            for (int index = 0; index < tone; index++)
            {
                int offset = (lead + index) * 2;
                pcm[offset] = (byte)(amplitude & 0xFF);
                pcm[offset + 1] = (byte)((amplitude >> 8) & 0xFF);
            }
            return pcm;
        }

        private static byte[] MakePcmWithQuietEdges(int quietAmplitude, double quietSeconds, int loudAmplitude, double loudSeconds)
        {
            byte[] lead = MakePcmTone(quietAmplitude, quietSeconds, 0.05, 0);
            byte[] mid = MakePcmTone(loudAmplitude, loudSeconds, 0, 0);
            byte[] trail = MakePcmTone(quietAmplitude, quietSeconds, 0, 0.05);
            byte[] pcm = new byte[lead.Length + mid.Length + trail.Length];
            Buffer.BlockCopy(lead, 0, pcm, 0, lead.Length);
            Buffer.BlockCopy(mid, 0, pcm, lead.Length, mid.Length);
            Buffer.BlockCopy(trail, 0, pcm, lead.Length + mid.Length, trail.Length);
            return pcm;
        }

        private static int SnapshotPeak(byte[] pcm)
        {
            int peak = 0;
            for (int index = 0; index + 1 < pcm.Length; index += 2)
            {
                int sample = Math.Abs((short)(pcm[index] | (pcm[index + 1] << 8)));
                if (sample > peak) peak = sample;
            }
            return peak;
        }

        private static string FuzzySettingsTest()
        {
            AppSettings settings = AppSettings.Defaults();
            ForegroundInfo context = new ForegroundInfo();
            context.Title = "Cursor Settings";
            return TextProcessor.Clean("open setsings", settings, context);
        }

        private static ForegroundInfo NotepadContext(string title)
        {
            ForegroundInfo context = new ForegroundInfo();
            context.ProcessName = "notepad";
            context.Title = title;
            return context;
        }

        private static ForegroundInfo DiscordContext(string contactName)
        {
            ForegroundInfo context = new ForegroundInfo();
            context.ProcessName = "Discord";
            context.Title = contactName;
            return context;
        }

        private static int AssertEqual(string name, string expected, string actual)
        {
            if (String.Equals(expected, actual, StringComparison.Ordinal))
            {
                Console.WriteLine("PASS " + name);
                return 0;
            }
            Console.WriteLine("FAIL " + name + " expected=[" + expected + "] actual=[" + actual + "]");
            return 1;
        }

        private static int AssertContains(string needle, string haystack)
        {
            if (haystack != null && haystack.IndexOf(needle, StringComparison.Ordinal) >= 0)
            {
                Console.WriteLine("PASS contains " + needle);
                return 0;
            }
            Console.WriteLine("FAIL missing " + needle + " in [" + haystack + "]");
            return 1;
        }

        private static int AssertTrue(bool value)
        {
            return AssertTrue("true", value);
        }

        private static int AssertTrue(string name, bool value)
        {
            if (value) { Console.WriteLine("PASS " + name); return 0; }
            Console.WriteLine("FAIL " + name);
            return 1;
        }

        private static int AssertFalse(bool value)
        {
            if (!value) { Console.WriteLine("PASS false"); return 0; }
            Console.WriteLine("FAIL expected false");
            return 1;
        }
    }
}
