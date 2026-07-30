using System;
using System.Collections.Generic;
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
            failures += AssertTrue(TranscriptionQuality.ShouldReject("T", 500, 12000));
            failures += AssertFalse(TranscriptionQuality.ShouldReject("no", 500, 12000));
            failures += AssertEqual("embedded lone T",
                "So we need to finish the project by Friday and then send it to the client for review.",
                TextProcessor.Clean("So we need to finish the project by Friday and T then send it to the client for review", AppSettings.Defaults()));
            failures += AssertEqual("mid prompt echo",
                "The integration is working well and we should ship tomorrow.",
                TextProcessor.Clean("The integration is working well Target window 1x garbage and we should ship tomorrow", AppSettings.Defaults()));
            failures += AssertTrue(TranscriptionQuality.IsLikelyEmbeddedHallucination("T", 0.08, 0.4, 0.5));
            failures += AssertFalse(TranscriptionQuality.IsLikelyEmbeddedHallucination("I", 0.08, 0.4, 0.5));
            failures += AssertContains("1.", TextProcessor.Clean("first get milk second get bread third get eggs", AppSettings.Defaults()));
            failures += AssertContains("- ", TextProcessor.Clean("bullet point apples bullet point bananas bullet point cherries", AppSettings.Defaults()));
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
            ForegroundInfo cursor = new ForegroundInfo();
            cursor.ProcessName = "Cursor";
            failures += AssertTrue(ForegroundContext.IsCursorFamily(cursor));
            failures += AssertTrue(FlowtypeVersion.IsNewerThanCurrent("v1.3.36"));
            failures += AssertFalse(FlowtypeVersion.IsNewerThanCurrent("v" + FlowtypeVersion.CurrentLabel));
            failures += AssertEqual("version label", "1.3.35", FlowtypeVersion.CurrentLabel);
            failures += AssertEqual("pcm duration", 1.0, PcmAudio.DurationSeconds(32000), 0.01);
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

        private static string FuzzySettingsTest()
        {
            AppSettings settings = AppSettings.Defaults();
            ForegroundInfo context = new ForegroundInfo();
            context.Title = "Cursor Settings";
            return TextProcessor.Clean("open setsings", settings, context);
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

        private static int AssertEqual(string name, double expected, double actual, double tolerance)
        {
            if (Math.Abs(expected - actual) <= tolerance)
            {
                Console.WriteLine("PASS " + name);
                return 0;
            }
            Console.WriteLine("FAIL " + name + " expected=" + expected + " actual=" + actual);
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
            if (value) { Console.WriteLine("PASS true"); return 0; }
            Console.WriteLine("FAIL expected true");
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
